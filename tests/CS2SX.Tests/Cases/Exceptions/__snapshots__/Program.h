#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct ExceptionsApp
{
    SwitchApp base;
};

void ExceptionsApp_Init(ExceptionsApp* self);
int ExceptionsApp_Divide(int a, int b);
void ExceptionsApp_OnInit(ExceptionsApp* self);

void ExceptionsApp_Free(ExceptionsApp* self);


#endif
