using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventStore;

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
