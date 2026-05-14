using Shared;

namespace AdministrationModule.Administrators;

internal record Email(string Address)
{
    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Fail<Email>("Email cannot be empty.");

        if (!email.Contains("@"))
            return Result.Fail<Email>("Email must contain '@'.");

        return Result.Ok(new Email(email));
    }
}
