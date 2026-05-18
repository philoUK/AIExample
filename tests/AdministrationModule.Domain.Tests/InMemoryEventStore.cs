using EventStore;

namespace AdministrationModule.Domain.Tests;

public class InMemoryEventStore : IEventStore
{
    private readonly List<StoredEvent> _events = [];
    private readonly List<StoredEvent> _pendingEvents = [];

    public void AppendEvent(StoredEvent storedEvent)
    {
        _pendingEvents.Add(storedEvent);
    }

    public Task<IEnumerable<StoredEvent>> GetEvents(Guid aggregateId)
    {
        var events = _events.Where(e => e.AggregateId == aggregateId);
        return Task.FromResult(events.AsEnumerable());
    }

    public Task<IEnumerable<StoredEvent>> GetEventsUntilSequence(
        Guid aggregateId,
        int sequenceNumber
    )
    {
        var events = _events.Where(e =>
            e.AggregateId == aggregateId && e.SequenceNumber <= sequenceNumber
        );
        return Task.FromResult(events.AsEnumerable());
    }

    public Task SaveChanges()
    {
        _events.AddRange(_pendingEvents);
        _pendingEvents.Clear();
        return Task.CompletedTask;
    }
}
