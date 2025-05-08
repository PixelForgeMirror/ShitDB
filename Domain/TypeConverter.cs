namespace ShitDB.Domain;

public class TypeConverter
{
    public string Convert(Type type, string value)
    {
        return type switch
        {
            Type.Integer => value,
            Type.String => value.Remove(value.Length - 1, 1).Remove(0, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}