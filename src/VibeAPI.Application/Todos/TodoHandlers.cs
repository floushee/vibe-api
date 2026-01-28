using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VibeAPI.Application.Common;

namespace VibeAPI.Application.Todos;

public sealed class ListTodosHandler(IVibeDbContext db, IMapper mapper)
    : IRequestHandler<ListTodosQuery, VibeAPI.Todos.ListTodosResponse>
{
    public async Task<VibeAPI.Todos.ListTodosResponse> Handle(ListTodosQuery request, CancellationToken cancellationToken)
    {
        var items = await db.Todos
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Skip(request.Offset)
            .Take(request.Limit)
            .ProjectTo<VibeAPI.Todos.Todo>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new(items, request.Offset, request.Limit);
    }
}

public sealed class GetTodoHandler(IVibeDbContext db, IMapper mapper)
    : IRequestHandler<GetTodoQuery, VibeAPI.Todos.Todo?>
{
    public async Task<VibeAPI.Todos.Todo?> Handle(GetTodoQuery request, CancellationToken cancellationToken)
    {
        return await db.Todos
            .AsNoTracking()
            .Where(t => t.Id == request.Id)
            .ProjectTo<VibeAPI.Todos.Todo>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}

public sealed class CreateTodoHandler(IVibeDbContext db, IMapper mapper)
    : IRequestHandler<CreateTodoCommand, VibeAPI.Todos.Todo>
{
    public async Task<VibeAPI.Todos.Todo> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var entity = new Entities.Todo
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

public sealed class DeleteTodoHandler(IVibeDbContext db)
    : IRequestHandler<DeleteTodoCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        _ = await db.Todos
            .Where(t => t.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        return Unit.Value;
    }
}
