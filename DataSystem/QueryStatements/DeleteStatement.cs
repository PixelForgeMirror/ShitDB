using ShitDB.DataSystem.QueryStatements.QueryParts;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem.QueryStatements;

public record DeleteStatement(TableDescriptor TableDescriptor, WhereClause WhereClause)
{
    public static Result<DeleteStatement, Exception> FromRawQuery(TableDescriptor descriptor, string whereClause)
    {
        var where = WhereClause.FromRawQuery(descriptor, whereClause);
        if (where.IsErr())
            return where.UnwrapErr();
        return new DeleteStatement(descriptor, where.Unwrap());
    }
}