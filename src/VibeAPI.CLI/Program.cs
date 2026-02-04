using System.CommandLine;
using Microsoft.Extensions.Configuration;
using VibeAPI.Client;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .Build();

var defaultBaseUrl = config["ApiBaseUrl"] ?? "http://localhost:5153";

var baseUrlOption = new Option<string>("--base-url")
{
    Description = "Base URL of the VibeAPI server",
    DefaultValueFactory = _ => defaultBaseUrl
};

var rootCommand = new RootCommand("VibeAPI CLI — manage todos via the VibeAPI");
rootCommand.Options.Add(baseUrlOption);

var todosCommand = new Command("todos", "Manage todo items");
rootCommand.Subcommands.Add(todosCommand);

// --- todos list ---
var listOffsetOption = new Option<int>("--offset")
{
    Description = "Number of items to skip",
    DefaultValueFactory = _ => 0
};
var listLimitOption = new Option<int>("--limit")
{
    Description = "Maximum number of items to return",
    DefaultValueFactory = _ => 50
};

var listCommand = new Command("list", "List todos");
listCommand.Options.Add(listOffsetOption);
listCommand.Options.Add(listLimitOption);
todosCommand.Subcommands.Add(listCommand);

listCommand.SetAction(async (parseResult, ct) =>
{
    var client = CreateClient(parseResult.GetValue(baseUrlOption)!);
    var offset = parseResult.GetValue(listOffsetOption);
    var limit = parseResult.GetValue(listLimitOption);

    var response = await client.ListTodosAsync(offset, limit, ct);
    Console.WriteLine($"Showing {response.Items.Count} todo(s) (offset={response.Offset}, limit={response.Limit}):");
    Console.WriteLine();
    foreach (var todo in response.Items)
    {
        PrintTodo(todo);
    }
});

// --- todos get <id> ---
var getIdArgument = new Argument<string>("id") { Description = "The ID of the todo" };

var getCommand = new Command("get", "Get a specific todo");
getCommand.Arguments.Add(getIdArgument);
todosCommand.Subcommands.Add(getCommand);

getCommand.SetAction(async (parseResult, ct) =>
{
    var client = CreateClient(parseResult.GetValue(baseUrlOption)!);
    var id = parseResult.GetValue(getIdArgument)!;

    var todo = await client.GetTodoAsync(id, ct);
    PrintTodo(todo);
});

// --- todos create <title> [--completed] ---
var createTitleArgument = new Argument<string>("title") { Description = "The title of the todo" };
var createCompletedOption = new Option<bool>("--completed") { Description = "Mark the todo as completed" };

var createCommand = new Command("create", "Create a new todo");
createCommand.Arguments.Add(createTitleArgument);
createCommand.Options.Add(createCompletedOption);
todosCommand.Subcommands.Add(createCommand);

createCommand.SetAction(async (parseResult, ct) =>
{
    var client = CreateClient(parseResult.GetValue(baseUrlOption)!);
    var title = parseResult.GetValue(createTitleArgument)!;
    var completed = parseResult.GetValue(createCompletedOption);

    var request = new CreateTodoRequest { Title = title, Completed = completed };
    var todo = await client.CreateTodoAsync(request, ct);
    Console.WriteLine("Created:");
    PrintTodo(todo);
});

// --- todos update <id> <title> [--completed] ---
var updateIdArgument = new Argument<string>("id") { Description = "The ID of the todo to update" };
var updateTitleArgument = new Argument<string>("title") { Description = "The new title" };
var updateCompletedOption = new Option<bool>("--completed") { Description = "Set completion status" };

var updateCommand = new Command("update", "Update an existing todo");
updateCommand.Arguments.Add(updateIdArgument);
updateCommand.Arguments.Add(updateTitleArgument);
updateCommand.Options.Add(updateCompletedOption);
todosCommand.Subcommands.Add(updateCommand);

updateCommand.SetAction(async (parseResult, ct) =>
{
    var client = CreateClient(parseResult.GetValue(baseUrlOption)!);
    var id = parseResult.GetValue(updateIdArgument)!;
    var title = parseResult.GetValue(updateTitleArgument)!;
    var completed = parseResult.GetValue(updateCompletedOption);

    var request = new UpdateTodoRequest { Title = title, Completed = completed };
    var todo = await client.UpdateTodoAsync(id, request, ct);
    Console.WriteLine("Updated:");
    PrintTodo(todo);
});

// --- todos delete <id> ---
var deleteIdArgument = new Argument<string>("id") { Description = "The ID of the todo to delete" };

var deleteCommand = new Command("delete", "Delete a todo");
deleteCommand.Arguments.Add(deleteIdArgument);
todosCommand.Subcommands.Add(deleteCommand);

deleteCommand.SetAction(async (parseResult, ct) =>
{
    var client = CreateClient(parseResult.GetValue(baseUrlOption)!);
    var id = parseResult.GetValue(deleteIdArgument)!;

    await client.DeleteTodoAsync(id, ct);
    Console.WriteLine($"Deleted todo {id}.");
});

return await rootCommand.Parse(args).InvokeAsync();

// --- Helpers ---

static VibeApiClient CreateClient(string baseUrl)
{
    var httpClient = new HttpClient();
    return new VibeApiClient(baseUrl, httpClient);
}

static void PrintTodo(Todo todo)
{
    var status = todo.Completed ? "[x]" : "[ ]";
    Console.WriteLine($"  {status} {todo.Id}  {todo.Title}");
    Console.WriteLine($"      Created: {todo.CreatedAt:u}  Updated: {todo.UpdatedAt:u}");
}
