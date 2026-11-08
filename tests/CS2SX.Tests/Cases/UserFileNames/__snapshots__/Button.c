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
    MenuEntry_Ctor(self, title);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)MenuEntry_Free);
    return self;
}

void MenuEntry_Dispose(MenuEntry* self)
{
    CS2SX_StrDrop(self->f_Title); self->f_Title = NULL;
}

void MenuEntry_Free(MenuEntry* self)
{
    if (!self) return;
    MenuEntry* _o = (MenuEntry*)self;
    if (--_o->_rc > 0) return;
    _o->_dtor(_o);
    free(_o);
}

MenuEntry* MenuEntry_Retain(MenuEntry* self)
{
    if (self) ((MenuEntry*)self)->_rc++;
    return self;
}
