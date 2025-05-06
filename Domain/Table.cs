namespace ShitDB.Domain;

public class Table(TableDescriptor descriptor, List<TableRow> rows)
{
    public TableDescriptor Descriptor { get; init; } = descriptor;
    public List<TableRow> TableRows { get; init; } = rows;
}