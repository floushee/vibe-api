# Agent instructions

These rules apply to *every* change in this repository.

## Always do

- Add tests using xUnit for new/changed behavior (prefer API-level integration tests in `tests/VibeAPI.Tests`).
- Add or update request examples in `src/VibeAPI.API/VibeAPI.http` for any new/changed endpoints.
- Keep repository docs up to date when code changes affect them (at minimum: this file, `README.md`, and `src/VibeAPI.API/VibeAPI.http`).
- Keep the public HTTP API compatible with the specs (unless the spec is being updated in the same change).
- Ensure file names match the primary type they contain (no placeholder `Class1.cs`).
- Keep one top-level `class` or `record` per file.

## Architecture & dependency direction

- `src/VibeAPI.AppHost`: .NET Aspire orchestration host (defines and runs all services).
- `src/VibeAPI.ServiceDefaults`: Shared service configuration (OpenTelemetry, resilience, health checks).
- `src/VibeAPI.API`: Minimal API endpoints, composes services via dependency injection.
- `src/VibeAPI.Application`: MediatR requests/handlers, DTOs, AutoMapper profiles, and `IVibeDbContext` abstraction.
- `src/VibeAPI.Data`: EF Core `VibeDbContext`, provider registration, migrations.
- `src/VibeAPI.Domain`: Domain models (persisted via EF Core).

Dependency flow should stay one-way:

`AppHost` -> `API` -> `Application` -> (`Data`, `Domain`)

`ServiceDefaults` is referenced by all service projects (read-only configuration).

Notes:

- AppHost orchestrates PostgreSQL and the API; both are added to the distributed application builder.
- The API calls `AddServiceDefaults()` for cross-cutting concerns (observability, resilience, health).
- Endpoints should call MediatR via `ISender` and keep logic thin.
- Validation belongs close to the HTTP boundary (see `TodoEndpoints`).
- Mapping belongs in AutoMapper profiles in `VibeAPI.Application`.
- Connection strings are injected by Aspire when running via AppHost; no hardcoding needed.

## Running & tooling

### Prerequisites

- .NET 10 SDK (version pinned in `global.json`).
- Docker Desktop (Aspire uses it to run PostgreSQL).
- HTTPS dev certificate trusted.

With .NET 10, Aspire is included as NuGet packages — no workload install needed.

### Running the app

```bash
dotnet run --project src/VibeAPI.AppHost
```

On Linux, use the `http` launch profile to avoid HTTPS certificate issues with containerized services:

```bash
dotnet run --project src/VibeAPI.AppHost --launch-profile http
```

This starts PostgreSQL, the API, PgAdmin, and the Aspire Dashboard. No environment variables or connection strings to configure — Aspire handles everything via service discovery.

## Database & migrations

- Migrations are owned by `VibeAPI.Data` and managed by `VibeAPI.API`.
- In `Development`, the API automatically applies pending migrations at startup.

### With Aspire (AppHost)

When running via `dotnet run --project src/VibeAPI.AppHost`:

- PostgreSQL is orchestrated by Aspire with a data volume for persistence.
- Database name: `vibedb` (created automatically).
- Migrations are applied automatically when the API starts.
- PgAdmin is available via the Aspire Dashboard for manual database inspection.

### Manual migration commands

Add migration:

```bash
dotnet tool run dotnet-ef migrations add <Name> \
  --project src/VibeAPI.Data/VibeAPI.Data.csproj \
  --startup-project src/VibeAPI.API/VibeAPI.API.csproj
```

Apply migration:

```bash
dotnet tool run dotnet-ef database update \
  --project src/VibeAPI.Data/VibeAPI.Data.csproj \
  --startup-project src/VibeAPI.API/VibeAPI.API.csproj
```

## Testing conventions

- Prefer end-to-end-ish tests using `WebApplicationFactory<Program>` (see `TodosApiTests`).
- Tests use SQLite in-memory and set `ASPNETCORE_ENVIRONMENT=Testing`.
- The API must respect `Testing` environment by not registering Npgsql (PostgreSQL) in that mode.
- Keep `public partial class Program;` in the API project so tests can reference the entrypoint.
- Tests run without Aspire orchestration; the TestWebApplicationFactory sets up a minimal, in-memory environment.
- Run tests with: `dotnet test`

## API conventions

- Routes are grouped under `/todos` and use standard HTTP status codes.
- For invalid UUIDs, return `400` (current behavior uses `Results.Problem(...)`).
- `DELETE /todos/{id}` is idempotent and returns `204` even if missing.
- Validation rules (current behavior):
	- `title` is trimmed, required, and must be `<= 200` characters.
	- `GET /todos` defaults to `offset=0&limit=50`, enforces `offset >= 0` and `1 <= limit <= 200`.
