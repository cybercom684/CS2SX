// Auto-generated from services\capssc.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Capssc
{
    public static extern Result capsscInitialize();
    public static extern void capsscExit();
    public static extern IntPtr capsscGetServiceSession();
    public static extern Result capsscCaptureRawImageWithTimeout(IntPtr buf, ulong size, ViLayerStack layer_stack, u64 width, u64 height, s64 buffer_count, s64 buffer_index, s64 timeout);
    public static extern Result capsscOpenRawScreenShotReadStream(ref u64 out_size, ref u64 out_width, ref u64 out_height, ViLayerStack layer_stack, s64 timeout);
    public static extern Result capsscCloseRawScreenShotReadStream();
    public static extern Result capsscReadRawScreenShotReadStream(ref u64 bytes_read, IntPtr buf, ulong size, u64 offset);
    public static extern Result capsscCaptureJpegScreenShot(ref u64 out_jpeg_size, IntPtr jpeg_buf, ulong jpeg_buf_size, ViLayerStack layer_stack, s64 timeout);
}
