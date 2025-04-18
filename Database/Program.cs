using Database;
using Database.Config;
using DataSystem;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<QueryDecoder>();
builder.Services.AddSingleton<ConnectionHandler>();

builder.Services.AddOptions<DatabaseConfig>()
    .BindConfiguration("Database")
    .ValidateOnStart();

var host = builder.Build();
host.Run();