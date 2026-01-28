# 003 - OpenAPI UI via Scalar

**Status:** Draft

## Summary

Add a browser-based OpenAPI UI using Scalar so developers can explore and try out the API without leaving the local dev environment.

This keeps the existing OpenAPI JSON generation (`AddOpenApi` + `MapOpenApi`) and adds a UI layer on top.

## Goals

- Provide an interactive docs UI in Development.
- Keep the existing OpenAPI JSON endpoint available.
- Avoid impacting tests (`Testing` environment) and production behavior.

## Non-goals

- Changing the public HTTP API behavior of the `/todos` endpoints.
- Adding authentication.
- Customizing the OpenAPI document beyond basics (can be added later).

## Proposed behavior

- In `Development`, the API serves:
  - OpenAPI JSON (existing)
  - Scalar UI (new)
- In `Testing`, Scalar is not required.

## Implementation notes

- Add Scalar ASP.NET Core package to `src/VibeAPI.API`.
- Map Scalar UI in `src/VibeAPI.API/Program.cs` in the Development-only block next to `app.MapOpenApi()`.

## Acceptance criteria

- [ ] Running `dotnet run --project src/VibeAPI.API` in Development exposes a Scalar UI endpoint.
- [ ] Existing endpoints continue to work.
- [ ] `dotnet test` passes.
- [ ] `src/VibeAPI.API/VibeAPI.http` includes a request/example to open or reference the docs endpoint.
