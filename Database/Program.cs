using Database;
using ShitDB.BufferManagement;
using ShitDB.Config;
using ShitDB.Database;
using ShitDB.DataSystem;

var builder = Host.CreateApplicationBuilder(args);

// Connection handling
builder.Services.AddHostedService<Worker>();
builder.Services.AddTransient<ConnectionHandler>();

// Query parsing
builder.Services.AddTransient<QueryDecoder>();
builder.Services.AddTransient<CreateHandler>();
builder.Services.AddTransient<DeleteHandler>();
builder.Services.AddTransient<InsertHandler>();
builder.Services.AddTransient<SelectHandler>();
builder.Services.AddTransient<UpdateHandler>();

// Io handling
builder.Services.AddTransient<TableInitializer>();

builder.Services.AddOptions<DatabaseConfig>()
    .BindConfiguration("Database")
    .ValidateOnStart();

var host = builder.Build();
host.Run();