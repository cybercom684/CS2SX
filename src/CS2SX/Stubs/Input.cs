// CS2SX Stub — wird nicht transpiliert
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

public struct StickPos
{
    public int x;
    public int y;
}

/// Touch-Zustand: bis zu 10 simultane Berührungen, x[i]/y[i] in Pixel (0..1280 / 0..720).
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