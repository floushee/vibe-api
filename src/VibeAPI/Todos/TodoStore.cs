using System.Collections.Concurrent;

namespace VibeAPI.Todos;

public sealed class TodoStore
{
    private readonly ConcurrentDictionary<Guid, TodoEntity> _todos = new();

    public IReadOnlyList<Todo> List(int offset, int limit)
    {
        return _todos.Values
            .OrderByDescending(t => t.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .Select(t => t.ToDto())
            .ToArray();
    }

    public bool TryGet(Guid id, out Todo todo)
    {
        if (_todos.TryGetValue(id, out var entity))
        {
            todo = entity.ToDto();
            return true;
        }

        todo = default!;
        return false;
    }

    public Todo Create(string title, bool completed, DateTimeOffset now)
    {
        var entity = new TodoEntity
        {
            Id = Guid.NewGuid(),
            Title = title,
            Completed = completed,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _todos[entity.Id] = entity;
        return entity.ToDto();
    }

    public bool TryUpdate(Guid id, string title, bool completed, DateTimeOffset now, out Todo updated)
    {
        updated = default!;

        if (!_todos.TryGetValue(id, out var entity))
        {
            return false;
        }

        entity.Title = title;
        entity.Completed = completed;
        entity.UpdatedAt = now;

        updated = entity.ToDto();
        return true;
    }

    public bool Delete(Guid id) => _todos.TryRemove(id, out _);
}
