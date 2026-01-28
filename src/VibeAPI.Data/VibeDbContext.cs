using Microsoft.EntityFrameworkCore;
using VibeAPI.Application.Common;
using VibeAPI.Entities;

namespace VibeAPI.Data;

public sealed class VibeDbContext(DbContextOptions<VibeDbContext> options)
	: DbContext(options), IVibeDbContext
{
	public DbSet<Todo> Todos => Set<Todo>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		var todo = modelBuilder.Entity<Todo>();

		todo.ToTable("todos");
		todo.HasKey(t => t.Id);

		todo.Property(t => t.Title)
			.IsRequired()
			.HasMaxLength(200);

		todo.Property(t => t.Completed).IsRequired();
		todo.Property(t => t.CreatedAt).IsRequired();
		todo.Property(t => t.UpdatedAt).IsRequired();

		todo.HasIndex(t => t.CreatedAt);
	}
}
