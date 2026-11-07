// Auto-generated from services\auddev.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Auddev
{
    public static extern Result auddevInitialize();
    public static extern void auddevExit();
    public static extern IntPtr auddevGetServiceSession();
    public static extern Result auddevListAudioDeviceName(ref AudioDeviceName DeviceNames, s32 max_names, ref s32 total_names);
    public static extern Result auddevSetAudioDeviceOutputVolume(ref AudioDeviceName DeviceName, float volume);
    public static extern Result auddevGetAudioDeviceOutputVolume(ref AudioDeviceName DeviceName, ref float volume);
    public static extern Result auddevGetActiveAudioDeviceName(ref AudioDeviceName DeviceName);
}
