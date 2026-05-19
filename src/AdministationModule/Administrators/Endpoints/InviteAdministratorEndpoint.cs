using System.Diagnostics;
using AdministrationContracts;
using AdministrationModule.Administrators.UseCases;
using EventStore;
using Microsoft.AspNetCore.Http;

namespace AdministrationModule.Administrators.Endpoints;

internal static class InviteAdministratorEndpoint
{
    internal static async Task<IResult> Handle(
        InviteAdministratorRequest request,
        EventCommandRouter router
    )
    {
        using var activity = ActivitySources.Endpoints.StartActivity("InviteAdministrator");
        activity?.SetTag("administrator.email", request.Email);

        // TODO: replace with real authenticated user id once auth is implemented
        var invitedBy = new Guid("00000000-0000-0000-0000-000000000001");
        var command = new InviteAdministratorCommand(
            invitedBy,
            request.FirstName,
            request.LastName,
            request.Email
        );
        var result = await router.HandleCommand(command);

        if (result.IsSuccess)
        {
            activity?.SetStatus(ActivityStatusCode.Ok);
            return Results.Created();
        }

        activity?.SetStatus(ActivityStatusCode.Error, string.Join(';', result.Errors));
        activity?.SetTag("error", true);
        activity?.SetTag("error.messages", string.Join(';', result.Errors));
        return Results.BadRequest(result.Errors);
    }
}
