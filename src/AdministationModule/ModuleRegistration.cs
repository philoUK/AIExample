using EventStore;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace AdministrationModule;

public static class ModuleRegistration
{
    public static void RegisterAdministrationModule(this IHostApplicationBuilder builder)
    {
        builder.RegisterModuleEventStore<AdministrationEventStoreDbContext>(
            "administration-eventstore");
        // Register command handlers as they are added, e.g.:
        // builder.Services.AddModuleCommandHandler<YourCommand, YourCommandHandler, AdministrationEventStoreDbContext>();
    }

    public static void MapAdministrationEndpoints(this IEndpointRouteBuilder app)
    {
        // Map your endpoints here
        // app.MapPost("/your-endpoint", YourEndpointHandler);
    }
}
