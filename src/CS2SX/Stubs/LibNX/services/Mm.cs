// Auto-generated from services\mm.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum MmuModuleId
{
    MmuModuleId_Ram = 2,
    MmuModuleId_Nvenc = 5,
    MmuModuleId_Nvdec = 6,
    MmuModuleId_Nvjpg = 7,
}

public unsafe struct MmuRequest
{
    public MmuModuleId module;
    public u32 id;
}

public static class Mm
{
    public static extern Result mmuInitialize();
    public static extern void mmuExit();
    public static extern IntPtr mmuGetServiceSession();
    public static extern Result mmuRequestInitialize(ref MmuRequest request, MmuModuleId module, u32 unk, bool autoclear);
    public static extern Result mmuRequestFinalize(ref MmuRequest request);
    public static extern Result mmuRequestGet(ref MmuRequest request, ref u32 out_freq_hz);
    public static extern Result mmuRequestSetAndWait(ref MmuRequest request, u32 freq_hz, s32 timeout);
}
