using System;
using System.Collections.Generic;

// Overloads that differ only in their parameter types (same number of
// parameters), next to overloads that differ in the number. Instance, static,
// called from another class and through a delegate. Expected output in comments.
public class Shape
{
    public string Name = "shape";
}

public class Printer
{
    private int _calls;
    public int Calls => _calls;

    public string Show(int value) { _calls++; return "int " + value; }
    public string Show(float value) { _calls++; return "float " + value; }
    public string Show(string value) { _calls++; return "string " + value; }
    public string Show(Shape value) { _calls++; return "shape " + value.Name; }
    public string Show(int[] values) { _calls++; return "array " + values.Length; }
    public string Show(List<int> values) { _calls++; return "list " + values.Count; }
    public string Show(int a, int b) { _calls++; return "two " + (a + b); }
    public string Show(int a, string b) { _calls++; return "mixed " + a + b; }
    public string Show() { _calls++; return "none"; }

    public static int Twice(int x) => x * 2;
    public static string Twice(string x) => x + x;

    public string All() => Show(1) + ", " + Show("s") + ", " + Show();
}

public class OverloadsApp : SwitchApp
{
    private static double Area(double side) => side * side;
    private static int Area(int side) => side * side;

    public override void OnInit()
    {
        var p = new Printer();
        Console.WriteLine(p.Show(7));                          // int 7
        Console.WriteLine(p.Show(1.5f));                       // float 1.5
        Console.WriteLine(p.Show("x"));                        // string x
        Console.WriteLine(p.Show(new Shape()));                // shape shape
        Console.WriteLine(p.Show(new int[] { 1, 2, 3 }));      // array 3
        Console.WriteLine(p.Show(new List<int> { 4, 5 }));     // list 2
        Console.WriteLine(p.Show(2, 3));                       // two 5
        Console.WriteLine(p.Show(2, "b"));                     // mixed 2b
        Console.WriteLine(p.Show());                           // none
        Console.WriteLine(p.All() + " " + p.Calls);            // int 1, string s, none 12
        Console.WriteLine(Printer.Twice(4) + " " + Printer.Twice("ab"));   // 8 abab
        Console.WriteLine(Area(3) + " " + Area(1.5));          // 9 2.25

        Func<string, string> twice = Printer.Twice;
        Console.WriteLine(twice("z"));                         // zz
    }
}
