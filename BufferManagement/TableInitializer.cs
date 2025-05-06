using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShitDB.Config;
using ShitDB.Domain;
using ShitDB.Util;
using System.Text.Json;

namespace ShitDB.BufferManagement;

public class TableInitializer(ILogger<TableInitializer> logger, IOptions<DatabaseConfig> dbConfig)
{
    public async Task<Result<bool, Exception>> Init(TableDescriptor descriptor)
    {
        try
        {
            if (!Directory.Exists(dbConfig.Value.DataFolderPath))
                Directory.CreateDirectory(dbConfig.Value.DataFolderPath ?? throw new Exception("Data directory not configured"));
            
            var tableSchemaFile = dbConfig.Value.DataFolderPath + "/" + descriptor.Name + "_schema.json";
            File.Create(tableSchemaFile).Close();
            File.Create(dbConfig.Value.DataFolderPath + "/" + descriptor.Name + ".json").Close();

            var tableDescriptor = JsonSerializer.Serialize(descriptor);

            await File.WriteAllTextAsync(tableSchemaFile, tableDescriptor);
        }
        catch (Exception e)
        {
            return e;
        }

        return true;
    }
}