// Auto-generated from services\lbl.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum LblBacklightSwitchStatus
{
    LblBacklightSwitchStatus_Disabled = 0,
    LblBacklightSwitchStatus_Enabled = 1,
    LblBacklightSwitchStatus_Enabling = 2,
    LblBacklightSwitchStatus_Disabling = 3,
}

public static class Lbl
{
    public static extern Result lblInitialize();
    public static extern void lblExit();
    public static extern IntPtr lblGetServiceSession();
    public static extern Result lblSaveCurrentSetting();
    public static extern Result lblLoadCurrentSetting();
    public static extern Result lblSetCurrentBrightnessSetting(float brightness);
    public static extern Result lblGetCurrentBrightnessSetting(ref float out_value);
    public static extern Result lblApplyCurrentBrightnessSettingToBacklight();
    public static extern Result lblGetBrightnessSettingAppliedToBacklight(ref float out_value);
    public static extern Result lblSwitchBacklightOn(u64 fade_time);
    public static extern Result lblSwitchBacklightOff(u64 fade_time);
    public static extern Result lblGetBacklightSwitchStatus(ref LblBacklightSwitchStatus out_value);
    public static extern Result lblEnableDimming();
    public static extern Result lblDisableDimming();
    public static extern Result lblIsDimmingEnabled(ref bool out_value);
    public static extern Result lblEnableAutoBrightnessControl();
    public static extern Result lblDisableAutoBrightnessControl();
    public static extern Result lblIsAutoBrightnessControlEnabled(ref bool out_value);
    public static extern Result lblSetAmbientLightSensorValue(float value);
    public static extern Result lblGetAmbientLightSensorValue(ref bool over_limit, ref float lux);
    public static extern Result lblIsAmbientLightSensorAvailable(ref bool out_value);
    public static extern Result lblSetCurrentBrightnessSettingForVrMode(float brightness);
    public static extern Result lblGetCurrentBrightnessSettingForVrMode(ref float out_value);
    public static extern Result lblEnableVrMode();
    public static extern Result lblDisableVrMode();
    public static extern Result lblIsVrModeEnabled(ref bool out_value);
}
