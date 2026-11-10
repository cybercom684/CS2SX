#include <stdlib.h>
#include "_generics.h"
#include "Program.h"
#include "Shapes.h"
#include "Square.h"

static Shape_vtable Square_vtable_instance;

void Square_Ctor(Square* self, float side)
{
    CS2SX_TRACE("Square..ctor", "Square.cs");
    self->f_history = (int*)CS2SX_ArrNew(4, sizeof(int));
    Shape_Ctor((Shape*)self);
    CS2SX_LINE(6);
    self->f_side = side;
}

Square* Square_New(float side)
{
    Square* self = (Square*)calloc(1, sizeof(Square));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Square_Dispose;
    self->base._gc = Square_Gc;
    self->base.vtable = &Square_vtable_instance;
    Square_Ctor(self, side);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Square_Free);
    return self;
}

float Square_Area(Square* self, Painter* p, float factor)
{
    CS2SX_TRACE("Square.Area", "Square.cs");
    CS2SX_LINE(11);
    CS2SX_NN(p)->f_Calls++;
    CS2SX_LINE(12);
    CS2SX_AT(self->f_history, CS2SX_NN(p)->f_Calls % 4) = CS2SX_NN(p)->f_Calls;
    CS2SX_LINE(13);
    return self->f_side * self->f_side * factor * Shape_Scale;
}

void Square_Gc(void* _self, CS2SX_GcVisit visit)
{
    Square* self = (Square*)_self; (void)self;
    Shape_Gc(_self, visit);
}

void Square_Dispose(Square* self)
{
    CS2SX_ArrFree(self->f_history); self->f_history = NULL;
    Shape_Dispose((Shape*)self);
}

void Square_Free(Square* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Shape_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Square* Square_Retain(Square* self)
{
    if (self) ((Shape*)self)->_rc++;
    return self;
}

static Shape_vtable Square_vtable_instance =
{
    .Area = (float(*)(void*, Painter*, float))Square_Area,
};
