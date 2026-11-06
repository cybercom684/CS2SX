// Auto-generated from services\aud.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Aud
{
    public static extern Result audaInitialize();
    public static extern void audaExit();
    public static extern Result auddInitialize();
    public static extern void auddExit();
    public static extern IntPtr audaGetServiceSession();
    public static extern IntPtr auddGetServiceSession();
    public static extern Result audaRequestSuspendAudio(u64 pid, u64 delay);
    public static extern Result audaRequestResumeAudio(u64 pid, u64 delay);
    public static extern Result audaGetAudioOutputProcessMasterVolume(u64 pid, ref float volume_out);
    public static extern Result audaSetAudioOutputProcessMasterVolume(u64 pid, u64 delay, float volume);
    public static extern Result audaGetAudioInputProcessMasterVolume(u64 pid, ref float volume_out);
    public static extern Result audaSetAudioInputProcessMasterVolume(u64 pid, u64 delay, float volume);
    public static extern Result audaGetAudioOutputProcessRecordVolume(u64 pid, ref float volume_out);
    public static extern Result audaSetAudioOutputProcessRecordVolume(u64 pid, u64 delay, float volume);
    public static extern Result auddRequestSuspendAudioForDebug(u64 pid, u64 delay);
    public static extern Result auddRequestResumeAudioForDebug(u64 pid, u64 delay);
}
