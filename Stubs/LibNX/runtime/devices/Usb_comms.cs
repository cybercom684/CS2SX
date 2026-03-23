// Auto-generated from runtime\devices\usb_comms.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Runtime.Devices;

public unsafe struct UsbCommsInterfaceInfo
{
    public u8 bInterfaceClass;
    public u8 bInterfaceSubClass;
    public u8 bInterfaceProtocol;
}

public static class Usb_comms
{
    public static extern Result usbCommsInitialize();
    public static extern Result usbCommsInitializeEx(u32 num_interfaces, ref UsbCommsInterfaceInfo infos, u16 idVendor, u16 idProduct);
    public static extern void usbCommsExit();
    public static extern void usbCommsSetErrorHandling(bool flag);
    public static extern ulong usbCommsRead(IntPtr buffer, ulong size);
    public static extern ulong usbCommsWrite(IntPtr buffer, ulong size);
    public static extern ulong usbCommsReadEx(IntPtr buffer, ulong size, u32 @interface);
    public static extern ulong usbCommsWriteEx(IntPtr buffer, ulong size, u32 @interface);
    public static extern Result usbCommsReadAsync(IntPtr buffer, ulong size, ref u32 urbId, u32 @interface);
    public static extern Result usbCommsGetReadResult(u32 urbId, ref u32 transferredSize, u32 @interface);
    public static extern Result usbCommsWriteAsync(IntPtr buffer, ulong size, ref u32 urbId, u32 @interface);
    public static extern Result usbCommsGetWriteResult(u32 urbId, ref u32 transferredSize, u32 @interface);
}
