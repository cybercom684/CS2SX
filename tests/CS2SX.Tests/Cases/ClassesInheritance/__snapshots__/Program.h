#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

typedef struct Shape_vtable
{
    float (*Area)(void* self);
    const char* (*Describe)(void* self);
} Shape_vtable;

struct Shape
{
    Shape_vtable* vtable;
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    const char* f_Name;
};

extern int Shape_Created;
Shape* Shape_New(const char* name);
void Shape_Ctor(Shape* self, const char* name);
/* abstract: float Shape_Area(Shape* self) */
const char* Shape_Describe(Shape* self);

void Shape__Destroy(Shape* self);
extern const CS2SX_ClassInfo Shape_ClassInfo;
void Shape_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Shape_GcKind;
void Shape_Free(Shape* self);
Shape* Shape_Retain(Shape* self);
struct Rect
{
    Shape base;
    float f_W;
    float f_H;
};

Rect* Rect_New(float w, float h);
void Rect_Ctor(Rect* self, float w, float h);
float Rect_Area(Rect* self);

void Rect__Destroy(Rect* self);
extern const CS2SX_ClassInfo Rect_ClassInfo;
void Rect_Gc(void* self, CS2SX_GcVisit visit);
void Rect_Free(Rect* self);
Rect* Rect_Retain(Rect* self);
struct Square
{
    Rect base;
};

Square* Square_New(float side);
void Square_Ctor(Square* self, float side);
const char* Square_Describe(Square* self);

void Square__Destroy(Square* self);
extern const CS2SX_ClassInfo Square_ClassInfo;
void Square_Gc(void* self, CS2SX_GcVisit visit);
void Square_Free(Square* self);
Square* Square_Retain(Square* self);
struct Circle
{
    Shape base;
    float f_r;
};

Circle* Circle_New(float r);
void Circle_Ctor(Circle* self, float r);
float Circle_Area(Circle* self);

void Circle__Destroy(Circle* self);
extern const CS2SX_ClassInfo Circle_ClassInfo;
void Circle_Gc(void* self, CS2SX_GcVisit visit);
void Circle_Free(Circle* self);
Circle* Circle_Retain(Circle* self);
struct ClassesInheritanceApp
{
    SwitchApp base;
};

void ClassesInheritanceApp_Init(ClassesInheritanceApp* self);
void ClassesInheritanceApp_OnInit(ClassesInheritanceApp* self);

void ClassesInheritanceApp_Free(ClassesInheritanceApp* self);


#endif
