using MediatR;
using VibeAPI.Application.Todos;

namespace VibeAPI.Todos;

public static class TodoEndpoints
{
    public static IEndpointRouteBuilder MapTodos(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/todos");

        group.MapGet("/", async (ISender sender, int? offset, int? limit, CancellationToken cancellationToken) =>
            {
                var (isValid, validation) = ValidateList(offset, limit);
                if (!isValid)
                {
                    return Results.ValidationProblem(validation);
                }

                var resolvedOffset = offset ?? 0;
                var resolvedLimit = limit ?? 50;

                var response = await sender.Send(new ListTodosQuery(resolvedOffset, resolvedLimit), cancellationToken);
                return Results.Ok(response);
            })
            .WithName("ListTodos")
            .Produces<ListTodosResponse>();

        group.MapGet("/{id}", async (ISender sender, string id, CancellationToken cancellationToken) =>
            {
                if (!TryParseId(id, out var guid, out var badRequest))
                {
                    return badRequest;
                }

                var todo = await sender.Send(new GetTodoQuery(guid), cancellationToken);
                return todo is not null ? Results.Ok(todo) : Results.NotFound();
            })
            .WithName("GetTodo")
            .Produces<Todo>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (ISender sender, CreateTodoRequest request, CancellationToken cancellationToken) =>
            {
                var (isValid, validation, title) = ValidateTitle(request.Title);
                if (!isValid)
                {
                    return Results.ValidationProblem(validation);
                }

                var todo = await sender.Send(
                    new CreateTodoCommand(title, request.Completed ?? false, DateTimeOffset.UtcNow),
                    cancellationToken);

                return Results.Created($"/todos/{todo.Id}", todo);
            })
            .WithName("CreateTodo")
            .Produces<Todo>(StatusCodes.Status201Created);

        group.MapPut("/{id}", async (ISender sender, string id, UpdateTodoRequest request, CancellationToken cancellationToken) =>
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

                var updated = await sender.Send(
                    new UpdateTodoCommand(guid, title, request.Completed, DateTimeOffset.UtcNow),
                    cancellationToken);

                return updated is not null ? Results.Ok(updated) : Results.NotFound();
            })
            .WithName("UpdateTodo")
            .Produces<Todo>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id}", async (ISender sender, string id, CancellationToken cancellationToken) =>
            {
                if (!TryParseId(id, out var guid, out var badRequest))
                {
                    return badRequest;
                }

                await sender.Send(new DeleteTodoCommand(guid), cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteTodo")
            .Produces(StatusCodes.Status204NoContent);

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
