#pragma once
#ifndef CS2SX_BUTTON_H
#define CS2SX_BUTTON_H

#include "_forward.h"

struct MenuEntry
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const char* f_Title;
};

MenuEntry* MenuEntry_New(const char* title);
void MenuEntry_Ctor(MenuEntry* self, const char* title);

void MenuEntry_Dispose(MenuEntry* self);
void MenuEntry_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind MenuEntry_GcKind;
void MenuEntry_Free(MenuEntry* self);
MenuEntry* MenuEntry_Retain(MenuEntry* self);


#endif
