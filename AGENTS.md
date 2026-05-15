# Agent Instructions

## Branching

When implementing a GitHub issue, always create a new branch before making any changes. Never commit implementation work directly to `main`. Name the branch after the issue (e.g. `issue/42-short-description`).

---

## Architectural Decision Records

## ADR-001: Per-Module Event Store Databases

**Date:** 2026-05-15  
**Status:** Implemented

### Problem

The initial implementation registered a single `IEventStore` bound to one shared PostgreSQL database (`"eventstore"`). All modules would have shared the same event store database, which creates tight coupling and prevents modules from evolving independently.

### Decision

Each module owns its own isolated event store database. Modules communicate with each other only via published contracts (e.g. `AdministratorInvited`), never by reading another module's event store directly.

### Decisions Made

| Decision | Choice | Rationale |
| --- | --- | --- |
| Schema across modules | Identical — same events table | No per-module customisation needed; avoids schema drift |
| Module isolation | Fully isolated — no cross-module reading | Modules communicate via contract events only |
| Registration ownership | Each module registers its own event store | Keeps modules self-contained |
| Migration files | Reuse EventStore project's migrations | No duplication; schema changes flow to all modules automatically |
| `SaveChanges()` owner | Moved from `EventCommandRouter` into `EventCommandHandler` | Router cannot know which `IEventStore` to save; handler owns its store |
| EF design-time factory | One factory per module project | Consistent with existing EventStore pattern; self-contained |

### Implementation

#### Infrastructure changes (EventStore project)

- `EventStoreDbContext` made `public` (was `internal`) with a non-generic `DbContextOptions` constructor so modules can subclass it.
- `DatabaseEvent` made `public` (required by the public DbContext property).
- `EventCommandHandler<TCommand>` gained an `internal Execute()` method that calls `Handle()` then `eventStore.SaveChanges()`. The router now calls `Execute`, not `Handle`.
- `EventCommandRouter` no longer takes `IEventStore` — it only needs `IServiceProvider`. `SaveChanges()` responsibility moved to the handler.
- `EventStoreMigrationService` takes a `connectionStringName` parameter. It reads the connection string from `IConfiguration`, creates a temporary `EventStoreDbContext` against that database, and applies EventStore's migrations. Using the base `EventStoreDbContext` type ensures EF Core finds the migrations in the EventStore assembly without additional configuration.
- `RegistrationExtensions` replaced with three focused helpers:
  - `RegisterModuleEventStore<TContext>(string connectionStringName)` — registers the module's DbContext, a keyed `IEventStore`, and the migration service.
  - `AddModuleCommandHandler<TCommand, THandler, TContext>()` — registers a command handler wired to the correct keyed `IEventStore`.
  - `RegisterEventCommandRouter()` — registers the single global router.

#### Keyed IEventStore

Multiple modules can all register `IEventStore` without conflict by using keyed services. The key is `typeof(TContext)` — a compile-time type, not a string. Handlers are registered via a factory lambda that resolves the correct keyed `IEventStore` for that module, so no `[FromKeyedServices]` attributes appear on handler classes.

#### Per-module pattern

Each module (e.g. `AdministrationModule`) provides:

1. **A DbContext subclass** in the module project:

   ```csharp
   public class AdministrationEventStoreDbContext(
       DbContextOptions<AdministrationEventStoreDbContext> options)
       : EventStoreDbContext(options);
   ```

2. **A design-time factory** in the module project (for `dotnet ef` tooling):

   ```csharp
   public class AdministrationEventStoreDbContextFactory
       : IDesignTimeDbContextFactory<AdministrationEventStoreDbContext>
   {
       public AdministrationEventStoreDbContext CreateDbContext(string[] args)
       {
           var options = new DbContextOptionsBuilder<AdministrationEventStoreDbContext>()
               .UseNpgsql("Host=localhost;Database=administration-eventstore;...")
               .Options;
           return new AdministrationEventStoreDbContext(options);
       }
   }
   ```

3. **Registration in `ModuleRegistration`**:

   ```csharp
   public static void RegisterAdministrationModule(this IHostApplicationBuilder builder)
   {
       builder.RegisterModuleEventStore<AdministrationEventStoreDbContext>("administration-eventstore");
       // builder.Services.AddModuleCommandHandler<MyCommand, MyHandler, AdministrationEventStoreDbContext>();
   }
   ```

4. **A database in AppHost**:

   ```csharp
   var administrationEventStore = postgres.AddDatabase("administration-eventstore");
   builder.AddProject<Projects.Api>("api").WithReference(administrationEventStore)...
   ```

### Adding a Future Module

For any new module (e.g. `OrdersModule`):

1. Create `OrdersEventStoreDbContext : EventStoreDbContext` in the module project.
2. Create `OrdersEventStoreDbContextFactory` in the module project.
3. Call `builder.RegisterModuleEventStore<OrdersEventStoreDbContext>("orders-eventstore")` in the module's registration.
4. Register handlers with `services.AddModuleCommandHandler<..., OrdersEventStoreDbContext>()`.
5. Add `postgres.AddDatabase("orders-eventstore")` in `AppHost.cs` and reference it from the API project.

### Migration strategy

Migrations are written once in the `EventStore` project for the base `EventStoreDbContext`. The migration service creates an `EventStoreDbContext` targeting each module's database and runs those migrations there. Since EF Core finds migrations by the DbContext type's assembly, EventStore's migrations are always used — no per-module migration files needed. When the schema changes, a single migration in `EventStore` applies to all module databases on next startup.
