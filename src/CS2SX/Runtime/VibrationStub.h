#pragma once
// ============================================================================
// Runtime/VibrationStub.h — HD Rumble wrapper for CS2SX
//
// Wraps the libnx HID vibration API for JoyCon and Pro Controller rumble.
// Detects the active controller style via padGetStyleSet so the correct
// vibration device handles are initialized.
// ============================================================================

#include <switch.h>

extern HidVibrationDeviceHandle _cs2sx_vib_handles[2];
extern s32  _cs2sx_vib_count;
extern bool _cs2sx_vib_ok;

void _cs2sx_vib_ensure(void);

void CS2SX_Vibration_Rumble(
    float lowFreq, float lowAmp, float highFreq, float highAmp);

static inline void CS2SX_Vibration_RumbleSimple(float strength)
{
    CS2SX_Vibration_Rumble(160.0f, strength, 320.0f, strength * 0.7f);
}

/// Rumbles for durationMs, then stops by itself (CS2SX_Vibration_Tick, every frame).
void CS2SX_Vibration_Pulse(float strength, int durationMs);
/// Called by the app loop each frame and at exit (weak: only when vibration is used).
void CS2SX_Vibration_Tick(void);

void CS2SX_Vibration_Stop(void);
