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
    const char* f_Name;
};

extern int Shape_Created;
Shape* Shape_New(const char* name);
void Shape_Ctor(Shape* self, const char* name);
/* abstract: float Shape_Area(Shape* self) */
const char* Shape_Describe(Shape* self);

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

void Rect_Free(Rect* self);
struct Square
{
    Rect base;
};

Square* Square_New(float side);
void Square_Ctor(Square* self, float side);
const char* Square_Describe(Square* self);

void Square_Free(Square* self);
struct Circle
{
    Shape base;
    float f_r;
};

Circle* Circle_New(float r);
void Circle_Ctor(Circle* self, float r);
float Circle_Area(Circle* self);

void Circle_Free(Circle* self);
struct ClassesInheritanceApp
{
    SwitchApp base;
};

void ClassesInheritanceApp_Init(ClassesInheritanceApp* self);
void ClassesInheritanceApp_OnInit(ClassesInheritanceApp* self);

void ClassesInheritanceApp_Free(ClassesInheritanceApp* self);


#endif
