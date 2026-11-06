// Auto-generated from services\pdm.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum PdmPlayEventType
{
    PdmPlayEventType_Applet = 0,
    PdmPlayEventType_Account = 1,
    PdmPlayEventType_PowerStateChange = 2,
    PdmPlayEventType_OperationModeChange = 3,
    PdmPlayEventType_Initialize = 4,
}

public enum PdmAppletEventType
{
    PdmAppletEventType_Launch = 0,
    PdmAppletEventType_Exit = 1,
    PdmAppletEventType_InFocus = 2,
    PdmAppletEventType_OutOfFocus = 3,
    PdmAppletEventType_OutOfFocus4 = 4,
    PdmAppletEventType_Exit5 = 5,
    PdmAppletEventType_Exit6 = 6,
}

public enum PdmPlayLogPolicy
{
    PdmPlayLogPolicy_All = 0,
    PdmPlayLogPolicy_LogOnly = 1,
    PdmPlayLogPolicy_None = 2,
    PdmPlayLogPolicy_Unknown3 = 3,
}

public unsafe struct PdmAppletEventV1
{
    public u64 program_id;
    public u32 entry_index;
    public u32 timestamp_user;
    public u32 timestamp_network;
    public u8 event_type;
    public fixed u8 pad[3];
}

public unsafe struct PdmAppletEvent
{
    public u64 program_id;
    public u32 entry_index;
    public u32 pad;
    public u64 timestamp_user;
    public u64 timestamp_network;
    public u8 event_type;
    public fixed u8 pad2[7];
}

public unsafe struct PdmPlayStatisticsV1
{
    public u64 program_id;
    public u32 first_entry_index;
    public u32 first_timestamp_user;
    public u32 first_timestamp_network;
    public u32 last_entry_index;
    public u32 last_timestamp_user;
    public u32 last_timestamp_network;
    public u32 playtime_minutes;
    public u32 total_launches;
}

public unsafe struct PdmPlayStatistics
{
    public u64 program_id;
    public u32 first_entry_index;
    public u32 pad;
    public u64 first_timestamp_user;
    public u64 first_timestamp_network;
    public u32 last_entry_index;
    public u32 pad2;
    public u64 last_timestamp_user;
    public u64 last_timestamp_network;
    public u64 playtime;
    public u32 total_launches;
    public u32 pad3;
}

public unsafe struct PdmLastPlayTime
{
    public u64 application_id;
    public u32 timestamp_user;
    public u32 timestamp_network;
    public u32 last_played_minutes;
    public u8 flag;
    public fixed u8 pad[3];
}

public unsafe struct PdmAccountEventV3
{
    public AccountUid uid;
    public u32 entry_index;
    public fixed u8 pad[4];
    public u64 timestamp_user;
    public u64 timestamp_network;
    public u64 timestamp_steady;
    public u8 type;
    public fixed u8 pad2[7];
}

public unsafe struct PdmAccountEventV10
{
    public AccountUid uid;
    public u64 program_id;
    public u32 entry_index;
    public fixed u8 pad[4];
    public u64 timestamp_user;
    public u64 timestamp_network;
    public u64 timestamp_steady;
    public u8 type;
    public fixed u8 pad2[7];
}

public unsafe struct PdmAccountEvent
{
    public AccountUid uid;
    public u64 program_id;
    public u32 entry_index;
    public fixed u8 pad[4];
    public u64 timestamp_user;
    public u64 timestamp_network;
    public u8 type;
    public fixed u8 pad2[7];
}

public unsafe struct PdmAccountPlayEvent
{
    public fixed u8 unk_x0[4];
    public fixed u32 application_id[2];
    public u8 unk_xc;
    public u64 timestamp0;
    public u64 timestamp1;
}

public unsafe struct PdmApplicationPlayStatistics
{
    public u64 application_id;
    public u64 playtime;
    public u64 total_launches;
}

public static class Pdm
{
    public static extern Result pdmqryInitialize();
    public static extern void pdmqryExit();
    public static extern IntPtr pdmqryGetServiceSession();
    public static extern Result pdmqryQueryAppletEvent(s32 entry_index, bool flag, ref PdmAppletEvent events, s32 count, ref s32 total_out);
    public static extern Result pdmqryQueryPlayStatisticsByApplicationId(u64 application_id, bool flag, ref PdmPlayStatistics stats);
    public static extern Result pdmqryQueryPlayStatisticsByApplicationIdAndUserAccountId(u64 application_id, AccountUid uid, bool flag, ref PdmPlayStatistics stats);
    public static extern Result pdmqryQueryLastPlayTime(bool flag, ref PdmLastPlayTime playtimes, ref u64 application_ids, s32 count, ref s32 total_out);
    public static extern Result pdmqryQueryPlayEvent(s32 entry_index, ref PdmPlayEvent events, s32 count, ref s32 total_out);
    public static extern Result pdmqryGetAvailablePlayEventRange(ref s32 total_entries, ref s32 start_entry_index, ref s32 end_entry_index);
    public static extern Result pdmqryQueryAccountEvent(s32 entry_index, ref PdmAccountEvent events, s32 count, ref s32 total_out);
    public static extern Result pdmqryQueryAccountPlayEvent(s32 entry_index, AccountUid uid, ref PdmAccountPlayEvent events, s32 count, ref s32 total_out);
    public static extern Result pdmqryGetAvailableAccountPlayEventRange(AccountUid uid, ref s32 total_entries, ref s32 start_entry_index, ref s32 end_entry_index);
    public static extern Result pdmqryQueryRecentlyPlayedApplication(AccountUid uid, bool flag, ref u64 application_ids, s32 count, ref s32 total_out);
    public static extern Result pdmqryGetRecentlyPlayedApplicationUpdateEvent(ref Event out_event);
}
