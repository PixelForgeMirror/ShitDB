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
            
            var tableFile = dbConfig.Value.DataFolderPath + "/" + descriptor.Name + ".json";
            File.Create(tableFile).Close();

            var table = new Table(descriptor, new List<TableRow>());

            var tableContent = JsonSerializer.Serialize(table);

            await File.WriteAllTextAsync(tableFile, tableContent);
        }
        catch (Exception e)
        {
            return e;
        }

        return true;
    }
}