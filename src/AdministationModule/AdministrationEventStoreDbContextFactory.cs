using EventStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AdministrationModule;

public class AdministrationEventStoreDbContextFactory
    : IDesignTimeDbContextFactory<AdministrationEventStoreDbContext>
{
    public AdministrationEventStoreDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AdministrationEventStoreDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=administration-eventstore;Username=postgres;Password=postgres")
            .Options;
        return new AdministrationEventStoreDbContext(options);
    }
}
