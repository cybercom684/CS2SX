// Base class declared in a different file than its subclasses: each subclass
// header must include this file's header (embedded `base` field).
public abstract class Shape
{
    public const int MaxSides = 12;
    public const float Scale = 1.5f;

    public abstract float Area(Painter p, float factor);
}

public class Painter
{
    public int Calls;
}
