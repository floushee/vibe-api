using MediatR;

namespace VibeAPI.Application.Todos;

public sealed record GetTodoQuery(Guid Id) : IRequest<VibeAPI.Todos.Todo?>;
