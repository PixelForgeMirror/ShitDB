using ShitDB.Domain;
using ShitDB.Util;
using Type = ShitDB.Domain.Type;

namespace ShitDB.DataSystem.QueryStatements.QueryParts;

public record TypeList(List<ColumnDescriptor> Columns)
{
    public static Result<TypeList, Exception> FromRawQuery(string typeList)
    {
        List<string> columns = typeList.Split(',').ToList();

        var cols = new List<ColumnDescriptor>();

        var whiteSpace = new[] { ' ', '\t', '\n', '\r' };
        foreach (var column in columns)
        {
            var col = column.Trim();
            var index = col.IndexOfAny(whiteSpace);

            var endOfWhitespace = index;
            while (endOfWhitespace < col.Length && char.IsWhiteSpace(col[endOfWhitespace])) endOfWhitespace++;

            var name = col.Substring(0, index);
            var typeString = col.Substring(endOfWhitespace).ToLower();

            Type type;

            if (typeString == "string")
                type = Type.String;
            else if (typeString == "integer")
                type = Type.Integer;
            else
                return new Exception("Unknown column type received: " + col);

            cols.Add(new ColumnDescriptor(name, type));
        }

        return new TypeList(cols);
    }
}