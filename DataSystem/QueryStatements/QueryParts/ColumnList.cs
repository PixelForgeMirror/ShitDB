using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem.QueryStatements.QueryParts;

public record ColumnList(List<(ColumnDescriptor Descriptor, int Index)> Columns)
{
    public static Result<ColumnList, Exception> FromRawQuery(TableDescriptor descriptor, string rawColumns)
    {
        var columns = rawColumns.Split(',').Select(x => x.Trim()).ToList();
        
        if (columns.Count != descriptor.Columns.Count)
            return new Exception(
                $"Provided {columns.Count} column names. Expected {descriptor.Columns.Count}.");
        
        List<(ColumnDescriptor, int)> cols = new();
        
        foreach (var column in descriptor.Columns)
        {
            var found = false;
            for (var i = 0; i < columns.Count; i++)
                if (column.Name == columns[i])
                {
                    found = true;
                    cols.Add((column, i));
                    break;
                }

            if (!found)
                return new Exception(
                    $"Column {column.Name} needed for table {descriptor.Name} was not provided in the query.");
        }
        
        return new ColumnList(cols);
    }
}