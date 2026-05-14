using Shared;

namespace AdministrationModule.Administrators;

internal record Invited(
    Guid InvitedBy,
    Guid RegistrationId,
    Email Email,
    string FirstName,
    string LastName,
    DateTime ExpiryDate
) : AdministratorStatus
{
    public override bool IsActive => false;

    public static Result<Invited> Create(
        Guid invitedBy,
        Guid registrationId,
        Email email,
        string firstName,
        string lastName
    )
    {
        var errors = new List<string>();
        if (invitedBy == Guid.Empty)
        {
            errors.Add("InvitedBy cannot be empty.");
        }
        if (registrationId == Guid.Empty)
        {
            errors.Add("RegistrationId cannot be empty.");
        }
        if (string.IsNullOrWhiteSpace(firstName))
        {
            errors.Add("FirstName cannot be empty.");
        }
        if (string.IsNullOrWhiteSpace(lastName))
        {
            errors.Add("LastName cannot be empty.");
        }
        if (errors.Any())
        {
            return Result.Fail<Invited>(errors.ToArray());
        }
        return Result.Ok(
            new Invited(
                invitedBy,
                registrationId,
                email,
                firstName,
                lastName,
                DateTime.UtcNow.AddDays(7)
            )
        );
    }
}
