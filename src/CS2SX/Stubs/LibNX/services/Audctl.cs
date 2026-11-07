// Auto-generated from services\audctl.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum AudioTarget
{
    AudioTarget_Invalid = 0,
    AudioTarget_Speaker = 1,
    AudioTarget_Headphone = 2,
    AudioTarget_Tv = 3,
    AudioTarget_UsbOutputDevice = 4,
    AudioTarget_Bluetooth = 5,
}

public enum AudioOutputMode
{
    AudioOutputMode_Invalid = 0,
    AudioOutputMode_Pcm1ch = 1,
    AudioOutputMode_Pcm2ch = 2,
    AudioOutputMode_Pcm6ch = 3,
    AudioOutputMode_PcmAuto = 4,
}

public enum AudioForceMutePolicy
{
    AudioForceMutePolicy_Disable = 0,
    AudioForceMutePolicy_SpeakerMuteOnHeadphoneUnplugged = 1,
}

public enum AudioHeadphoneOutputLevelMode
{
    AudioHeadphoneOutputLevelMode_Normal = 0,
    AudioHeadphoneOutputLevelMode_HighPower = 1,
}

public static class Audctl
{
    public static extern Result audctlInitialize();
    public static extern void audctlExit();
    public static extern IntPtr audctlGetServiceSession();
    public static extern Result audctlGetTargetVolume(ref s32 volume_out, AudioTarget target);
    public static extern Result audctlSetTargetVolume(AudioTarget target, s32 volume);
    public static extern Result audctlGetTargetVolumeMin(ref s32 volume_out);
    public static extern Result audctlGetTargetVolumeMax(ref s32 volume_out);
    public static extern Result audctlIsTargetMute(ref bool mute_out, AudioTarget target);
    public static extern Result audctlSetTargetMute(AudioTarget target, bool mute);
    public static extern Result audctlIsTargetConnected(ref bool connected_out, AudioTarget target);
    public static extern Result audctlSetDefaultTarget(AudioTarget target, u64 fade_in_ns, u64 fade_out_ns);
    public static extern Result audctlGetDefaultTarget(ref AudioTarget target_out);
    public static extern Result audctlGetAudioOutputMode(ref AudioOutputMode mode_out, AudioTarget target);
    public static extern Result audctlSetAudioOutputMode(AudioTarget target, AudioOutputMode mode);
    public static extern Result audctlSetForceMutePolicy(AudioForceMutePolicy policy);
    public static extern Result audctlGetForceMutePolicy(ref AudioForceMutePolicy policy_out);
    public static extern Result audctlGetOutputModeSetting(ref AudioOutputMode mode_out, AudioTarget target);
    public static extern Result audctlSetOutputModeSetting(AudioTarget target, AudioOutputMode mode);
    public static extern Result audctlSetOutputTarget(AudioTarget target);
    public static extern Result audctlSetInputTargetForceEnabled(bool enable);
    public static extern Result audctlSetHeadphoneOutputLevelMode(AudioHeadphoneOutputLevelMode mode);
    public static extern Result audctlGetHeadphoneOutputLevelMode(ref AudioHeadphoneOutputLevelMode mode_out);
    public static extern Result audctlAcquireAudioVolumeUpdateEventForPlayReport(ref Event event_out);
    public static extern Result audctlAcquireAudioOutputDeviceUpdateEventForPlayReport(ref Event event_out);
    public static extern Result audctlGetAudioOutputTargetForPlayReport(ref AudioTarget target_out);
    public static extern Result audctlNotifyHeadphoneVolumeWarningDisplayedEvent();
    public static extern Result audctlSetSystemOutputMasterVolume(float volume);
    public static extern Result audctlGetSystemOutputMasterVolume(ref float volume_out);
    public static extern Result audctlGetActiveOutputTarget(ref AudioTarget target);
}
