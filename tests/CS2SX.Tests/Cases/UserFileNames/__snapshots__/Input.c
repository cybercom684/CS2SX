#include <stdlib.h>
#include "Button.h"
#include "Input.h"
#include "Program.h"

void InputMapper_Ctor(InputMapper* self)
{
}

InputMapper* InputMapper_New(void)
{
    InputMapper* self = (InputMapper*)calloc(1, sizeof(InputMapper));
    if (!self) return NULL;
    self->_rc = 1;
    InputMapper_Ctor(self);
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
