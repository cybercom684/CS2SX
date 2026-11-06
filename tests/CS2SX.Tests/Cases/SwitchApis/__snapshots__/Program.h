#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct SwitchApisApp
{
    SwitchApp base;
    int x;
};

void SwitchApisApp_Init(SwitchApisApp* self);
void SwitchApisApp_OnInit(SwitchApisApp* self);
void SwitchApisApp_OnFrame(SwitchApisApp* self);

void SwitchApisApp_Free(SwitchApisApp* self);


#endif
