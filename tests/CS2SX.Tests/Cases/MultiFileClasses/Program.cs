using System.Collections.Generic;
using System.Diagnostics;

// Regression case for multi-file apps (found while building samples/LabDemo):
// cross-file base classes, List<Base>.Add(derived), const fields of other user
// classes, Stopwatch fields, and indexing fixed-size arrays of SDK structs.
public class MultiFileClassesApp : SwitchApp
{
    private readonly List<Shape> _shapes = new List<Shape>();
    private readonly Painter _painter = new Painter();
    private readonly Stopwatch _timer = new Stopwatch();
    private Stopwatch? _frameTimer;

    public override void OnInit()
    {
        _shapes.Add(new Square(2f));
        _shapes.Add(new Square(3f));
        _timer.Start();
        _frameTimer = Stopwatch.StartNew();
    }

    public override void OnFrame()
    {
        float total = 0;
        foreach (var s in _shapes) total += s.Area(_painter, 1f);

        TouchState t = Input.GetTouch();
        int x = t.count > 0 ? t.x[0] : -1;

        double ms = _frameTimer!.Elapsed.TotalMilliseconds;
        _frameTimer.Restart();
        Console.WriteLine($"{total} {Shape.MaxSides} {x} {_timer.ElapsedMilliseconds} {ms > 0}");
    }
}
