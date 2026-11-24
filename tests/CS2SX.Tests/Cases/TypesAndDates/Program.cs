using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Interface properties and default methods, generic interfaces and base classes,
// computed struct properties, a static constructor, .NET numeric constants, LINQ on
// dictionaries, DateTime and TimeSpan values, file APIs with `using System.IO`.
// Expected output in comments.
public interface IShape
{
    double Area { get; }
    string Name { get; }
    string Describe() => Name + " " + Area;
    int Corners() { return 0; }
}
public class Sq : IShape { public double Side = 2; public double Area => Side * Side; public string Name => "sq"; public int Corners() => 4; }
public class Circle : IShape { public double Area { get; set; } = 3; public string Name { get; } = "circle"; public string Describe() => "round " + Name; }
public interface INamed { string Title { get; set; } }
public class Doc : INamed { public string Title { get; set; } = "a"; }

public interface IRepo<T> { void Put(T item); T Get(int i); int Count { get; } }
public class IntRepo : IRepo<int> { List<int> _l = new List<int>(); public void Put(int x) => _l.Add(x); public int Get(int i) => _l[i]; public int Count => _l.Count; }
public class StrRepo : IRepo<string> { List<string> _l = new List<string>(); public void Put(string x) => _l.Add(x); public string Get(int i) => _l[i]; public int Count => _l.Count; }
public abstract class Box<T> { public T Value; public Box(T v) { Value = v; } public abstract string Show(); public T Get() => Value; }
public class IntBox : Box<int> { public IntBox(int v) : base(v) { } public override string Show() => "box" + Value; }
public class StrBox : Box<string> { public StrBox(string v) : base(v) { } public override string Show() => "s:" + Value; }

public struct Vec { public int X, Y; public int Len2 => X * X + Y * Y; public bool Longer(Vec o) => Len2 > o.Len2; }
public static class Cfg { public static readonly string Title; public static readonly int Max; static Cfg() { Title = "T" + 1; Max = 3 * 4; } }
public class Timer2 { public DateTime Start = new DateTime(2024, 5, 1); public TimeSpan Limit = TimeSpan.FromMinutes(2); public DateTime End => Start + Limit; }

public class TypesAndDatesApp : SwitchApp
{
    static int Total(IRepo<int> r) { int s = 0; for (int i = 0; i < r.Count; i++) s += r.Get(i); return s; }

    public override void OnInit()
    {
        foreach (var s in new List<IShape> { new Sq(), new Circle() })
            Console.WriteLine(s.Name + " " + s.Area + " [" + s.Describe() + "] " + s.Corners()); // sq 4 [sq 4] 4 / circle 3 [round circle] 0
        INamed n = new Doc();
        n.Title = "b";
        Console.WriteLine("title " + n.Title);                           // title b

        IRepo<int> r = new IntRepo(); r.Put(4); r.Put(5);
        IRepo<string> sr = new StrRepo(); sr.Put("x");
        Console.WriteLine("repo " + r.Count + " " + r.Get(1) + " " + Total(r) + " " + sr.Get(0)); // repo 2 5 9 x
        Box<int> b = new IntBox(7);
        var boxes = new List<Box<int>> { b, new IntBox(2) };
        Console.WriteLine("box " + b.Show() + " " + b.Value + " " + b.Get() + " " + new StrBox("q").Show() + " " + boxes[1].Show()); // box box7 7 7 s:q box2

        var v = new Vec { X = 3, Y = 4 };
        Console.WriteLine("vec " + v.Len2 + " " + v.Longer(new Vec { X = 1, Y = 1 }) + " " + Cfg.Title + " " + Cfg.Max); // vec 25 True T1 12
        Console.WriteLine("num " + float.MinValue + " " + double.Epsilon + " " + Math.PI + " " + (double.MinValue < 0) + " " + Int32.MaxValue); // num -3.4028235E+38 5E-324 3.141592653589793 True 2147483647

        var d = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        Console.WriteLine("dict " + string.Join(",", d.Select(kv => kv.Key + "=" + kv.Value)) + " " + d.Values.Sum() + " "
            + string.Join("", d.Keys) + " " + d.Where(kv => kv.Value > 1).Count()); // dict a=1,b=2 3 ab 1
        var a = new List<int> { 1, 2, 3 };
        Console.WriteLine("linq " + string.Join(",", a.Zip(a, (x, y) => x * y)) + " " + string.Join(",", a.SelectMany(x => new List<int> { x, x }))
            + " " + a.Aggregate(0, (acc, x) => acc + x) + " " + new List<Func<int>> { () => 1, () => 2 }.Sum(f => f())); // linq 1,4,9 1,1,2,2,3,3 6 3

        var dt = new DateTime(2024, 2, 28).AddDays(2);
        Console.WriteLine("date " + dt.ToString("yyyy-MM-dd") + " " + dt.DayOfWeek + " " + dt.DayOfYear); // date 2024-03-01 Friday 61
        var t = new DateTime(2023, 12, 31, 23, 59, 30);
        Console.WriteLine("add " + t.AddSeconds(45).ToString("yyyy-MM-dd HH:mm:ss") + " " + t.AddMonths(2).ToString("d") + " "
            + new DateTime(2024, 1, 31).AddMonths(1).Day + " " + DateTime.IsLeapYear(2100) + " " + DateTime.DaysInMonth(2024, 2)); // add 2024-01-01 00:00:15 02/29/2024 29 False 29
        TimeSpan span = new DateTime(2024, 3, 1) - new DateTime(2024, 2, 1);
        Console.WriteLine("span " + span.TotalDays + " " + span + " " + (span > TimeSpan.FromDays(28)) + " " + TimeSpan.FromSeconds(90.5) + " " + new TimeSpan(1, 2, 3, 4)); // span 29 29.00:00:00 True 00:01:30.5000000 1.02:03:04
        var later = t + TimeSpan.FromHours(1);
        Console.WriteLine("fmt " + later.ToString("dddd, dd MMM yyyy h:mm tt") + " " + t.ToString("s") + " " + t + " " + t.TimeOfDay + " " + (t.DayOfWeek == DayOfWeek.Sunday)); // fmt Monday, 01 Jan 2024 12:59 AM 2023-12-31T23:59:30 12/31/2023 23:59:30 23:59:30 True
        var tm = new Timer2();
        Console.WriteLine("timer " + tm.End.ToString("HH:mm") + $" {tm.Start:yyyy}" + " " + (DateTime.Today <= DateTime.Now)); // timer 00:02 2024 True
        try { var bad = new DateTime(2023, 2, 29); Console.WriteLine("no " + bad.Day); }
        catch (ArgumentOutOfRangeException) { Console.WriteLine("range"); } // range

        File.WriteAllText("sdmc:/cs2sx_types.txt", "a\nb\nc");
        Console.WriteLine("file " + File.ReadAllLines("sdmc:/cs2sx_types.txt").Length + " " + (Directory.GetFiles("sdmc:/").Length > 0)); // file 3 True
        File.Delete("sdmc:/cs2sx_types.txt");
        Console.WriteLine("done");
    }

    public override void OnFrame() { }
}
