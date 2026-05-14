namespace EventStore;

public abstract class EventCommandHandler<TCommand>(IEventStore eventStore)
{
    protected EventStream<TEntity> GetStream<TEntity>(Guid aggregateId)
        where TEntity : AggregateRoot, new()
    {
        return new EventStream<TEntity>(eventStore, aggregateId);
    }

    protected abstract Task Handle(TCommand command);
}
