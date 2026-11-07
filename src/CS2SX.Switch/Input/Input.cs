namespace CS2SX.Switch;

/// <summary>Controller buttons for <see cref="Input.IsDown"/> / IsHeld / IsUp.</summary>
public enum NpadButton
{
    A, B, X, Y,
    L, R, ZL, ZR,
    Plus, Minus,
    Up, Down, Left, Right,
    StickL, StickR,
    StickLUp, StickLDown, StickLLeft, StickLRight,
    StickRUp, StickRDown, StickRLeft, StickRRight,
}

/// <summary>Analog stick position, -32767..32767 per axis (deadzone already applied).</summary>
public struct StickPos
{
    public int x;
    public int y;

    /// <summary>|x| scaled to 0..100.</summary>
    public int NormX => 0;
    /// <summary>|y| scaled to 0..100.</summary>
    public int NormY => 0;
}

/// <summary>Touch state: up to 10 simultaneous touches, x[i]/y[i] in pixels (0..1280 / 0..720). Handheld mode only.</summary>
public struct TouchState
{
    public int count;
    public int[] x;
    public int[] y;
    public uint[] id;

    /// Gibt true zurück wenn Finger idx innerhalb von (rx, ry, rw, rh) liegt.
    public bool HitRect(int idx, int rx, int ry, int rw, int rh) => false;

    /// Erster Finger getippt?
    public bool IsTouched => count > 0;

    /// Koordinate des ersten Fingers (0/0 wenn kein Touch).
    public int X0 => count > 0 ? x[0] : 0;
    public int Y0 => count > 0 ? y[0] : 0;
}

public static class Input
{
    public static bool IsDown(NpadButton button) => false;
    public static bool IsHeld(NpadButton button) => false;
    public static bool IsUp(NpadButton button) => false;

    /// Aktueller Touch-Zustand (bis zu 10 simultane Berührungen).
    public static TouchState GetTouch() => new TouchState();

    /// Linker Analogstick.
    public static StickPos GetStickLeft() => new StickPos();

    /// Rechter Analogstick.
    public static StickPos GetStickRight() => new StickPos();
}
