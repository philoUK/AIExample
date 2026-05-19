namespace AdministrationModule.Administrators.Domain;

internal abstract record AdministratorStatus
{
    public abstract bool IsActive { get; }
}
