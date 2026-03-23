// Auto-generated from applets\hid_la.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Applets;

public enum HidLaControllerSupportMode
{
    HidLaControllerSupportMode_ShowControllerSupport = 0,
    HidLaControllerSupportMode_ShowControllerStrapGuide = 1,
    HidLaControllerSupportMode_ShowControllerFirmwareUpdate = 2,
    HidLaControllerSupportMode_ShowControllerKeyRemappingForSystem = 4,
}

public enum HidLaControllerSupportCaller
{
    HidLaControllerSupportCaller_Application = 0,
    HidLaControllerSupportCaller_System = 1,
}

public unsafe struct HidLaControllerSupportArgPrivate
{
    public u32 private_size;
    public u32 arg_size;
    public u8 flag0;
    public u8 flag1;
    public u8 mode;
    public u8 controller_support_caller;
    public u32 npad_style_set;
    public u32 npad_joy_hold_type;
}

public unsafe struct HidLaControllerSupportArgHeader
{
    public s8 player_count_min;
    public s8 player_count_max;
    public u8 enable_take_over_connection;
    public u8 enable_left_justify;
    public u8 enable_permit_joy_dual;
    public u8 enable_single_mode;
    public u8 enable_identification_color;
}

public unsafe struct HidLaControllerSupportArgColor
{
    public u8 r;
    public u8 g;
    public u8 b;
    public u8 a;
}

public unsafe struct HidLaControllerSupportArgV3
{
    public HidLaControllerSupportArgHeader hdr;
    // skipped array: HidLaControllerSupportArgColor identification_color[4]
    public u8 enable_explain_text;
    public byte explain_text;
}

public unsafe struct HidLaControllerSupportArg
{
    public HidLaControllerSupportArgHeader hdr;
    // skipped array: HidLaControllerSupportArgColor identification_color[8]
    public u8 enable_explain_text;
    public byte explain_text;
}

public unsafe struct HidLaControllerFirmwareUpdateArg
{
    public u8 enable_force_update;
    public fixed u8 pad[3];
}

public unsafe struct HidLaControllerKeyRemappingArg
{
    public u64 unk_x0;
    public u32 unk_x8;
    public u8 pad;
}

public unsafe struct HidLaControllerSupportResultInfo
{
    public s8 player_count;
    public fixed u8 pad[3];
    public u32 selected_id;
}

public unsafe struct HidLaControllerSupportResultInfoInternal
{
    public HidLaControllerSupportResultInfo info;
    public u32 res;
}

public static class Hid_la
{
    public static extern void hidLaCreateControllerSupportArg(ref HidLaControllerSupportArg arg);
    public static extern void hidLaCreateControllerFirmwareUpdateArg(ref HidLaControllerFirmwareUpdateArg arg);
    public static extern void hidLaCreateControllerKeyRemappingArg(ref HidLaControllerKeyRemappingArg arg);
    public static extern Result hidLaSetExplainText(ref HidLaControllerSupportArg arg, ref byte str, HidNpadIdType id);
    public static extern Result hidLaShowControllerSupport(ref HidLaControllerSupportResultInfo result_info, ref HidLaControllerSupportArg arg);
    public static extern Result hidLaShowControllerStrapGuide();
    public static extern Result hidLaShowControllerFirmwareUpdate(ref HidLaControllerFirmwareUpdateArg arg);
    public static extern Result hidLaShowControllerSupportForSystem(ref HidLaControllerSupportResultInfo result_info, ref HidLaControllerSupportArg arg, bool flag);
    public static extern Result hidLaShowControllerFirmwareUpdateForSystem(ref HidLaControllerFirmwareUpdateArg arg, HidLaControllerSupportCaller caller);
    public static extern Result hidLaShowControllerKeyRemappingForSystem(ref HidLaControllerKeyRemappingArg arg, HidLaControllerSupportCaller caller);
}
