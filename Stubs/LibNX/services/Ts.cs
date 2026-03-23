// Auto-generated from services\ts.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum TsLocation
{
    TsLocation_Internal = 0,
    TsLocation_External = 1,
}

public enum TsDeviceCode
{
    TsDeviceCode_LocationInternal = 0x41000001u,
    TsDeviceCode_LocationExternal = 0x41000002u,
}

public unsafe struct TsSession
{
    public Service s;
}

public static class Ts
{
    public static extern Result tsInitialize();
    public static extern void tsExit();
    public static extern IntPtr tsGetServiceSession();
    public static extern Result tsGetTemperatureRange(TsLocation location, ref s32 min_temperature, ref s32 max_temperature);
    public static extern Result tsGetTemperature(TsLocation location, ref s32 temperature);
    public static extern Result tsGetTemperatureMilliC(TsLocation location, ref s32 temperature);
    public static extern Result tsOpenSession(ref TsSession s, u32 device_code);
    public static extern Result tsSessionGetTemperature(ref TsSession s, ref float temperature);
    public static extern void tsSessionClose(ref TsSession s);
}
