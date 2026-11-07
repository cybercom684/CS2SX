public struct Vec2
{
    public float X;
    public float Y;

    public Vec2(float x, float y) { X = x; Y = y; }

    public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
    public static Vec2 operator *(Vec2 a, float s) => new Vec2(a.X * s, a.Y * s);

    public float Dot(Vec2 o) => X * o.X + Y * o.Y;
}

public class OperatorOverloadingApp : SwitchApp
{
    private Vec2 _pos = new Vec2(0, 0);
    private Vec2 _vel = new Vec2(1, 2);

    public override void OnFrame()
    {
        _pos = _pos + _vel * 0.5f;
        var d = _pos - new Vec2(1, 1);
        Console.WriteLine(_pos.X + "," + _pos.Y + " dot=" + d.Dot(_vel));
    }
}
