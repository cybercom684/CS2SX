// Auto-generated from services\nv.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum NvServiceType
{
    NvServiceType_Auto = -1,
    NvServiceType_Application = 0,
    NvServiceType_Applet = 1,
    NvServiceType_System = 2,
    NvServiceType_Factory = 3,
}

public enum NvEventId
{
    NvEventId_Gpu_SmException_BptIntReport = 1,
    NvEventId_Gpu_SmException_BptPauseReport = 2,
    NvEventId_Gpu_ErrorNotifier = 3,
    NvEventId_CtrlGpu_ErrorEventHandle = 1,
    NvEventId_CtrlGpu_Unknown = 2,
}

public static class Nv
{
    public static extern Result nvInitialize();
    public static extern void nvExit();
    public static extern IntPtr nvGetServiceSession();
    public static extern Result nvOpen(ref u32 fd, ref byte devicepath);
    public static extern Result nvIoctl(u32 fd, u32 request, IntPtr argp);
    public static extern Result nvIoctl2(u32 fd, u32 request, IntPtr argp, IntPtr inbuf, ulong inbuf_size);
    public static extern Result nvIoctl3(u32 fd, u32 request, IntPtr argp, IntPtr outbuf, ulong outbuf_size);
    public static extern Result nvClose(u32 fd);
    public static extern Result nvQueryEvent(u32 fd, u32 event_id, ref Event event_out);
    public static extern Result nvConvertError(int rc);
}
