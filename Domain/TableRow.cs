namespace ShitDB.Domain;

public class TableRow(List<string> entries)
{
    public List<string> Entries { get; init; } = entries;
}