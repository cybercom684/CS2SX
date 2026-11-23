using System;
using System.Collections.Generic;
using System.Linq;

// Enums in strings show their member names, like in .NET. Expected output in comments.
public enum Rank { Bronze, Silver, Gold }
public enum Code { Ok = 200, NotFound = 404 }

[Flags]
public enum Access { None = 0, Read = 1, Write = 2, Exec = 4, All = Read | Write | Exec }

public class Player
{
    public Rank Rank { get; set; } = Rank.Silver;
}

public class EnumNamesApp : SwitchApp
{
    public override void OnInit()
    {
        var r = Rank.Gold;
        Console.WriteLine(r);                                       // Gold
        Console.WriteLine("rank " + r + " " + r.ToString());          // rank Gold Gold
        Console.WriteLine($"interp {r} {(int)r} {Code.NotFound}");    // interp Gold 2 NotFound
        var p = new Player();
        Console.WriteLine("prop " + p.Rank);                        // prop Silver
        Console.WriteLine("odd " + (Rank)7 + " " + (Code)201);     // odd 7 201
        Console.WriteLine("flags " + (Access.Read | Access.Exec) + " / " + Access.All + " / " + Access.None + " / " + (Access)8);
        // flags Read, Exec / All / None / 8
        var s = "x";
        s += Rank.Bronze;
        Console.WriteLine(s);                                       // xBronze
        var list = new List<Rank> { Rank.Gold, Rank.Bronze };
        Console.WriteLine("list " + string.Join(",", list.Select(x => x.ToString())));   // list Gold,Bronze
        Console.WriteLine("done");
    }

    public override void OnFrame() { }
}
