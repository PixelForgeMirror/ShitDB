using System.Text.Json;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.BufferManagement;

public class TableInserter(FileResolver resolver, LockManager locks)
{
    public async Task<Result<bool, Exception>> Insert(TableDescriptor descriptor, TableRow row)
    {
        try
        {
            using (await locks.StartWrite(descriptor))
            {
                var tableContent = await File.ReadAllTextAsync(resolver.ResolveTable(descriptor));
                var tableRows = JsonSerializer.Deserialize<List<TableRow>>(tableContent);

                if (tableRows is null)
                    return new Exception("Failed loading table");

                tableRows.Add(row);

                await File.WriteAllTextAsync(resolver.ResolveTable(descriptor), JsonSerializer.Serialize(tableRows));
            }
        }
        catch (Exception e)
        {
            return e;
        }

        return true;
    }
}