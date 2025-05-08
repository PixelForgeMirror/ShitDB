using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ShitDB.BufferManagement;
using ShitDB.Domain;
using ShitDB.Util;
using Type = ShitDB.Domain.Type;

namespace ShitDB.DataSystem;

public class DeleteHandler(TableUpdater updater, TypeValidator validator, TypeConverter converter) : IQueryHandler
{
    public async Task<Result<List<TableRow>, Exception>> Execute(string query)
    {
        var match = Regex.Match(query, @"^DELETE\s+FROM\s+(\w+)\s+WHERE\s+(\w+\s*=\s*(?:\d+|"".*""))", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return new Exception("Invalid delete from statement");
        }

        string tableName = match.Groups[1].Value;
        List<string> whereClause = match.Groups[2].Value.Split('=').Select(val => val.Trim()).ToList();
        
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

            if (!validator.Validate(table.Descriptor.Columns[whereIndex].Type, whereClause[1]))
            {
                return new Exception($"The value {whereClause[1]} is not assignable to column {whereClause[0]} of type {table.Descriptor.Columns[whereIndex].Type}.");
            }
            whereClause[1] = converter.Convert(table.Descriptor.Columns[whereIndex].Type, whereClause[1]);

            List<int> removeIndices = new();

            for (int i = 0; i <  table.TableRows.Count; i++)
            {
                if (table.TableRows[i].Entries[whereIndex] == whereClause[1])
                {
                    removeIndices.Add(i);
                    rowsAffected++;
                }
            }
            
            removeIndices = removeIndices.OrderDescending().ToList();

            foreach (int removeIndex in removeIndices)
            {
                table.TableRows.RemoveAt(removeIndex);
            }

            await updater.Update(table);
            if (tableResult.IsErr())
                return tableResult.Map(_ => new List<TableRow>(),
                    inner => new Exception("Failed updating table", inner));
        }

        return new List<TableRow> { new([$"Deleted {rowsAffected} rows."]) };
    }
}