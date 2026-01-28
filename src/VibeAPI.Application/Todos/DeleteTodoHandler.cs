using MediatR;
using Microsoft.EntityFrameworkCore;
using VibeAPI.Application.Common;

namespace VibeAPI.Application.Todos;

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
