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
    self->_dtor = (void (*)(void*))InputMapper_Dispose;
    InputMapper_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)InputMapper_Free);
    return self;
}

void InputMapper_Poll(InputMapper* self)
{
    if ((g_cs2sx_kDown & HidNpadButton_A))
    {
        self->f_Presses++;
    }
}

void InputMapper_Dispose(InputMapper* self)
{
}

void InputMapper_Free(InputMapper* self)
{
    if (!self) return;
    InputMapper* _o = (InputMapper*)self;
    if (--_o->_rc > 0) return;
    _o->_dtor(_o);
    free(_o);
}

InputMapper* InputMapper_Retain(InputMapper* self)
{
    if (self) ((InputMapper*)self)->_rc++;
    return self;
}
