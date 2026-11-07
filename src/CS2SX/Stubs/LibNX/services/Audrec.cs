// Auto-generated from services\audrec.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct FinalOutputRecorderBuffer
{
    public u64 released_ns;
    public u64 next_buffer_ptr;
    public u64 sample_buffer_ptr;
    public u64 sample_buffer_capacity;
    public u64 data_size;
    public u64 data_offset;
}

public unsafe struct FinalOutputRecorderParameter
{
    public u32 sample_rate;
    public u32 channel_count;
}

public unsafe struct FinalOutputRecorderParameterInternal
{
    public u32 sample_rate;
    public u32 channel_count;
    public u32 sample_format;
    public u32 state;
}

public unsafe struct AudrecRecorder
{
    public Service s;
}

public static class Audrec
{
    public static extern Result audrecInitialize();
    public static extern void audrecExit();
    public static extern IntPtr audrecGetServiceSession();
    public static extern Result audrecOpenFinalOutputRecorder(ref AudrecRecorder recorder_out, ref FinalOutputRecorderParameter param_in, u64 aruid, ref FinalOutputRecorderParameterInternal param_out);
    public static extern Result audrecRecorderStart(ref AudrecRecorder recorder);
    public static extern Result audrecRecorderStop(ref AudrecRecorder recorder);
    public static extern Result audrecRecorderRegisterBufferEvent(ref AudrecRecorder recorder, ref Event out_event);
    public static extern Result audrecRecorderAppendFinalOutputRecorderBuffer(ref AudrecRecorder recorder, u64 buffer_client_ptr, ref FinalOutputRecorderBuffer param);
    public static extern Result audrecRecorderGetReleasedFinalOutputRecorderBuffers(ref AudrecRecorder recorder, ref u64 out_buffers, ref u64 inout_count, ref u64 out_released);
    public static extern void audrecRecorderClose(ref AudrecRecorder recorder);
}
