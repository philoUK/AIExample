using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace EventStore.Tests;

/// <summary>
/// Integration tests that spin up a real PostgreSQL container and verify that
/// <see cref="EventStoreMigrationService"/> produces the expected schema.
/// </summary>
public class EventStoreMigrationIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    public Task InitializeAsync() => _postgres.StartAsync();

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    // ── helpers ──────────────────────────────────────────────────────────────

    private EventStoreDbContext BuildDbContext()
    {
        var options = new DbContextOptionsBuilder<EventStoreDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new EventStoreDbContext(options);
    }

    private EventStoreMigrationService BuildMigrationService(EventStoreDbContext context)
    {
        var services = new ServiceCollection();
        services.AddSingleton(context);
        services.AddScoped(_ => context);
        var scopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
        return new EventStoreMigrationService(scopeFactory);
    }

    // ── tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AfterMigration_EventsTableExists()
    {
        await using var context = BuildDbContext();
        var service = BuildMigrationService(context);

        await service.StartAsync(CancellationToken.None);

        var tableExists = await TableExistsAsync("events");
        Assert.True(tableExists, "The 'events' table should exist after migrations run.");
    }

    [Fact]
    public async Task AfterMigration_EventsTableHasExpectedColumns()
    {
        await using var context = BuildDbContext();
        var service = BuildMigrationService(context);

        await service.StartAsync(CancellationToken.None);

        // information_schema.columns covers regular columns.
        var regularColumns = await GetColumnNamesAsync("events");
        Assert.Contains("Id", regularColumns, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("AggregateId", regularColumns, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("SequenceNumber", regularColumns, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Timestamp", regularColumns, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("EventTypeName", regularColumns, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("EventBody", regularColumns, StringComparer.OrdinalIgnoreCase);

        // xmin is a PostgreSQL system column — not visible in information_schema.columns.
        // Verify via pg_attribute (attnum < 0 means system column).
        var xminExists = await SystemColumnExistsAsync("events", "xmin");
        Assert.True(xminExists, "The 'xmin' system column should exist on the events table.");
    }

    [Fact]
    public async Task AfterMigration_UniqueIndexOnAggregateIdAndSequenceNumberExists()
    {
        await using var context = BuildDbContext();
        var service = BuildMigrationService(context);

        await service.StartAsync(CancellationToken.None);

        var indexExists = await UniqueIndexExistsAsync("events", "AggregateId", "SequenceNumber");
        Assert.True(
            indexExists,
            "A unique index on (AggregateId, SequenceNumber) should exist after migrations run."
        );
    }

    // ── raw-SQL helpers ───────────────────────────────────────────────────────

    private async Task<bool> TableExistsAsync(string tableName)
    {
        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_name   = @tableName
            """;
        cmd.Parameters.AddWithValue("tableName", tableName);

        var count = (long)(await cmd.ExecuteScalarAsync())!;
        return count > 0;
    }

    private async Task<IReadOnlyList<string>> GetColumnNamesAsync(string tableName)
    {
        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT column_name
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name   = @tableName
            """;
        cmd.Parameters.AddWithValue("tableName", tableName);

        var columns = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            columns.Add(reader.GetString(0));

        return columns;
    }

    private async Task<bool> SystemColumnExistsAsync(string tableName, string columnName)
    {
        // pg_attribute tracks both regular (attnum > 0) and system columns (attnum < 0).
        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(*)
            FROM pg_attribute att
            JOIN pg_class     tbl ON tbl.oid = att.attrelid
            JOIN pg_namespace nsp ON nsp.oid = tbl.relnamespace
            WHERE nsp.nspname  = 'public'
              AND tbl.relname  = @tableName
              AND att.attname  = @columnName
            """;
        cmd.Parameters.AddWithValue("tableName", tableName);
        cmd.Parameters.AddWithValue("columnName", columnName);

        var count = (long)(await cmd.ExecuteScalarAsync())!;
        return count > 0;
    }

    private async Task<bool> UniqueIndexExistsAsync(
        string tableName,
        string column1,
        string column2
    )
    {
        // pg_index / pg_attribute gives us the column list for each index;
        // we look for a unique index whose column set is exactly {column1, column2}.
        // Cast attname (name type) to text to allow comparison with text[].
        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(*)
            FROM pg_index       idx
            JOIN pg_class       tbl  ON tbl.oid  = idx.indrelid
            JOIN pg_namespace   nsp  ON nsp.oid  = tbl.relnamespace
            WHERE nsp.nspname     = 'public'
              AND tbl.relname     = @tableName
              AND idx.indisunique = true
              AND (
                  SELECT array_agg(att.attname::text ORDER BY att.attname::text)
                  FROM   pg_attribute att
                  WHERE  att.attrelid = tbl.oid
                    AND  att.attnum   = ANY(idx.indkey)
              ) = ARRAY(
                  SELECT unnest(ARRAY[@col1, @col2]::text[]) ORDER BY 1
              )
            """;
        cmd.Parameters.AddWithValue("tableName", tableName);
        cmd.Parameters.AddWithValue("col1", column1);
        cmd.Parameters.AddWithValue("col2", column2);

        var count = (long)(await cmd.ExecuteScalarAsync())!;
        return count > 0;
    }
}
