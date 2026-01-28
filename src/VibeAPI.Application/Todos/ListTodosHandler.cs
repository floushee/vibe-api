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
