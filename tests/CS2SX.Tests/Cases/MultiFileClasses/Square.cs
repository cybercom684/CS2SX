public class Square : Shape
{
    private readonly float _side;
    private readonly int[] _history = new int[4];   // array field → CS2SX_ArrFree in destructor

    public Square(float side) { _side = side; }

    // Class-typed parameter in an override: vtable slot must use Painter*.
    public override float Area(Painter p, float factor)
    {
        p.Calls++;
        _history[p.Calls % 4] = p.Calls;
        return _side * _side * factor * Scale;   // const of another user class
    }
}
