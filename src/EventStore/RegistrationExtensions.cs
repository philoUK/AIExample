using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventStore;

public static class RegistrationExtensions
{
    public static void RegisterEventStore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EventStoreDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("EventStore")));

        services.AddScoped<IEventStore, PostgresEventStore>();
        services.AddHostedService<EventStoreMigrationService>();
    }
}
