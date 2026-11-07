// Auto-generated from runtime\ringcon.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Runtime;

public enum RingConDataValid
{
    RingConDataValid_Ok = 0,
    RingConDataValid_CRC = 1,
    RingConDataValid_Cal = 2,
}

public enum RingConErrorFlag
{
    RingConErrorFlag_BadUserCalUpdate = 0,
    RingConErrorFlag_BadFlag = 4,
    RingConErrorFlag_BadUserCal = 5,
    RingConErrorFlag_BadManuCal = 6,
}

public unsafe struct RingConFwVersion
{
    public u8 fw_main_ver;
    public u8 fw_sub_ver;
}

public unsafe struct RingConManuCal
{
    public s16 os_max;
    public s16 hk_max;
    public s16 zero_min;
    public s16 zero_max;
}

public unsafe struct RingConUserCal
{
    public s16 os_max;
    public s16 hk_max;
    public s16 zero;
    public RingConDataValid data_valid;
}

public unsafe struct RingConPollingData
{
    public s16 data;
    public u64 sampling_number;
}

public unsafe struct RingCon
{
    public bool bus_initialized;
    public HidbusBusHandle handle;
    public IntPtr workbuf;
    public ulong workbuf_size;
    public u64 polling_last_sampling_number;
    public u32 error_flags;
    public u64 id_l, id_h;
    public RingConFwVersion fw_ver;
    public u32 flag;
    public s16 unk_cal;
    public s32 total_push_count;
    public RingConManuCal manu_cal;
    public RingConUserCal user_cal;
}

public static class Ringcon
{
    public static extern Result ringconCreate(ref RingCon c, HidNpadIdType id);
    public static extern void ringconClose(ref RingCon c);
    public static extern Result ringconUpdateUserCal(ref RingCon c, RingConUserCal cal);
    public static extern Result ringconReadFwVersion(ref RingCon c, ref RingConFwVersion @out);
    public static extern Result ringconReadId(ref RingCon c, ref u64 id_l, ref u64 id_h);
    public static extern Result ringconGetPollingData(ref RingCon c, ref RingConPollingData @out, s32 count, ref s32 total_out);
    public static extern Result ringconCmdx00020105(ref RingCon c, ref u32 @out);
    public static extern Result ringconReadManuCal(ref RingCon c, ref RingConManuCal @out);
    public static extern Result ringconReadUnkCal(ref RingCon c, ref s16 @out);
    public static extern Result ringconReadUserCal(ref RingCon c, ref RingConUserCal @out);
    public static extern Result ringconReadRepCount(ref RingCon c, ref s32 @out, ref RingConDataValid data_valid);
    public static extern Result ringconReadTotalPushCount(ref RingCon c, ref s32 @out, ref RingConDataValid data_valid);
    public static extern Result ringconResetRepCount(ref RingCon c);
    public static extern Result ringconWriteUserCal(ref RingCon c, RingConUserCal cal);
}
