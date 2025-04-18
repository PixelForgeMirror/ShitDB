using System.Text.RegularExpressions;
using Domain;
using Microsoft.Extensions.Logging;
using Util;
using Type = Domain.Type;

namespace DataSystem;

public class QueryDecoder(ILogger<QueryDecoder> logger)
{
    public async Task<Result<List<string>, Exception>> DecodeQuery(string query)
    {
        switch (query)
        {
            case var q when Regex.IsMatch(q, @"^CREATE TABLE", RegexOptions.IgnoreCase):
                return await CreateTable(q);
            case var q when Regex.IsMatch(q, @"^INSERT INTO", RegexOptions.IgnoreCase):
                InsertInto(q);
                break;

            case var q when Regex.IsMatch(q, @"^SELECT", RegexOptions.IgnoreCase):
                Select(q);
                break;

            case var q when Regex.IsMatch(q, @"^UPDATE", RegexOptions.IgnoreCase):
                Update(q);
                break;

            case var q when Regex.IsMatch(q, @"^DELETE FROM", RegexOptions.IgnoreCase):
                Delete(q);
                break;

            default:
                logger.LogError($"Unknown query received: {query}");
                break;
        }

        return new List<string>();
    }
    
    private async Task<Result<List<string>, Exception>> CreateTable(string query)
    {
        var match = Regex.Match(query, @"^CREATE\s+TABLE\s+(\w+)\s*\(\s*((?:\w+\s+(?:string|integer)\s*,\s*)*(?:\w+\s+(?:string|integer)))\s*\)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
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
            
            logger.LogInformation(table.ToString());
            
            // todo: pass onto paging file and store the record
        }
        else
        {
            return new Exception("Invalid create table statement");
        }
        return new List<string>();
    }

    private void InsertInto(string query)
    {
    }

    private void Select(string query)
    {
    }

    private void Update(string query)
    {
    }

    private void Delete(string query)
    {
    }
}
