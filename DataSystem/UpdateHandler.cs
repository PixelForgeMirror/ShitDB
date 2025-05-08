using System.Text.RegularExpressions;
using ShitDB.BufferManagement;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem;

public class UpdateHandler(TableUpdater updater, TypeValidator validator) : IQueryHandler
{
    public async Task<Result<List<TableRow>, Exception>> Execute(string query)
    {
        var match = Regex.Match(query, @"^UPDATE\s+(\w+)\s+SET\s+((?:\s*\w+\s*=\s*\w+\s*,)*(?:\s*\w+\s*=\s*\w+))\s+WHERE\s+(\w+\s*=\s*\w+)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return new Exception("Invalid update statement");
        }

        string tableName = match.Groups[1].Value;
        List<(string Column, string Value)> assignments = match.Groups[2].Value.Split(',').Select(val =>
        {
            var temp = val.Split('=').Select(innerVal => innerVal.Trim()).ToList();
            return (temp[0], temp[1]);
        }).ToList();
        List<string> whereClause = match.Groups[3].Value.Split('=').Select(val => val.Trim()).ToList();
        
        int rowsAffected = 0;
        
        using (updater)
        {

            var tableResult = await updater.Fetch(tableName);

            if (tableResult.IsErr())
                return tableResult.Map(_ => new List<TableRow>(),
                    inner => new Exception("Failed fetching table", inner));
            var table = tableResult.Unwrap();

            var whereIndex = table.Descriptor.Columns.FindIndex(val => val.Name == whereClause[0]);
            if (whereIndex == -1)
                return new Exception(
                    $"Where clause referenced column {whereClause.FirstOrDefault()} which is not part of table {table.Descriptor.Name}");

            List<int> updateIndices = new List<int>();

            foreach (var assignment in assignments)
            {
                var index = table.Descriptor.Columns.FindIndex(val => val.Name == assignment.Column);
                if (index == -1)
                    return new Exception(
                        $"The column to update {assignment.Column} is not part of the table {table.Descriptor.Name}.");
                updateIndices.Add(index);
            }

            foreach (var row in table.TableRows)
            {
                if (row.Entries[whereIndex] == whereClause[1])
                {
                    for (int i = 0; i < updateIndices.Count; i++)
                    {
                        var descriptor = table.Descriptor.Columns[updateIndices[i]];
                        if (!validator.Validate(descriptor.Type, assignments[i].Value))
                            return new Exception($"Cannot assign value {assignments[i].Value} to column {descriptor.Name} of type {descriptor.Type}.");
                        
                        row.Entries[updateIndices[i]] = assignments[i].Value;
                    }
                    rowsAffected++;
                }
            }

            await updater.Update(table);
            if (tableResult.IsErr())
                return tableResult.Map(_ => new List<TableRow>(),
                    inner => new Exception("Failed updating table", inner));
        }

        return new List<TableRow> { new([$"Updated {rowsAffected} rows."]) };
    }
}