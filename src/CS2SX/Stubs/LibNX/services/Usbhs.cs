// Auto-generated from services\usbhs.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum UsbHsInterfaceFilterFlags
{
    UsbHsInterfaceFilterFlags_idVendor = (1 << 0),
    UsbHsInterfaceFilterFlags_idProduct = (1 << 1),
    UsbHsInterfaceFilterFlags_bcdDevice_Min = (1 << 2),
    UsbHsInterfaceFilterFlags_bcdDevice_Max = (1 << 3),
    UsbHsInterfaceFilterFlags_bDeviceClass = (1 << 4),
    UsbHsInterfaceFilterFlags_bDeviceSubClass = (1 << 5),
    UsbHsInterfaceFilterFlags_bDeviceProtocol = (1 << 6),
    UsbHsInterfaceFilterFlags_bInterfaceClass = (1 << 7),
    UsbHsInterfaceFilterFlags_bInterfaceSubClass = (1 << 8),
    UsbHsInterfaceFilterFlags_bInterfaceProtocol = (1 << 9),
}

public unsafe struct UsbHsInterfaceFilter
{
    public u16 Flags;
    public u16 idVendor;
    public u16 idProduct;
    public u16 bcdDevice_Min;
    public u16 bcdDevice_Max;
    public u8 bDeviceClass;
    public u8 bDeviceSubClass;
    public u8 bDeviceProtocol;
    public u8 bInterfaceClass;
    public u8 bInterfaceSubClass;
    public u8 bInterfaceProtocol;
}

public unsafe struct UsbHsXferReport
{
    public u32 xferId;
    public Result res;
    public u32 requestedSize;
    public u32 transferredSize;
    public u64 id;
}

public unsafe struct UsbHsRingHeader
{
    public vu64 write_index;
    public vu64 read_index;
}

public unsafe struct UsbHsClientIfSession
{
    public Service s;
    public Event event0;
    public Event eventCtrlXfer;
    public s32 ID;
    public UsbHsInterface inf;
}

public unsafe struct UsbHsClientEpSession
{
    public Service s;
    public Event eventXfer;
    public u32 maxUrbCount;
    public u64 max_reports;
    public IntPtr ringbuf;
}

public static class Usbhs
{
    public static extern Result usbHsInitialize();
    public static extern void usbHsExit();
    public static extern IntPtr usbHsGetServiceSession();
    public static extern IntPtr usbHsGetInterfaceStateChangeEvent();
    public static extern Result usbHsQueryAllInterfaces(ref UsbHsInterfaceFilter filter, ref UsbHsInterface interfaces, ulong interfaces_maxsize, ref s32 total_entries);
    public static extern Result usbHsQueryAvailableInterfaces(ref UsbHsInterfaceFilter filter, ref UsbHsInterface interfaces, ulong interfaces_maxsize, ref s32 total_entries);
    public static extern Result usbHsQueryAcquiredInterfaces(ref UsbHsInterface interfaces, ulong interfaces_maxsize, ref s32 total_entries);
    public static extern Result usbHsCreateInterfaceAvailableEvent(ref Event out_event, bool autoclear, u8 index, ref UsbHsInterfaceFilter filter);
    public static extern Result usbHsDestroyInterfaceAvailableEvent(ref Event @event, u8 index);
    public static extern Result usbHsAcquireUsbIf(ref UsbHsClientIfSession s, ref UsbHsInterface @interface);
    public static extern void usbHsIfClose(ref UsbHsClientIfSession s);
    public static extern Result usbHsIfSetInterface(ref UsbHsClientIfSession s, ref UsbHsInterfaceInfo inf, u8 id);
    public static extern Result usbHsIfGetInterface(ref UsbHsClientIfSession s, ref UsbHsInterfaceInfo inf);
    public static extern Result usbHsIfGetAlternateInterface(ref UsbHsClientIfSession s, ref UsbHsInterfaceInfo inf, u8 id);
    public static extern Result usbHsIfGetCurrentFrame(ref UsbHsClientIfSession s, ref u32 @out);
    public static extern Result usbHsIfCtrlXfer(ref UsbHsClientIfSession s, u8 bmRequestType, u8 bRequest, u16 wValue, u16 wIndex, u16 wLength, IntPtr buffer, ref u32 transferredSize);
    public static extern Result usbHsIfOpenUsbEp(ref UsbHsClientIfSession s, ref UsbHsClientEpSession ep, u16 maxUrbCount, u32 maxXferSize, ref struct usb_endpoint_descriptor desc);
    public static extern Result usbHsIfResetDevice(ref UsbHsClientIfSession s);
    public static extern void usbHsEpClose(ref UsbHsClientEpSession s);
    public static extern Result usbHsEpPostBufferAsync(ref UsbHsClientEpSession s, IntPtr buffer, u32 size, u64 id, ref u32 xferId);
    public static extern Result usbHsEpGetXferReport(ref UsbHsClientEpSession s, ref UsbHsXferReport reports, u32 max_reports, ref u32 count);
    public static extern Result usbHsEpPostBuffer(ref UsbHsClientEpSession s, IntPtr buffer, u32 size, ref u32 transferredSize);
    public static extern Result usbHsEpBatchBufferAsync(ref UsbHsClientEpSession s, IntPtr buffer, ref u32 urbs, u32 urbCount, u64 id, u32 unk1, u32 unk2, ref u32 xferId);
    public static extern Result usbHsEpCreateSmmuSpace(ref UsbHsClientEpSession s, IntPtr buffer, u32 size);
    public static extern Result usbHsEpShareReportRing(ref UsbHsClientEpSession s, IntPtr buffer, ulong size);
}
