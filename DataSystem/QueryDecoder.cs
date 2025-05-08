using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ShitDB.DataSystem.QueryHandler;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem;

public class QueryDecoder(
    ILogger<QueryDecoder> logger,
    CreateHandler createHandler,
    InsertHandler insertHandler,
    SelectHandler selectHandler,
    UpdateHandler updateHandler,
    DeleteHandler deleteHandler
)
{
    public async Task<Result<List<TableRow>, Exception>> DecodeQuery(string query)
    {
        switch (query)
        {
            case var q when Regex.IsMatch(q, @"^CREATE TABLE", RegexOptions.IgnoreCase):
                return await createHandler.Execute(q);
            case var q when Regex.IsMatch(q, @"^INSERT INTO", RegexOptions.IgnoreCase):
                return await insertHandler.Execute(q);
            case var q when Regex.IsMatch(q, @"^SELECT", RegexOptions.IgnoreCase):
                return await selectHandler.Execute(q);
            case var q when Regex.IsMatch(q, @"^UPDATE", RegexOptions.IgnoreCase):
                return await updateHandler.Execute(q);
            case var q when Regex.IsMatch(q, @"^DELETE FROM", RegexOptions.IgnoreCase):
                return await deleteHandler.Execute(q);
            default:
                logger.LogError($"Unknown query received: {query}");
                return new Exception("Unknown query type.");
        }

        return new List<TableRow>();
    }
}