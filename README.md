# AIExample

An event-sourced ASP.NET Core 10 API orchestrated with .NET Aspire, using PostgreSQL as the event store.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker (required when running via Aspire AppHost — manages PostgreSQL automatically)

### Install the Aspire workload

```bash
dotnet workload install aspire
```

## Running the application

### Option 1: AppHost (recommended)

The AppHost project orchestrates the full stack — it provisions a PostgreSQL container, runs database migrations, and starts the API. It also exposes the Aspire Dashboard for live telemetry.

```bash
dotnet run --project src/AppHost
```

Once running:

- **Aspire Dashboard** — displayed in the terminal output (typically `http://localhost:15888`). Shows structured logs, distributed traces, and metrics for all services.
- **API** — URL shown in the dashboard. Swagger UI is available at `/openapi/v1.json` in development.
- **PostgreSQL** — managed automatically; no manual database setup required.

### Option 2: API project directly

Run the API on its own without Aspire. You must supply a PostgreSQL connection string manually.

```bash
dotnet run --project src/Api
```

Set the connection string via an environment variable or `appsettings.json`:

```bash
# Environment variable (overrides appsettings)
ConnectionStrings__EventStore="Host=localhost;Database=eventstore;Username=postgres;Password=postgres"
```

Or edit [src/Api/appsettings.json](src/Api/appsettings.json):

```json
{
  "ConnectionStrings": {
    "EventStore": "Host=localhost;Database=eventstore;Username=postgres;Password=postgres"
  }
}
```

Database migrations run automatically on startup.

## Adding migrations

When the `EventStoreDbContext` schema changes, generate a new EF Core migration from the repository root:

```bash
dotnet ef migrations add <MigrationName> -p src/EventStore -s src/Api
```

The `-p` flag points to the project that owns the `DbContext`; `-s` provides the start-up project so EF can resolve configuration. The design-time factory in `EventStoreDbContextFactory` supplies a local connection string so the CLI works without Aspire running.

Commit both the generated migration file (e.g. `Migrations/<timestamp>_<MigrationName>.cs`) and the updated snapshot (`Migrations/EventStoreDbContextModelSnapshot.cs`). Migrations are applied automatically at API start-up by `EventStoreMigrationService`.

## Telemetry

### Azure Application Insights

Set the following environment variable to enable Azure Monitor export:

```bash
APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=...;IngestionEndpoint=..."
```

When this variable is absent the application runs normally without sending telemetry to Azure.

### OpenTelemetry (OTLP)

Set `OTEL_EXPORTER_OTLP_ENDPOINT` to forward traces, metrics, and logs to any OTLP-compatible backend (e.g. Jaeger, Grafana Tempo):

```bash
OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4317"
```

When running via AppHost, the Aspire Dashboard acts as the OTLP collector automatically — no additional configuration needed.
