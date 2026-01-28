namespace VibeAPI.Entities;

public sealed class Todo
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public bool Completed { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
}
