namespace EventStore;

public class DatabaseEvent
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public int SequenceNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public string? EventTypeName { get; set; }
    public string? EventBody { get; set; }
    public uint RowVersion { get; set; }
}
