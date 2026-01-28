using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VibeAPI.Application.Common;

namespace VibeAPI.Application.Todos;

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
