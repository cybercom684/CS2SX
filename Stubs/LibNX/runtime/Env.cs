// Auto-generated from runtime\env.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Runtime;

public unsafe struct ConfigEntry
{
    public u32 Key;
    public u32 Flags;
    public fixed u64 Value[2];
}

public static class Env
{
    public static extern void envSetup(IntPtr ctx, Handle main_thread, LoaderReturnFn saved_lr);
    public static extern IntPtr envGetLoaderInfo();
    public static extern u64 envGetLoaderInfoSize();
    public static extern Handle envGetMainThreadHandle();
    public static extern bool envIsNso();
    public static extern bool envHasHeapOverride();
    public static extern IntPtr envGetHeapOverrideAddr();
    public static extern u64 envGetHeapOverrideSize();
    public static extern bool envHasArgv();
    public static extern IntPtr envGetArgv();
    public static extern bool envIsSyscallHinted(unsigned svc);
    public static extern Handle envGetOwnProcessHandle();
    public static extern LoaderReturnFn envGetExitFuncPtr();
    public static extern void envSetExitFuncPtr(LoaderReturnFn addr);
    public static extern Result envSetNextLoad(ref byte path, ref byte argv);
    public static extern bool envHasNextLoad();
    public static extern Result envGetLastLoadResult();
    public static extern bool envHasRandomSeed();
    public static extern void envGetRandomSeed(u64 @out);
    public static extern IntPtr envGetUserIdStorage();
}
