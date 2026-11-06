public enum State { Menu, Playing, Paused, GameOver }

public struct Point
{
    public int X;
    public int Y;
    public Point(int x, int y) { X = x; Y = y; }
    public int ManhattanLength() => (X < 0 ? -X : X) + (Y < 0 ? -Y : Y);
}

public record Score(string Player, int Points);

public class StructsRecordsEnumsApp : SwitchApp
{
    private State _state = State.Menu;

    public override void OnInit()
    {
        var p = new Point(3, -4);
        var q = p;
        q.X = 10;
        Console.WriteLine(p.X + " " + q.X + " " + p.ManhattanLength());

        var s = new Score("anna", 500);
        Console.WriteLine(s.Player + " " + s.Points);

        _state = State.Playing;
        switch (_state)
        {
            case State.Menu: Console.WriteLine("menu"); break;
            case State.Playing: Console.WriteLine("playing"); break;
            default: Console.WriteLine("other"); break;
        }

        string label = _state switch
        {
            State.Paused => "paused",
            State.GameOver => "over",
            _ => "running",
        };
        Console.WriteLine(label + " " + (int)_state);

        object o = 5;
        if (o is int n && n > 3)
            Console.WriteLine("pattern matched " + n);
    }
}
