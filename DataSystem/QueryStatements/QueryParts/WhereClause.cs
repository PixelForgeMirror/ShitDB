using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem.QueryStatements.QueryParts;

public record WhereClause(string ColumnName, int ColumnIndex, string Value)
{
    public static Result<WhereClause, Exception> FromRawQuery(TableDescriptor descriptor, string clause)
    {
        List<string> parts = clause.Split('=').Select(x => x.Trim()).ToList();

        var whereIndex = descriptor.Columns.FindIndex(val => val.Name == parts[0]);
        if (whereIndex == -1)
            return new Exception(
                $"Where clause referenced column {parts.FirstOrDefault()} which is not part of table {descriptor.Name}");

        TypeValidator validator = new();
        TypeConverter converter = new();

        if (!validator.Validate(descriptor.Columns[whereIndex].Type, parts[1]))
            return new Exception(
                $"The value {parts[1]} is not assignable to column {parts[0]} of type {descriptor.Columns[whereIndex].Type}.");
        parts[1] = converter.Convert(descriptor.Columns[whereIndex].Type, parts[1]);

        return new WhereClause(parts[0], whereIndex, parts[1]);
    }
}