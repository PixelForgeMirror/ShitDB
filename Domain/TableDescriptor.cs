namespace ShitDB.Domain;

public class TableDescriptor(string name, List<ColumnDescriptor> columns)
{
    protected bool Equals(TableDescriptor other)
    {
        return Equals(other as object);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TableDescriptor)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public static TableDescriptor FromName(string tableName)
    {
        return new TableDescriptor(tableName, new());
    }
    
    public List<ColumnDescriptor> Columns { get; init; } = columns;
    public string Name { get; init; } = name;
}