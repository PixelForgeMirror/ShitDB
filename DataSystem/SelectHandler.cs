using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ShitDB.BufferManagement;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem;

public class SelectHandler(ILogger<SelectHandler> logger, TableFetcher fetcher) : IQueryHandler
{
    public async Task<Result<List<TableRow>, Exception>> Execute(string query)
    {var match = Regex.Match(query, @"^SELECT\s+((?:(?:\w+|\*)\s*,\s*)*(?:\w+|\*))\s+FROM\s+(\w+)(?:\s+WHERE\s+(\w+\s*=\s*\w+))?", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return new Exception("Invalid insert into statement");
        }

        string tableName = match.Groups[2].Value;
        List<string> columns = match.Groups[1].Value.Split(',').Select(val => val.Trim()).ToList();
        List<string> whereClause = !String.IsNullOrEmpty(match.Groups[3].Value) ?  match.Groups[3].Value.Split("=").Select(val => val.Trim()).ToList() : new();

        foreach (var column in columns)
        {
            if (column == "*")
            {
                columns = ["*"];
                break;
            }
        }
        
        var tableResult = await fetcher.Fetch(tableName);
        
        if (tableResult.IsErr())
            return tableResult.Map(_ => new List<TableRow>(), inner => new Exception("Failed fetching table", inner));
        var table = tableResult.Unwrap();

        var filteredRows = new List<TableRow>();
        
        var whereIndex = table.Descriptor.Columns.FindIndex(val => val.Name == whereClause.FirstOrDefault());

        if (whereIndex == -1)
        {
            filteredRows = table.TableRows;
        }
        else
        {
            foreach (var row in table.TableRows)
            {
                if (row.Entries[whereIndex] == whereClause[1])
                {
                    filteredRows.Add(row);
                }
            }
        }

        return filteredRows;
    }
}