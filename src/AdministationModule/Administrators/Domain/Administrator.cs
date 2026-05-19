using AdministrationContracts;
using EventStore;
using Shared;

namespace AdministrationModule.Administrators.Domain;

internal class Administrator : AggregateRoot
{
    public AdministratorStatus? Status { get; private set; }
    public bool IsActive => Status?.IsActive ?? false;
    public bool IsPending => Status is Invited;

    public Result Invite(
        Guid invitedBy,
        RegistrationId registrationId,
        string firstName,
        string lastName,
        Email email
    )
    {
        if (IsActive)
        {
            return Result.Fail("Administrator is already active.");
        }
        if (IsPending)
        {
            return Result.Fail("Administrator invitation is already pending.");
        }
        var statusResult = Invited.Create(
            invitedBy,
            registrationId.Value,
            email,
            firstName,
            lastName
        );
        if (statusResult.IsFailure)
        {
            return Result.Fail(statusResult.Errors.ToArray());
        }
        var status = statusResult.Value;
        var @event = new AdministratorInvited(
            invitedBy,
            status.RegistrationId,
            status.FirstName,
            status.LastName,
            status.Email.Address,
            status.ExpiryDate
        );
        RaiseEvent(@event);
        return Result.Ok();
    }

    public void Apply(AdministratorInvited @event)
    {
        Status = new Invited(
            @event.InvitedBy,
            @event.RegistrationId,
            new Email(@event.EmailAddress),
            @event.FirstName,
            @event.LastName,
            @event.ExpiryDate
        );
    }
}
