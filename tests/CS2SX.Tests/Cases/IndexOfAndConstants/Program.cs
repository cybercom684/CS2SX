using System;

// IndexOf / LastIndexOf with start index, count, a char variable and
// StringComparison; float/double constants with integer initializers (also from
// another class). Expected output in comments.
public static class K
{
    public const float R = 6;
    public const double H = 3;
    public const double Big = 1e30;
    public const double N = double.NaN;
}

public class IndexOfAndConstantsApp : SwitchApp
{
    const float R = 6;
    const double D = 3;
    const float F2 = 2.5f;
    static readonly float S = 4;

    public override void OnInit()
    {
        string s = "a.b.c.d";
        Console.WriteLine("idx " + s.IndexOf('.', 2) + " " + s.IndexOf(".", 4) + " " + s.IndexOf('x', 1) + " "
            + s.IndexOf('.', 2, 2) + " " + s.IndexOf('.', 4, 1));                         // idx 3 5 -1 3 -1
        Console.WriteLine("last " + s.LastIndexOf('.', 4) + " " + s.LastIndexOf(".", 2) + " " + s.LastIndexOf('.', 6, 2)
            + " " + s.LastIndexOf('.'));                                                  // last 3 1 5 5
        char dot = '.';
        Console.WriteLine("var " + s.IndexOf(dot) + " " + s.IndexOf(dot, 4));             // var 1 5
        Console.WriteLine("case " + "Hello World".IndexOf("WORLD", StringComparison.OrdinalIgnoreCase) + " "
            + "Hello World".IndexOf("o", 5) + " " + "Hello World".IndexOf("WORLD"));      // case 6 7 -1
        string u = "äb.c.";
        Console.WriteLine("utf " + u.IndexOf('.', 3) + " " + u.IndexOf(".", 3) + " " + u.LastIndexOf('.', 3)); // utf 4 4 2
        try { Console.WriteLine("no " + s.IndexOf('.', 99)); }
        catch (ArgumentOutOfRangeException) { Console.WriteLine("range"); }               // range

        Console.WriteLine("const " + R * 2 + " " + D / 2 + " " + F2 + " " + S + " " + K.R / 4 + " " + K.H / 2
            + " " + K.Big + " " + K.N);                                                   // const 12 1.5 2.5 4 1.5 1.5 1E+30 NaN
        Console.WriteLine("done");
    }

    public override void OnFrame() { }
}
