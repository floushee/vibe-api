using AutoMapper;
using MediatR;
using VibeAPI.Application.Common;

namespace VibeAPI.Application.Todos;

public sealed class CreateTodoHandler(IVibeDbContext db, IMapper mapper)
    : IRequestHandler<CreateTodoCommand, VibeAPI.Todos.Todo>
{
    public async Task<VibeAPI.Todos.Todo> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var entity = new VibeAPI.Domain.Todo
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Completed = request.Completed,
            CreatedAt = request.Now.UtcDateTime,
            UpdatedAt = request.Now.UtcDateTime,
        };

        db.Todos.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return mapper.Map<VibeAPI.Todos.Todo>(entity);
    }
}
