using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using VibeAPI.Application;
using VibeAPI.Application.Common;
using VibeAPI.Data;
using VibeAPI.Todos;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.AddNpgsqlDbContext<VibeDbContext>("vibedb");
    builder.Services.AddScoped<IVibeDbContext>(sp => sp.GetRequiredService<VibeDbContext>());
}
builder.Services.AddMediatR(typeof(ApplicationAssemblyMarker).Assembly);
builder.Services.AddAutoMapper(typeof(ApplicationAssemblyMarker).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

var aspnetcoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? string.Empty;
var hasHttpsUrl = aspnetcoreUrls.Contains("https://", StringComparison.OrdinalIgnoreCase);
if (!app.Environment.IsDevelopment() || hasHttpsUrl)
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<VibeDbContext>();
    await db.Database.MigrateAsync();
}

app.MapGet("/", () => Results.Ok(new { name = "VibeAPI", status = "ok" }));

app.MapTodos();

app.MapDefaultEndpoints();

app.Run();

public partial class Program;
