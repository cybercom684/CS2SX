#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

// C# value-type struct: Vec2
typedef struct Vec2 {
    float X;
    float Y;
} Vec2;
Vec2 Vec2_New(float x, float y);
float Vec2_Dot(Vec2* self, Vec2 o);
Vec2 Vec2_op_add(Vec2 a, Vec2 b);
Vec2 Vec2_op_sub(Vec2 a, Vec2 b);
Vec2 Vec2_op_mul(Vec2 a, float s);
int Vec2_Equals(Vec2 a, Vec2 b);

struct OperatorOverloadingApp
{
    SwitchApp base;
    Vec2 f_pos;
    Vec2 f_vel;
};

void OperatorOverloadingApp_Init(OperatorOverloadingApp* self);
void OperatorOverloadingApp_OnFrame(OperatorOverloadingApp* self);

void OperatorOverloadingApp_Free(OperatorOverloadingApp* self);


#endif
