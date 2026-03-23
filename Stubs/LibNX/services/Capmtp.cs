// Auto-generated from services\capmtp.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Capmtp
{
    public static extern Result capmtpInitialize(IntPtr mem, ulong size, u32 app_count, u32 max_img, u32 max_vid, ref byte other_name);
    public static extern void capmtpExit();
    public static extern IntPtr capmtpGetRootServiceSession();
    public static extern IntPtr capmtpGetServiceSession();
    public static extern Result capmtpStartCommandHandler();
    public static extern Result capmtpStopCommandHandler();
    public static extern bool capmtpIsRunning();
    public static extern bool capmtpIsConnected();
    public static extern Result capmtpGetScanError();
}
