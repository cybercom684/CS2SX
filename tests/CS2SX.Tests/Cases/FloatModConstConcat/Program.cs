using System;

// float/double % and %= (C has no floating-point %), constant string
// concatenations (also across classes), and statements hoisted out of an
// else-if condition (a string concatenation, an out var). Found while building
// nxStore. Expected output in comments.
public static class P
{
    public const string Root = "sdmc:/switch";
    public const string Sub = Root + "/app";
    public const string Deep = Sub + "/" + "cache" + "/x\"q";
}

public class FloatModConstConcatApp : SwitchApp
{
    const string Local = P.Root + "/local";

    public override void OnInit()
    {
        float f = 7.5f;
        double d = -7.5;
        Console.WriteLine("mod " + (f % 2f) + " " + (d % 2.0) + " " + (5.25 % 1.0) + " " + (-1f % 3f)); // mod 1.5 -1.5 0.25 -1
        f %= 4f;
        d %= 4.0;
        Console.WriteLine("modeq " + f + " " + d);                                      // modeq 3.5 -3.5
        int i = 7;
        i %= 3;
        Console.WriteLine("int " + (7 % 3) + " " + i);                                  // int 1 1

        Console.WriteLine("const " + P.Sub + " " + Local + " " + P.Deep);              // const sdmc:/switch/app sdmc:/switch/local sdmc:/switch/app/cache/x"q

        for (int n = 0; n < 4; n++)
        {
            string a = "x" + n;
            string r;
            if (n == 0) r = "zero";
            else if (a == "x" + 1) r = "one";
            else if (int.TryParse("2" + n, out int v) && v == 22) r = "two";
            else r = "other";
            Console.WriteLine("chain " + n + " " + r);                                  // chain 0 zero / 1 one / 2 two / 3 other
        }
        Console.WriteLine("done");
    }

    public override void OnFrame() { }
}
