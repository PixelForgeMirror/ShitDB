using ShitDB.Domain;
using ShitDB.Util;

namespace ShitDB.DataSystem.QueryStatements.QueryParts;

public record Assignment(string Column, string Value);

public record AssignmentList(List<Assignment> Assignments)
{
    public static Result<AssignmentList, Exception> FromRawQuery(TableDescriptor descriptor, string rawColumns)
    {
        List<string> assignments = rawColumns.Split(',').Select(x => x.Trim()).ToList();

        List<Assignment> assignmentList = new List<Assignment>();
        
        TypeValidator validator = new TypeValidator();
        TypeConverter converter = new TypeConverter();
        
        foreach (var assignment in assignments)
        {
            var temp = assignment.Split('=').Select(innerVal => innerVal.Trim()).ToList();

            foreach (var column in descriptor.Columns)
            {
                if (column.Name == temp[0])
                {
                    if (!validator.Validate(column.Type, temp[1]))
                        return new Exception($"The value {temp[1]} is not assignable to column {temp[0]} of type {column.Type}.");
                    
                    assignmentList.Add(new Assignment(temp[0], converter.Convert(column.Type, temp[1])));
                    break;
                }
            }
        }
        
        return new AssignmentList(assignmentList);
    }
}