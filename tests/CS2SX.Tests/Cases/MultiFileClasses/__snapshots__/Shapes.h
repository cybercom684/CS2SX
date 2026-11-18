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
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
};

#define Shape_MaxSides (12)
#define Shape_Scale (1.5f)
Shape* Shape_New(void);
void Shape_Ctor(Shape* self);
/* abstract: float Shape_Area(Shape* self, Painter* p, float factor) */

void Shape__Destroy(Shape* self);
extern const CS2SX_ClassInfo Shape_ClassInfo;
void Shape_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Shape_GcKind;
void Shape_Free(Shape* self);
Shape* Shape_Retain(Shape* self);
struct Painter
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    int f_Calls;
};

Painter* Painter_New(void);
void Painter_Ctor(Painter* self);

void Painter__Destroy(Painter* self);
extern const CS2SX_ClassInfo Painter_ClassInfo;
void Painter_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Painter_GcKind;
void Painter_Free(Painter* self);
Painter* Painter_Retain(Painter* self);


#endif
