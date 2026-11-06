// Auto-generated from audio\driver.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Audio;

public enum AudioDriverWaveBufState
{
    AudioDriverWaveBufState_Free,
    AudioDriverWaveBufState_Waiting,
    AudioDriverWaveBufState_Queued,
    AudioDriverWaveBufState_Playing,
    AudioDriverWaveBufState_Done,
}

public unsafe struct AudioDriver
{
    public IntPtr etc;
    public AudioRendererConfig config;
    public IntPtr in_mempools;
    public IntPtr in_channels;
    public IntPtr in_voices;
    public IntPtr in_mixes;
    public IntPtr in_sinks;
}

public static class Driver
{
    public static extern Result audrvCreate(ref AudioDriver d, ref AudioRendererConfig config, int num_final_mix_channels);
    public static extern Result audrvUpdate(ref AudioDriver d);
    public static extern void audrvClose(ref AudioDriver d);
    public static extern int audrvMemPoolAdd(ref AudioDriver d, IntPtr buffer, ulong size);
    public static extern bool audrvMemPoolRemove(ref AudioDriver d, int id);
    public static extern bool audrvMemPoolAttach(ref AudioDriver d, int id);
    public static extern bool audrvMemPoolDetach(ref AudioDriver d, int id);
    public static extern bool audrvVoiceInit(ref AudioDriver d, int id, int num_channels, PcmFormat format, int sample_rate);
    public static extern void audrvVoiceDrop(ref AudioDriver d, int id);
    public static extern void audrvVoiceStop(ref AudioDriver d, int id);
    public static extern bool audrvVoiceIsPaused(ref AudioDriver d, int id);
    public static extern bool audrvVoiceIsPlaying(ref AudioDriver d, int id);
    public static extern bool audrvVoiceAddWaveBuf(ref AudioDriver d, int id, ref AudioDriverWaveBuf wavebuf);
    public static extern u32 audrvVoiceGetWaveBufSeq(ref AudioDriver d, int id);
    public static extern u32 audrvVoiceGetPlayedSampleCount(ref AudioDriver d, int id);
    public static extern u32 audrvVoiceGetVoiceDropsCount(ref AudioDriver d, int id);
    public static extern void audrvVoiceSetBiquadFilter(ref AudioDriver d, int id, int biquad_id, float a0, float a1, float a2, float b0, float b1, float b2);
    public static extern int audrvMixAdd(ref AudioDriver d, int sample_rate, int num_channels);
    public static extern void audrvMixRemove(ref AudioDriver d, int id);
    public static extern int audrvDeviceSinkAdd(ref AudioDriver d, ref byte device_name, int num_channels, ref u8 channel_ids);
    public static extern void audrvSinkRemove(ref AudioDriver d, int id);
}
