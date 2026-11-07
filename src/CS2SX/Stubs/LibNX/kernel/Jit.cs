// Auto-generated from kernel\jit.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public enum JitType
{
    JitType_SetProcessMemoryPermission,
    JitType_CodeMemory,
}

public static class Jit
{
    public static extern Result jitCreate(ref Jit j, ulong size);
    public static extern Result jitTransitionToWritable(ref Jit j);
    public static extern Result jitTransitionToExecutable(ref Jit j);
    public static extern Result jitClose(ref Jit j);
}
