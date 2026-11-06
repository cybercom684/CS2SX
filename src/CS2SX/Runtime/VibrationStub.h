#pragma once
// ============================================================================
// Runtime/VibrationStub.h — HD Rumble wrapper for CS2SX
//
// Wraps the libnx HID vibration API for JoyCon and Pro Controller rumble.
// Detects the active controller style via padGetStyleSet so the correct
// vibration device handles are initialized.
// ============================================================================

#include <switch.h>

static HidVibrationDeviceHandle _cs2sx_vib_handles[2];
static s32  _cs2sx_vib_count = 0;
static bool _cs2sx_vib_ok    = false;

static inline void _cs2sx_vib_ensure(void)
{
    if (_cs2sx_vib_ok) return;

    // Wait until the pad has been updated at least once
    u32 styleSet = padGetStyleSet(&g_cs2sx_pad);
    if (styleSet == 0) return;

    _cs2sx_vib_ok = true;

    // Initialise handles only for the currently active controller type.
    // hidInitializeVibrationDevices succeeds for ANY valid parameters, even
    // when the device isn't present, so we MUST query the actual style first.
    if (styleSet & HidNpadStyleTag_NpadHandheld) {
        if (R_SUCCEEDED(hidInitializeVibrationDevices(
                _cs2sx_vib_handles, 2,
                HidNpadIdType_Handheld, HidNpadStyleTag_NpadHandheld)))
        { _cs2sx_vib_count = 2; return; }
    }
    if (styleSet & HidNpadStyleTag_NpadJoyDual) {
        if (R_SUCCEEDED(hidInitializeVibrationDevices(
                _cs2sx_vib_handles, 2,
                HidNpadIdType_No1, HidNpadStyleTag_NpadJoyDual)))
        { _cs2sx_vib_count = 2; return; }
    }
    if (styleSet & HidNpadStyleTag_NpadFullKey) {
        if (R_SUCCEEDED(hidInitializeVibrationDevices(
                _cs2sx_vib_handles, 1,
                HidNpadIdType_No1, HidNpadStyleTag_NpadFullKey)))
        { _cs2sx_vib_count = 1; return; }
    }
    // Fallback: try Handheld regardless
    if (R_SUCCEEDED(hidInitializeVibrationDevices(
            _cs2sx_vib_handles, 2,
            HidNpadIdType_Handheld, HidNpadStyleTag_NpadHandheld)))
        _cs2sx_vib_count = 2;
}

static inline void CS2SX_Vibration_Rumble(
    float lowFreq, float lowAmp, float highFreq, float highAmp)
{
    _cs2sx_vib_ensure();
    if (_cs2sx_vib_count == 0) return;

    HidVibrationValue val;
    val.amp_low   = lowAmp  < 0.0f ? 0.0f : (lowAmp  > 1.0f ? 1.0f : lowAmp);
    val.freq_low  = lowFreq;
    val.amp_high  = highAmp < 0.0f ? 0.0f : (highAmp > 1.0f ? 1.0f : highAmp);
    val.freq_high = highFreq;

    HidVibrationValue vals[2];
    vals[0] = val;
    vals[1] = val;
    hidSendVibrationValues(_cs2sx_vib_handles, vals, _cs2sx_vib_count);
}

static inline void CS2SX_Vibration_RumbleSimple(float strength)
{
    CS2SX_Vibration_Rumble(160.0f, strength, 320.0f, strength * 0.7f);
}

static inline void CS2SX_Vibration_Pulse(float strength, int durationMs)
{
    (void)durationMs;
    CS2SX_Vibration_RumbleSimple(strength);
}

static inline void CS2SX_Vibration_Stop(void)
{
    _cs2sx_vib_ensure();
    if (_cs2sx_vib_count == 0) return;
    HidVibrationValue zero;
    zero.amp_low = 0.0f; zero.freq_low = 160.0f;
    zero.amp_high = 0.0f; zero.freq_high = 320.0f;
    HidVibrationValue zeros[2];
    zeros[0] = zero; zeros[1] = zero;
    hidSendVibrationValues(_cs2sx_vib_handles, zeros, _cs2sx_vib_count);
}
