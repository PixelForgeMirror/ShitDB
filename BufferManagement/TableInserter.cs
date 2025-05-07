using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShitDB.Config;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.BufferManagement;

public class TableInserter(ILogger<TableInserter> logger, IOptions<DatabaseConfig> dbConfig, FileResolver resolver)
{
    public async Task<Result<bool, Exception>> Insert(TableDescriptor descriptor, TableRow row)
    {

        var tableContent = await File.ReadAllTextAsync(resolver.ResolveTable(descriptor));
        var tableRows = JsonSerializer.Deserialize<List<TableRow>>(tableContent);
        
        if (tableRows is null)
            return new Exception("Failed loading table");
        
        tableRows.Add(row);

        await File.WriteAllTextAsync(resolver.ResolveTable(descriptor), JsonSerializer.Serialize(tableRows));
        
        return true;
    }
}