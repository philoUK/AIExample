namespace EventStore;

public abstract class AggregateRoot
{
    private readonly List<object> _uncommittedEvents = [];

    protected void RaiseEvent(object @event)
    {
        _uncommittedEvents.Add(@event);
        Apply(@event);
    }

    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

    public IReadOnlyCollection<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    public void Apply(object @event) { }
}
