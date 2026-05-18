using System.Diagnostics;
using System.Reflection;
using Shared;

namespace EventStore;

public class EventCommandRouter(IServiceProvider serviceProvider)
{
    public async Task<Result> HandleCommand(object command)
    {
        var commandType = command.GetType();
        using var activity = ActivitySources.Commands.StartActivity(commandType.Name);
        activity?.SetTag("command.type", commandType.Name);

        try
        {
            var handlerType = typeof(EventCommandHandler<>).MakeGenericType(commandType);
            var handler = serviceProvider.GetService(handlerType);
            var methodInfo = handler
                ?.GetType()
                .GetMethod(
                    "Execute",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
            if (methodInfo?.Invoke(handler, [command]) is Task<Result> task)
            {
                var result = await task;
                if (!result.IsSuccess)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, string.Join(';', result.Errors));
                    activity?.SetTag("error", true);
                    return result;
                }
            }
            activity?.SetStatus(ActivityStatusCode.Ok);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            var actual =
                ex is TargetInvocationException tie && tie.InnerException is not null
                    ? tie.InnerException
                    : ex;
            activity?.SetStatus(ActivityStatusCode.Error, actual.Message);
            activity?.SetTag("exception.type", actual.GetType().FullName);
            activity?.SetTag("exception.message", actual.Message);
            activity?.SetTag("exception.stacktrace", actual.StackTrace);
            if (actual != ex)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(actual).Throw();
            throw;
        }
    }
}
