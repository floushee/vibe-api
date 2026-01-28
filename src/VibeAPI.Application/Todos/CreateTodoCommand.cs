using MediatR;

namespace VibeAPI.Application.Todos;

public sealed record CreateTodoCommand(string Title, bool Completed, DateTimeOffset Now) : IRequest<VibeAPI.Todos.Todo>;
