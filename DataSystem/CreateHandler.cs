using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ShitDB.BufferManagement;
using ShitDB.Domain;
using ShitDB.Util;
using Type = ShitDB.Domain.Type;

namespace ShitDB.DataSystem;

public class CreateHandler(ILogger<CreateHandler> logger, TableInitializer initializer) : IQueryHandler
{
    public async Task<Result<List<TableRow>, Exception>> Execute(string query)
    {
        var match = Regex.Match(query, @"^CREATE\s+TABLE\s+(\w+)\s*\(\s*((?:\w+\s+(?:string|integer)\s*,\s*)*(?:\w+\s+(?:string|integer)))\s*\)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return new Exception("Invalid create table statement");
        }

        string tableName = match.Groups[1].Value;
        List<string> columns = match.Groups[2].Value.Split(',').ToList();

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

        var result = await initializer.Init(table);
        return result.MapOk(_ => new List<TableRow> { new(["Created 1 table."]) });
    }
}