// Auto-generated from services\hidbus.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum HidbusBusType
{
    HidbusBusType_LeftJoyRail = 0,
    HidbusBusType_RightJoyRail = 1,
    HidbusBusType_RightLarkRail = 2,
}

public enum HidbusJoyPollingMode
{
    HidbusJoyPollingMode_SixAxisSensorDisable = 0,
    HidbusJoyPollingMode_SixAxisSensorEnable = 1,
    HidbusJoyPollingMode_ButtonOnly = 2,
}

public unsafe struct HidbusBusHandle
{
    public u32 abstracted_pad_id;
    public u8 internal_index;
    public u8 player_number;
    public u8 bus_type_id;
    public u8 is_valid;
}

public unsafe struct HidbusJoyPollingReceivedData
{
    public u8 data;
    public u64 out_size;
    public u64 sampling_number;
}

public unsafe struct HidbusDataAccessorHeader
{
    public Result res;
    public u32 pad;
    public u8 unused;
    public u64 latest_entry;
    public u64 total_entries;
}

public unsafe struct HidbusJoyDisableSixAxisPollingDataAccessorEntryData
{
    public u8 data;
    public u8 out_size;
    public u8 pad;
    public u64 sampling_number;
}

public unsafe struct HidbusJoyDisableSixAxisPollingDataAccessorEntry
{
    public u64 sampling_number;
    public HidbusJoyDisableSixAxisPollingDataAccessorEntryData data;
}

public unsafe struct HidbusJoyEnableSixAxisPollingDataAccessorEntryData
{
    public u8 data;
    public u8 out_size;
    public fixed u8 pad[7];
    public u64 sampling_number;
}

public unsafe struct HidbusJoyEnableSixAxisPollingDataAccessorEntry
{
    public u64 sampling_number;
    public HidbusJoyEnableSixAxisPollingDataAccessorEntryData data;
}

public unsafe struct HidbusJoyButtonOnlyPollingDataAccessorEntryData
{
    public u8 data;
    public u8 out_size;
    public fixed u8 pad[3];
    public u64 sampling_number;
}

public unsafe struct HidbusJoyButtonOnlyPollingDataAccessorEntry
{
    public u64 sampling_number;
    public HidbusJoyButtonOnlyPollingDataAccessorEntryData data;
}

public unsafe struct HidbusJoyDisableSixAxisPollingDataAccessor
{
    public HidbusDataAccessorHeader hdr;
    public HidbusJoyDisableSixAxisPollingDataAccessorEntry entries;
}

public unsafe struct HidbusJoyEnableSixAxisPollingDataAccessor
{
    public HidbusDataAccessorHeader hdr;
    public HidbusJoyEnableSixAxisPollingDataAccessorEntry entries;
}

public unsafe struct HidbusJoyButtonOnlyPollingDataAccessor
{
    public HidbusDataAccessorHeader hdr;
    public HidbusJoyButtonOnlyPollingDataAccessorEntry entries;
}

public unsafe struct HidbusStatusManagerEntryCommon
{
    public u8 is_connected;
    public fixed u8 pad[3];
    public Result is_connected_result;
    public u8 is_enabled;
    public u8 is_in_focus;
    public u8 is_polling_mode;
    public u8 reserved;
    public u32 polling_mode;
}

public unsafe struct HidbusStatusManagerEntryV5
{
    public HidbusStatusManagerEntryCommon common;
    public u8 unk_x10;
}

public unsafe struct HidbusStatusManagerEntry
{
    public HidbusStatusManagerEntryCommon common;
    public u8 unk_x10;
}

public unsafe struct HidbusStatusManagerV5
{
    public HidbusStatusManagerEntryV5 entries;
}

public unsafe struct HidbusStatusManager
{
    public HidbusStatusManagerEntry entries;
    public u8 unused;
}

public static class Hidbus
{
    public static extern Result hidbusGetServiceSession(ref Service srv_out);
    public static extern IntPtr hidbusGetSharedmemAddr();
    public static extern Result hidbusGetBusHandle(ref HidbusBusHandle handle, ref bool flag, HidNpadIdType id, HidbusBusType bus_type);
    public static extern Result hidbusInitialize(HidbusBusHandle handle);
    public static extern Result hidbusFinalize(HidbusBusHandle handle);
    public static extern Result hidbusEnableExternalDevice(HidbusBusHandle handle, bool flag, u32 device_id);
    public static extern Result hidbusSendAndReceive(HidbusBusHandle handle, IntPtr inbuf, ulong inbuf_size, IntPtr outbuf, ulong outbuf_size, ref u64 out_size);
    public static extern Result hidbusEnableJoyPollingReceiveMode(HidbusBusHandle handle, IntPtr inbuf, ulong inbuf_size, IntPtr workbuf, ulong workbuf_size, HidbusJoyPollingMode polling_mode);
    public static extern Result hidbusDisableJoyPollingReceiveMode(HidbusBusHandle handle);
    public static extern Result hidbusGetJoyPollingReceivedData(HidbusBusHandle handle, ref HidbusJoyPollingReceivedData recv_data, s32 count);
}
