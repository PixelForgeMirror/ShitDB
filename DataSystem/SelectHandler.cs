using Microsoft.Extensions.Logging;
using ShitDB.Util;

namespace ShitDB.DataSystem;

public class SelectHandler(ILogger<SelectHandler> logger) : IQueryHandler
{
    public async Task<Result<List<string>, Exception>> Execute(string query)
    {
        throw new NotImplementedException();
    }
}