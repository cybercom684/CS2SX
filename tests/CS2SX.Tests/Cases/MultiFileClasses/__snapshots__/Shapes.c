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
    self->vtable = &Shape_vtable_instance;
    Shape_Ctor(self);
    return self;
}

void Shape_Free(Shape* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Shape* Shape_Retain(Shape* self)
{
    if (self) self->_rc++;
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
    Painter_Ctor(self);
    return self;
}

void Painter_Free(Painter* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Painter* Painter_Retain(Painter* self)
{
    if (self) self->_rc++;
    return self;
}
