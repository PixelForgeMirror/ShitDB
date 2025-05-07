using Microsoft.Extensions.Options;
using ShitDB.Config;
using ShitDB.Domain;
using ShitDB.Util;
using System.Text.Json;

namespace ShitDB.BufferManagement;

public class TableInitializer(IOptions<DatabaseConfig> dbConfig, FileResolver resolver, LockManager locks)
{
    public async Task<Result<bool, Exception>> Init(TableDescriptor descriptor)
    {
        try
        {
            using (await locks.StartWrite(descriptor))
            {
                if (dbConfig.Value.DataFolderPath is null)
                    return new Exception("Data directory not configured");

                if (!Directory.Exists(dbConfig.Value.DataFolderPath))
                    Directory.CreateDirectory(dbConfig.Value.DataFolderPath);

                File.Create(resolver.ResolveSchema(descriptor)).Close();
                File.Create(resolver.ResolveTable(descriptor)).Close();

                var tableDescriptor = JsonSerializer.Serialize(descriptor);

                await File.WriteAllTextAsync(resolver.ResolveSchema(descriptor), tableDescriptor);
                await File.WriteAllTextAsync(resolver.ResolveTable(descriptor), "[]");
            }
        }
        catch (Exception e)
        {
            return e;
        }

        return true;
    }
}