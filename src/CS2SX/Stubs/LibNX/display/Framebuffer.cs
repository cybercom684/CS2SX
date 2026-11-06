// Auto-generated from display\framebuffer.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Display;

public unsafe struct Framebuffer
{
    public IntPtr win;
    public NvMap map;
    public IntPtr buf;
    public IntPtr buf_linear;
    public u32 stride;
    public u32 width_aligned;
    public u32 height_aligned;
    public u32 num_fbs;
    public u32 fb_size;
    public bool has_init;
}

public static class Framebuffer
{
    public static extern Result framebufferCreate(ref Framebuffer fb, ref NWindow win, u32 width, u32 height, u32 format, u32 num_fbs);
    public static extern Result framebufferMakeLinear(ref Framebuffer fb);
    public static extern void framebufferClose(ref Framebuffer fb);
    public static extern IntPtr framebufferBegin(ref Framebuffer fb, ref u32 out_stride);
    public static extern void framebufferEnd(ref Framebuffer fb);
}
