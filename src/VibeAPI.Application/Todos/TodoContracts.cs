namespace VibeAPI.Todos;

public sealed record Todo(
    Guid Id,
    string Title,
    bool Completed,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateTodoRequest(string Title, bool? Completed);

public sealed record UpdateTodoRequest(string Title, bool Completed);

public sealed record ListTodosResponse(IReadOnlyList<Todo> Items, int Offset, int Limit);
