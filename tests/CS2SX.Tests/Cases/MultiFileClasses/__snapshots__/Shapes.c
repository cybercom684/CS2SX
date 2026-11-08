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
}

Shape* Shape_New(void)
{
    Shape* self = (Shape*)calloc(1, sizeof(Shape));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Shape_Dispose;
    self->vtable = &Shape_vtable_instance;
    Shape_Ctor(self);
    return self;
}

void Shape_Dispose(Shape* self)
{
}

void Shape_Free(Shape* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) return;
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
}

Painter* Painter_New(void)
{
    Painter* self = (Painter*)calloc(1, sizeof(Painter));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Painter_Dispose;
    Painter_Ctor(self);
    return self;
}

void Painter_Dispose(Painter* self)
{
}

void Painter_Free(Painter* self)
{
    if (!self) return;
    Painter* _o = (Painter*)self;
    if (--_o->_rc > 0) return;
    _o->_dtor(_o);
    free(_o);
}

Painter* Painter_Retain(Painter* self)
{
    if (self) ((Painter*)self)->_rc++;
    return self;
}
