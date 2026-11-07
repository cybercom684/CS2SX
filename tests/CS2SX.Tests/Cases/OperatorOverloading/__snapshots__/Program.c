#include <stdlib.h>
#include "Program.h"

// ── Struct: Vec2 ────────────────────────────────────────
Vec2 Vec2_New(float x, float y)
{
    Vec2 _self_value = {0};
    Vec2* self = &_self_value;
    self->X = x;
    self->Y = y;
    return _self_value;
}

float Vec2_Dot(Vec2* self, Vec2 o)
{
    return self->X * o.X + self->Y * o.Y;
}

Vec2 Vec2_op_add(Vec2 a, Vec2 b)
{
    return Vec2_New(a.X + b.X, a.Y + b.Y);
}

Vec2 Vec2_op_sub(Vec2 a, Vec2 b)
{
    return Vec2_New(a.X - b.X, a.Y - b.Y);
}

Vec2 Vec2_op_mul(Vec2 a, float s)
{
    return Vec2_New(a.X * s, a.Y * s);
}

int Vec2_Equals(Vec2 a, Vec2 b)
{
    return memcmp(&a, &b, sizeof(Vec2)) == 0;
}

void OperatorOverloadingApp_Init(OperatorOverloadingApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))OperatorOverloadingApp_OnFrame;
    self->f_pos = Vec2_New(0, 0);
    self->f_vel = Vec2_New(1, 2);
}

void OperatorOverloadingApp_OnFrame(OperatorOverloadingApp* self)
{
    self->f_pos = Vec2_op_add(self->f_pos, Vec2_op_mul(self->f_vel, 0.5f));
    Vec2 d = Vec2_op_sub(self->f_pos, Vec2_New(1, 1));
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%s,%s dot=%s", Float_ToString(self->f_pos.X), Float_ToString(self->f_pos.Y), Float_ToString(Vec2_Dot(&d, self->f_vel)));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
}

void OperatorOverloadingApp_Free(OperatorOverloadingApp* self)
{
    if (!self) return;
    free(self);
}
