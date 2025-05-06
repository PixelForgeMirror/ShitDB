using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShitDB.Config;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.BufferManagement;

public class SchemaFetcher(ILogger<SchemaFetcher> logger, IOptions<DatabaseConfig> dbConfig)
{
    public async Task<Result<TableDescriptor, Exception>> Fetch(string tableName)
    {
        try
        {
            var tableContent = await File.ReadAllTextAsync($"{dbConfig.Value.DataFolderPath}/{tableName}_schema.json");
            
            var tableSchema = JsonSerializer.Deserialize<TableDescriptor>(tableContent);

            return tableSchema!;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}