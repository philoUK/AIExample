using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace EventStore;

internal class PostgresEventStore(EventStoreDbContext context) : IEventStore
{
    public async Task<IEnumerable<StoredEvent>> GetEvents(Guid aggregateId)
    {
        var rows = await context
            .Events.Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.SequenceNumber)
            .ToListAsync();

        return rows.Select(Deserialize);
    }

    public async Task<IEnumerable<StoredEvent>> GetEventsUntilSequence(
        Guid aggregateId,
        int sequenceNumber
    )
    {
        var rows = await context
            .Events.Where(e => e.AggregateId == aggregateId && e.SequenceNumber <= sequenceNumber)
            .OrderBy(e => e.SequenceNumber)
            .ToListAsync();

        return rows.Select(Deserialize);
    }

    public void AppendEvent(StoredEvent storedEvent)
    {
        var row = new DatabaseEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = storedEvent.AggregateId,
            SequenceNumber = storedEvent.SequenceNumber,
            Timestamp = storedEvent.Timestamp,
            EventTypeName = storedEvent.EventData.GetType().FullName,
            EventBody = JsonSerializer.Serialize(
                storedEvent.EventData,
                storedEvent.EventData.GetType()
            ),
        };

        context.Events.Add(row);
    }

    public async Task SaveChanges() => await context.SaveChangesAsync();

    private static StoredEvent Deserialize(DatabaseEvent row)
    {
        var type =
            AppDomain
                .CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(row.EventTypeName!))
                .FirstOrDefault(t => t != null)
            ?? throw new InvalidOperationException(
                $"Cannot resolve event type '{row.EventTypeName}'."
            );

        var eventData =
            JsonSerializer.Deserialize(row.EventBody!, type)
            ?? throw new InvalidOperationException(
                $"Cannot deserialize event body for type '{row.EventTypeName}'."
            );

        return new StoredEvent(row.AggregateId, row.SequenceNumber, row.Timestamp, eventData);
    }
}
