using System.Text.RegularExpressions;
using ShitDB.BufferManagement;
using ShitDB.DataSystem.QueryStatements;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem.QueryHandler;

public class DeleteHandler(TableUpdater updater, TypeValidator validator, TypeConverter converter) : IQueryHandler
{
    public async Task<Result<List<TableRow>, Exception>> Execute(string query)
    {
        var match = Regex.Match(query, @"^DELETE\s+FROM\s+(\w+)\s+WHERE\s+(\w+\s*=\s*(?:\d+|"".*""))",
            RegexOptions.IgnoreCase);
        if (!match.Success) return new Exception("Invalid delete from statement");

        var tableName = match.Groups[1].Value;
        var whereClause = match.Groups[2].Value;

        var rowsAffected = 0;

        using (updater)
        {
            var tableResult = await updater.Fetch(tableName);

            if (tableResult.IsErr())
                return tableResult.Map(_ => new List<TableRow>(),
                    inner => new Exception("Failed fetching table", inner));
            var table = tableResult.Unwrap();

            var deleteStatementResult = DeleteStatement.FromRawQuery(table.Descriptor, whereClause);
            if (deleteStatementResult.IsErr())
                return deleteStatementResult.UnwrapErr();

            var deleteStatement = deleteStatementResult.Unwrap();

            List<int> removeIndices = new();

            for (var i = 0; i < table.TableRows.Count; i++)
                if (table.TableRows[i].Entries[deleteStatement.WhereClause.ColumnIndex] ==
                    deleteStatement.WhereClause.Value)
                {
                    removeIndices.Add(i);
                    rowsAffected++;
                }

            removeIndices = removeIndices.OrderDescending().ToList();

            foreach (var removeIndex in removeIndices) table.TableRows.RemoveAt(removeIndex);

            await updater.Update(table);
            if (tableResult.IsErr())
                return tableResult.Map(_ => new List<TableRow>(),
                    inner => new Exception("Failed updating table", inner));
        }

        return new List<TableRow> { new([$"Deleted {rowsAffected} rows."]) };
    }
}