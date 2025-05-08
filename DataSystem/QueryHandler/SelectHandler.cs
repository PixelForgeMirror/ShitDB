using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ShitDB.BufferManagement;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem.QueryHandler;

public class SelectHandler(ILogger<SelectHandler> logger, TableFetcher fetcher, TypeConverter converter) : IQueryHandler
{
    public async Task<Result<List<TableRow>, Exception>> Execute(string query)
    {
        var match = Regex.Match(query,
            @"^SELECT\s+((?:(?:\w+|\*)\s*,\s*)*(?:\w+|\*))\s+FROM\s+(\w+)(?:\s+WHERE\s+(\w+\s*=\s*(?:\d+|"".*"")))?",
            RegexOptions.IgnoreCase);
        if (!match.Success) return new Exception("Invalid select statement");

        var tableName = match.Groups[2].Value;
        List<string> columns = match.Groups[1].Value.Split(',').Select(val => val.Trim()).ToList();
        List<string> whereClause = !string.IsNullOrEmpty(match.Groups[3].Value)
            ? match.Groups[3].Value.Split("=").Select(val => val.Trim()).ToList()
            : new List<string>();

        var wildcardSelect = false;

        foreach (var column in columns)
            if (column == "*")
            {
                wildcardSelect = true;
                break;
            }

        var tableResult = await fetcher.Fetch(tableName);

        if (tableResult.IsErr())
            return tableResult.Map(_ => new List<TableRow>(), inner => new Exception("Failed fetching table", inner));
        var table = tableResult.Unwrap();

        var filteredRows = new List<TableRow>();

        var whereIndex = table.Descriptor.Columns.FindIndex(val => val.Name == whereClause.FirstOrDefault());

        if (whereIndex == -1 && whereClause.Count > 0)
            return new Exception(
                $"Where clause referenced column {whereClause.FirstOrDefault()} which is not part of table {table.Descriptor.Name}");

        if (whereIndex == -1)
        {
            filteredRows = table.TableRows;
        }
        else
        {
            whereClause[1] = converter.Convert(table.Descriptor.Columns[whereIndex].Type, whereClause[1]);
            foreach (var row in table.TableRows)
                if (row.Entries[whereIndex] == whereClause[1])
                    filteredRows.Add(row);
        }

        if (!wildcardSelect)
        {
            var keepIndices = new List<int>();

            foreach (var selected in columns)
            {
                var index = table.Descriptor.Columns.FindIndex(val => val.Name == selected);
                if (index == -1)
                    return new Exception(
                        $"The selected column {selected} is not part of the table {table.Descriptor.Name}.");
                keepIndices.Add(index);
            }

            var selectedRows = new List<TableRow>();

            foreach (var row in filteredRows)
            {
                var filteredRow = new List<string>();

                foreach (var index in keepIndices) filteredRow.Add(row.Entries[index]);

                selectedRows.Add(new TableRow(filteredRow));
            }

            filteredRows = selectedRows;
        }

        return filteredRows;
    }
}