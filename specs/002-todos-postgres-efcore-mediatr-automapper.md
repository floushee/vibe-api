# Todos persistence via EF Core + PostgreSQL (MediatR + AutoMapper)

## Goal
Replace the in-memory dictionary-backed Todos storage with a database-backed implementation using:

- EF Core for persistence
- PostgreSQL (Npgsql) as the primary provider
- MediatR for application flow (queries/commands/handlers)
- AutoMapper for entity ↔ DTO mapping

The public HTTP API stays compatible with the existing Todos endpoints.

## High-level architecture
Projects are split by responsibility:

- `src/VibeAPI.API` (API)
  - Minimal API endpoints
  - Composition root / DI wiring
- `src/VibeAPI.Application` (Application)
  - DTO contracts (kept under `namespace VibeAPI.Todos` to preserve API types)
  - MediatR requests + handlers
  - AutoMapper profiles
  - `IVibeDbContext` abstraction
- `src/VibeAPI.Data` (Data)
  - EF Core `DbContext` implementation (`VibeDbContext`)
  - Provider wiring (`AddVibeData`)
  - Migrations
- `src/VibeAPI.Entities` (Entities)
  - Persistence entities (e.g. `Todo`)

## Data model
`Todo` is persisted with:

- `Id` (Guid)
- `Title` (string, required, max length 200)
- `Completed` (bool)
- `CreatedAt` (UTC `DateTime`)
- `UpdatedAt` (UTC `DateTime`)

The HTTP DTOs expose timestamps as `DateTimeOffset`.

## Request flow (MediatR)
The API endpoints in `VibeAPI` call MediatR using `ISender`.

Commands / queries live in `src/VibeAPI.Application/Todos`:

- List: `ListTodosQuery(offset, limit)` → `ListTodosResponse`
- Get: `GetTodoQuery(id)` → `Todo?`
- Create: `CreateTodoCommand(title, completed, now)` → `Todo`
- Update: `UpdateTodoCommand(id, title, completed, now)` → `Todo?`
- Delete: `DeleteTodoCommand(id)` → `Unit`

Handlers perform EF Core operations via `IVibeDbContext`.

## AutoMapper
Mapping is defined in `TodoMappingProfile`.

- Entity timestamps are stored as UTC `DateTime`
- DTO timestamps are `DateTimeOffset`

The mapping uses constructor-parameter mapping for record DTOs.

## Configuration
Connection string is read from `ConnectionStrings:VibeDb`.

Default values are in:

- `src/VibeAPI.API/appsettings.json`
- `src/VibeAPI.API/appsettings.Development.json`

For local development, you can override via environment variables (recommended):

- `ConnectionStrings__VibeDb`

## Local PostgreSQL via Docker
This repo includes a `docker-compose.yml` to run PostgreSQL locally.

- Start Postgres: `docker compose up -d`
- Stop Postgres: `docker compose down`

It maps the container's `5432` to host port `5433`.

It initializes one database on first run:

- `vibeapidb`

With `ASPNETCORE_ENVIRONMENT=Development`, the API applies migrations at startup.

## Migrations
Migrations are owned by `VibeAPI.Data`.

Typical workflow:

- Add migration (from repo root):
  - `dotnet tool run dotnet-ef migrations add <Name> --project src/VibeAPI.Data/VibeAPI.Data.csproj --startup-project src/VibeAPI.API/VibeAPI.API.csproj`
- Apply migration:
  - `dotnet tool run dotnet-ef database update --project src/VibeAPI.Data/VibeAPI.Data.csproj --startup-project src/VibeAPI.API/VibeAPI.API.csproj`

In Development, the API applies migrations at startup.

## Testing strategy
Integration tests use an in-memory SQLite database to avoid requiring PostgreSQL.

Key points:

- Test host sets environment to `Testing`
- `Program` skips Npgsql registration in `Testing`
- Tests register SQLite `VibeDbContext` and run `EnsureCreated()`

This keeps tests fast and fully self-contained.

## API surface (unchanged)
Endpoints remain:

- `GET /todos?offset=&limit=`
- `GET /todos/{id}`
- `POST /todos`
- `PUT /todos/{id}`
- `DELETE /todos/{id}`
