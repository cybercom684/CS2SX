// Auto-generated from services\uart.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum UartPort
{
    UartPort_Bluetooth = 1,
    UartPort_JoyConR = 2,
    UartPort_JoyConL = 3,
    UartPort_MCU = 4,
}

public enum UartPortForDev
{
    UartPortForDev_JoyConR = 1,
    UartPortForDev_JoyConL = 2,
    UartPortForDev_Bluetooth = 3,
}

public enum UartFlowControlMode
{
    UartFlowControlMode_None = 0,
    UartFlowControlMode_Hardware = 1,
}

public enum UartPortEventType
{
    UartPortEventType_SendBufferEmpty = 0,
    UartPortEventType_SendBufferReady = 1,
    UartPortEventType_ReceiveBufferReady = 2,
    UartPortEventType_ReceiveEnd = 3,
}

public unsafe struct UartPortSession
{
    public Service s;
}

public static class Uart
{
    public static extern Result uartInitialize();
    public static extern void uartExit();
    public static extern IntPtr uartGetServiceSession();
    public static extern Result uartHasPort(UartPort port, ref bool @out);
    public static extern Result uartHasPortForDev(UartPortForDev port, ref bool @out);
    public static extern Result uartIsSupportedBaudRate(UartPort port, u32 baud_rate, ref bool @out);
    public static extern Result uartIsSupportedBaudRateForDev(UartPortForDev port, u32 baud_rate, ref bool @out);
    public static extern Result uartIsSupportedFlowControlMode(UartPort port, UartFlowControlMode flow_control_mode, ref bool @out);
    public static extern Result uartIsSupportedFlowControlModeForDev(UartPortForDev port, UartFlowControlMode flow_control_mode, ref bool @out);
    public static extern Result uartCreatePortSession(ref UartPortSession s);
    public static extern Result uartIsSupportedPortEvent(UartPort port, UartPortEventType port_event_type, ref bool @out);
    public static extern Result uartIsSupportedPortEventForDev(UartPortForDev port, UartPortEventType port_event_type, ref bool @out);
    public static extern Result uartIsSupportedDeviceVariation(UartPort port, u32 device_variation, ref bool @out);
    public static extern Result uartIsSupportedDeviceVariationForDev(UartPortForDev port, u32 device_variation, ref bool @out);
    public static extern void uartPortSessionClose(ref UartPortSession s);
    public static extern Result uartPortSessionOpenPort(ref UartPortSession s, ref bool @out, UartPort port, u32 baud_rate, UartFlowControlMode flow_control_mode, u32 device_variation, bool is_invert_tx, bool is_invert_rx, bool is_invert_rts, bool is_invert_cts, IntPtr send_buffer, u64 send_buffer_length, IntPtr receive_buffer, u64 receive_buffer_length);
    public static extern Result uartPortSessionOpenPortForDev(ref UartPortSession s, ref bool @out, UartPortForDev port, u32 baud_rate, UartFlowControlMode flow_control_mode, u32 device_variation, bool is_invert_tx, bool is_invert_rx, bool is_invert_rts, bool is_invert_cts, IntPtr send_buffer, u64 send_buffer_length, IntPtr receive_buffer, u64 receive_buffer_length);
    public static extern Result uartPortSessionGetWritableLength(ref UartPortSession s, ref u64 @out);
    public static extern Result uartPortSessionSend(ref UartPortSession s, IntPtr in_data, ulong size, ref u64 @out);
    public static extern Result uartPortSessionGetReadableLength(ref UartPortSession s, ref u64 @out);
    public static extern Result uartPortSessionReceive(ref UartPortSession s, IntPtr out_data, ulong size, ref u64 @out);
    public static extern Result uartPortSessionBindPortEvent(ref UartPortSession s, UartPortEventType port_event_type, s64 threshold, ref bool @out, ref Event out_event);
    public static extern Result uartPortSessionUnbindPortEvent(ref UartPortSession s, UartPortEventType port_event_type, ref bool @out);
}
