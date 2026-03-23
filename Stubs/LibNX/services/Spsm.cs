// Auto-generated from services\spsm.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Spsm
{
    public static extern Result spsmInitialize();
    public static extern void spsmExit();
    public static extern IntPtr spsmGetServiceSession();
    public static extern Result spsmShutdown(bool reboot);
    public static extern Result spsmPutErrorState();
}
