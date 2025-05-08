using Microsoft.Extensions.Options;
using ShitDB.Config;
using ShitDB.Domain;

namespace ShitDB.BufferManagement;

public class FileResolver(IOptions<DatabaseConfig> dbConfig)
{
    public string ResolveSchema(TableDescriptor descriptor)
    {
        return $"{dbConfig.Value.DataFolderPath}/{descriptor.Name}_schema.json";
    }

    public string ResolveTable(TableDescriptor descriptor)
    {
        return $"{dbConfig.Value.DataFolderPath}/{descriptor.Name}.json";
    }
}