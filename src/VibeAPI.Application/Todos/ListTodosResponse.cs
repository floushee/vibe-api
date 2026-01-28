namespace VibeAPI.Todos;

public sealed record ListTodosResponse(IReadOnlyList<Todo> Items, int Offset, int Limit);
