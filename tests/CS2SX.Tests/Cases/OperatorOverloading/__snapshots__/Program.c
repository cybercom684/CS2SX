#include <stdlib.h>
#include "Program.h"

// ── Struct: Vec2 ────────────────────────────────────────
typedef struct Vec2 {
    float X;
    float Y;
} Vec2;

float Vec2_Dot(Vec2* self, Vec2* o)
{
    return self->f_X * o->f_X + self->f_Y * o->f_Y;
}

int Vec2_Equals(Vec2 a, Vec2 b)
{
    return memcmp(&a, &b, sizeof(Vec2)) == 0;
}


void OperatorOverloadingApp_Init(OperatorOverloadingApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))OperatorOverloadingApp_OnFrame;
    self->f_pos = (Vec2){ 0, 0 };
    self->f_vel = (Vec2){ 1, 2 };
}

void OperatorOverloadingApp_OnFrame(OperatorOverloadingApp* self)
{
    self->f_pos = self->f_pos + self->f_vel * 0.5f;
    Vec2 d = self->f_pos - (Vec2){ 1, 1 };
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d,%d dot=%d", self->f_pos->f_X, self->f_pos->f_Y, Vec2_Dot(d, self->f_vel));
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d,%d dot=%d", self->f_pos->f_X, self->f_pos->f_Y, Vec2_Dot(d, self->f_vel));
    const char* _ps_0 = _sb1;
    printf("%s\n", _ps_0 ? _ps_0 : "");
}

void OperatorOverloadingApp_Free(OperatorOverloadingApp* self)
{
    if (!self) return;
    if (self->f_pos) { Vec2_Free(self->f_pos); self->f_pos = NULL; }
    if (self->f_vel) { Vec2_Free(self->f_vel); self->f_vel = NULL; }
    free(self);
}
