#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct RuntimeExceptionsApp
{
    SwitchApp base;
};

void RuntimeExceptionsApp_Init(RuntimeExceptionsApp* self);
const char* RuntimeExceptionsApp_Try(Action_t action);
void RuntimeExceptionsApp_OnInit(RuntimeExceptionsApp* self);

void RuntimeExceptionsApp_Free(RuntimeExceptionsApp* self);


#endif
