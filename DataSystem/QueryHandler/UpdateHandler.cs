using System.Text.RegularExpressions;
using ShitDB.BufferManagement;
using ShitDB.DataSystem.QueryStatements;
using ShitDB.DataSystem.QueryStatements.QueryParts;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem.QueryHandler;

public class UpdateHandler(TableUpdater updater, TypeValidator validator, TypeConverter converter) : IQueryHandler
{
    public async Task<Result<List<TableRow>, Exception>> Execute(string query)
    {
        var match = Regex.Match(query,
            @"^UPDATE\s+(\w+)\s+SET\s+((?:\s*\w+\s*=\s*(?:\d+|"".*"")\s*,)*(?:\s*\w+\s*=\s*(?:\d+|"".*"")))\s+WHERE\s+(\w+\s*=\s*(?:\d+|"".*""))",
            RegexOptions.IgnoreCase);
        if (!match.Success) return new Exception("Invalid update statement");

        var tableName = match.Groups[1].Value;
        var assignments = match.Groups[2].Value;
        var whereClause = match.Groups[3].Value;

        var rowsAffected = 0;

        using (updater)
        {
            var tableResult = await updater.Fetch(tableName);

            if (tableResult.IsErr())
                return tableResult.Map(_ => new List<TableRow>(),
                    inner => new Exception("Failed fetching table", inner));
            var table = tableResult.Unwrap();

            var updateStatementResult = UpdateStatement.FromRawQuery(table.Descriptor, assignments, whereClause);
            if (updateStatementResult.IsErr())
                return updateStatementResult.UnwrapErr();
            var updateStatement = updateStatementResult.Unwrap();

            var updateIndices = new List<int>();

            foreach (var assignment in updateStatement.AssignmentList.Assignments)
            {
                var index = table.Descriptor.Columns.FindIndex(val => val.Name == assignment.Column);
                if (index == -1)
                    return new Exception(
                        $"The column to update {assignment.Column} is not part of the table {table.Descriptor.Name}.");
                updateIndices.Add(index);
            }

            foreach (var row in table.TableRows)
                if (row.Entries[updateStatement.WhereClause.ColumnIndex] == updateStatement.WhereClause.Value)
                {
                    for (var i = 0; i < updateIndices.Count; i++)
                    {
                        var descriptor = table.Descriptor.Columns[updateIndices[i]];
                        updateStatement.AssignmentList.Assignments[i] = new Assignment(descriptor.Name, updateStatement.AssignmentList.Assignments[i].Value);
                        row.Entries[updateIndices[i]] = updateStatement.AssignmentList.Assignments[i].Value;
                    }

                    rowsAffected++;
                }

            await updater.Update(table);
            if (tableResult.IsErr())
                return tableResult.Map(_ => new List<TableRow>(),
                    inner => new Exception("Failed updating table", inner));
        }

        return new List<TableRow> { new([$"Updated {rowsAffected} rows."]) };
    }
}