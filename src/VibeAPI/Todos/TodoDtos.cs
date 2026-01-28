namespace VibeAPI.Todos;

public sealed record CreateTodoRequest(string Title, bool? Completed);

public sealed record UpdateTodoRequest(string Title, bool Completed);

public sealed record ListTodosResponse(IReadOnlyList<Todo> Items, int Offset, int Limit);
