// Auto-generated from services\audout.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum AudioOutState
{
    AudioOutState_Started = 0,
    AudioOutState_Stopped = 1,
}

public static class Audout
{
    public static extern Result audoutInitialize();
    public static extern void audoutExit();
    public static extern Result audoutaInitialize();
    public static extern void audoutaExit();
    public static extern Result audoutdInitialize();
    public static extern void audoutdExit();
    public static extern IntPtr audoutGetServiceSession();
    public static extern IntPtr audoutGetServiceSession_AudioOut();
    public static extern IntPtr audoutaGetServiceSession();
    public static extern IntPtr audoutdGetServiceSession();
    public static extern Result audoutListAudioOuts(ref byte DeviceNames, s32 count, ref u32 DeviceNamesCount);
    public static extern Result audoutOpenAudioOut(ref byte DeviceNameIn, ref byte DeviceNameOut, u32 SampleRateIn, u32 ChannelCountIn, ref u32 SampleRateOut, ref u32 ChannelCountOut, ref PcmFormat Format, ref AudioOutState State);
    public static extern Result audoutGetAudioOutState(ref AudioOutState State);
    public static extern Result audoutStartAudioOut();
    public static extern Result audoutStopAudioOut();
    public static extern Result audoutAppendAudioOutBuffer(ref AudioOutBuffer Buffer);
    public static extern Result audoutGetReleasedAudioOutBuffer(ref AudioOutBuffer Buffer, ref u32 ReleasedBuffersCount);
    public static extern Result audoutContainsAudioOutBuffer(ref AudioOutBuffer Buffer, ref bool ContainsBuffer);
    public static extern Result audoutGetAudioOutBufferCount(ref u32 count);
    public static extern Result audoutGetAudioOutPlayedSampleCount(ref u64 count);
    public static extern Result audoutFlushAudioOutBuffers(ref bool flushed);
    public static extern Result audoutSetAudioOutVolume(float volume);
    public static extern Result audoutGetAudioOutVolume(ref float volume);
    public static extern Result audoutPlayBuffer(ref AudioOutBuffer source, ref AudioOutBuffer released);
    public static extern Result audoutWaitPlayFinish(ref AudioOutBuffer released, ref u32 released_count, u64 timeout);
    public static extern u32 audoutGetSampleRate();
    public static extern u32 audoutGetChannelCount();
    public static extern PcmFormat audoutGetPcmFormat();
    public static extern AudioOutState audoutGetDeviceState();
    public static extern Result audoutaRequestSuspend(u64 pid, u64 delay);
    public static extern Result audoutaRequestResume(u64 pid, u64 delay);
    public static extern Result audoutaGetProcessMasterVolume(u64 pid, ref float volume_out);
    public static extern Result audoutaSetProcessMasterVolume(u64 pid, u64 delay, float volume);
    public static extern Result audoutaGetProcessRecordVolume(u64 pid, ref float volume_out);
    public static extern Result audoutaSetProcessRecordVolume(u64 pid, u64 delay, float volume);
    public static extern Result audoutdRequestSuspendForDebug(u64 pid, u64 delay);
    public static extern Result audoutdRequestResumeForDebug(u64 pid, u64 delay);
}
