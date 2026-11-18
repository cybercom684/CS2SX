#pragma once
#ifndef CS2SX_SQUARE_H
#define CS2SX_SQUARE_H

#include "_forward.h"
#include "Shapes.h"
#include "_generics.h"

struct Square
{
    Shape base;
    float f_side;
    int* f_history;
};

Square* Square_New(float side);
void Square_Ctor(Square* self, float side);
float Square_Area(Square* self, Painter* p, float factor);

void Square__Destroy(Square* self);
extern const CS2SX_ClassInfo Square_ClassInfo;
void Square_Gc(void* self, CS2SX_GcVisit visit);
void Square_Free(Square* self);
Square* Square_Retain(Square* self);


#endif
