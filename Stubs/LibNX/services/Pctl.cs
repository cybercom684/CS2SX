// Auto-generated from services\pctl.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct PctlRestrictionSettings
{
    public u8 rating_age;
    public bool sns_post_restriction;
    public bool free_communication_restriction;
}

public static class Pctl
{
    public static extern Result pctlInitialize();
    public static extern void pctlExit();
    public static extern IntPtr pctlGetServiceSession();
    public static extern IntPtr pctlGetServiceSession_Service();
    public static extern Result pctlIsRestrictionTemporaryUnlocked(ref bool flag);
    public static extern Result pctlConfirmStereoVisionPermission();
    public static extern Result pctlIsRestrictionEnabled(ref bool flag);
    public static extern Result pctlGetSafetyLevel(ref u32 safety_level);
    public static extern Result pctlGetCurrentSettings(ref PctlRestrictionSettings settings);
    public static extern Result pctlGetFreeCommunicationApplicationListCount(ref u32 count);
    public static extern Result pctlResetConfirmedStereoVisionPermission();
    public static extern Result pctlIsStereoVisionPermitted(ref bool flag);
    public static extern Result pctlIsPairingActive(ref bool flag);
    public static extern Result pctlGetSynchronizationEvent(ref Event out_event);
    public static extern Result pctlGetPlayTimerEventToRequestSuspension(ref Event out_event);
    public static extern Result pctlIsPlayTimerAlarmDisabled(ref bool flag);
    public static extern Result pctlGetUnlinkedEvent(ref Event out_event);
}
