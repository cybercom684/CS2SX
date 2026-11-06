#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct LambdasApp
{
    SwitchApp base;
    Action_t f_onTick;
    int f_ticks;
};

void LambdasApp_Init(LambdasApp* self);
void LambdasApp_OnInit(LambdasApp* self);
void LambdasApp_Apply(int value, Action_int_t callback);
void LambdasApp_OnFrame(LambdasApp* self);

void LambdasApp_Free(LambdasApp* self);


#endif
