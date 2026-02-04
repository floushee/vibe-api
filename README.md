# VibeAPI

A small, spec-driven ASP.NET Core Minimal API sample.

- Minimal API endpoints in `src/VibeAPI.API`
- CQRS-ish flow via MediatR in `src/VibeAPI.Application`
- Persistence via EF Core in `src/VibeAPI.Data` + domain model in `src/VibeAPI.Domain`
- xUnit integration tests in `tests/VibeAPI.Tests` (SQLite in-memory)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (version pinned in `global.json`)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Aspire uses it to run PostgreSQL)

> **Note:** With .NET 10, Aspire is included as NuGet packages — no separate workload install needed.

## Quick start

```bash
dotnet run --project src/VibeAPI.AppHost
```

> **Linux:** Use the `http` launch profile to avoid HTTPS certificate issues with containerized services:
>
> ```bash
> dotnet run --project src/VibeAPI.AppHost --launch-profile http
> ```

That's it. Aspire orchestrates everything — no environment variables or connection strings to configure.

This starts:
- **PostgreSQL** container with a persistent data volume
- **API** service (development mode, hot reload enabled)
- **Aspire Dashboard** for logs, traces, metrics, and health checks
- **PgAdmin** for database management

The API will be available at `http://localhost:5153`.

The Aspire Dashboard URL is shown in the console output.

Try it via the request file:

- `src/VibeAPI.API/VibeAPI.http`

OpenAPI / docs (Development):

- OpenAPI JSON: `GET /openapi/v1.json`
- Scalar UI: `GET /scalar/v1`

## Configuration

All configuration is handled automatically by Aspire:
- PostgreSQL is orchestrated under the resource name `"postgres"`
- The API references the database as `"vibedb"`
- Connection strings are injected at runtime via service discovery
- OpenTelemetry is collected automatically and visible in the Aspire Dashboard

No environment variables or manual connection strings needed.

## Tests

Run all tests:

```bash
dotnet test
```

Tests are self-contained:

- They force `ASPNETCORE_ENVIRONMENT=Testing`
- The API skips Npgsql wiring in `Testing`
- Tests register an in-memory SQLite `VibeDbContext`

## Migrations (EF Core)

Migrations are owned by `VibeAPI.Data` and managed by `VibeAPI.API`.

In Development, the API automatically applies pending migrations at startup.

### Manual migration commands:

Local tools (if not already restored):

```bash
dotnet tool restore
```

Add a migration:

```bash
dotnet tool run dotnet-ef migrations add <Name> \
  --project src/VibeAPI.Data/VibeAPI.Data.csproj \
  --startup-project src/VibeAPI.API/VibeAPI.API.csproj
```

Apply migrations:

```bash
dotnet tool run dotnet-ef database update \
  --project src/VibeAPI.Data/VibeAPI.Data.csproj \
  --startup-project src/VibeAPI.API/VibeAPI.API.csproj
```

## Endpoints

Root:

- `GET /` (basic health/info)

Todos:

- `GET /todos?offset=&limit=`
- `GET /todos/{id}`
- `POST /todos`
- `PUT /todos/{id}`
- `DELETE /todos/{id}` (idempotent `204`)

Notes:

- `GET /todos` defaults to `offset=0` and `limit=50`; it validates `offset >= 0` and `1 <= limit <= 200`.
- `POST /todos` trims and validates `title` (required, max 200 chars); `completed` defaults to `false`.

## .NET Aspire

This project uses .NET Aspire for local development and distributed application orchestration.

### Projects

- `src/VibeAPI.AppHost`: Orchestrates the entire application (API + PostgreSQL)
- `src/VibeAPI.ServiceDefaults`: Shared service configuration (OpenTelemetry, resilience, health checks)
- `src/VibeAPI.API`: ASP.NET Core Minimal API endpoints
- `src/VibeAPI.Application`: CQRS/MediatR handlers, DTOs, mapping
- `src/VibeAPI.Data`: EF Core DbContext and migrations
- `src/VibeAPI.Domain`: Domain models
- `src/VibeAPI.Client`: NSwag-generated typed API client library
- `src/VibeAPI.CLI`: Console app (System.CommandLine) for ad-hoc API interaction

### Key Features

- **Service Orchestration**: All services managed from a single AppHost
- **Automatic Service Discovery**: Services reference each other by name; connection strings are injected at runtime
- **Health Checks**: `/health` (full) and `/alive` (liveness) endpoints
- **OpenTelemetry Integration**: Automatic collection of metrics, logs, and traces with OTLP export support
- **Resilience**: HTTP clients configured with standard resilience patterns
- **Dashboard**: Built-in observability dashboard for monitoring services
- **Data Persistence**: PostgreSQL data volume ensures state persists between runs

### AppHost Configuration

The AppHost defines:

```csharp
// PostgreSQL service with data volume and PgAdmin
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .AddDatabase("vibedb");

// API service with reference to PostgreSQL
var api = builder.AddProject<Projects.VibeAPI_API>("vibeapi")
    .WithReference(postgres);
```

- Resource name: `postgres`
- Database: `vibedb`
- API: `vibeapi`
- PgAdmin is included for database management (accessible via dashboard)

## CLI

A command-line client is available for ad-hoc interaction with the API.

```bash
# List todos (uses default base URL http://localhost:5153)
dotnet run --project src/VibeAPI.CLI -- todos list

# CRUD operations
dotnet run --project src/VibeAPI.CLI -- todos get <id>
dotnet run --project src/VibeAPI.CLI -- todos create "Buy milk"
dotnet run --project src/VibeAPI.CLI -- todos update <id> "Buy oat milk" --completed
dotnet run --project src/VibeAPI.CLI -- todos delete <id>

# Target a different API instance
dotnet run --project src/VibeAPI.CLI -- --base-url http://other-host:5000 todos list
```

The CLI uses a generated typed client from `VibeAPI.Client`. The client is auto-generated at build time from the API's OpenAPI spec via NSwag.

## Documentation hygiene

When you change endpoints, behavior, or developer workflows, keep these in sync:

- `README.md`
- `AGENTS.md`
- `src/VibeAPI.API/VibeAPI.http`
