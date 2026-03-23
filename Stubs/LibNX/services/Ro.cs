// Auto-generated from services\ro.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Ro
{
    public static extern Result ldrRoInitialize();
    public static extern void ldrRoExit();
    public static extern IntPtr ldrRoGetServiceSession();
    public static extern Result ro1Initialize();
    public static extern void ro1Exit();
    public static extern IntPtr ro1GetServiceSession();
    public static extern Result roDmntInitialize();
    public static extern void roDmntExit();
    public static extern IntPtr roDmntGetServiceSession();
    public static extern Result ldrRoLoadNro(ref u64 out_address, u64 nro_address, u64 nro_size, u64 bss_address, u64 bss_size);
    public static extern Result ldrRoUnloadNro(u64 nro_address);
    public static extern Result ldrRoLoadNrr(u64 nrr_address, u64 nrr_size);
    public static extern Result ldrRoUnloadNrr(u64 nrr_address);
    public static extern Result ldrRoLoadNrrEx(u64 nrr_address, u64 nrr_size);
    public static extern Result ro1LoadNro(ref u64 out_address, u64 nro_address, u64 nro_size, u64 bss_address, u64 bss_size);
    public static extern Result ro1UnloadNro(u64 nro_address);
    public static extern Result ro1LoadNrr(u64 nrr_address, u64 nrr_size);
    public static extern Result ro1UnloadNrr(u64 nrr_address);
    public static extern Result ro1LoadNrrEx(u64 nrr_address, u64 nrr_size);
    public static extern Result roDmntGetProcessModuleInfo(u64 pid, ref LoaderModuleInfo out_module_infos, ulong max_out_modules, ref s32 num_out);
}
