using Microsoft.AspNetCore.Http.HttpResults;

namespace VibeAPI.Todos;

public static class TodoEndpoints
{
    public static IEndpointRouteBuilder MapTodos(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/todos");

        group.MapGet("/", (TodoStore store, int? offset, int? limit) =>
            {
                var (isValid, validation) = ValidateList(offset, limit);
                if (!isValid)
                {
                    return Results.ValidationProblem(validation);
                }

                var resolvedOffset = offset ?? 0;
                var resolvedLimit = limit ?? 50;

                var items = store.List(resolvedOffset, resolvedLimit);
                return Results.Ok(new ListTodosResponse(items, resolvedOffset, resolvedLimit));
            })
            .WithName("ListTodos");

        group.MapGet("/{id}", (TodoStore store, string id) =>
            {
                if (!TryParseId(id, out var guid, out var badRequest))
                {
                    return badRequest;
                }

                return store.TryGet(guid, out var todo)
                    ? Results.Ok(todo)
                    : Results.NotFound();
            })
            .WithName("GetTodo");

        group.MapPost("/", (TodoStore store, CreateTodoRequest request) =>
            {
                var (isValid, validation, title) = ValidateTitle(request.Title);
                if (!isValid)
                {
                    return Results.ValidationProblem(validation);
                }

                var todo = store.Create(title, request.Completed ?? false, DateTimeOffset.UtcNow);
                return Results.Created($"/todos/{todo.Id}", todo);
            })
            .WithName("CreateTodo");

        group.MapPut("/{id}", (TodoStore store, string id, UpdateTodoRequest request) =>
            {
                if (!TryParseId(id, out var guid, out var badRequest))
                {
                    return badRequest;
                }

                var (isValid, validation, title) = ValidateTitle(request.Title);
                if (!isValid)
                {
                    return Results.ValidationProblem(validation);
                }

                return store.TryUpdate(guid, title, request.Completed, DateTimeOffset.UtcNow, out var updated)
                    ? Results.Ok(updated)
                    : Results.NotFound();
            })
            .WithName("UpdateTodo");

        group.MapDelete("/{id}", (TodoStore store, string id) =>
            {
                if (!TryParseId(id, out var guid, out var badRequest))
                {
                    return badRequest;
                }

                store.Delete(guid);
                return Results.NoContent();
            })
            .WithName("DeleteTodo");

        return app;
    }

    private static bool TryParseId(string id, out Guid guid, out IResult badRequest)
    {
        if (Guid.TryParse(id, out guid))
        {
            badRequest = default!;
            return true;
        }

        badRequest = Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid id",
            detail: "The provided id is not a valid UUID.");

        return false;
    }

    private static (bool isValid, Dictionary<string, string[]> validation, string title) ValidateTitle(string? title)
    {
        title = (title ?? string.Empty).Trim();

        var errors = new Dictionary<string, string[]>();

        if (title.Length is < 1)
        {
            errors["title"] = ["Title is required."];
        }
        else if (title.Length > 200)
        {
            errors["title"] = ["Title must be at most 200 characters."];
        }

        return (errors.Count == 0, errors, title);
    }

    private static (bool isValid, Dictionary<string, string[]> validation) ValidateList(int? offset, int? limit)
    {
        var errors = new Dictionary<string, string[]>();

        if (offset is < 0)
        {
            errors["offset"] = ["Offset must be >= 0."];
        }

        if (limit is < 1 or > 200)
        {
            errors["limit"] = ["Limit must be between 1 and 200."];
        }

        return (errors.Count == 0, errors);
    }
}
