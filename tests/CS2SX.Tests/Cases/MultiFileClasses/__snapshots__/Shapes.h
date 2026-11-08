#pragma once
#ifndef CS2SX_SHAPES_H
#define CS2SX_SHAPES_H

#include "_forward.h"
#include "_generics.h"

typedef struct Shape_vtable
{
    float (*Area)(void* self, Painter* p, float factor);
} Shape_vtable;

struct Shape
{
    Shape_vtable* vtable;
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
};

#define Shape_MaxSides (12)
#define Shape_Scale (1.5f)
Shape* Shape_New(void);
void Shape_Ctor(Shape* self);
/* abstract: float Shape_Area(Shape* self, Painter* p, float factor) */

void Shape_Dispose(Shape* self);
void Shape_Free(Shape* self);
Shape* Shape_Retain(Shape* self);
struct Painter
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    int f_Calls;
};

Painter* Painter_New(void);
void Painter_Ctor(Painter* self);

void Painter_Dispose(Painter* self);
void Painter_Free(Painter* self);
Painter* Painter_Retain(Painter* self);


#endif
