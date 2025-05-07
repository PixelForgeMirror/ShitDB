using System.Text.Json;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.BufferManagement;

public class TableUpdater(FileResolver resolver, LockManager locks): IDisposable
{
    private IDisposable? _lock;
    
    public async Task<Result<Table, Exception>> Fetch(string tableName)
    {
        _lock ??= await locks.StartWrite(TableDescriptor.FromName(tableName));
        try
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
        catch (Exception e)
        {
            return e;
        }
    }
    
    public async Task<Result<bool, Exception>> Update(Table table)
    {
        _lock ??= await locks.StartWrite(table.Descriptor);
        try
        {
            var tableSchema = JsonSerializer.Serialize(table.Descriptor);
            await File.WriteAllTextAsync(resolver.ResolveSchema(table.Descriptor), tableSchema);

            var tableContent = JsonSerializer.Serialize(table.TableRows);
            await File.WriteAllTextAsync(resolver.ResolveTable(table.Descriptor), tableContent);

            return true;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public void Dispose()
    {
        _lock?.Dispose();
    }
}