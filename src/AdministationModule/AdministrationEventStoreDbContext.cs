using EventStore;
using Microsoft.EntityFrameworkCore;

namespace AdministrationModule;

public class AdministrationEventStoreDbContext(
    DbContextOptions<AdministrationEventStoreDbContext> options)
    : EventStoreDbContext(options);
