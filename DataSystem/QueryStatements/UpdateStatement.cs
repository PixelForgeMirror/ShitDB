using ShitDB.DataSystem.QueryStatements.QueryParts;
using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem.QueryStatements;

public record UpdateStatement(TableDescriptor TableDescriptor, AssignmentList AssignmentList, WhereClause WhereClause)
{
    public static Result<UpdateStatement, Exception> FromRawQuery(TableDescriptor tableDescriptor, string assignmentList, string whereClause)
    {
        var assignments = AssignmentList.FromRawQuery(tableDescriptor, assignmentList);
        if (assignments.IsErr())
            return assignments.UnwrapErr();
        var where = WhereClause.FromRawQuery(tableDescriptor, whereClause);
        if (where.IsErr())
            return where.UnwrapErr();
        
        return new UpdateStatement(tableDescriptor, assignments.Unwrap(), where.Unwrap());
    }
}