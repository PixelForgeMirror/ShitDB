using Database;
using Database.Config;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddOptions<DatabaseConfig>()
    .BindConfiguration("Database")
    .ValidateOnStart();

var host = builder.Build();
host.Run();