using Microsoft.EntityFrameworkCore;

namespace EventStore;

internal class EventStoreDbContext(DbContextOptions<EventStoreDbContext> options)
    : DbContext(options)
{
    public DbSet<DatabaseEvent> Events => Set<DatabaseEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DatabaseEvent>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AggregateId, e.SequenceNumber }).IsUnique();
            entity
                .Property(e => e.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        });
    }
}
