using EventStore;
using Microsoft.EntityFrameworkCore;

namespace AdministrationModule.Infrastructure;

public class AdministrationEventStoreDbContext(
    DbContextOptions<AdministrationEventStoreDbContext> options
) : EventStoreDbContext(options);
