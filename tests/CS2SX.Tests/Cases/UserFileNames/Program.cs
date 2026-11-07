public class UserFileNamesApp : SwitchApp
{
    private readonly InputMapper _input = new InputMapper();
    private readonly MenuEntry _entry = new MenuEntry("Start");

    public override void OnFrame()
    {
        _input.Poll();
        Console.WriteLine(_entry.Title + " " + _input.Presses);
    }
}
