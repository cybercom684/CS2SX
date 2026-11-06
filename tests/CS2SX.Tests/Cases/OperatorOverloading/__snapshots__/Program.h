#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

// C# value-type struct: Vec2
typedef struct Vec2 Vec2;
float Vec2_Dot(Vec2* self, Vec2* o);

struct OperatorOverloadingApp
{
    SwitchApp base;
    Vec2* f_pos;
    Vec2* f_vel;
};

void OperatorOverloadingApp_Init(OperatorOverloadingApp* self);
void OperatorOverloadingApp_OnFrame(OperatorOverloadingApp* self);

void OperatorOverloadingApp_Free(OperatorOverloadingApp* self);


#endif
