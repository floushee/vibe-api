using MediatR;

namespace VibeAPI.Application.Todos;

public sealed record ListTodosQuery(int Offset, int Limit) : IRequest<VibeAPI.Todos.ListTodosResponse>;
