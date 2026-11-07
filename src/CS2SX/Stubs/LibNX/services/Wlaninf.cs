// Auto-generated from services\wlaninf.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum WlanInfState
{
    WlanInfState_NotConnected = 1,
    WlanInfState_Connecting,
    WlanInfState_Connected,
}

public static class Wlaninf
{
    public static extern Result wlaninfInitialize();
    public static extern void wlaninfExit();
    public static extern IntPtr wlaninfGetServiceSession();
    public static extern Result wlaninfGetState(ref WlanInfState @out);
    public static extern Result wlaninfGetRSSI(ref s32 @out);
}
