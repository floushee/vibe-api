using MediatR;

namespace VibeAPI.Application.Todos;

public sealed record UpdateTodoCommand(Guid Id, string Title, bool Completed, DateTimeOffset Now) : IRequest<VibeAPI.Todos.Todo?>;
