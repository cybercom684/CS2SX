#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

// C# value-type struct: Meters
typedef struct Meters {
    int Value;
} Meters;
#ifndef CS2SX_Nullable_Meters_DEFINED
#define CS2SX_Nullable_Meters_DEFINED
typedef struct Nullable_Meters { Meters value; int has; } Nullable_Meters;
#endif
int Meters_implicit_int(Meters m);
Meters Meters_explicit_Meters(int v);
int Meters_Equals(Meters a, Meters b);
#ifndef CS2SX_LIST_Meters_DEFINED
#define CS2SX_LIST_Meters_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Meters, Meters, CS2SX_ID, CS2SX_NOREF, Meters_Equals)
#endif

struct Outer
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
};

Outer* Outer_New(void);
void Outer_Ctor(Outer* self);
Inner* Outer_Make(Outer* self);

void Outer__Destroy(Outer* self);
extern const CS2SX_ClassInfo Outer_ClassInfo;
void Outer_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Outer_GcKind;
void Outer_Free(Outer* self);
Outer* Outer_Retain(Outer* self);
struct Inner
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    int f_V;
};

Inner* Inner_New(void);
void Inner_Ctor(Inner* self);
int Inner_Twice(Inner* self);

void Inner__Destroy(Inner* self);
extern const CS2SX_ClassInfo Inner_ClassInfo;
void Inner_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Inner_GcKind;
void Inner_Free(Inner* self);
Inner* Inner_Retain(Inner* self);
struct Registry
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
};

extern List_int* Registry_Items;
extern Dict_str_int* Registry_Names;
extern int Registry_Count;
Registry* Registry_New(void);
void Registry_Ctor(Registry* self);

void Registry__Destroy(Registry* self);
extern const CS2SX_ClassInfo Registry_ClassInfo;
void Registry_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Registry_GcKind;
void Registry_Free(Registry* self);
Registry* Registry_Retain(Registry* self);
struct Link
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Link* f_Next;
    int f_Value;
};

Link* Link_New(void);
void Link_Ctor(Link* self);

void Link__Destroy(Link* self);
extern const CS2SX_ClassInfo Link_ClassInfo;
void Link_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Link_GcKind;
void Link_Free(Link* self);
Link* Link_Retain(Link* self);
struct LanguageFixesApp
{
    SwitchApp base;
    int f_a;
    int f_b;
};

void LanguageFixesApp_Init(LanguageFixesApp* self);
int LanguageFixesApp_AddThree(int v);
_Tuple2_int_str LanguageFixesApp_Pair();
_Tuple2_int_int LanguageFixesApp_Range(List_int* values);
void LanguageFixesApp_OnInit(LanguageFixesApp* self);

void LanguageFixesApp_Free(LanguageFixesApp* self);


#endif
