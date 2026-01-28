using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.Sqlite;
using VibeAPI.Application.Common;
using VibeAPI.Data;
using VibeAPI.Todos;

namespace VibeAPI.Tests;

internal sealed class VibeApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public VibeApiFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<VibeDbContext>>();
            services.RemoveAll<VibeDbContext>();
            services.RemoveAll<IVibeDbContext>();

            services.AddDbContext<VibeDbContext>(options => options.UseSqlite(_connection));
            services.AddScoped<IVibeDbContext>(sp => sp.GetRequiredService<VibeDbContext>());
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VibeDbContext>();
        db.Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

public class TodosApiTests
{
    [Fact]
    public async Task Post_creates_todo_and_returns_location()
    {
        using var factory = new VibeApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/todos", new CreateTodoRequest("Buy milk", false));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.Equal("Buy milk", created.Title);
        Assert.False(created.Completed);
    }

    [Fact]
    public async Task Get_by_id_returns_404_when_missing()
    {
        using var factory = new VibeApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/todos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_by_id_returns_400_when_id_is_invalid()
    {
        using var factory = new VibeApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/todos/not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_returns_items_with_pagination_shape()
    {
        using var factory = new VibeApiFactory();
        using var client = factory.CreateClient();

        _ = await client.PostAsJsonAsync("/todos", new CreateTodoRequest("A", null));
        _ = await client.PostAsJsonAsync("/todos", new CreateTodoRequest("B", null));

        var response = await client.GetAsync("/todos?offset=0&limit=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<ListTodosResponse>();
        Assert.NotNull(list);
        Assert.Equal(0, list!.Offset);
        Assert.Equal(50, list.Limit);
        Assert.True(list.Items.Count >= 2);
    }

    [Fact]
    public async Task Put_updates_todo()
    {
        using var factory = new VibeApiFactory();
        using var client = factory.CreateClient();

        var createdResponse = await client.PostAsJsonAsync("/todos", new CreateTodoRequest("Initial", null));
        var created = await createdResponse.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(created);

        var updateResponse = await client.PutAsJsonAsync($"/todos/{created!.Id}", new UpdateTodoRequest("Updated", true));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<Todo>();
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated!.Title);
        Assert.True(updated.Completed);
    }

    [Fact]
    public async Task Delete_is_idempotent_and_returns_204()
    {
        using var factory = new VibeApiFactory();
        using var client = factory.CreateClient();

        var missingId = Guid.NewGuid();
        var response = await client.DeleteAsync($"/todos/{missingId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Post_validates_title()
    {
        using var factory = new VibeApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/todos", new CreateTodoRequest(" ", null));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_validates_limit_range()
    {
        using var factory = new VibeApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/todos?offset=0&limit=500");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
