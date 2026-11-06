// Auto-generated from services\csrng.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Csrng
{
    public static extern Result csrngInitialize();
    public static extern void csrngExit();
    public static extern IntPtr csrngGetServiceSession();
    public static extern Result csrngGetRandomBytes(IntPtr @out, ulong out_size);
}
