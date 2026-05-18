using AdministrationContracts;
using AdministrationModule.Administrators.UseCases;
using EventStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AdministrationModule;

public static class ModuleRegistration
{
    public static void RegisterAdministrationModule(this IHostApplicationBuilder builder)
    {
        builder.RegisterModuleEventStore<AdministrationEventStoreDbContext>(
            "administration-eventstore");
        builder.Services.AddModuleCommandHandler<InviteAdministratorCommand, InviteAdministratorCommandHandler, AdministrationEventStoreDbContext>();
    }

    public static void MapAdministrationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/administrators/invite", async (InviteAdministratorRequest request, EventCommandRouter router) =>
        {
            // TODO: replace with real authenticated user id once auth is implemented
            var invitedBy = new Guid("00000000-0000-0000-0000-000000000001");
            var command = new InviteAdministratorCommand(invitedBy, request.FirstName, request.LastName, request.Email);
            var result = await router.HandleCommand(command);
            return result.IsSuccess
                ? Results.Created()
                : Results.BadRequest(result.Errors);
        });
    }
}
