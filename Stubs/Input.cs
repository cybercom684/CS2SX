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

public static class Input
{
    public static bool IsDown(NpadButton button) => false;
    public static bool IsHeld(NpadButton button) => false;
    public static bool IsUp(NpadButton button) => false;
}