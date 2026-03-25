namespace CS2SX.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ArgAttribute : Attribute
{
    public string Flag { get; }          // z.B. "--verbose", "-v"
    public bool IsPositional { get; }    // positional arg (kein flag prefix)
    public int Position { get; }         // index bei positional args

    public ArgAttribute(string flag)
    {
        Flag = flag;
        IsPositional = false;
    }

    public ArgAttribute(int position)
    {
        Position = position;
        IsPositional = true;
    }
}