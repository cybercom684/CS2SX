#include <stdlib.h>
#include "Button.h"
#include "Input.h"
#include "Program.h"

MenuEntry* MenuEntry_New(const char* title)
{
    MenuEntry* self = (MenuEntry*)malloc(sizeof(MenuEntry));
    if (!self) return NULL;
    memset(self, 0, sizeof(MenuEntry));
    self->_rc = 1;
    self->f_Title = _cs2sx_heap_strdup(title);
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
