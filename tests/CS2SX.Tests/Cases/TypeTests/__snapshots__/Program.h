#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_interfaces.h"
#include "_generics.h"

typedef struct Shape_vtable
{
    const char* (*Kind)(void* self);
} Shape_vtable;

struct Shape
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Shape_vtable* vtable;
    /* implements IShape */
};

Shape* Shape_New(void);
void Shape_Ctor(Shape* self);
/* abstract: const char* Shape_Kind(Shape* self) */

void Shape__Destroy(Shape* self);
extern const CS2SX_ClassInfo Shape_ClassInfo;
void Shape_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Shape_GcKind;
void Shape_Free(Shape* self);
Shape* Shape_Retain(Shape* self);
extern IShape_vtable Shape_IShape_vtable_instance;
Shape* Shape_Retain(Shape* self);
void Shape_Free(Shape* self);
extern const CS2SX_GcKind Shape_GcKind;
extern const CS2SX_ClassInfo Shape_ClassInfo;
static inline IShape* Shape_as_IShape(Shape* self)
{
    return (IShape*)CS2SX_Iface_Wrap(sizeof(IShape), &Shape_IShape_vtable_instance, self, (void* (*)(void*))Shape_Retain, (CS2SX_FreeFn)Shape_Free, &Shape_GcKind, &Shape_ClassInfo);
}

struct Circle
{
    Shape base;
    int f_R;
};

Circle* Circle_New(void);
void Circle_Ctor(Circle* self);
const char* Circle_Kind(Circle* self);

void Circle__Destroy(Circle* self);
extern const CS2SX_ClassInfo Circle_ClassInfo;
void Circle_Gc(void* self, CS2SX_GcVisit visit);
void Circle_Free(Circle* self);
Circle* Circle_Retain(Circle* self);
extern IShape_vtable Shape_IShape_vtable_instance;
Shape* Shape_Retain(Shape* self);
void Shape_Free(Shape* self);
extern const CS2SX_GcKind Shape_GcKind;
extern const CS2SX_ClassInfo Circle_ClassInfo;
static inline IShape* Circle_as_IShape(Circle* self)
{
    return (IShape*)CS2SX_Iface_Wrap(sizeof(IShape), &Shape_IShape_vtable_instance, self, (void* (*)(void*))Shape_Retain, (CS2SX_FreeFn)Shape_Free, &Shape_GcKind, &Circle_ClassInfo);
}

struct Square
{
    Shape base;
    int f_Side;
};

Square* Square_New(void);
void Square_Ctor(Square* self);
const char* Square_Kind(Square* self);

void Square__Destroy(Square* self);
extern const CS2SX_ClassInfo Square_ClassInfo;
void Square_Gc(void* self, CS2SX_GcVisit visit);
void Square_Free(Square* self);
Square* Square_Retain(Square* self);
extern IShape_vtable Shape_IShape_vtable_instance;
Shape* Shape_Retain(Shape* self);
void Shape_Free(Shape* self);
extern const CS2SX_GcKind Shape_GcKind;
extern const CS2SX_ClassInfo Square_ClassInfo;
static inline IShape* Square_as_IShape(Square* self)
{
    return (IShape*)CS2SX_Iface_Wrap(sizeof(IShape), &Shape_IShape_vtable_instance, self, (void* (*)(void*))Shape_Retain, (CS2SX_FreeFn)Shape_Free, &Shape_GcKind, &Square_ClassInfo);
}

struct Cube
{
    Square base;
};

Cube* Cube_New(void);
void Cube_Ctor(Cube* self);
const char* Cube_Kind(Cube* self);

void Cube__Destroy(Cube* self);
extern const CS2SX_ClassInfo Cube_ClassInfo;
void Cube_Gc(void* self, CS2SX_GcVisit visit);
void Cube_Free(Cube* self);
Cube* Cube_Retain(Cube* self);
extern IShape_vtable Shape_IShape_vtable_instance;
Shape* Shape_Retain(Shape* self);
void Shape_Free(Shape* self);
extern const CS2SX_GcKind Shape_GcKind;
extern const CS2SX_ClassInfo Cube_ClassInfo;
static inline IShape* Cube_as_IShape(Cube* self)
{
    return (IShape*)CS2SX_Iface_Wrap(sizeof(IShape), &Shape_IShape_vtable_instance, self, (void* (*)(void*))Shape_Retain, (CS2SX_FreeFn)Shape_Free, &Shape_GcKind, &Cube_ClassInfo);
}

struct TypeTestsApp
{
    SwitchApp base;
};

void TypeTestsApp_Init(TypeTestsApp* self);
const char* TypeTestsApp_Describe(Shape* s);
const char* TypeTestsApp_Switch(Shape* s);
void TypeTestsApp_OnInit(TypeTestsApp* self);

void TypeTestsApp_Free(TypeTestsApp* self);


#endif
