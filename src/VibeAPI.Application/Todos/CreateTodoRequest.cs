namespace VibeAPI.Todos;

public sealed record CreateTodoRequest(string Title, bool? Completed);
