// Auto-generated from services\capsdc.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Capsdc
{
    public static extern Result capsdcInitialize();
    public static extern void capsdcExit();
    public static extern IntPtr capsdcGetServiceSession();
    public static extern Result capsdcDecodeJpeg(u32 width, u32 height, ref CapsScreenShotDecodeOption opts, IntPtr jpeg, ulong jpeg_size, IntPtr out_image, ulong out_image_size);
    public static extern Result capsdcShrinkJpeg(u32 width, u32 height, ref CapsScreenShotDecodeOption opts, IntPtr jpeg, ulong jpeg_size, IntPtr out_jpeg, ulong out_jpeg_size, ref u64 out_result_size);
    public static extern Result capsdcShrinkJpegEx(u32 scaled_width, u32 scaled_height, u32 jpeg_quality, ref CapsScreenShotDecodeOption opts, IntPtr jpeg, ulong jpeg_size, IntPtr out_jpeg, ulong out_jpeg_size, ref u64 out_result_size);
}
