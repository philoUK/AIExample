namespace AdministrationContracts;

public record AdministratorInvited(
    Guid InvitedBy,
    Guid RegistrationId,
    string FirstName,
    string LastName,
    string EmailAddress,
    DateTime ExpiryDate
);
