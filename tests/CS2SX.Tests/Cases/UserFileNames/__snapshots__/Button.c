#include <stdlib.h>
#include "Button.h"
#include "Input.h"
#include "Program.h"

void MenuEntry_Ctor(MenuEntry* self, const char* title)
{
    self->f_Title = _cs2sx_heap_strdup(title);
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
