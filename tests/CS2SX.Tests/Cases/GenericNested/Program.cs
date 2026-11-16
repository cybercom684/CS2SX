using System;
using System.Collections.Generic;

// Generic methods inside generic classes: the method is specialised per call,
// the class per type as before. Expected output in comments.
public class Box<T>
{
    public T Value;
    public Box(T value) { Value = value; }

    public U Map<U>(Func<T, U> f) => f(Value);
    public bool Is<U>(U other, Func<T, U, bool> same) => same(Value, other);
    public T Pick<U>(U ignored) => Value;
    public T Same() => Echo(Value);
    private U Echo<U>(U x) => x;
}

public class GenericNestedApp : SwitchApp
{
    public override void OnInit()
    {
        var n = new Box<int>(21);
        var s = new Box<string>("ab");
        Console.WriteLine(n.Map(x => x * 2) + " " + n.Map(x => "n" + x));           // 42 n21
        Console.WriteLine(s.Map(x => x.Length) + " " + s.Map(x => x + x));           // 2 abab
        Console.WriteLine(n.Is("21", (a, b) => ("" + a) == b) + " " + n.Pick(1.5f)); // True 21
        Console.WriteLine(n.Same() + " " + s.Same());                                // 21 ab
    }
}
