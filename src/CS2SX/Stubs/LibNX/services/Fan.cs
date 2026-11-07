// Auto-generated from services\fan.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct FanController
{
    public Service s;
}

public static class Fan
{
    public static extern Result fanInitialize();
    public static extern void fanExit();
    public static extern IntPtr fanGetServiceSession();
    public static extern Result fanOpenController(ref FanController @out, u32 device_code);
    public static extern void fanControllerClose(ref FanController controller);
    public static extern Result fanControllerSetRotationSpeedLevel(ref FanController controller, float level);
    public static extern Result fanControllerGetRotationSpeedLevel(ref FanController controller, ref float level);
}
