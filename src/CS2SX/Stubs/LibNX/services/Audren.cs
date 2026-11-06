// Auto-generated from services\audren.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum AudioRendererOutputRate
{
    AudioRendererOutputRate_32kHz,
    AudioRendererOutputRate_48kHz,
}

public enum AudioRendererMemPoolState
{
    AudioRendererMemPoolState_Invalid,
    AudioRendererMemPoolState_New,
    AudioRendererMemPoolState_RequestDetach,
    AudioRendererMemPoolState_Detached,
    AudioRendererMemPoolState_RequestAttach,
    AudioRendererMemPoolState_Attached,
    AudioRendererMemPoolState_Released,
}

public enum AudioRendererVoicePlayState
{
    AudioRendererVoicePlayState_Started,
    AudioRendererVoicePlayState_Stopped,
    AudioRendererVoicePlayState_Paused,
}

public enum AudioRendererSinkType
{
    AudioRendererSinkType_Invalid,
    AudioRendererSinkType_Device,
    AudioRendererSinkType_CircularBuffer,
}

public unsafe struct AudioRendererConfig
{
    public AudioRendererOutputRate output_rate;
    public int num_voices;
    public int num_effects;
    public int num_sinks;
    public int num_mix_objs;
    public int num_mix_buffers;
}

public unsafe struct AudioRendererUpdateDataHeader
{
    public u32 revision;
    public u32 behavior_sz;
    public u32 mempools_sz;
    public u32 voices_sz;
    public u32 channels_sz;
    public u32 effects_sz;
    public u32 mixes_sz;
    public u32 sinks_sz;
    public u32 perfmgr_sz;
    public fixed u32 _padding[6];
    public u32 total_sz;
}

public unsafe struct AudioRendererBehaviorInfoIn
{
    public u32 revision;
    public u32 _padding1;
    public u64 flags;
}

public unsafe struct AudioRendererBehaviorInfoOut
{
    public fixed u64 unknown[20];
    public fixed u64 _padding1[2];
}

public unsafe struct AudioRendererMemPoolInfoIn
{
    public IntPtr address;
    public u64 size;
    public AudioRendererMemPoolState state;
    public fixed u32 _padding2[3];
}

public unsafe struct AudioRendererMemPoolInfoOut
{
    public AudioRendererMemPoolState new_state;
    public fixed u32 _padding2[3];
}

public unsafe struct AudioRendererChannelInfoIn
{
    public u32 id;
    public fixed float mix[24];
    public bool is_used;
    public fixed u8 _padding1[11];
}

public unsafe struct AudioRendererBiquadFilter
{
    public bool enable;
    public u8 _padding;
    public fixed s16 numerator[3];
    public fixed s16 denominator[2];
}

public unsafe struct AudioRendererAdpcmParameters
{
    public fixed u16 coefficients[16];
}

public unsafe struct AudioRendererAdpcmContext
{
    public u16 index;
    public s16 history0;
    public s16 history1;
}

public unsafe struct AudioRendererWaveBuf
{
    public IntPtr address;
    public u64 size;
    public s32 start_sample_offset;
    public s32 end_sample_offset;
    public bool is_looping;
    public bool end_of_stream;
    public bool sent_to_server;
    public fixed u8 _padding1[5];
    public IntPtr context_addr;
    public u64 context_sz;
    public u64 _padding2;
}

public unsafe struct AudioRendererVoiceInfoIn
{
    public u32 id;
    public u32 node_id;
    public bool is_new;
    public bool is_used;
    public AudioRendererVoicePlayState state : _8;
    public PcmFormat sample_format : _8;
    public u32 sample_rate;
    public u32 priority;
    public u32 sorting_order;
    public u32 channel_count;
    public float pitch;
    public float volume;
    // skipped array: AudioRendererBiquadFilter biquads[2]
    public u32 wavebuf_count;
    public s16 wavebuf_head;
    public u16 _padding1;
    public u32 _padding2;
    public IntPtr extra_params_ptr;
    public u64 extra_params_sz;
    public u32 dest_mix_id;
    public u32 dest_splitter_id;
    // skipped array: AudioRendererWaveBuf wavebufs[4]
    public fixed u32 channel_ids[6];
    public fixed u8 _padding3[24];
}

public unsafe struct AudioRendererVoiceInfoOut
{
    public u64 played_sample_count;
    public u32 num_wavebufs_consumed;
    public u32 voice_drops_count;
}

public unsafe struct AudioRendererMixInfoIn
{
    public float volume;
    public u32 sample_rate;
    public u32 buffer_count;
    public bool is_used;
    public fixed u8 _padding1[3];
    public u32 mix_id;
    public u32 _padding2;
    public u32 node_id;
    public fixed u32 _padding3[2];
    public float mix;
    public u32 dest_mix_id;
    public u32 dest_splitter_id;
    public u32 _padding4;
}

public unsafe struct AudioRendererDownMixParameters
{
    public fixed u8 coefficients[16];
}

public unsafe struct AudioRendererDeviceSinkInfoIn
{
    public fixed byte name[255];
    public u8 _padding1;
    public u32 input_count;
    public fixed u8 inputs[6];
    public u8 _padding2;
    public bool downmix_params_enabled;
    public AudioRendererDownMixParameters downmix_params;
}

public unsafe struct AudioRendererCircularBufferSinkInfoIn
{
    public IntPtr buffer_ptr;
    public u32 buffer_sz;
    public u32 input_count;
    public u32 sample_count;
    public u32 last_read_offset;
    public PcmFormat sample_format;
    public fixed u8 inputs[6];
    public fixed u8 _padding2[6];
}

public unsafe struct AudioRendererSinkInfoOut
{
    public u32 last_written_offset;
    public u32 unk1;
    public u64 unk2;
    public fixed u64 _padding1[2];
}

public unsafe struct AudioRendererPerformanceBufferInfoIn
{
    public u32 detail_target;
    public fixed u32 _padding1[3];
}

public unsafe struct AudioRendererPerformanceBufferInfoOut
{
    public u32 written_sz;
    public fixed u32 _padding1[3];
}

public static class Audren
{
    public static extern Result audrenInitialize(ref AudioRendererConfig config);
    public static extern void audrenExit();
    public static extern IntPtr audrenGetServiceSession_AudioRenderer();
    public static extern IntPtr audrenGetFrameEvent();
    public static extern void audrenWaitFrame();
    public static extern Result audrenGetState(ref u32 out_state);
    public static extern Result audrenRequestUpdateAudioRenderer(IntPtr in_param_buf, ulong in_param_buf_size, IntPtr out_param_buf, ulong out_param_buf_size, IntPtr perf_buf, ulong perf_buf_size);
    public static extern Result audrenStartAudioRenderer();
    public static extern Result audrenStopAudioRenderer();
    public static extern Result audrenSetAudioRendererRenderingTimeLimit(int percent);
}
