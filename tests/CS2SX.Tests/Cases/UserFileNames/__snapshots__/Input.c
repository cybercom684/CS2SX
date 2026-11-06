#include <stdlib.h>
#include "Button.h"
#include "Input.h"
#include "Program.h"

InputMapper* InputMapper_New(void)
{
    InputMapper* self = (InputMapper*)malloc(sizeof(InputMapper));
    if (!self) return NULL;
    memset(self, 0, sizeof(InputMapper));
    self->_rc = 1;
    return self;
}

void InputMapper_Poll(InputMapper* self)
{
    if ((g_cs2sx_kDown & HidNpadButton_A))
    {
        self->f_Presses++;
    }
}

void InputMapper_Free(InputMapper* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

InputMapper* InputMapper_Retain(InputMapper* self)
{
    if (self) self->_rc++;
    return self;
}
