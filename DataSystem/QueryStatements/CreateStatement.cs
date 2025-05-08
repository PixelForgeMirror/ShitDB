using ShitDB.DataSystem.QueryStatements.QueryParts;
using ShitDB.Util;

namespace ShitDB.DataSystem.QueryStatements;

public record CreateStatement(string TableName, TypeList TypeList)
{
    public static Result<CreateStatement, Exception> FromRawQuery(string tableName, string typeList)
    {
        var types = TypeList.FromRawQuery(typeList);
        if (types.IsErr())
            return types.UnwrapErr();
        return new CreateStatement(tableName, types.Unwrap());
    }
}