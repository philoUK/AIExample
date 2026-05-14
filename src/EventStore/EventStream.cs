namespace EventStore;

public class EventStream<TEntity>(IEventStore eventStore, Guid aggregateId)
    where TEntity : AggregateRoot, new()
{
    private int _lastSequenceNumber;

    public async Task<TEntity> GetEntity()
    {
        var events = await eventStore.GetEvents(aggregateId);
        TEntity entity = new();
        foreach (var @event in events)
        {
            entity.Apply((dynamic)@event.EventData);
            _lastSequenceNumber = @event.SequenceNumber;
        }
        return entity;
    }

    public async Task<TEntity> GetEntityBySequence(int sequenceNumber)
    {
        var events = await eventStore.GetEventsUntilSequence(aggregateId, sequenceNumber);
        TEntity entity = new();
        foreach (var @event in events)
        {
            entity.Apply((dynamic)@event.EventData);
        }
        return entity;
    }

    public void Append(object @event)
    {
        _lastSequenceNumber++;
        var storedEvent = new StoredEvent(
            aggregateId,
            _lastSequenceNumber,
            DateTime.UtcNow,
            @event
        );
        eventStore.AppendEvent(storedEvent);
    }
}
