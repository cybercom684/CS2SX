#pragma once
// ============================================================================
// Runtime/MotionStub.h — Gyroscope / Accelerometer for CS2SX
//
// Wraps the libnx HidSixAxisSensor API.
// Detects the active controller style via padGetStyleSet so the correct
// sensor handles are initialized. Uses handles[0] for all readings.
// Field layout matches the C# MotionState struct in Stubs/Motion.cs.
// ============================================================================

#include <switch.h>
#include <string.h>

typedef struct {
    float accelX, accelY, accelZ;   ///< Beschleunigung in m/s²
    float gyroX,  gyroY,  gyroZ;    ///< Winkelgeschwindigkeit in rad/s
    float angleX, angleY, angleZ;   ///< Kumulierter Rotationswinkel in rad
} CS2SX_MotionState;

extern HidSixAxisSensorHandle _cs2sx_motion_handles[2];
extern s32  _cs2sx_motion_count;
extern bool _cs2sx_motion_ok;

void _cs2sx_motion_ensure(void);

static inline int CS2SX_Motion_IsAvailable(void)
{
    _cs2sx_motion_ensure();
    return _cs2sx_motion_count > 0;
}

CS2SX_MotionState CS2SX_Motion_Get(void);

void CS2SX_Motion_ResetAngles(void);
