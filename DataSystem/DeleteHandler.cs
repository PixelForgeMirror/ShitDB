using Microsoft.Extensions.Logging;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem;

public class DeleteHandler(ILogger<DeleteHandler> logger) : IQueryHandler
{
    public async Task<Result<List<TableRow>, Exception>> Execute(string query)
    {
        throw new NotImplementedException();
    }
}