// Auto-generated from kernel\random.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public static class Random
{
    public static extern void randomGet(IntPtr buf, ulong len);
    public static extern u64 randomGet64();
}
