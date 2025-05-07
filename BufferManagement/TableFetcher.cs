using System.Text.Json;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.BufferManagement;

public class TableFetcher(FileResolver resolver, LockManager locks)
{
    public async Task<Result<Table, Exception>> Fetch(string tableName)
    {
        try
        {
            using (await locks.StartRead(TableDescriptor.FromName(tableName)))
            {
                var schemaContent =
                    await File.ReadAllTextAsync(resolver.ResolveSchema(TableDescriptor.FromName(tableName)));
                var tableSchema = JsonSerializer.Deserialize<TableDescriptor>(schemaContent);
                if (tableSchema is null)
                    return new Exception("Failed loading schema");

                var tableContent =
                    await File.ReadAllTextAsync(resolver.ResolveTable(TableDescriptor.FromName(tableName)));
                var tableRows = JsonSerializer.Deserialize<List<TableRow>>(tableContent);
                if (tableRows is null)
                    return new Exception("Failed loading table");

                return new Table(tableSchema, tableRows);
            }
        }
        catch (Exception e)
        {
            return e;
        }
    }
}