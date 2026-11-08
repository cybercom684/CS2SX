public class Game
{
    public int[] Data = new int[2];
    public int Read(int i) => Data[i];
    public void Step(int i)
    {
        var v = Read(i);
        System.Console.WriteLine("v " + v);
    }
}

// Exceptions carry the C# call trace (file and line per method).
public class StackTracesApp : SwitchApp
{
    public override void OnInit()
    {
        var g = new Game();
        try
        {
            g.Step(5);
        }
        catch (System.Exception e)
        {
            System.Console.WriteLine(e.GetType().Name);
            System.Console.Write(e.StackTrace);
        }
        Helper();
        System.Console.WriteLine("after");
    }

    private static void Helper()
    {
        try
        {
            throw new System.InvalidOperationException("boom");
        }
        catch (System.InvalidOperationException e)
        {
            System.Console.Write(e.StackTrace);
        }
    }
}
