using Microsoft.EntityFrameworkCore;
using VibeAPI.Domain;

namespace VibeAPI.Application.Common;

public interface IVibeDbContext
{
    DbSet<Todo> Todos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
