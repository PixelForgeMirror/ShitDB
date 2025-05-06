using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ShitDB.BufferManagement;
using ShitDB.Domain;
using ShitDB.Util;
using Type = ShitDB.Domain.Type;

namespace ShitDB.DataSystem;

public class InsertHandler(ILogger<InsertHandler> logger, SchemaFetcher schemaFetcher) : IQueryHandler
{
    public async Task<Result<List<string>, Exception>> Execute(string query)
    {
        var match = Regex.Match(query, @"^INSERT\s+INTO\s+(\w+)\s*(?:\(\s*((?:\s*\w+\s*,)*\s*\w+)\s*\))?\s+VALUES\s+\(\s*((?:\s*\w+\s*,)*\s*\w+)\s*\)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            string tableName = match.Groups[1].Value;
            List<string>? columns = match.Groups.Count == 3 ? match.Groups[2].Value.Split(',').ToList() : null;
            List<string> values = match.Groups.Count == 4 ? match.Groups[3].Value.Split(',').ToList() : match.Groups[2].Value.Split(',').ToList();
            
            var tableDescriptor = await schemaFetcher.Fetch(tableName);

            var cols = new List<ColumnDescriptor>();
            
            var whiteSpace = new char[] { ' ', '\t', '\n', '\r' };
            foreach (var column in columns)
            {
                string col = column.Trim();
                int index = col.IndexOfAny(whiteSpace);

                int endOfWhitespace = index;
                while (endOfWhitespace < col.Length && char.IsWhiteSpace(col[endOfWhitespace]))
                {
                    endOfWhitespace++;
                }

                string name = col.Substring(0, index);
                string typeString = col.Substring(endOfWhitespace).ToLower();

                Type type;

                if (typeString == "string")
                {
                    type = Type.String;    
                }
                else if (typeString == "integer")
                {
                    type = Type.Integer;
                }
                else
                {
                    return new Exception("Unknown column type received: " + col);
                }
                
                cols.Add(new ColumnDescriptor(name, type));
            }
            
            TableDescriptor table = new (tableName, cols);
            
            logger.LogInformation(table.ToString());
            
            // todo: pass onto paging file and store the record
        }
        else
        {
            return new Exception("Invalid create table statement");
        }
        return new List<string>();
    }
}