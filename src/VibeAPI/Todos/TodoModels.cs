namespace VibeAPI.Todos;

public sealed record Todo(
    Guid Id,
    string Title,
    bool Completed,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

internal sealed class TodoEntity
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    public required bool Completed { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; set; }

    public Todo ToDto() => new(Id, Title, Completed, CreatedAt, UpdatedAt);
}
