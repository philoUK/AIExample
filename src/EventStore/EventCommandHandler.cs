namespace EventStore;

public abstract class EventCommandHandler<TCommand>(IEventStore eventStore)
{
    protected EventStream<TEntity> GetStream<TEntity>(Guid aggregateId)
        where TEntity : AggregateRoot, new()
    {
        return new EventStream<TEntity>(eventStore, aggregateId);
    }

    internal async Task Execute(TCommand command)
    {
        await Handle(command);
        await eventStore.SaveChanges();
    }

    protected abstract Task Handle(TCommand command);
}
