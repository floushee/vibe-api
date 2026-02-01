var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .AddDatabase("vibedb");

var api = builder.AddProject<Projects.VibeAPI_API>("vibeapi")
    .WithReference(postgres);

builder.Build().Run();
