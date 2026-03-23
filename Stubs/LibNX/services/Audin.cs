// Auto-generated from services\audin.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum AudioInState
{
    AudioInState_Started = 0,
    AudioInState_Stopped = 1,
}

public static class Audin
{
    public static extern Result audinInitialize();
    public static extern void audinExit();
    public static extern IntPtr audinGetServiceSession();
    public static extern IntPtr audinGetServiceSession_AudioIn();
    public static extern Result audinListAudioIns(ref byte DeviceNames, s32 count, ref u32 DeviceNamesCount);
    public static extern Result audinOpenAudioIn(ref byte DeviceNameIn, ref byte DeviceNameOut, u32 SampleRateIn, u32 ChannelCountIn, ref u32 SampleRateOut, ref u32 ChannelCountOut, ref PcmFormat Format, ref AudioInState State);
    public static extern Result audinGetAudioInState(ref AudioInState State);
    public static extern Result audinStartAudioIn();
    public static extern Result audinStopAudioIn();
    public static extern Result audinAppendAudioInBuffer(ref AudioInBuffer Buffer);
    public static extern Result audinGetReleasedAudioInBuffer(ref AudioInBuffer Buffer, ref u32 ReleasedBuffersCount);
    public static extern Result audinContainsAudioInBuffer(ref AudioInBuffer Buffer, ref bool ContainsBuffer);
    public static extern Result audinCaptureBuffer(ref AudioInBuffer source, ref AudioInBuffer released);
    public static extern Result audinWaitCaptureFinish(ref AudioInBuffer released, ref u32 released_count, u64 timeout);
    public static extern u32 audinGetSampleRate();
    public static extern u32 audinGetChannelCount();
    public static extern PcmFormat audinGetPcmFormat();
    public static extern AudioInState audinGetDeviceState();
}
