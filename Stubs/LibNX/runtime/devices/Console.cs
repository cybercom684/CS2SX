// Auto-generated from runtime\devices\console.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Runtime.Devices;

public enum debugDevice
{
    debugDevice_NULL,
    debugDevice_SVC,
    debugDevice_CONSOLE,
}

public unsafe struct ConsoleRenderer
{
    public IntPtr con);
    public IntPtr con);
    public IntPtr c);
    public IntPtr con);
    public IntPtr con);
}

public unsafe struct ConsoleFont
{
    public IntPtr gfx;
    public u16 asciiOffset;
    public u16 numChars;
    public u16 tileWidth;
    public u16 tileHeight;
}

public static class Console
{
    public static extern void consoleSetFont(ref PrintConsole console, ref ConsoleFont font);
    public static extern void consoleSetWindow(ref PrintConsole console, int x, int y, int width, int height);
    public static extern IntPtr consoleGetDefault();
    public static extern IntPtr consoleInit(ref PrintConsole console);
    public static extern void consoleExit(ref PrintConsole console);
    public static extern void consoleUpdate(ref PrintConsole console);
    public static extern void consoleDebugInit(debugDevice device);
    public static extern void consoleClear();
}
