namespace ShitDB.Domain;

public class TypeValidator
{
    public bool Validate(Type type, string value)
    {
        return type switch
        {
            Type.Integer => int.TryParse(value, out var i),
            Type.String => true,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}