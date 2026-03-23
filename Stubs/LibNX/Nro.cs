// Auto-generated from nro.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX;

public unsafe struct NroSegment
{
    public u32 file_off;
    public u32 size;
}

public unsafe struct NroStart
{
    public u32 unused;
    public u32 mod_offset;
    public fixed u8 padding[8];
}

public unsafe struct NroHeader
{
    public u32 magic;
    public u32 unk1;
    public u32 size;
    public u32 unk2;
    // skipped array: NroSegment segments[3]
    public u32 bss_size;
    public u32 unk3;
    public u8 build_id;
    public u8 padding;
}

public unsafe struct NroAssetSection
{
    public u64 offset;
    public u64 size;
}

public unsafe struct NroAssetHeader
{
    public u32 magic;
    public u32 version;
    public NroAssetSection icon;
    public NroAssetSection nacp;
    public NroAssetSection romfs;
}

