using MediatR;

namespace VibeAPI.Application.Todos;

public sealed record DeleteTodoCommand(Guid Id) : IRequest<Unit>;
