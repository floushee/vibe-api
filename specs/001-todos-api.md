# 001 - Todos API

**Status:** Draft

## Summary

Add a simple JSON REST API for managing a todo list: list todos, get by id, create, update, and delete.

## Goals

- Provide CRUD endpoints for todos.
- Keep the API minimal and consistent with ASP.NET Minimal APIs.
- Define clear request/response shapes, validation rules, and error codes.

## Non-goals

- Authentication/authorization.
- Multi-user scoping (e.g., “my todos”).
- Persistence requirements (in-memory vs DB) — implementation detail for the next step.
- Advanced query/search/filter beyond basic pagination.

## User stories

- As a user, I want to list todos, so I can see what I need to do.
- As a user, I want to create a todo, so I can track new tasks.
- As a user, I want to mark a todo as completed, so I can track progress.
- As a user, I want to update a todo’s title, so I can correct it.
- As a user, I want to delete a todo, so I can remove it.

## API surface

### Data model (wire format)

**Todo**

```json
{
  "id": "b5b0a4f7-9e13-44c6-9e7e-4d3a2f4c1e3d",
  "title": "Buy milk",
  "completed": false,
  "createdAt": "2026-01-28T12:34:56.789Z",
  "updatedAt": "2026-01-28T12:34:56.789Z"
}
```

- `id`: UUID string.
- `title`: required, 1..200 chars, trimmed.
- `completed`: boolean.
- `createdAt`/`updatedAt`: RFC3339/ISO-8601 timestamps in UTC.

### Endpoints

| Method | Route | Auth | Request | Response | Notes |
|---|---|---|---|---|---|
| GET | /todos | none | Query: `offset` (>=0, default 0), `limit` (1..200, default 50) | 200 `{ items: Todo[], offset: number, limit: number }` | Ordered by `createdAt` descending by default |
| GET | /todos/{id} | none | Path: `id` (UUID) | 200 `Todo` | 404 if not found |
| POST | /todos | none | JSON: `{ title: string, completed?: boolean }` | 201 `Todo` | Sets `Location: /todos/{id}` |
| PUT | /todos/{id} | none | JSON: `{ title: string, completed: boolean }` | 200 `Todo` | Full update; 404 if not found |
| DELETE | /todos/{id} | none | — | 204 | Idempotent delete is OK (see Errors) |

#### Request/response details

**GET /todos**

Response body:

```json
{
  "items": [ /* Todo[] */ ],
  "offset": 0,
  "limit": 50
}
```

**POST /todos**

Request:

```json
{
  "title": "Buy milk",
  "completed": false
}
```

- If `completed` is omitted, it defaults to `false`.

**PUT /todos/{id}**

Request:

```json
{
  "title": "Buy oat milk",
  "completed": true
}
```

### Errors

- `400 Bad Request`
  - Invalid UUID in `{id}`.
  - Validation failures (`title` empty/too long; `limit` out of range; etc.).
- `404 Not Found`
  - Todo does not exist for `GET /todos/{id}` and `PUT /todos/{id}`.
  - For `DELETE /todos/{id}`:
    - Preferred: return `204 No Content` even if the todo didn’t exist (idempotent).
    - Acceptable alternative: return `404 Not Found` if you want strictness.
  - Implementation must pick one behavior; see Acceptance criteria.
- `415 Unsupported Media Type` when JSON body is required but `Content-Type` is not `application/json`.

Error response shape (for 4xx/5xx where a body is returned):

```json
{
  "error": {
    "code": "validation_error",
    "message": "Title is required",
    "details": {
      "field": "title"
    }
  }
}
```

(Exact structure may reuse ASP.NET default `ProblemDetails`; if so, document the mapping during implementation.)

## Security & privacy

- AuthN/AuthZ: none.
- Data classification: non-sensitive user-entered text.
- Abuse cases: overly long titles / request flooding; mitigate with validation and reasonable defaults.

## Observability

- Logs: log route, status code, and validation failures (without sensitive data).
- Metrics (optional): request count + latency per endpoint.

## Acceptance criteria

- [ ] `GET /todos` returns `200` with `{ items, offset, limit }`.
- [ ] `GET /todos/{id}` returns `200` for existing todo and `404` for missing.
- [ ] `POST /todos` validates `title`, returns `201` with created `Todo` and `Location` header.
- [ ] `PUT /todos/{id}` validates body, returns updated `Todo` or `404` if missing.
- [ ] `DELETE /todos/{id}` behavior is chosen and documented (idempotent `204` preferred).
- [ ] All endpoints produce/consume JSON as specified.

## Testing plan

- Unit tests (xUnit): validation rules, update semantics, delete behavior.
- Integration tests (xUnit + `WebApplicationFactory`): happy path and error cases per endpoint.
- Manual checks: use the requests in `src/VibeAPI/VibeAPI.http`.

## Rollout

- Feature flag: no.
- Backward compatibility: additive.

## Open questions

- Should `DELETE /todos/{id}` be idempotent (`204` even when missing) or strict (`404` when missing)?
- Should list ordering be configurable or fixed?

## Out of scope / future work

- PATCH semantics, partial updates.
- Filtering/search (e.g., `completed=true`).
- Auth + per-user todo lists.
