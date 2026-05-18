using Shared;

namespace AdministrationModule.Administrators;

internal record RegistrationId(Guid Value)
{
    public static Result<RegistrationId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Fail<RegistrationId>("Registration ID cannot be empty.");
        }
        return Result.Ok(new RegistrationId(value));
    }
}
