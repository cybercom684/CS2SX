// Auto-generated from services\time.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum TimeServiceType
{
    TimeServiceType_User = 0,
    TimeServiceType_Menu = 1,
    TimeServiceType_System = 2,
    TimeServiceType_Repair = 3,
    TimeServiceType_SystemUser = 4,
}

public enum TimeType
{
    TimeType_UserSystemClock,
    TimeType_NetworkSystemClock,
    TimeType_LocalSystemClock,
    TimeType_Default = TimeType_UserSystemClock,
}

public unsafe struct TimeCalendarTime
{
    public u16 year;
    public u8 month;
    public u8 day;
    public u8 hour;
    public u8 minute;
    public u8 second;
    public u8 pad;
}

public unsafe struct TimeCalendarAdditionalInfo
{
    public u32 wday;
    public u32 yday;
    public fixed byte timezoneName[8];
    public u32 DST;
    public s32 offset;
}

public unsafe struct TimeZoneRule
{
    public u8 data;
}

public unsafe struct TimeLocationName
{
    public byte name;
}

public unsafe struct TimeSteadyClockTimePoint
{
    public s64 time_point;
    public Uuid source_id;
}

public unsafe struct TimeStandardSteadyClockTimePointType
{
    public s64 base_time;
    public Uuid source_id;
}

public unsafe struct TimeSystemClockContext
{
    public s64 offset;
    public TimeSteadyClockTimePoint timestamp;
}

public static class Time
{
    public static extern Result timeInitialize();
    public static extern void timeExit();
    public static extern IntPtr timeGetServiceSession();
    public static extern IntPtr timeGetServiceSession_SystemClock(TimeType type);
    public static extern IntPtr timeGetServiceSession_SteadyClock();
    public static extern IntPtr timeGetServiceSession_TimeZoneService();
    public static extern IntPtr timeGetSharedmemAddr();
    public static extern Result timeGetStandardSteadyClockTimePoint(ref TimeSteadyClockTimePoint @out);
    public static extern Result timeGetStandardSteadyClockInternalOffset(ref s64 @out);
    public static extern Result timeGetCurrentTime(TimeType type, ref u64 timestamp);
    public static extern Result timeSetCurrentTime(TimeType type, u64 timestamp);
    public static extern Result timeGetDeviceLocationName(ref TimeLocationName name);
    public static extern Result timeSetDeviceLocationName(ref TimeLocationName name);
    public static extern Result timeGetTotalLocationNameCount(ref s32 total_location_name_count);
    public static extern Result timeLoadLocationNameList(s32 index, ref TimeLocationName location_name_array, s32 location_name_max, ref s32 location_name_count);
    public static extern Result timeLoadTimeZoneRule(ref TimeLocationName name, ref TimeZoneRule rule);
    public static extern Result timeToCalendarTime(ref TimeZoneRule rule, u64 timestamp, ref TimeCalendarTime caltime, ref TimeCalendarAdditionalInfo info);
    public static extern Result timeToCalendarTimeWithMyRule(u64 timestamp, ref TimeCalendarTime caltime, ref TimeCalendarAdditionalInfo info);
    public static extern Result timeToPosixTime(ref TimeZoneRule rule, ref TimeCalendarTime caltime, ref u64 timestamp_list, s32 timestamp_list_count, ref s32 timestamp_count);
    public static extern Result timeToPosixTimeWithMyRule(ref TimeCalendarTime caltime, ref u64 timestamp_list, s32 timestamp_list_count, ref s32 timestamp_count);
}
