using System;
using System.Collections.Generic;

// Generic virtual/abstract/interface methods and generic structs, specialised per
// type at the C# level. Expected output in comments.
public interface IFormatter
{
    string Format<T>(T value);
}

public class Plain : IFormatter
{
    public string Format<T>(T value) => "" + value;
}

public class Bracket : IFormatter
{
    public string Format<T>(T value) => "[" + value + "]";
}

public abstract class Picker
{
    public abstract T Pick<T>(T a, T b);
    public virtual T Echo<T>(T x) => x;
}

public class FirstPicker : Picker
{
    public override T Pick<T>(T a, T b) => a;
}

public class SecondPicker : Picker
{
    public override T Pick<T>(T a, T b) => b;
    public override T Echo<T>(T x) => Pick(x, x);
}

public struct Pair<T>
{
    public T A;
    public T B;
    public Pair(T a, T b) { A = a; B = b; }
    public Pair<T> Swapped() => new Pair<T>(B, A);
}

public struct Slot<K, V>
{
    public K Key;
    public V Value;
}

public class GenericTypesApp : SwitchApp
{
    public override void OnInit()
    {
        var formatters = new List<IFormatter> { new Plain(), new Bracket() };
        foreach (var f in formatters)
            Console.WriteLine("fmt " + f.Format(7) + " " + f.Format("x"));   // fmt 7 x / fmt [7] [x]

        Picker p1 = new FirstPicker();
        Picker p2 = new SecondPicker();
        Console.WriteLine("pick " + p1.Pick(1, 2) + " " + p2.Pick(1, 2) + " " + p2.Pick("a", "b")); // pick 1 2 b
        Console.WriteLine("echo " + p1.Echo(5) + " " + p2.Echo(6));          // echo 5 6

        var pi = new Pair<int>(3, 4);
        var ps = new Pair<string>("l", "r").Swapped();
        Console.WriteLine("pair " + pi.A + pi.B + " " + ps.A + ps.B + " " + pi.Swapped().A);  // pair 34 rl 4

        var slot = new Slot<string, int> { Key = "hp", Value = 30 };
        Console.WriteLine("slot " + slot.Key + "=" + slot.Value);           // slot hp=30
    }
}
