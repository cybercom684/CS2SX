// Auto-generated from services\grc.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum GrcStream
{
    GrcStream_Video = 0,
    GrcStream_Audio = 1,
}

public unsafe struct GrcGameMovieTrimmer
{
    public Service s;
    public TransferMemory tmem;
}

public unsafe struct GrcMovieMaker
{
    public Service a;
    public Service s;
    public Service video_proxy;
    public Event recording_event;
    public Event audio_event;
    public TransferMemory tmem;
    public NWindow win;
    public u64 layer_handle;
    public bool layer_open;
    public bool started_flag;
}

public unsafe struct GrcGameMovieId
{
    public CapsAlbumFileId file_id;
    public u8 reserved;
}

public unsafe struct GrcOffscreenRecordingParameter
{
    public u8 unk_x0;
    public u32 unk_x10;
    public s32 video_bitrate;
    public s32 video_width;
    public s32 video_height;
    public s32 video_framerate;
    public s32 video_keyFrameInterval;
    public s32 audio_bitrate;
    public s32 audio_samplerate;
    public s32 audio_channel_count;
    public s32 audio_sample_format;
    public s32 video_imageOrientation;
    public u8 unk_x3c;
}

public static class Grc
{
    public static extern Result grcTrimGameMovie(ref GrcGameMovieId dst_movieid, ref GrcGameMovieId src_movieid, ulong tmem_size, IntPtr thumbnail, s32 start, s32 end);
    public static extern void grcCreateOffscreenRecordingParameter(ref GrcOffscreenRecordingParameter param);
    public static extern Result grcCreateMovieMaker(ref GrcMovieMaker m, ulong size);
    public static extern void grcMovieMakerClose(ref GrcMovieMaker m);
    public static extern Result grcMovieMakerAbort(ref GrcMovieMaker m);
    public static extern Result grcMovieMakerStart(ref GrcMovieMaker m, ref GrcOffscreenRecordingParameter param);
    public static extern Result grcMovieMakerFinish(ref GrcMovieMaker m, s32 width, s32 height, IntPtr userdata, ulong userdata_size, IntPtr thumbnail, ulong thumbnail_size, ref CapsApplicationAlbumEntry entry);
    public static extern Result grcMovieMakerGetError(ref GrcMovieMaker m);
    public static extern Result grcMovieMakerEncodeAudioSample(ref GrcMovieMaker m, IntPtr buffer, ulong size);
    public static extern Result grcdInitialize();
    public static extern void grcdExit();
    public static extern IntPtr grcdGetServiceSession();
    public static extern Result grcdBegin();
    public static extern Result grcdTransfer(GrcStream stream, IntPtr buffer, ulong size, ref u32 num_frames, ref u32 data_size, ref u64 start_timestamp);
}
