using AdministrationModule.Administrators.Endpoints;
using AdministrationModule.Administrators.UseCases;
using EventStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AdministrationModule;

public static class ModuleRegistration
{
    public static void RegisterAdministrationModule(this IHostApplicationBuilder builder)
    {
        builder.Services.AddValidation();
        builder.RegisterModuleEventStore<AdministrationEventStoreDbContext>(
            "administration-eventstore"
        );
        builder.Services.AddModuleCommandHandler<
            InviteAdministratorCommand,
            InviteAdministratorCommandHandler,
            AdministrationEventStoreDbContext
        >();
    }

    public static void MapAdministrationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/administrators/invite", InviteAdministratorEndpoint.Handle);
    }
}
