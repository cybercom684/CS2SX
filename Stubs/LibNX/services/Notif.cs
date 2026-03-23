// Auto-generated from services\notif.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum NotifServiceType
{
    NotifServiceType_Application = 0,
    NotifServiceType_System = 1,
}

public unsafe struct NotifAlarmTime
{
    public s32 hour;
    public s32 minute;
}

public unsafe struct NotifWeeklyScheduleAlarmSetting
{
    public u8 unk_x0;
    public fixed s16 settings[7];
}

public unsafe struct NotifAlarmSetting
{
    public u16 alarm_setting_id;
    public u8 kind;
    public u8 muted;
    public fixed u8 pad[4];
    public AccountUid uid;
    public u64 application_id;
    public u64 unk_x20;
    public NotifWeeklyScheduleAlarmSetting schedule;
}

public static class Notif
{
    public static extern Result notifInitialize(NotifServiceType service_type);
    public static extern void notifExit();
    public static extern IntPtr notifGetServiceSession();
    public static extern void notifAlarmSettingCreate(ref NotifAlarmSetting alarm_setting);
    public static extern Result notifAlarmSettingIsEnabled(ref NotifAlarmSetting alarm_setting, u32 day_of_week, ref bool @out);
    public static extern Result notifAlarmSettingGet(ref NotifAlarmSetting alarm_setting, u32 day_of_week, ref NotifAlarmTime @out);
    public static extern Result notifAlarmSettingEnable(ref NotifAlarmSetting alarm_setting, u32 day_of_week, s32 hour, s32 minute);
    public static extern Result notifAlarmSettingDisable(ref NotifAlarmSetting alarm_setting, u32 day_of_week);
    public static extern Result notifRegisterAlarmSetting(ref u16 alarm_setting_id, ref NotifAlarmSetting alarm_setting, IntPtr buffer, ulong size);
    public static extern Result notifUpdateAlarmSetting(ref NotifAlarmSetting alarm_setting, IntPtr buffer, ulong size);
    public static extern Result notifListAlarmSettings(ref NotifAlarmSetting alarm_settings, s32 count, ref s32 total_out);
    public static extern Result notifLoadApplicationParameter(u16 alarm_setting_id, IntPtr buffer, ulong size, ref u32 actual_size);
    public static extern Result notifDeleteAlarmSetting(u16 alarm_setting_id);
    public static extern Result notifTryPopNotifiedApplicationParameter(IntPtr buffer, u64 size, ref u64 out_size);
}
