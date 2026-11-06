namespace CS2SX.Switch;

public struct BatteryInfo
{
    public int percent;
    public bool charging;
    public bool connected;
}

public struct TimeInfo
{
    public int hour;
    public int minute;
    public int second;
}

/// <summary>Console system information.</summary>
public static class NX
{
    public static BatteryInfo GetBattery() => new BatteryInfo();
    public static TimeInfo GetTime() => new TimeInfo();
}

/// <summary>Low-level helpers for manual memory management.</summary>
public static class Sys
{
    /// <summary>
    /// Frees a heap string owned by the app (e.g. a string field you replace often),
    /// typically from a finalizer. NULL-safe. Never call it on string literals.
    /// </summary>
    public static void FreeStr(string? s) { }
}
