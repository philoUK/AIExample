using EventStore;
using Shared;

namespace AdministrationModule.Administrators.UseCases;

internal record InviteAdministratorCommand(
    Guid InvitedBy,
    string FirstName,
    string LastName,
    string Email
);

internal class InviteAdministratorCommandHandler : EventCommandHandler<InviteAdministratorCommand>
{
    public InviteAdministratorCommandHandler(IEventStore eventStore)
        : base(eventStore) { }

    protected override async Task<Result> Handle(InviteAdministratorCommand command)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Fail([.. emailResult.Errors]);

        var stream = GetStream<Administrator>(Guid.NewGuid());
        var administrator = await stream.GetEntity();
        var result = administrator.Invite(
            command.InvitedBy,
            new RegistrationId(Guid.NewGuid()),
            command.FirstName,
            command.LastName,
            emailResult.Value
        );
        if (result.IsSuccess)
        {
            foreach (var @event in administrator.GetUncommittedEvents())
            {
                stream.Append(@event);
            }
        }
        return result;
    }
}
