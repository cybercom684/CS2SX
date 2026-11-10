#include <stdlib.h>
#include "_generics.h"
#include "Program.h"
#include "Shapes.h"
#include "Square.h"

static Shape_vtable Shape_vtable_instance =
{
};

void Shape_Ctor(Shape* self)
{
    CS2SX_TRACE("Shape..ctor", "Shapes.cs");
}

Shape* Shape_New(void)
{
    Shape* self = (Shape*)calloc(1, sizeof(Shape));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Shape_Dispose;
    self->_gc = Shape_Gc;
    self->vtable = &Shape_vtable_instance;
    Shape_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Shape_Free);
    return self;
}

void Shape_Gc(void* _self, CS2SX_GcVisit visit)
{
    Shape* self = (Shape*)_self; (void)self;
}

static void Shape_GcDyn(void* self, CS2SX_GcVisit visit) { ((Shape*)self)->_gc(self, visit); }
const CS2SX_GcKind Shape_GcKind = { offsetof(Shape, _rc), Shape_GcDyn, (CS2SX_FreeFn)Shape_Free };

void Shape_Dispose(Shape* self)
{
}

void Shape_Free(Shape* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Shape_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Shape* Shape_Retain(Shape* self)
{
    if (self) ((Shape*)self)->_rc++;
    return self;
}

void Painter_Ctor(Painter* self)
{
    CS2SX_TRACE("Painter..ctor", "Shapes.cs");
}

Painter* Painter_New(void)
{
    Painter* self = (Painter*)calloc(1, sizeof(Painter));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Painter_Dispose;
    self->_gc = Painter_Gc;
    Painter_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Painter_Free);
    return self;
}

void Painter_Gc(void* _self, CS2SX_GcVisit visit)
{
    Painter* self = (Painter*)_self; (void)self;
}

static void Painter_GcDyn(void* self, CS2SX_GcVisit visit) { ((Painter*)self)->_gc(self, visit); }
const CS2SX_GcKind Painter_GcKind = { offsetof(Painter, _rc), Painter_GcDyn, (CS2SX_FreeFn)Painter_Free };

void Painter_Dispose(Painter* self)
{
}

void Painter_Free(Painter* self)
{
    if (!self) return;
    Painter* _o = (Painter*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Painter_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Painter* Painter_Retain(Painter* self)
{
    if (self) ((Painter*)self)->_rc++;
    return self;
}
