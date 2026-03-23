// Auto-generated from services\ldr.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct LoaderProgramInfoV1
{
    public u8 main_thread_priority;
    public u8 default_cpu_id;
    public u16 application_type;
    public u32 main_thread_stack_size;
    public u64 program_id;
    public u32 acid_sac_size;
    public u32 aci0_sac_size;
    public u32 acid_fac_size;
    public u32 aci0_fah_size;
    public u8 ac_buffer;
}

public unsafe struct LoaderProgramInfo
{
    public u8 main_thread_priority;
    public u8 default_cpu_id;
    public u16 application_type;
    public u32 main_thread_stack_size;
    public u64 program_id;
    public u32 acid_sac_size;
    public u32 aci0_sac_size;
    public u32 acid_fac_size;
    public u32 aci0_fah_size;
    public u8 unused_20;
    public u8 ac_buffer;
}

public unsafe struct LoaderModuleInfo
{
    public u8 build_id;
    public u64 base_address;
    public u64 size;
}

public unsafe struct LoaderProgramAttributes
{
    public u8 platform;
    public u8 content_attributes;
}

public static class Ldr
{
    public static extern Result ldrShellInitialize();
    public static extern void ldrShellExit();
    public static extern IntPtr ldrShellGetServiceSession();
    public static extern Result ldrDmntInitialize();
    public static extern void ldrDmntExit();
    public static extern IntPtr ldrDmntGetServiceSession();
    public static extern Result ldrPmInitialize();
    public static extern void ldrPmExit();
    public static extern IntPtr ldrPmGetServiceSession();
    public static extern Result ldrShellSetProgramArguments(u64 program_id, IntPtr args, ulong args_size);
    public static extern Result ldrShellFlushArguments();
    public static extern Result ldrDmntSetProgramArguments(u64 program_id, IntPtr args, ulong args_size);
    public static extern Result ldrDmntFlushArguments();
    public static extern Result ldrDmntGetProcessModuleInfo(u64 pid, ref LoaderModuleInfo out_module_infos, ulong max_out_modules, ref s32 num_out);
    public static extern Result ldrPmCreateProcess(u64 pin_id, u32 flags, Handle reslimit_h, ref LoaderProgramAttributes attrs, ref Handle out_process_h);
    public static extern Result ldrPmGetProgramInfo(ref NcmProgramLocation loc, ref LoaderProgramAttributes attrs, ref LoaderProgramInfo out_program_info);
    public static extern Result ldrPmGetProgramInfoV1(ref NcmProgramLocation loc, ref LoaderProgramInfoV1 out_program_info);
    public static extern Result ldrPmPinProgram(ref NcmProgramLocation loc, ref u64 out_pin_id);
    public static extern Result ldrPmUnpinProgram(u64 pin_id);
    public static extern Result ldrPmSetEnabledProgramVerification(bool enabled);
}
