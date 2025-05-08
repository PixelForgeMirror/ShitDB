using System.Text.RegularExpressions;
using ShitDB.BufferManagement;
using ShitDB.DataSystem.QueryStatements;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem.QueryHandler;

public class InsertHandler(
    SchemaFetcher schemaFetcher,
    TypeValidator typeValidator,
    TableInserter inserter,
    TypeConverter converter) : IQueryHandler
{
    public async Task<Result<List<TableRow>, Exception>> Execute(string query)
    {
        var match = Regex.Match(query,
            @"^INSERT\s+INTO\s+(\w+)\s*(?:\(\s*((?:\s*\w+\s*,)*\s*\w+)\s*\))?\s*VALUES\s*\(\s*((?:\s*(?:\d+|"".*"")\s*,)*\s*(?:\d+|"".*""))\s*\)",
            RegexOptions.IgnoreCase);
        if (!match.Success) return new Exception("Invalid insert into statement");

        var tableName = match.Groups[1].Value;
        var columns = match.Groups[2].Value;
        var values = match.Groups[3].Value;

        var tableDescriptorResult = await schemaFetcher.Fetch(tableName);
        if (tableDescriptorResult.IsErr())
            return tableDescriptorResult.Map(_ => new List<TableRow>(),
                inner => new Exception("Table not found", inner));

        var tableDescriptor = tableDescriptorResult.Unwrap();
        
        var insertStatementResult = InsertStatement.FromRawQuery(tableDescriptor, columns, values);
        if (insertStatementResult.IsErr())
            return insertStatementResult.UnwrapErr();
        var insertStatement = insertStatementResult.Unwrap();

        var result = await inserter.Insert(tableDescriptor, new TableRow(insertStatement.ValuesList.Values));

        return result.MapOk(_ => new List<TableRow> { new(["Inserted 1 row."]) });
    }
}