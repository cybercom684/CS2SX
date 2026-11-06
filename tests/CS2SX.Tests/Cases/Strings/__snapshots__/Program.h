#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct StringsApp
{
    SwitchApp base;
};

void StringsApp_Init(StringsApp* self);
void StringsApp_OnInit(StringsApp* self);

void StringsApp_Free(StringsApp* self);


#endif
