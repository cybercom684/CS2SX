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
    MenuEntry_Ctor(self, title);
    return self;
}

void MenuEntry_Free(MenuEntry* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

MenuEntry* MenuEntry_Retain(MenuEntry* self)
{
    if (self) self->_rc++;
    return self;
}
