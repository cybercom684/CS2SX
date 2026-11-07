#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct UserFileNamesApp
{
    SwitchApp base;
    InputMapper* f_input;
    MenuEntry* f_entry;
};

void UserFileNamesApp_Init(UserFileNamesApp* self);
void UserFileNamesApp_OnFrame(UserFileNamesApp* self);

void UserFileNamesApp_Free(UserFileNamesApp* self);


#endif
