// Auto-generated from services\usbds.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum UsbComplexId
{
    UsbComplexId_Default = 0x2,
}

public enum UsbDeviceSpeed
{
    UsbDeviceSpeed_None = 0x0,
    UsbDeviceSpeed_Low = 0x1,
    UsbDeviceSpeed_Full = 0x2,
    UsbDeviceSpeed_High = 0x3,
    UsbDeviceSpeed_Super = 0x4,
}

public unsafe struct UsbDsInterface
{
    public bool initialized;
    public u8 interface_index;
    public Service s;
    public Event SetupEvent;
    public Event CtrlInCompletionEvent;
    public Event CtrlOutCompletionEvent;
}

public unsafe struct UsbDsEndpoint
{
    public bool initialized;
    public Service s;
    public Event CompletionEvent;
}

public unsafe struct UsbDsDeviceInfo
{
    public u16 idVendor;
    public u16 idProduct;
    public u16 bcdDevice;
    public byte Manufacturer;
    public byte Product;
    public byte SerialNumber;
}

public unsafe struct UsbDsReportEntry
{
    public u32 id;
    public u32 requestedSize;
    public u32 transferredSize;
    public u32 urb_status;
}

public unsafe struct UsbDsReportData
{
    // skipped array: UsbDsReportEntry report[8]
    public u32 report_count;
}

public static class Usbds
{
    public static extern Result usbDsInitialize();
    public static extern void usbDsExit();
    public static extern IntPtr usbDsGetServiceSession();
    public static extern Result usbDsWaitReady(u64 timeout);
    public static extern Result usbDsParseReportData(ref UsbDsReportData reportdata, u32 urbId, ref u32 requestedSize, ref u32 transferredSize);
    public static extern IntPtr usbDsGetStateChangeEvent();
    public static extern Result usbDsGetState(ref UsbState @out);
    public static extern Result usbDsGetDsInterface(ref UsbDsInterface @out, ref struct usb_interface_descriptor descriptor, ref byte interface_name);
    public static extern Result usbDsSetVidPidBcd(ref UsbDsDeviceInfo deviceinfo);
    public static extern Result usbDsRegisterInterface(ref UsbDsInterface @out);
    public static extern Result usbDsRegisterInterfaceEx(ref UsbDsInterface @out, u8 intf_num);
    public static extern Result usbDsClearDeviceData();
    public static extern Result usbDsAddUsbStringDescriptor(ref u8 out_index, ref byte @string);
    public static extern Result usbDsAddUsbLanguageStringDescriptor(ref u8 out_index, ref u16 lang_ids, u16 num_langs);
    public static extern Result usbDsDeleteUsbStringDescriptor(u8 index);
    public static extern Result usbDsSetUsbDeviceDescriptor(UsbDeviceSpeed speed, ref struct usb_device_descriptor descriptor);
    public static extern Result usbDsSetBinaryObjectStore(IntPtr bos, ulong bos_size);
    public static extern Result usbDsEnable();
    public static extern Result usbDsDisable();
    public static extern Result usbDsGetSpeed(ref UsbDeviceSpeed @out);
    public static extern void usbDsInterface_Close(ref UsbDsInterface @interface);
    public static extern Result usbDsInterface_GetSetupPacket(ref UsbDsInterface @interface, IntPtr buffer, ulong size);
    public static extern Result usbDsInterface_EnableInterface(ref UsbDsInterface @interface);
    public static extern Result usbDsInterface_DisableInterface(ref UsbDsInterface @interface);
    public static extern Result usbDsInterface_CtrlInPostBufferAsync(ref UsbDsInterface @interface, IntPtr buffer, ulong size, ref u32 urbId);
    public static extern Result usbDsInterface_CtrlOutPostBufferAsync(ref UsbDsInterface @interface, IntPtr buffer, ulong size, ref u32 urbId);
    public static extern Result usbDsInterface_GetCtrlInReportData(ref UsbDsInterface @interface, ref UsbDsReportData @out);
    public static extern Result usbDsInterface_GetCtrlOutReportData(ref UsbDsInterface @interface, ref UsbDsReportData @out);
    public static extern Result usbDsInterface_StallCtrl(ref UsbDsInterface @interface);
    public static extern Result usbDsInterface_GetDsEndpoint(ref UsbDsInterface @interface, ref UsbDsEndpoint endpoint, ref struct usb_endpoint_descriptor descriptor);
    public static extern Result usbDsInterface_RegisterEndpoint(ref UsbDsInterface @interface, ref UsbDsEndpoint endpoint, u8 endpoint_address);
    public static extern Result usbDsInterface_AppendConfigurationData(ref UsbDsInterface @interface, UsbDeviceSpeed speed, IntPtr buffer, ulong size);
    public static extern void usbDsEndpoint_Close(ref UsbDsEndpoint endpoint);
    public static extern Result usbDsEndpoint_Cancel(ref UsbDsEndpoint endpoint);
    public static extern Result usbDsEndpoint_PostBufferAsync(ref UsbDsEndpoint endpoint, IntPtr buffer, ulong size, ref u32 urbId);
    public static extern Result usbDsEndpoint_GetReportData(ref UsbDsEndpoint endpoint, ref UsbDsReportData @out);
    public static extern Result usbDsEndpoint_Stall(ref UsbDsEndpoint endpoint);
    public static extern Result usbDsEndpoint_SetZlt(ref UsbDsEndpoint endpoint, bool zlt);
}
