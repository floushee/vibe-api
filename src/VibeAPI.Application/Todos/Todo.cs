namespace VibeAPI.Todos;

public sealed record Todo(
    Guid Id,
    string Title,
    bool Completed,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
