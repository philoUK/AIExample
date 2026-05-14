namespace AdministrationModule.Administrators;

internal abstract record AdministratorStatus
{
    public abstract bool IsActive { get; }
}
