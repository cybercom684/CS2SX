# CS2SX.Switch

The Nintendo Switch API of [CS2SX](https://github.com/cybercom684/CS2SX): write Switch homebrew in C#, and `dotnet build` produces an `.nro`.

An app project references this package and derives from `SwitchApp`:

```csharp
public class MyApp : SwitchApp
{
    public override void OnInit() { Graphics.Init(1280, 720); }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.RGB(20, 24, 36));
        Graphics.DrawText(40, 40, "Hello Switch!", Color.RGB(255, 255, 255), 4);
    }
}
```

The package contains the API (graphics, input for several players, audio, files, SaveData, HTTP, sensors, forms) and the MSBuild integration that runs the `cs2sx` transpiler on `dotnet build`. Its method bodies never run: cs2sx translates the app to C and builds it with devkitPro (libnx).

Getting started, the language guide and the API reference: https://github.com/cybercom684/CS2SX/blob/master/docs/index.md
