// Auto-generated from services\fspr.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Fspr
{
    public static extern Result fsprInitialize();
    public static extern void fsprExit();
    public static extern IntPtr fsprGetServiceSession();
    public static extern Result fsprRegisterProgram(u64 pid, u64 tid, NcmStorageId sid, IntPtr fs_access_header, ulong fah_size, IntPtr fs_access_control, ulong fac_size, u8 fs_access_control_restriction_mode);
    public static extern Result fsprUnregisterProgram(u64 pid);
    public static extern Result fsprSetCurrentProcess();
    public static extern Result fsprSetEnabledProgramVerification(bool enabled);
}
