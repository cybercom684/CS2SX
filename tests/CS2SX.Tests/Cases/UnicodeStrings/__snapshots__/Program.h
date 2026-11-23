#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct UnicodeStringsApp
{
    SwitchApp base;
};

void UnicodeStringsApp_Init(UnicodeStringsApp* self);
void UnicodeStringsApp_OnInit(UnicodeStringsApp* self);
void UnicodeStringsApp_OnFrame(UnicodeStringsApp* self);

void UnicodeStringsApp_Free(UnicodeStringsApp* self);


#endif
