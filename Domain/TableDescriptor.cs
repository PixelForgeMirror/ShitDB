namespace Domain;

public class TableDescriptor(string name, List<ColumnDescriptor> columns)
{
    public List<ColumnDescriptor> Columns { get; init; } = columns;
    public string Name { get; init; } = name;
}