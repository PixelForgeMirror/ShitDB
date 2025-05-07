using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShitDB.Config;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.BufferManagement;

public class SchemaFetcher(ILogger<SchemaFetcher> logger, IOptions<DatabaseConfig> dbConfig, FileResolver resolver)
{
    public async Task<Result<TableDescriptor, Exception>> Fetch(string tableName)
    {
        try
        {
            var schemaContent = await File.ReadAllTextAsync(resolver.ResolveSchema(TableDescriptor.FromName(tableName)));
            
            var tableSchema = JsonSerializer.Deserialize<TableDescriptor>(schemaContent);

            return tableSchema == null ? new Exception("Failed loading schema") : tableSchema;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}