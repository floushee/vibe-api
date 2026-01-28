# VibeAPI

A small, spec-driven ASP.NET Core Minimal API sample.

- Minimal API endpoints in `src/VibeAPI.API`
- CQRS-ish flow via MediatR in `src/VibeAPI.Application`
- Persistence via EF Core in `src/VibeAPI.Data` + entities in `src/VibeAPI.Entities`
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
dotnet run --project src/VibeAPI.API
```

The default launch profile listens on:

- http://localhost:5153

Try it via the request file:

- `src/VibeAPI.API/VibeAPI.http`

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

Todos:

- `GET /todos?offset=&limit=`
- `GET /todos/{id}`
- `POST /todos`
- `PUT /todos/{id}`
- `DELETE /todos/{id}` (idempotent `204`)

## Specs

Specs live in `specs/`:

- `specs/001-todos-api.md`
- `specs/002-todos-postgres-efcore-mediatr-automapper.md`
