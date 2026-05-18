using Shared;

namespace EventStore;

public abstract class EventCommandHandler<TCommand>(IEventStore eventStore)
{
    protected EventStream<TEntity> GetStream<TEntity>(Guid aggregateId)
        where TEntity : AggregateRoot, new()
    {
        return new EventStream<TEntity>(eventStore, aggregateId);
    }

    internal async Task<Result> Execute(TCommand command)
    {
        var result = await Handle(command);
        if (result.IsSuccess)
        {
            await eventStore.SaveChanges();
        }
        return result;
    }

    protected abstract Task<Result> Handle(TCommand command);
}
