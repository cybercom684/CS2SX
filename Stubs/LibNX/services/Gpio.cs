// Auto-generated from services\gpio.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum GpioPadName
{
    GpioPadName_AudioCodec = 1,
    GpioPadName_ButtonVolUp = 25,
    GpioPadName_ButtonVolDown = 26,
    GpioPadName_SdCd = 56,
}

public enum GpioDirection
{
    GpioDirection_Input = 0,
    GpioDirection_Output = 1,
}

public enum GpioValue
{
    GpioValue_Low = 0,
    GpioValue_High = 1,
}

public enum GpioInterruptMode
{
    GpioInterruptMode_LowLevel = 0,
    GpioInterruptMode_HighLevel = 1,
    GpioInterruptMode_RisingEdge = 2,
    GpioInterruptMode_FallingEdge = 3,
    GpioInterruptMode_AnyEdge = 4,
}

public enum GpioInterruptStatus
{
    GpioInterruptStatus_Inactive = 0,
    GpioInterruptStatus_Active = 1,
}

public unsafe struct GpioPadSession
{
    public Service s;
}

public static class Gpio
{
    public static extern Result gpioInitialize();
    public static extern void gpioExit();
    public static extern IntPtr gpioGetServiceSession();
    public static extern Result gpioOpenSession(ref GpioPadSession @out, GpioPadName name);
    public static extern Result gpioOpenSession2(ref GpioPadSession @out, u32 device_code, u32 access_mode);
    public static extern Result gpioIsWakeEventActive(ref bool @out, GpioPadName name);
    public static extern Result gpioIsWakeEventActive2(ref bool @out, u32 device_code);
    public static extern Result gpioPadSetDirection(ref GpioPadSession p, GpioDirection dir);
    public static extern Result gpioPadGetDirection(ref GpioPadSession p, ref GpioDirection @out);
    public static extern Result gpioPadSetInterruptMode(ref GpioPadSession p, GpioInterruptMode mode);
    public static extern Result gpioPadGetInterruptMode(ref GpioPadSession p, ref GpioInterruptMode @out);
    public static extern Result gpioPadSetInterruptEnable(ref GpioPadSession p, bool en);
    public static extern Result gpioPadGetInterruptEnable(ref GpioPadSession p, ref bool @out);
    public static extern Result gpioPadGetInterruptStatus(ref GpioPadSession p, ref GpioInterruptStatus @out);
    public static extern Result gpioPadClearInterruptStatus(ref GpioPadSession p);
    public static extern Result gpioPadSetValue(ref GpioPadSession p, GpioValue val);
    public static extern Result gpioPadGetValue(ref GpioPadSession p, ref GpioValue @out);
    public static extern Result gpioPadBindInterrupt(ref GpioPadSession p, ref Event @out);
    public static extern Result gpioPadUnbindInterrupt(ref GpioPadSession p);
    public static extern Result gpioPadSetDebounceEnabled(ref GpioPadSession p, bool en);
    public static extern Result gpioPadGetDebounceEnabled(ref GpioPadSession p, ref bool @out);
    public static extern Result gpioPadSetDebounceTime(ref GpioPadSession p, s32 ms);
    public static extern Result gpioPadGetDebounceTime(ref GpioPadSession p, ref s32 @out);
    public static extern void gpioPadClose(ref GpioPadSession p);
}
