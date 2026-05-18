namespace AdministrationModule.Domain.Tests;

using AdministrationModule.Administrators;
using EventStore;

public class AdministratorTests
{
    private readonly Guid InvitedBy = Guid.NewGuid();
    private readonly RegistrationId RegistrationId = new RegistrationId(Guid.NewGuid());
    private readonly string FirstName = "John";
    private readonly string LastName = "Smith";
    private readonly Email ValidEmail = new("johnsmith@testing.com");

    [Fact]
    public void Invite_BlankInvitedByFails()
    {
        var admin = new Administrator();
        var result = admin.Invite(Guid.Empty, RegistrationId, FirstName, LastName, ValidEmail);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Invite_BlankFirstNameFails()
    {
        var admin = new Administrator();
        var result = admin.Invite(InvitedBy, RegistrationId, string.Empty, LastName, ValidEmail);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Invite_BlankLastNameFails()
    {
        var admin = new Administrator();
        var result = admin.Invite(InvitedBy, RegistrationId, FirstName, string.Empty, ValidEmail);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Invite_NewAdmin_AllDataCorrect_Succeeds()
    {
        var admin = new Administrator();
        var result = admin.Invite(InvitedBy, RegistrationId, FirstName, LastName, ValidEmail);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Invite_FailsIfAdministratorIsPending()
    {
        var store = new InMemoryEventStore();
        var adminId = Guid.NewGuid();
        var stream = new EventStream<Administrator>(store, adminId);
        var admin = await stream.GetEntity();
        admin.Invite(InvitedBy, RegistrationId, FirstName, LastName, ValidEmail);
        admin.GetUncommittedEvents().ToList().ForEach(e => stream.Append(e));
        await store.SaveChanges();
        var admin2 = await stream.GetEntity();
        var result = admin2.Invite(InvitedBy, RegistrationId, FirstName, LastName, ValidEmail);
        Assert.False(result.IsSuccess);
    }
}
