#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct ControlFlowApp
{
    SwitchApp base;
};

void ControlFlowApp_Init(ControlFlowApp* self);
void ControlFlowApp_OnInit(ControlFlowApp* self);

void ControlFlowApp_Free(ControlFlowApp* self);


#endif
