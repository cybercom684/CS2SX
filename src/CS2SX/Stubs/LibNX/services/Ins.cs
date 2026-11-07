// Auto-generated from services\ins.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Ins
{
    public static extern Result insrInitialize();
    public static extern void insrExit();
    public static extern IntPtr insrGetServiceSession();
    public static extern Result insrGetLastTick(u32 id, ref u64 tick);
    public static extern Result insrGetReadableEvent(u32 id, ref Event @out);
    public static extern Result inssInitialize();
    public static extern void inssExit();
    public static extern IntPtr inssGetServiceSession();
    public static extern Result inssGetWritableEvent(u32 id, ref Event @out);
}
