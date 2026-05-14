using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventStore;

public static class RegistrationExtensions
{
    public static void RegisterEventStore(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<EventStoreDbContext>("eventstore");

        builder.Services.AddScoped<IEventStore, PostgresEventStore>();
        builder.Services.AddHostedService<EventStoreMigrationService>();
    }
}
