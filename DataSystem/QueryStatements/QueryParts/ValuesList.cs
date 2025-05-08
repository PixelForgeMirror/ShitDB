using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem.QueryStatements.QueryParts;

public record ValuesList(List<string> Values)
{
    public static Result<ValuesList, Exception> FromRawQuery(TableDescriptor descriptor, ColumnList? columnList, string valueList)
    {
        TypeValidator validator = new TypeValidator();
        TypeConverter converter = new TypeConverter();
        
        List<string> sortedValues = valueList.Split(',').Select(x => x.Trim()).ToList();
        
        if (sortedValues.Count != descriptor.Columns.Count)
            return new Exception(
                $"Provided {sortedValues.Count} values. Expected {descriptor.Columns.Count}.");

        if (columnList is not null)
        {
            var temp = new string[descriptor.Columns.Count];
            for (int i = 0; i < columnList.Columns.Count; i++)
            {
                temp[columnList.Columns[i].Index] = sortedValues[i];
            }
            sortedValues = temp.ToList();
        }
        
        for (var i = 0; i < descriptor.Columns.Count; i++)
        {
            if (!validator.Validate(descriptor.Columns[i].Type, sortedValues[i]))
                return new Exception(
                    $"The value provided {sortedValues[i]} for column {descriptor.Columns[i].Name} is not compatible with type {descriptor.Columns[i].Type}.");

            sortedValues[i] = converter.Convert(descriptor.Columns[i].Type, sortedValues[i]);
        }

        return new ValuesList(sortedValues);
    }
}