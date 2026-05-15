using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventStore;

public static class RegistrationExtensions
{
    public static void RegisterModuleEventStore<TContext>(
        this IHostApplicationBuilder builder,
        string connectionStringName)
        where TContext : EventStoreDbContext
    {
        builder.AddNpgsqlDbContext<TContext>(connectionStringName);

        builder.Services.AddKeyedScoped<IEventStore>(typeof(TContext), (sp, _) =>
            new PostgresEventStore(sp.GetRequiredService<TContext>()));

        builder.Services.AddHostedService(sp =>
            new EventStoreMigrationService(
                sp.GetRequiredService<IServiceScopeFactory>(),
                connectionStringName));
    }

    public static IServiceCollection AddModuleCommandHandler<TCommand, THandler, TContext>(
        this IServiceCollection services)
        where THandler : EventCommandHandler<TCommand>
        where TContext : EventStoreDbContext
    {
        services.AddScoped<EventCommandHandler<TCommand>>(sp =>
        {
            var eventStore = sp.GetRequiredKeyedService<IEventStore>(typeof(TContext));
            return ActivatorUtilities.CreateInstance<THandler>(sp, eventStore);
        });
        return services;
    }

    public static IServiceCollection RegisterEventCommandRouter(this IServiceCollection services)
    {
        services.AddScoped<EventCommandRouter>();
        return services;
    }
}
