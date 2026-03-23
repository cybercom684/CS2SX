// Auto-generated from services\set.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum ColorSetId
{
    ColorSetId_Light = 0,
    ColorSetId_Dark = 1,
}

public enum SetSysProductModel
{
    SetSysProductModel_Invalid = 0,
    SetSysProductModel_Nx = 1,
    SetSysProductModel_Copper = 2,
    SetSysProductModel_Iowa = 3,
    SetSysProductModel_Hoag = 4,
    SetSysProductModel_Calcio = 5,
    SetSysProductModel_Aula = 6,
}

public enum SetLanguage
{
    SetLanguage_JA = 0,
    SetLanguage_ENUS = 1,
    SetLanguage_FR = 2,
    SetLanguage_DE = 3,
    SetLanguage_IT = 4,
    SetLanguage_ES = 5,
    SetLanguage_ZHCN = 6,
    SetLanguage_KO = 7,
    SetLanguage_NL = 8,
    SetLanguage_PT = 9,
    SetLanguage_RU = 10,
    SetLanguage_ZHTW = 11,
    SetLanguage_ENGB = 12,
    SetLanguage_FRCA = 13,
    SetLanguage_ES419 = 14,
    SetLanguage_ZHHANS = 15,
    SetLanguage_ZHHANT = 16,
    SetLanguage_PTBR = 17,
    SetLanguage_Total,
}

public enum SetRegion
{
    SetRegion_JPN = 0,
    SetRegion_USA = 1,
    SetRegion_EUR = 2,
    SetRegion_AUS = 3,
    SetRegion_HTK = 4,
    SetRegion_CHN = 5,
}

public enum SetSysConnectionFlag
{
    SetSysConnectionFlag_ConnectAutomaticallyFlag = (1 << 0),
    SetSysConnectionFlag_Unknown = (1 << 1),
}

public enum SetSysAccessPointSecurityType
{
    SetSysAccessPointSecurityType_None = 0,
    SetSysAccessPointSecurityType_Shared = 1,
    SetSysAccessPointSecurityType_Wpa = 2,
    SetSysAccessPointSecurityType_Wpa2 = 3,
}

public enum SetSysAccessPointSecurityStandard
{
    SetSysAccessPointSecurityStandard_None = 0,
    SetSysAccessPointSecurityStandard_Wep = 1,
    SetSysAccessPointSecurityStandard_Wpa = 2,
}

public enum SetSysAutoSettings
{
    SetSysAutoSettings_AutoIp = (1 << 0),
    SetSysAutoSettings_AutoDns = (1 << 1),
}

public enum SetSysProxyFlags
{
    SetSysProxyFlags_UseProxyFlag = (1 << 0),
    SetSysProxyFlags_ProxyAutoAuthenticateFlag = (1 << 1),
}

public enum SetSysUserSelectorFlag
{
    SetSysUserSelectorFlag_SkipsIfSingleUser = (1 << 0),
}

public enum SetSysEulaVersionClockType
{
    SetSysEulaVersionClockType_NetworkSystemClock = 0,
    SetSysEulaVersionClockType_SteadyClock = 1,
}

public enum SetSysNotificationVolume
{
    SetSysNotificationVolume_Mute = 0,
    SetSysNotificationVolume_Low = 1,
    SetSysNotificationVolume_High = 2,
}

public enum SetSysFriendPresenceOverlayPermission
{
    SetSysFriendPresenceOverlayPermission_NotConfirmed = 0,
    SetSysFriendPresenceOverlayPermission_NoDisplay = 1,
    SetSysFriendPresenceOverlayPermission_FavoriteFriends = 2,
    SetSysFriendPresenceOverlayPermission_Friends = 3,
}

public enum SetSysAudioDevice
{
    SetSysAudioDevice_Console = 0,
    SetSysAudioDevice_Headphone = 1,
    SetSysAudioDevice_Tv = 2,
}

public enum SetSysPrimaryAlbumStorage
{
    SetSysPrimaryAlbumStorage_Nand = 0,
    SetSysPrimaryAlbumStorage_SdCard = 1,
}

public enum SetSysHandheldSleepPlan
{
    SetSysHandheldSleepPlan_1Min = 0,
    SetSysHandheldSleepPlan_3Min = 1,
    SetSysHandheldSleepPlan_5Min = 2,
    SetSysHandheldSleepPlan_10Min = 3,
    SetSysHandheldSleepPlan_30Min = 4,
    SetSysHandheldSleepPlan_Never = 5,
}

public enum SetSysConsoleSleepPlan
{
    SetSysConsoleSleepPlan_1Hour = 0,
    SetSysConsoleSleepPlan_2Hour = 1,
    SetSysConsoleSleepPlan_3Hour = 2,
    SetSysConsoleSleepPlan_6Hour = 3,
    SetSysConsoleSleepPlan_12Hour = 4,
    SetSysConsoleSleepPlan_Never = 5,
}

public enum SetSysAudioOutputModeTarget
{
    SetSysAudioOutputModeTarget_Unknown0 = 0,
    SetSysAudioOutputModeTarget_Unknown1 = 1,
    SetSysAudioOutputModeTarget_Unknown2 = 2,
    SetSysAudioOutputModeTarget_Unknown3 = 3,
}

public enum SetSysAudioOutputMode
{
    SetSysAudioOutputMode_Unknown1 = 1,
}

public enum SetSysServiceDiscoveryControlSettings
{
    SetSysServiceDiscoveryControlSettings_IsChangeEnvironmentIdentifierDisabled = (1 << 0),
}

public enum SetSysErrorReportSharePermission
{
    SetSysErrorReportSharePermission_NotConfirmed = 0,
    SetSysErrorReportSharePermission_Granted = 1,
    SetSysErrorReportSharePermission_Denied = 2,
}

public enum SetKeyboardLayout
{
    SetKeyboardLayout_Japanese = 0,
    SetKeyboardLayout_EnglishUs = 1,
    SetKeyboardLayout_EnglishUsInternational = 2,
    SetKeyboardLayout_EnglishUk = 3,
    SetKeyboardLayout_French = 4,
    SetKeyboardLayout_FrenchCa = 5,
    SetKeyboardLayout_Spanish = 6,
    SetKeyboardLayout_SpanishLatin = 7,
    SetKeyboardLayout_German = 8,
    SetKeyboardLayout_Italian = 9,
    SetKeyboardLayout_Portuguese = 10,
    SetKeyboardLayout_Russian = 11,
    SetKeyboardLayout_Korean = 12,
    SetKeyboardLayout_ChineseSimplified = 13,
    SetKeyboardLayout_ChineseTraditional = 14,
}

public enum SetChineseTraditionalInputMethod
{
    SetChineseTraditionalInputMethod_Unknown1 = 1,
    SetChineseTraditionalInputMethod_Unknown2 = 2,
}

public enum SetSysPtmCycleCountReliability
{
    PtmCycleCountReliability_Default = 0,
    PtmCycleCountReliability_Unk = 1,
}

public enum SetSysPlatformRegion
{
    SetSysPlatformRegion_Global = 1,
    SetSysPlatformRegion_China = 2,
}

public enum SetSysTouchScreenMode
{
    SetSysTouchScreenMode_Stylus = 0,
    SetSysTouchScreenMode_Standard = 1,
}

public enum SetSysBlockType
{
    SetSysBlockType_Audio = 1,
    SetSysBlockType_Video = 2,
    SetSysBlockType_VendorSpecific = 3,
    SetSysBlockType_Speaker = 4,
}

public enum SetSysControllerType
{
    SetSysControllerType_JoyConR = 1,
    SetSysControllerType_JoyConL = 2,
    SetSysControllerType_ProCon = 3,
}

public unsafe struct SetBatteryLot
{
    public byte lot;
}

public unsafe struct SetSysNetworkSettings
{
    public byte name;
    public Uuid uuid;
    public u32 connection_flags;
    public u32 wired_flag;
    public u32 connect_to_hidden_network;
    public byte access_point_ssid;
    public u32 access_point_ssid_len;
    public u32 access_point_security_type;
    public u32 access_point_security_standard;
    public byte access_point_passphrase;
    public u32 access_point_passphrase_len;
    public u32 auto_settings;
    public u32 manual_ip_address;
    public u32 manual_subnet_mask;
    public u32 manual_gateway;
    public u32 primary_dns;
    public u32 secondary_dns;
    public u32 proxy_flags;
    public byte proxy_server;
    public u16 proxy_port;
    public u16 padding1;
    public byte proxy_autoauth_user;
    public byte proxy_autoauth_pass;
    public u16 mtu;
    public u16 padding2;
}

public unsafe struct SetSysLcdBacklightBrightnessMapping
{
    public float brightness_applied_to_backlight;
    public float ambient_light_sensor_value;
    public float unk_x8;
}

public unsafe struct SetSysBacklightSettings
{
    public u32 auto_brightness_flags;
    public float screen_brightness;
    public SetSysLcdBacklightBrightnessMapping brightness_mapping;
    public float unk_x14;
    public float unk_x18;
    public float unk_x1C;
    public float unk_x20;
    public float unk_x24;
}

public unsafe struct SetSysBacklightSettingsEx
{
    public u32 auto_brightness_flags;
    public float screen_brightness;
    public float current_brightness_for_vr_mode;
    public SetSysLcdBacklightBrightnessMapping brightness_mapping;
    public float unk_x18;
    public float unk_x1C;
    public float unk_x20;
    public float unk_x24;
    public float unk_x28;
}

public unsafe struct SetSysBluetoothDevicesSettings
{
    public BtdrvAddress addr;
    public BtmBdName name;
    public BtmClassOfDevice class_of_device;
    public u8 link_key;
    public u8 link_key_present;
    public u16 version;
    public u32 trusted_services;
    public u16 vid;
    public u16 pid;
    public u8 sub_class;
    public u8 attribute_mask;
    public u16 descriptor_length;
    public u8 descriptor;
    public u8 key_type;
    public u8 device_type;
    public u16 brr_size;
    public u8 brr;
    public u8 audio_source_volume;
    public byte name2;
    public u8 audio_sink_volume;
    public u32 audio_flags;
    public u8 reserved;
}

public unsafe struct SetSysFirmwareVersion
{
    public u8 major;
    public u8 minor;
    public u8 micro;
    public u8 padding1;
    public u8 revision_major;
    public u8 revision_minor;
    public u8 padding2;
    public u8 padding3;
    public byte platform;
    public byte version_hash;
    public byte display_version;
    public byte display_title;
}

public unsafe struct SetSysFirmwareVersionDigest
{
    public byte digest;
}

public unsafe struct SetSysSerialNumber
{
    public byte number;
}

public unsafe struct SetSysDeviceNickName
{
    public byte nickname;
}

public unsafe struct SetSysUserSelectorSettings
{
    public u32 flags;
}

public unsafe struct SetSysAccountSettings
{
    public SetSysUserSelectorSettings settings;
}

public unsafe struct SetSysAudioVolume
{
    public u32 unk_x0;
    public u8 volume;
}

public unsafe struct SetSysEulaVersion
{
    public u32 version;
    public s32 region_code;
    public s32 clock_type;
    public u32 pad;
    public u64 network_clock_time;
    public TimeSteadyClockTimePoint steady_clock_time;
}

public unsafe struct SetSysNotificationTime
{
    public s32 hour;
    public s32 minute;
}

public unsafe struct SetSysNotificationSettings
{
    public u32 flags;
    public s32 volume;
    public SetSysNotificationTime start_time;
    public SetSysNotificationTime end_time;
}

public unsafe struct SetSysAccountNotificationSettings
{
    public AccountUid uid;
    public u32 flags;
    public s8 friend_presence_overlay_permission;
    public fixed u8 pad[3];
}

public unsafe struct SetSysTvSettings
{
    public u32 flags;
    public s32 tv_resolution;
    public s32 hdmi_content_type;
    public s32 rgb_range;
    public s32 cmu_mode;
    public u32 underscan;
    public float gamma;
    public float contrast;
}

public unsafe struct SetSysModeLine
{
    public u16 pixel_clock;
    public u8 horizontal_active_pixels_lsb;
    public u8 horizontal_blanking_pixels_lsb;
    public u8 horizontal_blanking_pixels_msb : _4;
    public u8 horizontal_active_pixels_msb : _4;
    public u8 vertical_active_lines_lsb;
    public u8 vertical_blanking_lines_lsb;
    public u8 vertical_blanking_lines_msb : _4;
    public u8 vertical_active_lines_msb : _4;
    public u8 horizontal_sync_offset_pixels_lsb;
    public u8 horizontal_sync_pulse_width_pixels_lsb;
    public u8 vertical_sync_pulse_width_lines_lsb : _4;
    public u8 vertical_sync_offset_lines_lsb : _4;
    public u8 vertical_sync_pulse_width_lines_msb : _2;
    public u8 vertical_sync_offset_lines_msb : _2;
    public u8 horizontal_sync_pulse_width_pixels_msb : _2;
    public u8 horizontal_sync_offset_pixels_msb : _2;
    public u8 horizontal_image_size_mm_lsb;
    public u8 vertical_image_size_mm_lsb;
    public u8 vertical_image_size_mm_msb : _4;
    public u8 horizontal_image_size_mm_msb : _4;
    public u8 horizontal_border_pixels;
    public u8 vertical_border_lines;
    public u8 features_bitmap_0 : _1;
    public u8 features_bitmap_1 : _1;
    public u8 features_bitmap_2 : _1;
    public u8 features_bitmap_34 : _2;
    public u8 features_bitmap_56 : _2;
    public u8 interlaced : _1;
}

public unsafe struct SetSysDataDeletionSettings
{
    public u32 flags;
    public s32 use_count;
}

public unsafe struct SetSysSleepSettings
{
    public u32 flags;
    public s32 handheld_sleep_plan;
    public s32 console_sleep_plan;
}

public unsafe struct SetSysInitialLaunchSettings
{
    public u32 flags;
    public u32 pad;
    public TimeSteadyClockTimePoint timestamp;
}

public unsafe struct SetSysPtmFuelGaugeParameter
{
    public u16 rcomp0;
    public u16 tempc0;
    public u16 fullcap;
    public u16 fullcapnom;
    public u16 lavgempty;
    public u16 qresidual00;
    public u16 qresidual10;
    public u16 qresidual20;
    public u16 qresidual30;
    public u16 cycles;
    public u32 cycles_actual;
}

public unsafe struct SetSysColor4u8Type
{
    public fixed u8 field[4];
}

public unsafe struct SetSysNxControllerLegacySettings
{
    public BtdrvAddress address;
    public u8 type;
    public byte serial;
    public SetSysColor4u8Type body_color;
    public SetSysColor4u8Type button_color;
    public fixed u8 unk_x1F[8];
    public u8 unk_x27;
    public u8 interface_type;
}

public unsafe struct SetSysNxControllerSettings
{
    public BtdrvAddress address;
    public u8 type;
    public byte serial;
    public SetSysColor4u8Type body_color;
    public SetSysColor4u8Type button_color;
    public fixed u8 unk_x1F[8];
    public u8 unk_x27;
    public u8 interface_type;
    public u8 unk_x29;
}

public unsafe struct SetSysConsoleSixAxisSensorAccelerationBias
{
    public float unk_x0;
    public float unk_x4;
    public float unk_x8;
}

public unsafe struct SetSysConsoleSixAxisSensorAngularVelocityBias
{
    public float unk_x0;
    public float unk_x4;
    public float unk_x8;
}

public unsafe struct SetSysConsoleSixAxisSensorAccelerationGain
{
    public float unk_x0;
    public float unk_x4;
    public float unk_x8;
    public float unk_xC;
    public float unk_x10;
    public float unk_x14;
    public float unk_x18;
    public float unk_x1C;
    public float unk_x20;
}

public unsafe struct SetSysConsoleSixAxisSensorAngularVelocityGain
{
    public float unk_x0;
    public float unk_x4;
    public float unk_x8;
    public float unk_xC;
    public float unk_x10;
    public float unk_x14;
    public float unk_x18;
    public float unk_x1C;
    public float unk_x20;
}

public unsafe struct SetSysAllowedSslHosts
{
    public u8 hosts;
}

public unsafe struct SetSysHostFsMountPoint
{
    public byte mount;
}

public unsafe struct SetSysBlePairingSettings
{
    public BtdrvAddress address;
    public u16 unk_x6;
    public u16 unk_x8;
    public u8 unk_xA;
    public u8 unk_xB;
    public u8 unk_xC;
    public u8 unk_xD;
    public u8 unk_xE;
    public u8 unk_xF;
    public u8 padding;
}

public unsafe struct SetSysConsoleSixAxisSensorAngularVelocityTimeBias
{
    public float unk_x0;
    public float unk_x4;
    public float unk_x8;
}

public unsafe struct SetSysConsoleSixAxisSensorAngularAcceleration
{
    public float unk_x0;
    public float unk_x4;
    public float unk_x8;
    public float unk_xC;
    public float unk_x10;
    public float unk_x14;
    public float unk_x18;
    public float unk_x1C;
    public float unk_x20;
}

public unsafe struct SetSysRebootlessSystemUpdateVersion
{
    public u32 version;
    public u8 reserved;
    public byte display_version;
}

public unsafe struct SetSysAccountOnlineStorageSettings
{
    public AccountUid uid;
    public u32 unk_x10;
    public u32 unk_x14;
}

public unsafe struct SetSysAnalogStickUserCalibration
{
    public u16 unk_x0;
    public u16 unk_x2;
    public u16 unk_x4;
    public u16 unk_x6;
    public u16 unk_x8;
    public u16 unk_xA;
    public u16 unk_xC;
    public u16 unk_xE;
}

public unsafe struct SetSysThemeId
{
    public u64 theme_id;
}

public unsafe struct SetSysThemeSettings
{
    public u64 theme_settings;
}

public unsafe struct SetSysHomeMenuScheme
{
    public u32 main_color;
    public u32 back_color;
    public u32 sub_color;
    public u32 bezel_color;
    public u32 extra_color;
}

public unsafe struct SetSysButtonConfigSettings
{
    public u8 settings;
}

public unsafe struct SetSysButtonConfigRegisteredSettings
{
    public u8 settings;
}

public unsafe struct SetCalAccelerometerOffset
{
    public u8 offset;
}

public unsafe struct SetCalAccelerometerScale
{
    public u8 scale;
}

public unsafe struct SetCalAmiiboEcdsaCertificate
{
    public u32 size;
    public u8 cert;
}

public unsafe struct SetCalAmiiboEcqvBlsCertificate
{
    public u32 size;
    public u8 cert;
}

public unsafe struct SetCalAmiiboEcqvBlsKey
{
    public u32 size;
    public u8 key;
    public u32 generation;
}

public unsafe struct SetCalAmiiboEcqvBlsRootCertificate
{
    public u32 size;
    public u8 cert;
}

public unsafe struct SetCalAmiiboEcqvCertificate
{
    public u32 size;
    public u8 cert;
}

public unsafe struct SetCalAmiiboKey
{
    public u32 size;
    public u8 key;
    public u32 generation;
}

public unsafe struct SetCalAnalogStickFactoryCalibration
{
    public u8 calibration;
}

public unsafe struct SetCalAnalogStickModelParameter
{
    public u8 parameter;
}

public unsafe struct SetCalBdAddress
{
    public u8 bd_addr;
}

public unsafe struct SetCalConfigurationId1
{
    public u8 cfg;
}

public unsafe struct SetCalConsoleSixAxisSensorHorizontalOffset
{
    public u8 offset;
}

public unsafe struct SetCalCountryCode
{
    public byte code;
}

public unsafe struct SetCalEccB233DeviceCertificate
{
    public u8 cert;
}

public unsafe struct SetCalEccB233DeviceKey
{
    public u32 size;
    public u8 key;
    public u32 generation;
}

public unsafe struct SetCalGameCardCertificate
{
    public u8 cert;
}

public unsafe struct SetCalGameCardKey
{
    public u32 size;
    public u8 key;
    public u32 generation;
}

public unsafe struct SetCalGyroscopeOffset
{
    public u8 offset;
}

public unsafe struct SetCalGyroscopeScale
{
    public u8 scale;
}

public unsafe struct SetCalMacAddress
{
    public u8 addr;
}

public unsafe struct SetCalRsa2048DeviceCertificate
{
    public u8 cert;
}

public unsafe struct SetCalRsa2048DeviceKey
{
    public u32 size;
    public u8 key;
    public u32 generation;
}

public unsafe struct SetCalSpeakerParameter
{
    public u8 parameter;
}

public unsafe struct SetCalSslCertificate
{
    public u32 size;
    public u8 cert;
}

public unsafe struct SetCalSslKey
{
    public u32 size;
    public u8 key;
    public u32 generation;
}

public unsafe struct SetCalRegionCode
{
    public u32 code;
}

public static class Set
{
    public static extern Result setInitialize();
    public static extern void setExit();
    public static extern IntPtr setGetServiceSession();
    public static extern Result setMakeLanguage(u64 LanguageCode, ref SetLanguage Language);
    public static extern Result setMakeLanguageCode(SetLanguage Language, ref u64 LanguageCode);
    public static extern Result setGetSystemLanguage(ref u64 LanguageCode);
    public static extern Result setGetLanguageCode(ref u64 LanguageCode);
    public static extern Result setGetAvailableLanguageCodes(ref s32 total_entries, ref u64 LanguageCodes, ulong max_entries);
    public static extern Result setGetAvailableLanguageCodeCount(ref s32 total);
    public static extern Result setGetRegionCode(ref SetRegion @out);
    public static extern Result setGetQuestFlag(ref bool @out);
    public static extern Result setGetDeviceNickname(ref SetSysDeviceNickName nickname);
    public static extern Result setsysInitialize();
    public static extern void setsysExit();
    public static extern IntPtr setsysGetServiceSession();
    public static extern Result setsysSetLanguageCode(u64 LanguageCode);
    public static extern Result setsysSetNetworkSettings(ref SetSysNetworkSettings settings, s32 count);
    public static extern Result setsysGetNetworkSettings(ref s32 total_out, ref SetSysNetworkSettings settings, s32 count);
    public static extern Result setsysGetFirmwareVersion(ref SetSysFirmwareVersion @out);
    public static extern Result setsysGetFirmwareVersionDigest(ref SetSysFirmwareVersionDigest @out);
    public static extern Result setsysGetLockScreenFlag(ref bool @out);
    public static extern Result setsysSetLockScreenFlag(bool flag);
    public static extern Result setsysGetBacklightSettings(ref SetSysBacklightSettings @out);
    public static extern Result setsysSetBacklightSettings(ref SetSysBacklightSettings settings);
    public static extern Result setsysSetBluetoothDevicesSettings(ref SetSysBluetoothDevicesSettings settings, s32 count);
    public static extern Result setsysGetBluetoothDevicesSettings(ref s32 total_out, ref SetSysBluetoothDevicesSettings settings, s32 count);
    public static extern Result setsysGetExternalSteadyClockSourceId(ref Uuid @out);
    public static extern Result setsysSetExternalSteadyClockSourceId(ref Uuid uuid);
    public static extern Result setsysGetUserSystemClockContext(ref TimeSystemClockContext @out);
    public static extern Result setsysSetUserSystemClockContext(ref TimeSystemClockContext context);
    public static extern Result setsysGetAccountSettings(ref SetSysAccountSettings @out);
    public static extern Result setsysSetAccountSettings(SetSysAccountSettings settings);
    public static extern Result setsysGetAudioVolume(SetSysAudioDevice device, ref SetSysAudioVolume @out);
    public static extern Result setsysSetAudioVolume(SetSysAudioDevice device, ref SetSysAudioVolume volume);
    public static extern Result setsysGetEulaVersions(ref s32 total_out, ref SetSysEulaVersion versions, s32 count);
    public static extern Result setsysSetEulaVersions(ref SetSysEulaVersion versions, s32 count);
    public static extern Result setsysGetColorSetId(ref ColorSetId @out);
    public static extern Result setsysSetColorSetId(ColorSetId id);
    public static extern Result setsysGetConsoleInformationUploadFlag(ref bool @out);
    public static extern Result setsysSetConsoleInformationUploadFlag(bool flag);
    public static extern Result setsysGetAutomaticApplicationDownloadFlag(ref bool @out);
    public static extern Result setsysSetAutomaticApplicationDownloadFlag(bool flag);
    public static extern Result setsysGetNotificationSettings(ref SetSysNotificationSettings @out);
    public static extern Result setsysSetNotificationSettings(ref SetSysNotificationSettings settings);
    public static extern Result setsysGetAccountNotificationSettings(ref s32 total_out, ref SetSysAccountNotificationSettings settings, s32 count);
    public static extern Result setsysSetAccountNotificationSettings(ref SetSysAccountNotificationSettings settings, s32 count);
    public static extern Result setsysGetVibrationMasterVolume(ref float @out);
    public static extern Result setsysSetVibrationMasterVolume(float volume);
    public static extern Result setsysGetSettingsItemValueSize(ref byte name, ref byte item_key, ref u64 size_out);
    public static extern Result setsysGetSettingsItemValue(ref byte name, ref byte item_key, IntPtr value_out, ulong value_out_size, ref u64 size_out);
    public static extern Result setsysGetTvSettings(ref SetSysTvSettings @out);
    public static extern Result setsysSetTvSettings(ref SetSysTvSettings settings);
    public static extern Result setsysGetEdid(ref SetSysEdid @out);
    public static extern Result setsysSetEdid(ref SetSysEdid edid);
    public static extern Result setsysGetAudioOutputMode(SetSysAudioOutputModeTarget target, ref SetSysAudioOutputMode @out);
    public static extern Result setsysSetAudioOutputMode(SetSysAudioOutputModeTarget target, SetSysAudioOutputMode mode);
    public static extern Result setsysGetSpeakerAutoMuteFlag(ref bool @out);
    public static extern Result setsysSetSpeakerAutoMuteFlag(bool flag);
    public static extern Result setsysGetQuestFlag(ref bool @out);
    public static extern Result setsysSetQuestFlag(bool flag);
    public static extern Result setsysGetDataDeletionSettings(ref SetSysDataDeletionSettings @out);
    public static extern Result setsysSetDataDeletionSettings(ref SetSysDataDeletionSettings settings);
    public static extern Result setsysGetInitialSystemAppletProgramId(ref u64 @out);
    public static extern Result setsysGetOverlayDispProgramId(ref u64 @out);
    public static extern Result setsysGetDeviceTimeZoneLocationName(ref TimeLocationName @out);
    public static extern Result setsysSetDeviceTimeZoneLocationName(ref TimeLocationName name);
    public static extern Result setsysGetWirelessCertificationFileSize(ref u64 out_size);
    public static extern Result setsysGetWirelessCertificationFile(IntPtr buffer, ulong size, ref u64 out_size);
    public static extern Result setsysSetRegionCode(SetRegion region);
    public static extern Result setsysGetNetworkSystemClockContext(ref TimeSystemClockContext @out);
    public static extern Result setsysSetNetworkSystemClockContext(ref TimeSystemClockContext context);
    public static extern Result setsysIsUserSystemClockAutomaticCorrectionEnabled(ref bool @out);
    public static extern Result setsysSetUserSystemClockAutomaticCorrectionEnabled(bool flag);
    public static extern Result setsysGetDebugModeFlag(ref bool @out);
    public static extern Result setsysGetPrimaryAlbumStorage(ref SetSysPrimaryAlbumStorage @out);
    public static extern Result setsysSetPrimaryAlbumStorage(SetSysPrimaryAlbumStorage storage);
    public static extern Result setsysGetUsb30EnableFlag(ref bool @out);
    public static extern Result setsysSetUsb30EnableFlag(bool flag);
    public static extern Result setsysGetBatteryLot(ref SetBatteryLot @out);
    public static extern Result setsysGetSerialNumber(ref SetSysSerialNumber @out);
    public static extern Result setsysGetNfcEnableFlag(ref bool @out);
    public static extern Result setsysSetNfcEnableFlag(bool flag);
    public static extern Result setsysGetSleepSettings(ref SetSysSleepSettings @out);
    public static extern Result setsysSetSleepSettings(ref SetSysSleepSettings settings);
    public static extern Result setsysGetWirelessLanEnableFlag(ref bool @out);
    public static extern Result setsysSetWirelessLanEnableFlag(bool flag);
    public static extern Result setsysGetInitialLaunchSettings(ref SetSysInitialLaunchSettings @out);
    public static extern Result setsysSetInitialLaunchSettings(ref SetSysInitialLaunchSettings settings);
    public static extern Result setsysGetDeviceNickname(ref SetSysDeviceNickName nickname);
    public static extern Result setsysSetDeviceNickname(ref SetSysDeviceNickName nickname);
    public static extern Result setsysGetProductModel(ref SetSysProductModel model);
    public static extern Result setsysGetLdnChannel(ref s32 @out);
    public static extern Result setsysSetLdnChannel(s32 channel);
    public static extern Result setsysAcquireTelemetryDirtyFlagEventHandle(ref Event out_event);
    public static extern Result setsysGetTelemetryDirtyFlags(ref u64 flags_0, ref u64 flags_1);
    public static extern Result setsysGetPtmBatteryLot(ref SetBatteryLot @out);
    public static extern Result setsysSetPtmBatteryLot(ref SetBatteryLot lot);
    public static extern Result setsysGetPtmFuelGaugeParameter(ref SetSysPtmFuelGaugeParameter @out);
    public static extern Result setsysSetPtmFuelGaugeParameter(ref SetSysPtmFuelGaugeParameter parameter);
    public static extern Result setsysGetBluetoothEnableFlag(ref bool @out);
    public static extern Result setsysSetBluetoothEnableFlag(bool flag);
    public static extern Result setsysGetMiiAuthorId(ref Uuid @out);
    public static extern Result setsysSetShutdownRtcValue(u64 value);
    public static extern Result setsysGetShutdownRtcValue(ref u64 @out);
    public static extern Result setsysAcquireFatalDirtyFlagEventHandle(ref Event out_event);
    public static extern Result setsysGetFatalDirtyFlags(ref u64 flags_0, ref u64 flags_1);
    public static extern Result setsysGetAutoUpdateEnableFlag(ref bool @out);
    public static extern Result setsysSetAutoUpdateEnableFlag(bool flag);
    public static extern Result setsysGetNxControllerSettings(ref s32 total_out, ref SetSysNxControllerLegacySettings settings, s32 count);
    public static extern Result setsysSetNxControllerSettings(ref SetSysNxControllerLegacySettings settings, s32 count);
    public static extern Result setsysGetBatteryPercentageFlag(ref bool @out);
    public static extern Result setsysSetBatteryPercentageFlag(bool flag);
    public static extern Result setsysGetExternalRtcResetFlag(ref bool @out);
    public static extern Result setsysSetExternalRtcResetFlag(bool flag);
    public static extern Result setsysGetUsbFullKeyEnableFlag(ref bool @out);
    public static extern Result setsysSetUsbFullKeyEnableFlag(bool flag);
    public static extern Result setsysSetExternalSteadyClockInternalOffset(u64 offset);
    public static extern Result setsysGetExternalSteadyClockInternalOffset(ref u64 @out);
    public static extern Result setsysGetBacklightSettingsEx(ref SetSysBacklightSettingsEx @out);
    public static extern Result setsysSetBacklightSettingsEx(ref SetSysBacklightSettingsEx settings);
    public static extern Result setsysGetHeadphoneVolumeWarningCount(ref u32 @out);
    public static extern Result setsysSetHeadphoneVolumeWarningCount(u32 count);
    public static extern Result setsysGetBluetoothAfhEnableFlag(ref bool @out);
    public static extern Result setsysSetBluetoothAfhEnableFlag(bool flag);
    public static extern Result setsysGetBluetoothBoostEnableFlag(ref bool @out);
    public static extern Result setsysSetBluetoothBoostEnableFlag(bool flag);
    public static extern Result setsysGetInRepairProcessEnableFlag(ref bool @out);
    public static extern Result setsysSetInRepairProcessEnableFlag(bool flag);
    public static extern Result setsysGetHeadphoneVolumeUpdateFlag(ref bool @out);
    public static extern Result setsysSetHeadphoneVolumeUpdateFlag(bool flag);
    public static extern Result setsysNeedsToUpdateHeadphoneVolume(ref u8 a0, ref u8 a1, ref u8 a2, bool flag);
    public static extern Result setsysGetPushNotificationActivityModeOnSleep(ref u32 @out);
    public static extern Result setsysSetPushNotificationActivityModeOnSleep(u32 mode);
    public static extern Result setsysGetServiceDiscoveryControlSettings(ref SetSysServiceDiscoveryControlSettings @out);
    public static extern Result setsysSetServiceDiscoveryControlSettings(SetSysServiceDiscoveryControlSettings settings);
    public static extern Result setsysGetErrorReportSharePermission(ref SetSysErrorReportSharePermission @out);
    public static extern Result setsysSetErrorReportSharePermission(SetSysErrorReportSharePermission permission);
    public static extern Result setsysGetAppletLaunchFlags(ref u32 @out);
    public static extern Result setsysSetAppletLaunchFlags(u32 flags);
    public static extern Result setsysGetConsoleSixAxisSensorAccelerationBias(ref SetSysConsoleSixAxisSensorAccelerationBias @out);
    public static extern Result setsysSetConsoleSixAxisSensorAccelerationBias(ref SetSysConsoleSixAxisSensorAccelerationBias bias);
    public static extern Result setsysGetConsoleSixAxisSensorAngularVelocityBias(ref SetSysConsoleSixAxisSensorAngularVelocityBias @out);
    public static extern Result setsysSetConsoleSixAxisSensorAngularVelocityBias(ref SetSysConsoleSixAxisSensorAngularVelocityBias bias);
    public static extern Result setsysGetConsoleSixAxisSensorAccelerationGain(ref SetSysConsoleSixAxisSensorAccelerationGain @out);
    public static extern Result setsysSetConsoleSixAxisSensorAccelerationGain(ref SetSysConsoleSixAxisSensorAccelerationGain gain);
    public static extern Result setsysGetConsoleSixAxisSensorAngularVelocityGain(ref SetSysConsoleSixAxisSensorAngularVelocityGain @out);
    public static extern Result setsysSetConsoleSixAxisSensorAngularVelocityGain(ref SetSysConsoleSixAxisSensorAngularVelocityGain gain);
    public static extern Result setsysGetKeyboardLayout(ref SetKeyboardLayout @out);
    public static extern Result setsysSetKeyboardLayout(SetKeyboardLayout layout);
    public static extern Result setsysGetWebInspectorFlag(ref bool @out);
    public static extern Result setsysGetAllowedSslHosts(ref s32 total_out, ref SetSysAllowedSslHosts @out, s32 count);
    public static extern Result setsysGetHostFsMountPoint(ref SetSysHostFsMountPoint @out);
    public static extern Result setsysGetRequiresRunRepairTimeReviser(ref bool @out);
    public static extern Result setsysSetRequiresRunRepairTimeReviser(bool flag);
    public static extern Result setsysSetBlePairingSettings(ref SetSysBlePairingSettings settings, s32 count);
    public static extern Result setsysGetBlePairingSettings(ref s32 total_out, ref SetSysBlePairingSettings settings, s32 count);
    public static extern Result setsysGetConsoleSixAxisSensorAngularVelocityTimeBias(ref SetSysConsoleSixAxisSensorAngularVelocityTimeBias @out);
    public static extern Result setsysSetConsoleSixAxisSensorAngularVelocityTimeBias(ref SetSysConsoleSixAxisSensorAngularVelocityTimeBias bias);
    public static extern Result setsysGetConsoleSixAxisSensorAngularAcceleration(ref SetSysConsoleSixAxisSensorAngularAcceleration @out);
    public static extern Result setsysSetConsoleSixAxisSensorAngularAcceleration(ref SetSysConsoleSixAxisSensorAngularAcceleration acceleration);
    public static extern Result setsysGetRebootlessSystemUpdateVersion(ref SetSysRebootlessSystemUpdateVersion @out);
    public static extern Result setsysGetDeviceTimeZoneLocationUpdatedTime(ref TimeSteadyClockTimePoint @out);
    public static extern Result setsysSetDeviceTimeZoneLocationUpdatedTime(ref TimeSteadyClockTimePoint time_point);
    public static extern Result setsysGetUserSystemClockAutomaticCorrectionUpdatedTime(ref TimeSteadyClockTimePoint @out);
    public static extern Result setsysSetUserSystemClockAutomaticCorrectionUpdatedTime(ref TimeSteadyClockTimePoint time_point);
    public static extern Result setsysGetAccountOnlineStorageSettings(ref s32 total_out, ref SetSysAccountOnlineStorageSettings settings, s32 count);
    public static extern Result setsysSetAccountOnlineStorageSettings(ref SetSysAccountOnlineStorageSettings settings, s32 count);
    public static extern Result setsysGetPctlReadyFlag(ref bool @out);
    public static extern Result setsysSetPctlReadyFlag(bool flag);
    public static extern Result setsysGetAnalogStickUserCalibrationL(ref SetSysAnalogStickUserCalibration @out);
    public static extern Result setsysSetAnalogStickUserCalibrationL(ref SetSysAnalogStickUserCalibration calibration);
    public static extern Result setsysGetAnalogStickUserCalibrationR(ref SetSysAnalogStickUserCalibration @out);
    public static extern Result setsysSetAnalogStickUserCalibrationR(ref SetSysAnalogStickUserCalibration calibration);
    public static extern Result setsysGetPtmBatteryVersion(ref u8 @out);
    public static extern Result setsysSetPtmBatteryVersion(u8 version);
    public static extern Result setsysGetUsb30HostEnableFlag(ref bool @out);
    public static extern Result setsysSetUsb30HostEnableFlag(bool flag);
    public static extern Result setsysGetUsb30DeviceEnableFlag(ref bool @out);
    public static extern Result setsysSetUsb30DeviceEnableFlag(bool flag);
    public static extern Result setsysGetThemeId(s32 type, ref SetSysThemeId @out);
    public static extern Result setsysSetThemeId(s32 type, ref SetSysThemeId theme_id);
    public static extern Result setsysGetChineseTraditionalInputMethod(ref SetChineseTraditionalInputMethod @out);
    public static extern Result setsysSetChineseTraditionalInputMethod(SetChineseTraditionalInputMethod method);
    public static extern Result setsysGetPtmCycleCountReliability(ref SetSysPtmCycleCountReliability @out);
    public static extern Result setsysSetPtmCycleCountReliability(SetSysPtmCycleCountReliability reliability);
    public static extern Result setsysGetHomeMenuScheme(ref SetSysHomeMenuScheme @out);
    public static extern Result setsysGetThemeSettings(ref SetSysThemeSettings @out);
    public static extern Result setsysSetThemeSettings(ref SetSysThemeSettings settings);
    public static extern Result setsysGetThemeKey(ref FsArchiveMacKey @out);
    public static extern Result setsysSetThemeKey(ref FsArchiveMacKey key);
    public static extern Result setsysGetZoomFlag(ref bool @out);
    public static extern Result setsysSetZoomFlag(bool flag);
    public static extern Result setsysGetT(ref bool @out);
    public static extern Result setsysSetT(bool flag);
    public static extern Result setsysGetPlatformRegion(ref SetSysPlatformRegion @out);
    public static extern Result setsysSetPlatformRegion(SetSysPlatformRegion region);
    public static extern Result setsysGetHomeMenuSchemeModel(ref u32 @out);
    public static extern Result setsysGetMemoryUsageRateFlag(ref bool @out);
    public static extern Result setsysGetTouchScreenMode(ref SetSysTouchScreenMode @out);
    public static extern Result setsysSetTouchScreenMode(SetSysTouchScreenMode mode);
    public static extern Result setsysGetButtonConfigSettingsFull(ref s32 total_out, ref SetSysButtonConfigSettings settings, s32 count);
    public static extern Result setsysSetButtonConfigSettingsFull(ref SetSysButtonConfigSettings settings, s32 count);
    public static extern Result setsysGetButtonConfigSettingsEmbedded(ref s32 total_out, ref SetSysButtonConfigSettings settings, s32 count);
    public static extern Result setsysSetButtonConfigSettingsEmbedded(ref SetSysButtonConfigSettings settings, s32 count);
    public static extern Result setsysGetButtonConfigSettingsLeft(ref s32 total_out, ref SetSysButtonConfigSettings settings, s32 count);
    public static extern Result setsysSetButtonConfigSettingsLeft(ref SetSysButtonConfigSettings settings, s32 count);
    public static extern Result setsysGetButtonConfigSettingsRight(ref s32 total_out, ref SetSysButtonConfigSettings settings, s32 count);
    public static extern Result setsysSetButtonConfigSettingsRight(ref SetSysButtonConfigSettings settings, s32 count);
    public static extern Result setsysGetButtonConfigRegisteredSettingsEmbedded(ref SetSysButtonConfigRegisteredSettings settings);
    public static extern Result setsysSetButtonConfigRegisteredSettingsEmbedded(ref SetSysButtonConfigRegisteredSettings settings);
    public static extern Result setsysGetButtonConfigRegisteredSettings(ref s32 total_out, ref SetSysButtonConfigRegisteredSettings settings, s32 count);
    public static extern Result setsysSetButtonConfigRegisteredSettings(ref SetSysButtonConfigRegisteredSettings settings, s32 count);
    public static extern Result setsysGetFieldTestingFlag(ref bool @out);
    public static extern Result setsysSetFieldTestingFlag(bool flag);
    public static extern Result setsysGetNxControllerSettingsEx(ref s32 total_out, ref SetSysNxControllerSettings settings, s32 count);
    public static extern Result setsysSetNxControllerSettingsEx(ref SetSysNxControllerSettings settings, s32 count);
    public static extern Result setcalInitialize();
    public static extern void setcalExit();
    public static extern IntPtr setcalGetServiceSession();
    public static extern Result setcalGetBdAddress(ref SetCalBdAddress @out);
    public static extern Result setcalGetConfigurationId1(ref SetCalConfigurationId1 @out);
    public static extern Result setcalGetAccelerometerOffset(ref SetCalAccelerometerOffset @out);
    public static extern Result setcalGetAccelerometerScale(ref SetCalAccelerometerScale @out);
    public static extern Result setcalGetGyroscopeOffset(ref SetCalGyroscopeOffset @out);
    public static extern Result setcalGetGyroscopeScale(ref SetCalGyroscopeScale @out);
    public static extern Result setcalGetWirelessLanMacAddress(ref SetCalMacAddress @out);
    public static extern Result setcalGetWirelessLanCountryCodeCount(ref s32 out_count);
    public static extern Result setcalGetWirelessLanCountryCodes(ref s32 total_out, ref SetCalCountryCode codes, s32 count);
    public static extern Result setcalGetSerialNumber(ref SetCalSerialNumber @out);
    public static extern Result setcalSetInitialSystemAppletProgramId(u64 program_id);
    public static extern Result setcalSetOverlayDispProgramId(u64 program_id);
    public static extern Result setcalGetBatteryLot(ref SetBatteryLot @out);
    public static extern Result setcalGetEciDeviceCertificate(ref SetCalEccB233DeviceCertificate @out);
    public static extern Result setcalGetEticketDeviceCertificate(ref SetCalRsa2048DeviceCertificate @out);
    public static extern Result setcalGetSslKey(ref SetCalSslKey @out);
    public static extern Result setcalGetSslCertificate(ref SetCalSslCertificate @out);
    public static extern Result setcalGetGameCardKey(ref SetCalGameCardKey @out);
    public static extern Result setcalGetGameCardCertificate(ref SetCalGameCardCertificate @out);
    public static extern Result setcalGetEciDeviceKey(ref SetCalEccB233DeviceKey @out);
    public static extern Result setcalGetEticketDeviceKey(ref SetCalRsa2048DeviceKey @out);
    public static extern Result setcalGetSpeakerParameter(ref SetCalSpeakerParameter @out);
    public static extern Result setcalGetLcdVendorId(ref u32 out_vendor_id);
    public static extern Result setcalGetEciDeviceCertificate2(ref SetCalRsa2048DeviceCertificate @out);
    public static extern Result setcalGetEciDeviceKey2(ref SetCalRsa2048DeviceKey @out);
    public static extern Result setcalGetAmiiboKey(ref SetCalAmiiboKey @out);
    public static extern Result setcalGetAmiiboEcqvCertificate(ref SetCalAmiiboEcqvCertificate @out);
    public static extern Result setcalGetAmiiboEcdsaCertificate(ref SetCalAmiiboEcdsaCertificate @out);
    public static extern Result setcalGetAmiiboEcqvBlsKey(ref SetCalAmiiboEcqvBlsKey @out);
    public static extern Result setcalGetAmiiboEcqvBlsCertificate(ref SetCalAmiiboEcqvBlsCertificate @out);
    public static extern Result setcalGetAmiiboEcqvBlsRootCertificate(ref SetCalAmiiboEcqvBlsRootCertificate @out);
    public static extern Result setcalGetUsbTypeCPowerSourceCircuitVersion(ref u8 out_version);
    public static extern Result setcalGetAnalogStickModuleTypeL(ref u8 out_type);
    public static extern Result setcalGetAnalogStickModelParameterL(ref SetCalAnalogStickModelParameter @out);
    public static extern Result setcalGetAnalogStickFactoryCalibrationL(ref SetCalAnalogStickFactoryCalibration @out);
    public static extern Result setcalGetAnalogStickModuleTypeR(ref u8 out_type);
    public static extern Result setcalGetAnalogStickModelParameterR(ref SetCalAnalogStickModelParameter @out);
    public static extern Result setcalGetAnalogStickFactoryCalibrationR(ref SetCalAnalogStickFactoryCalibration @out);
    public static extern Result setcalGetConsoleSixAxisSensorModuleType(ref u8 out_type);
    public static extern Result setcalGetConsoleSixAxisSensorHorizontalOffset(ref SetCalConsoleSixAxisSensorHorizontalOffset @out);
    public static extern Result setcalGetBatteryVersion(ref u8 out_version);
    public static extern Result setcalGetDeviceId(ref u64 out_device_id);
    public static extern Result setcalGetConsoleSixAxisSensorMountType(ref u8 out_type);
}
