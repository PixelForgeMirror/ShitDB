using Microsoft.Extensions.Logging;
using ShitDB.Util;

namespace ShitDB.DataSystem;

public class UpdateHandler(ILogger<UpdateHandler> logger) : IQueryHandler
{
    public Task<Result<List<string>, Exception>> Execute(string query)
    {
        throw new NotImplementedException();
    }
}