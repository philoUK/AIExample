namespace EventStore;

public interface IEventStore
{
    Task<IEnumerable<StoredEvent>> GetEvents(Guid aggregateId);
    Task<IEnumerable<StoredEvent>> GetEventsUntilSequence(Guid aggregateId, int sequenceNumber);
    void AppendEvent(StoredEvent storedEvent);
    Task SaveChanges();
}
