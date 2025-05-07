using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ShitDB.BufferManagement;
using ShitDB.Domain;
using ShitDB.Util;
using Type = ShitDB.Domain.Type;

namespace ShitDB.DataSystem;

public class InsertHandler(ILogger<InsertHandler> logger, SchemaFetcher schemaFetcher, TypeValidator typeValidator, TableInserter inserter) : IQueryHandler
{
    public async Task<Result<List<TableRow>, Exception>> Execute(string query)
    {
        var match = Regex.Match(query, @"^INSERT\s+INTO\s+(\w+)\s*(?:\(\s*((?:\s*\w+\s*,)*\s*\w+)\s*\))?\s*VALUES\s*\(\s*((?:\s*\w+\s*,)*\s*\w+)\s*\)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return new Exception("Invalid insert into statement");
        }

        string tableName = match.Groups[1].Value;
        List<string> columns = !String.IsNullOrEmpty(match.Groups[2].Value) ? match.Groups[2].Value.Split(',').Select(val => val.Trim()).ToList() : new ();
        List<string> values = match.Groups[3].Value.Split(',').Select(val => val.Trim()).ToList();

        var tableDescriptorResult = await schemaFetcher.Fetch(tableName);
        if (tableDescriptorResult.IsErr())
        {
            return tableDescriptorResult.Map(_ => new List<TableRow>(),inner => new Exception("Table not found", inner));
        }
        var tableDescriptor = tableDescriptorResult.Unwrap();

        if ((columns.Count != 0 && columns.Count != tableDescriptor.Columns.Count) || values.Count != tableDescriptor.Columns.Count)
            return new Exception($"Provided {columns.Count} column names and {values.Count} values. Expected {tableDescriptor.Columns.Count}.");
        
        List<string> sortedValues = new();

        if (columns.Count > 0)
        {
            foreach (var schemaColumn in tableDescriptor.Columns)
            {
                bool found = false;
                for (int i = 0; i < columns.Count; i++)
                {
                    if (schemaColumn.Name.Equals(columns[i].Trim()))
                    {
                        found = true;
                        sortedValues.Add(values[i]);
                        columns.RemoveAt(i);
                        values.RemoveAt(i);
                        break;
                    }
                }
                if (!found)
                    return new Exception($"Column {schemaColumn.Name} needed for table {tableDescriptor.Name} was not provided in the query.");
            }
        }
        else
        {
            sortedValues = values;
        }
            
        for (int i = 0; i < tableDescriptor.Columns.Count; i++)
        {
            if (!typeValidator.Validate(tableDescriptor.Columns[i].Type, sortedValues[i]))
            {
                return new Exception($"The value provided {sortedValues[i]} for column {tableDescriptor.Columns[i].Name} is not compatible with type {tableDescriptor.Columns[i].Type}.");
            }
        }
        
        var result = await inserter.Insert(tableDescriptor, new TableRow(sortedValues));
            
        return result.MapOk(_ => new List<TableRow> { new(["Inserted 1 row."]) });
    }
}