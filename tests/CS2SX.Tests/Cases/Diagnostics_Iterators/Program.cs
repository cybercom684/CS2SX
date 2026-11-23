using System;
using System.Collections;
using System.Collections.Generic;

// Forms of yield that cannot be lowered. CS2SX must reject each one with a
// clear error (expect-errors.txt).
public class Diagnostics_IteratorsApp : SwitchApp
{
    private IEnumerator<int> InTry()
    {
        try
        {
            yield return 1;
        }
        finally { Console.WriteLine("x"); }
    }

    private IEnumerator Values()
    {
        yield return 5;
    }

    private IEnumerable Plain()
    {
        yield return 1;
    }

    private IEnumerator<int> OutVar(string s)
    {
        if (int.TryParse(s, out var n))
        {
            yield return n;
            yield return n + 1;
        }
    }

    private IEnumerable<int> Local()
    {
        int k = 1; IEnumerable<int> Inner() { yield return k; }
        return Inner();
    }

    public override void OnInit() { }
    public override void OnFrame() { }
}
