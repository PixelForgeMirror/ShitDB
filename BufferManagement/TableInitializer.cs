using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShitDB.Config;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.BufferManagement;

public class TableInitializer(ILogger<TableInitializer> logger, IOptions<DatabaseConfig> dbConfig)
{
    public async Task<Result<bool, Exception>> Init(TableDescriptor descriptor)
    {
        File.Create(dbConfig.Value.DataFolderPath + "/" + descriptor.Name);
        
        return true;
    }
}