using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventStore.Tests;

public class EventCommandRouterTests : IDisposable
{
    private readonly List<Activity> _stopped = [];
    private readonly ActivityListener _listener;
    private readonly FakeEventStore _store = new();

    public EventCommandRouterTests()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = src => src.Name == "AIExample.Commands",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = a => _stopped.Add(a),
        };
        ActivitySource.AddActivityListener(_listener);
    }

    public void Dispose() => _listener.Dispose();

    [Fact]
    public async Task HandleCommand_CreatesActivityWithCommandTypeTag()
    {
        var router = BuildRouter(new SuccessHandler(_store));

        await router.HandleCommand(new TestCommand());

        var activity = Assert.Single(_stopped);
        Assert.Equal(nameof(TestCommand), activity.OperationName);
        Assert.Equal(nameof(TestCommand), activity.GetTagItem("command.type"));
    }

    [Fact]
    public async Task HandleCommand_WhenHandlerThrows_ActivityStatusIsError()
    {
        var router = BuildRouter(new ThrowingHandler(_store));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => router.HandleCommand(new TestCommand())
        );

        var activity = Assert.Single(_stopped);
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
    }

    [Fact]
    public async Task HandleCommand_WhenHandlerSucceeds_ActivityStatusIsNotError()
    {
        var router = BuildRouter(new SuccessHandler(_store));

        await router.HandleCommand(new TestCommand());

        var activity = Assert.Single(_stopped);
        Assert.NotEqual(ActivityStatusCode.Error, activity.Status);
    }

    private EventCommandRouter BuildRouter(EventCommandHandler<TestCommand> handler)
    {
        var services = new ServiceCollection();
        services.AddSingleton<EventCommandHandler<TestCommand>>(handler);
        return new EventCommandRouter(services.BuildServiceProvider());
    }
}

// ── test doubles ──────────────────────────────────────────────────────────────

record TestCommand;

class SuccessHandler(IEventStore eventStore) : EventCommandHandler<TestCommand>(eventStore)
{
    protected override Task Handle(TestCommand command) => Task.CompletedTask;
}

class ThrowingHandler(IEventStore eventStore) : EventCommandHandler<TestCommand>(eventStore)
{
    protected override async Task Handle(TestCommand command)
    {
        await Task.Yield();
        throw new InvalidOperationException("handler error");
    }
}

class FakeEventStore : IEventStore
{
    public Task<IEnumerable<StoredEvent>> GetEvents(Guid aggregateId) =>
        Task.FromResult(Enumerable.Empty<StoredEvent>());

    public Task<IEnumerable<StoredEvent>> GetEventsUntilSequence(
        Guid aggregateId,
        int sequenceNumber
    ) => Task.FromResult(Enumerable.Empty<StoredEvent>());

    public void AppendEvent(StoredEvent storedEvent) { }

    public Task SaveChanges() => Task.CompletedTask;
}
