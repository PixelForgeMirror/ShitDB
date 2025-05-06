namespace ShitDB.Domain;

public class ColumnDescriptor(string name, Type type)
{
    public string Name { get; init; } = name;
    public Type Type { get; init; } = type;
}