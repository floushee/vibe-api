# Agent instructions

These rules apply to *every* change in this repository.

## Always do

- Add tests using xUnit for new/changed behavior (prefer API-level integration tests in `tests/VibeAPI.Tests`).
- Add or update request examples in `src/VibeAPI.API/VibeAPI.http` for any new/changed endpoints.
- Keep the public HTTP API compatible with the specs (unless the spec is being updated in the same change).
- Ensure file names match the primary type they contain (no placeholder `Class1.cs`).

## Architecture & dependency direction

- `src/VibeAPI.API`: Minimal API endpoints + composition root (DI).
- `src/VibeAPI.Application`: MediatR requests/handlers, DTOs, AutoMapper profiles, and `IVibeDbContext` abstraction.
- `src/VibeAPI.Data`: EF Core `VibeDbContext`, provider registration, migrations.
- `src/VibeAPI.Entities`: persistence entities.

Dependency flow should stay one-way:

`API`  `Application`  (`Data`, `Entities`)

Notes:

- Endpoints should call MediatR via `ISender` and keep logic thin.
- Validation belongs close to the HTTP boundary (see `TodoEndpoints`).
- Mapping belongs in AutoMapper profiles in `VibeAPI.Application`.

## Running & tooling

- SDK is pinned via `global.json` (install the specified .NET SDK if you hit build errors).
- Restore local tools before using EF commands: `dotnet tool restore`.

## Database & migrations

- Local Postgres is available via `docker-compose.yml` and listens on host port `5433`.
- In `Development`, the API applies EF migrations at startup.
- Migrations are owned by `VibeAPI.Data`.

Common commands (run from repo root):

- Start DB: `docker compose up -d`
- Stop DB: `docker compose down`
- Add migration:
	`dotnet tool run dotnet-ef migrations add <Name> --project src/VibeAPI.Data/VibeAPI.Data.csproj --startup-project src/VibeAPI.API/VibeAPI.API.csproj`
- Apply migration:
	`dotnet tool run dotnet-ef database update --project src/VibeAPI.Data/VibeAPI.Data.csproj --startup-project src/VibeAPI.API/VibeAPI.API.csproj`

## Testing conventions

- Prefer end-to-end-ish tests using `WebApplicationFactory<Program>` (see `TodosApiTests`).
- Tests use SQLite in-memory and set `ASPNETCORE_ENVIRONMENT=Testing`.
- `Program` must keep respecting `Testing` by not registering Npgsql when in that environment.

## API conventions

- Routes are grouped under `/todos` and use standard HTTP status codes.
- For invalid UUIDs, return `400` (current behavior uses `Results.Problem(...)`).
- `DELETE /todos/{id}` is idempotent and returns `204` even if missing.
