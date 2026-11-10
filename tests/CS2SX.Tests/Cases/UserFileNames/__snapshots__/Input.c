#include <stdlib.h>
#include "Button.h"
#include "Input.h"
#include "Program.h"

void InputMapper_Ctor(InputMapper* self)
{
    CS2SX_TRACE("InputMapper..ctor", "Input.cs");
}

InputMapper* InputMapper_New(void)
{
    InputMapper* self = (InputMapper*)calloc(1, sizeof(InputMapper));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))InputMapper_Dispose;
    self->_gc = InputMapper_Gc;
    InputMapper_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)InputMapper_Free);
    return self;
}

void InputMapper_Poll(InputMapper* self)
{
    CS2SX_TRACE("InputMapper.Poll", "Input.cs");
    CS2SX_LINE(9);
    if ((g_cs2sx_kDown & HidNpadButton_A))
    {
        CS2SX_LINE(9);
        self->f_Presses++;
    }
}

void InputMapper_Gc(void* _self, CS2SX_GcVisit visit)
{
    InputMapper* self = (InputMapper*)_self; (void)self;
}

static void InputMapper_GcDyn(void* self, CS2SX_GcVisit visit) { ((InputMapper*)self)->_gc(self, visit); }
const CS2SX_GcKind InputMapper_GcKind = { offsetof(InputMapper, _rc), InputMapper_GcDyn, (CS2SX_FreeFn)InputMapper_Free };

void InputMapper_Dispose(InputMapper* self)
{
}

void InputMapper_Free(InputMapper* self)
{
    if (!self) return;
    InputMapper* _o = (InputMapper*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &InputMapper_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

InputMapper* InputMapper_Retain(InputMapper* self)
{
    if (self) ((InputMapper*)self)->_rc++;
    return self;
}
