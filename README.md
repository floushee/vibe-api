# VibeAPI

A small, spec-driven ASP.NET Core Minimal API sample.

- Minimal API endpoints in `src/VibeAPI.API`
- CQRS-ish flow via MediatR in `src/VibeAPI.Application`
- Persistence via EF Core in `src/VibeAPI.Data` + domain model in `src/VibeAPI.Domain`
- xUnit integration tests in `tests/VibeAPI.Tests` (SQLite in-memory)

## Prerequisites

- .NET SDK version from `global.json`
- Docker (optional, for local PostgreSQL)

## Quick start (API + local Postgres)

1) Start PostgreSQL:

```bash
docker compose up -d
```

2) Run the API (Development):

```bash
dotnet run --project src/VibeAPI.API --launch-profile http
```

To run with HTTPS locally:

```bash
dotnet run --project src/VibeAPI.API --launch-profile https
```

The default launch profile listens on:

- http://localhost:5153

Try it via the request file:

- `src/VibeAPI.API/VibeAPI.http`

OpenAPI / docs (Development):

- OpenAPI JSON: `GET /openapi/v1.json`
- Scalar UI: `GET /scalar/v1`

## Configuration

Connection string:

- `ConnectionStrings:VibeDb` (see `src/VibeAPI.API/appsettings*.json`)

Recommended override via environment variables:

- `ConnectionStrings__VibeDb`

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

Local tools:

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

## Specs

Specs live in `specs/`:

- `specs/001-todos-api.md`
- `specs/002-todos-postgres-efcore-mediatr-automapper.md`
- `specs/003-openapi-scalar-ui.md`

## Documentation hygiene

When you change endpoints, behavior, or developer workflows, keep these in sync:

- `README.md`
- `AGENTS.md`
- `src/VibeAPI.API/VibeAPI.http`
- Relevant `specs/*`
