using System.Text.Json;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.BufferManagement;

public class SchemaFetcher(FileResolver resolver, LockManager locks)
{
    public async Task<Result<TableDescriptor, Exception>> Fetch(string tableName)
    {
        try
        {
            using (await locks.StartRead(TableDescriptor.FromName(tableName)))
            {
                var schemaContent =
                    await File.ReadAllTextAsync(resolver.ResolveSchema(TableDescriptor.FromName(tableName)));

                var tableSchema = JsonSerializer.Deserialize<TableDescriptor>(schemaContent);
                return tableSchema == null ? new Exception("Failed loading schema") : tableSchema;
            }
        }
        catch (Exception e)
        {
            return e;
        }
    }
}