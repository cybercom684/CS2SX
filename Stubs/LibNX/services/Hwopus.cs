// Auto-generated from services\hwopus.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct HwopusDecoder
{
    public Service s;
    public TransferMemory tmem;
    public bool multistream;
}

public unsafe struct HwopusHeader
{
    public u32 size;
    public u32 final_range;
}

public unsafe struct HwopusMultistreamState
{
    public s32 SampleRate;
    public s32 ChannelCount;
    public s32 TotalStreamCount;
    public s32 StereoStreamCount;
    public fixed u8 channel_mapping[256];
}

public static class Hwopus
{
    public static extern Result hwopusDecoderInitialize(ref HwopusDecoder decoder, s32 SampleRate, s32 ChannelCount);
    public static extern void hwopusDecoderExit(ref HwopusDecoder decoder);
    public static extern Result hwopusDecoderMultistreamInitialize(ref HwopusDecoder decoder, s32 SampleRate, s32 ChannelCount, s32 TotalStreamCount, s32 StereoStreamCount, ref u8 channel_mapping);
    public static extern Result hwopusDecodeInterleaved(ref HwopusDecoder decoder, ref s32 DecodedDataSize, ref s32 DecodedSampleCount, IntPtr opusin, ulong opusin_size, ref s16 pcmbuf, ulong pcmbuf_size);
}
