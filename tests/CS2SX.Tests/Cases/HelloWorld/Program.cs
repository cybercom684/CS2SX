public class HelloWorldApp : SwitchApp
{
    private int _frames;

    public override void OnInit()
    {
        Console.WriteLine("Hello from CS2SX!");
        Console.WriteLine("Press + to exit.");
    }

    public override void OnFrame()
    {
        _frames++;
        if (_frames % 60 == 0)
            Console.WriteLine("seconds: " + (_frames / 60));
    }
}
