using VibeAPI.Todos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<VibeAPI.Todos.TodoStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var aspnetcoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? string.Empty;
var hasHttpsUrl = aspnetcoreUrls.Contains("https://", StringComparison.OrdinalIgnoreCase);
if (!app.Environment.IsDevelopment() || hasHttpsUrl)
{
    app.UseHttpsRedirection();
}

app.MapGet("/", () => Results.Ok(new { name = "VibeAPI", status = "ok" }));

app.MapTodos();

app.Run();

public partial class Program;
