// Auto-generated from nvidia\graphic_buffer.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Nvidia;

public unsafe struct NvSurface
{
    public u32 width;
    public u32 height;
    public NvColorFormat color_format;
    public NvLayout layout;
    public u32 pitch;
    public u32 unused;
    public u32 offset;
    public NvKind kind;
    public u32 block_height_log2;
    public NvDisplayScanFormat scan;
    public u32 second_field_offset;
    public u64 flags;
    public u64 size;
    public fixed u32 unk[6];
}

public unsafe struct NvGraphicBuffer
{
    public NativeHandle header;
    public s32 unk0;
    public s32 nvmap_id;
    public u32 unk2;
    public u32 magic;
    public u32 pid;
    public u32 type;
    public u32 usage;
    public u32 format;
    public u32 ext_format;
    public u32 stride;
    public u32 total_size;
    public u32 num_planes;
    public u32 unk12;
    // skipped array: NvSurface planes[3]
    public u64 unused;
}

