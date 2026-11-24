using System;
using System.Collections.Generic;
using System.Linq;

// Methods of the .NET class library without a translation. CS2SX must report each
// one on its C# line instead of passing it through to the C compiler. An enum whose
// C names collide with libnx (Perm_None) is reported too.
public enum Perm { None, Read }

public class Diagnostics_LibraryApp : SwitchApp
{
    public override void OnInit()
    {
        var l = new List<int> { 3, 1 };
        l.TrimExcess();
        var s = new Stack<int>();
        s.TrimExcess();
        Console.WriteLine(l.BinarySearch(3));
        Console.WriteLine("abc".Normalize());
    }

    public override void OnFrame() { }
}
