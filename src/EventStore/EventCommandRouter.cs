namespace EventStore;

public class EventCommandRouter(IEventStore eventStore, IServiceProvider serviceProvider)
{
    public async Task HandleCommand(object command)
    {
        var commandType = command.GetType();
        var handlerType = typeof(EventCommandHandler<>).MakeGenericType(commandType);
        var handler = serviceProvider.GetService(handlerType);
        var methodInfo = handlerType.GetMethod("Handle");
        if (methodInfo?.Invoke(handler, [command]) is Task task)
            await task;
        await eventStore.SaveChanges();
    }
}
