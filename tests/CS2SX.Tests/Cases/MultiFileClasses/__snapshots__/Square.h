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

void Square_Free(Square* self);


#endif
