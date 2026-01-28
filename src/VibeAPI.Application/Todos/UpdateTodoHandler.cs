using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VibeAPI.Application.Common;

namespace VibeAPI.Application.Todos;

public sealed class UpdateTodoHandler(IVibeDbContext db, IMapper mapper)
    : IRequestHandler<UpdateTodoCommand, VibeAPI.Todos.Todo?>
{
    public async Task<VibeAPI.Todos.Todo?> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.Todos
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Title = request.Title;
        entity.Completed = request.Completed;
        entity.UpdatedAt = request.Now.UtcDateTime;

        await db.SaveChangesAsync(cancellationToken);

        return mapper.Map<VibeAPI.Todos.Todo>(entity);
    }
}
