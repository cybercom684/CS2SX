// Runtime module for MotionStub.h — generated split: state and non-trivial functions live
// here exactly once (a header `static` would give every translation unit its own copy).
#include "cs2sx_prelude.h"

HidSixAxisSensorHandle _cs2sx_motion_handles[2];
s32  _cs2sx_motion_count = 0;
bool _cs2sx_motion_ok    = false;
void _cs2sx_motion_ensure(void)
{
    if (_cs2sx_motion_ok) return;

    // Wait until the pad has been updated at least once
    u32 styleSet = padGetStyleSet(&g_cs2sx_pad);
    if (styleSet == 0) return;

    _cs2sx_motion_ok = true;

    // hidGetSixAxisSensorHandles succeeds for ANY valid parameters even when
    // the device isn't present — query the actual style first.
    if (styleSet & HidNpadStyleTag_NpadHandheld) {
        if (R_SUCCEEDED(hidGetSixAxisSensorHandles(
                _cs2sx_motion_handles, 1,
                HidNpadIdType_Handheld, HidNpadStyleTag_NpadHandheld)))
        {
            hidStartSixAxisSensor(_cs2sx_motion_handles[0]);
            _cs2sx_motion_count = 1;
            return;
        }
    }
    if (styleSet & HidNpadStyleTag_NpadJoyDual) {
        if (R_SUCCEEDED(hidGetSixAxisSensorHandles(
                _cs2sx_motion_handles, 2,
                HidNpadIdType_No1, HidNpadStyleTag_NpadJoyDual)))
        {
            hidStartSixAxisSensor(_cs2sx_motion_handles[0]);
            hidStartSixAxisSensor(_cs2sx_motion_handles[1]);
            _cs2sx_motion_count = 2;
            return;
        }
    }
    if (styleSet & HidNpadStyleTag_NpadFullKey) {
        if (R_SUCCEEDED(hidGetSixAxisSensorHandles(
                _cs2sx_motion_handles, 1,
                HidNpadIdType_No1, HidNpadStyleTag_NpadFullKey)))
        {
            hidStartSixAxisSensor(_cs2sx_motion_handles[0]);
            _cs2sx_motion_count = 1;
            return;
        }
    }
    // Fallback: try Handheld regardless
    if (R_SUCCEEDED(hidGetSixAxisSensorHandles(
            _cs2sx_motion_handles, 1,
            HidNpadIdType_Handheld, HidNpadStyleTag_NpadHandheld)))
    {
        hidStartSixAxisSensor(_cs2sx_motion_handles[0]);
        _cs2sx_motion_count = 1;
    }
}

CS2SX_MotionState CS2SX_Motion_Get(void)
{
    CS2SX_MotionState s;
    memset(&s, 0, sizeof(s));
    _cs2sx_motion_ensure();
    if (_cs2sx_motion_count == 0) return s;

    HidSixAxisSensorState raw;
    memset(&raw, 0, sizeof(raw));
    hidGetSixAxisSensorStates(_cs2sx_motion_handles[0], &raw, 1);

    s.accelX = raw.acceleration.x;   s.accelY = raw.acceleration.y;   s.accelZ = raw.acceleration.z;
    s.gyroX  = raw.angular_velocity.x; s.gyroY = raw.angular_velocity.y; s.gyroZ = raw.angular_velocity.z;
    s.angleX = raw.angle.x;          s.angleY = raw.angle.y;          s.angleZ = raw.angle.z;
    return s;
}

void CS2SX_Motion_ResetAngles(void)
{
    _cs2sx_motion_ensure();
    if (_cs2sx_motion_count == 0) return;
    for (s32 i = 0; i < _cs2sx_motion_count; i++) {
        hidStopSixAxisSensor(_cs2sx_motion_handles[i]);
        hidStartSixAxisSensor(_cs2sx_motion_handles[i]);
    }
}

