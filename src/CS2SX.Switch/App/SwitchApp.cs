namespace CS2SX.Switch;

/// <summary>
/// Base class of every CS2SX application. Derive from it and override the
/// lifecycle methods; the runtime drives the main loop at 60 fps.
/// </summary>
/// <example>
/// <code>
/// public class MyApp : SwitchApp
/// {
///     public override void OnInit()  { Console.WriteLine("Hello"); }
///     public override void OnFrame() { if (Input.IsDown(NpadButton.A)) App.Quit(); }
/// }
/// </code>
/// </example>
public abstract class SwitchApp
{
    /// <summary>The console-mode UI form; controls added here are updated and drawn every frame.</summary>
    public Form Form { get; } = new Form();

    /// <summary>Buttons pressed this frame (raw HidNpadButton bit mask).</summary>
    public ulong kDown;

    /// <summary>Buttons held this frame (raw HidNpadButton bit mask).</summary>
    public ulong kHeld;

    /// <summary>Called once after the runtime has initialised graphics, input and services.</summary>
    public virtual void OnInit() { }

    /// <summary>Called once per frame (60 fps).</summary>
    public virtual void OnFrame() { }

    /// <summary>Called once when the main loop ends (Plus pressed or <see cref="App.Quit"/>).</summary>
    public virtual void OnExit() { }

    /// <summary>Adds a control to <see cref="Form"/>.</summary>
    public void Add(Control control) => Form.Add(control);
}

/// <summary>
/// <see cref="SwitchApp"/> with input/system state the runtime refreshes each frame
/// (sticks, touch, battery).
/// </summary>
public abstract class SwitchAppEx : SwitchApp
{
    public StickPos _stickL;
    public StickPos _stickR;
    public TouchState _touch;
    public BatteryInfo _battery;
}

/// <summary>Application control.</summary>
public static class App
{
    /// <summary>Ends the main loop after the current frame; <see cref="SwitchApp.OnExit"/> still runs.</summary>
    public static void Quit() { }

    /// <summary>
    /// Queues another homebrew <c>.nro</c> to be started after this app exits (via the
    /// homebrew loader), then quits. Without hbloader support it just quits.
    /// </summary>
    public static void LaunchNro(string path) { }
}
