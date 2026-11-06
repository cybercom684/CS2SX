// Runtime module for VibrationStub.h — generated split: state and non-trivial functions live
// here exactly once (a header `static` would give every translation unit its own copy).
#include "cs2sx_prelude.h"

HidVibrationDeviceHandle _cs2sx_vib_handles[2];
s32  _cs2sx_vib_count = 0;
bool _cs2sx_vib_ok    = false;
void _cs2sx_vib_ensure(void)
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

void CS2SX_Vibration_Rumble(
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

void CS2SX_Vibration_Stop(void)
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

