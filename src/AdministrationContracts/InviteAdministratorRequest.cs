using System.ComponentModel.DataAnnotations;

namespace AdministrationContracts;

public record InviteAdministratorRequest(
    [property: Required] string FirstName,
    [property: Required] string LastName,
    [property: Required] string Email
);
