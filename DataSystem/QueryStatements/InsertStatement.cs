using ShitDB.DataSystem.QueryStatements.QueryParts;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem.QueryStatements;

public record InsertStatement(TableDescriptor TableDescriptor, ColumnList? ColumnList, ValuesList ValuesList)
{
    public static Result<InsertStatement, Exception> FromRawQuery(TableDescriptor descriptor, string columnList, string valuesList)
    {
        ColumnList? colList = null;
        if (!String.IsNullOrEmpty(columnList))
        {
            var temp = ColumnList.FromRawQuery(descriptor, columnList);
            if (temp.IsErr())
                return temp.UnwrapErr();
            colList = temp.Unwrap();
        }
        var values = ValuesList.FromRawQuery(descriptor, colList, valuesList);
        if (values.IsErr())
            return values.UnwrapErr();
        return new InsertStatement(descriptor, colList, values.Unwrap());
    }
}