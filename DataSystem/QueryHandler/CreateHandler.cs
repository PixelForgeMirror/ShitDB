using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ShitDB.BufferManagement;
using ShitDB.DataSystem.QueryStatements;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem.QueryHandler;

public class CreateHandler(ILogger<CreateHandler> logger, TableInitializer initializer) : IQueryHandler
{
    public async Task<Result<List<TableRow>, Exception>> Execute(string query)
    {
        var match = Regex.Match(query,
            @"^CREATE\s+TABLE\s+(\w+)\s*\(\s*((?:\w+\s+(?:string|integer)\s*,\s*)*(?:\w+\s+(?:string|integer)))\s*\)",
            RegexOptions.IgnoreCase);
        if (!match.Success) return new Exception("Invalid create table statement");

        var statement = CreateStatement.FromRawQuery(match.Groups[1].Value, match.Groups[2].Value);
        if (statement.IsErr())
            return statement.UnwrapErr();

        TableDescriptor table = new(statement.Unwrap().TableName, statement.Unwrap().TypeList.Columns);

        var result = await initializer.Init(table);
        return result.MapOk(_ => new List<TableRow> { new(["Created 1 table."]) });
    }
}