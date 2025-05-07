using Database;
using ShitDB.BufferManagement;
using ShitDB.Config;
using ShitDB.Database;
using ShitDB.DataSystem;
using ShitDB.Domain;

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
builder.Services.AddTransient<TypeValidator>();

// Io handling
builder.Services.AddTransient<TableInitializer>();
builder.Services.AddTransient<SchemaFetcher>();
builder.Services.AddTransient<FileResolver>();
builder.Services.AddTransient<TableInserter>();
builder.Services.AddTransient<TableFetcher>();
builder.Services.AddSingleton<LockManager>();
builder.Services.AddTransient<TableUpdater>();

builder.Services.AddOptions<DatabaseConfig>()
    .BindConfiguration("Database")
    .ValidateOnStart();

var host = builder.Build();
host.Run();