using MediatR;

namespace VibeAPI.Application.Todos;

public sealed record ListTodosQuery(int Offset, int Limit) : IRequest<VibeAPI.Todos.ListTodosResponse>;

public sealed record GetTodoQuery(Guid Id) : IRequest<VibeAPI.Todos.Todo?>;

public sealed record CreateTodoCommand(string Title, bool Completed, DateTimeOffset Now) : IRequest<VibeAPI.Todos.Todo>;

public sealed record UpdateTodoCommand(Guid Id, string Title, bool Completed, DateTimeOffset Now) : IRequest<VibeAPI.Todos.Todo?>;

public sealed record DeleteTodoCommand(Guid Id) : IRequest<Unit>;
