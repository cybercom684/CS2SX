using System;
using System.Collections.Generic;

// Values an object cannot hold, and tests an object cannot answer. CS2SX must
// reject each one with a clear error (expect-errors.txt).
public struct Point { public int X; }
public record Pair(int A, int B);
public interface IShape { int Size(); }

public class Diagnostics_ObjectApp : SwitchApp
{
    private object? _o;

    public override void OnInit()
    {
        _o = new Point();
        object list = new List<int>();
        object pair = new Pair(1, 2);
        Console.WriteLine(_o is IShape);
        Console.WriteLine(_o is Point);
    }

    public override void OnFrame() { }
}
