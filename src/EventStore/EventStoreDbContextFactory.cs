using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventStore;

// Used only by EF Core CLI tooling (dotnet ef migrations add / dbcontext info).
// At runtime the connection string comes from Aspire via RegistrationExtensions.RegisterEventStore().
// This class is never called during normal app startup.
internal class EventStoreDbContextFactory : IDesignTimeDbContextFactory<EventStoreDbContext>
{
    public EventStoreDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EventStoreDbContext>()
            .UseNpgsql("Host=localhost;Database=eventstore;Username=postgres;Password=postgres")
            .Options;
        return new EventStoreDbContext(options);
    }
}
