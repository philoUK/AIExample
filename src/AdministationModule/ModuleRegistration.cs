using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AdministrationModule;

public static class ModuleRegistration
{
    public static void RegisterAdministrationModule(this IServiceCollection services)
    {
        // Register command handlers
        // services.AddTransient<CommandHandler<YourCommand>, YourCommandHandler>();
    }

    public static void MapAdministrationEndpoints(this IEndpointRouteBuilder app)
    {
        // Map your endpoints here
        // app.MapPost("/your-endpoint", YourEndpointHandler);
    }
}
