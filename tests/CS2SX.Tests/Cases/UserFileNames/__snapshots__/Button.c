#include <stdlib.h>
#include "Button.h"
#include "Input.h"
#include "Program.h"

void MenuEntry_Ctor(MenuEntry* self, const char* title)
{
    CS2SX_TRACE("MenuEntry..ctor", "Button.cs");
    CS2SX_LINE(4);
    CS2SX_STR_STORE(self->f_Title, title);
}

MenuEntry* MenuEntry_New(const char* title)
{
    MenuEntry* self = (MenuEntry*)calloc(1, sizeof(MenuEntry));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))MenuEntry_Dispose;
    self->_gc = MenuEntry_Gc;
    MenuEntry_Ctor(self, title);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)MenuEntry_Free);
    return self;
}

void MenuEntry_Gc(void* _self, CS2SX_GcVisit visit)
{
    MenuEntry* self = (MenuEntry*)_self; (void)self;
}

static void MenuEntry_GcDyn(void* self, CS2SX_GcVisit visit) { ((MenuEntry*)self)->_gc(self, visit); }
const CS2SX_GcKind MenuEntry_GcKind = { offsetof(MenuEntry, _rc), MenuEntry_GcDyn, (CS2SX_FreeFn)MenuEntry_Free };

void MenuEntry_Dispose(MenuEntry* self)
{
    CS2SX_StrDrop(self->f_Title); self->f_Title = NULL;
}

void MenuEntry_Free(MenuEntry* self)
{
    if (!self) return;
    MenuEntry* _o = (MenuEntry*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &MenuEntry_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

MenuEntry* MenuEntry_Retain(MenuEntry* self)
{
    if (self) ((MenuEntry*)self)->_rc++;
    return self;
}
