#include <stdlib.h>
#include "_generics.h"
#include "Program.h"
#include "Shapes.h"
#include "Square.h"

static Shape_vtable Square_vtable_instance;

void Square_Ctor(Square* self, float side)
{
    self->f_history = (int*)CS2SX_ArrNew(4, sizeof(int));
    Shape_Ctor((Shape*)self);
    self->f_side = side;
}

Square* Square_New(float side)
{
    Square* self = (Square*)calloc(1, sizeof(Square));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base.vtable = &Square_vtable_instance;
    Square_Ctor(self, side);
    return self;
}

float Square_Area(Square* self, Painter* p, float factor)
{
    CS2SX_NN(p)->f_Calls++;
    CS2SX_AT(self->f_history, CS2SX_NN(p)->f_Calls % 4) = CS2SX_NN(p)->f_Calls;
    return self->f_side * self->f_side * factor * Shape_Scale;
}

void Square_Free(Square* self)
{
    if (!self) return;
    CS2SX_ArrFree(self->f_history); self->f_history = NULL;
    Shape_Free((Shape*)self);
}

static Shape_vtable Square_vtable_instance =
{
    .Area = (float(*)(void*, Painter*, float))Square_Area,
};
