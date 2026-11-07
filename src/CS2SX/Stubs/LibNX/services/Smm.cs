// Auto-generated from services\smm.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Smm
{
    public static extern Result smManagerInitialize();
    public static extern void smManagerExit();
    public static extern Result smManagerRegisterProcess(u64 pid, IntPtr acid_sac, ulong acid_sac_size, IntPtr aci0_sac, ulong aci0_sac_size);
    public static extern Result smManagerUnregisterProcess(u64 pid);
    public static extern Result smManagerCmifInitialize();
    public static extern void smManagerCmifExit();
    public static extern IntPtr smManagerCmifGetServiceSession();
    public static extern Result smManagerCmifRegisterProcess(u64 pid, IntPtr acid_sac, ulong acid_sac_size, IntPtr aci0_sac, ulong aci0_sac_size);
    public static extern Result smManagerCmifUnregisterProcess(u64 pid);
    public static extern Result smManagerTipcInitialize();
    public static extern void smManagerTipcExit();
    public static extern IntPtr smManagerTipcGetServiceSession();
    public static extern Result smManagerTipcRegisterProcess(u64 pid, IntPtr acid_sac, ulong acid_sac_size, IntPtr aci0_sac, ulong aci0_sac_size);
    public static extern Result smManagerTipcUnregisterProcess(u64 pid);
}
