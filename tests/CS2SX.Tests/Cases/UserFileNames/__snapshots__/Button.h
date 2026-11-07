#pragma once
#ifndef CS2SX_BUTTON_H
#define CS2SX_BUTTON_H

#include "_forward.h"

struct MenuEntry
{
    int _rc;
    const char* f_Title;
};

MenuEntry* MenuEntry_New(const char* title);
void MenuEntry_Ctor(MenuEntry* self, const char* title);

void MenuEntry_Free(MenuEntry* self);
MenuEntry* MenuEntry_Retain(MenuEntry* self);


#endif
