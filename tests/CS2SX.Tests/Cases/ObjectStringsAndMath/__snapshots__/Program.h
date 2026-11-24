#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct Plain
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    int f_V;
};

Plain* Plain_New(void);
void Plain_Ctor(Plain* self);

void Plain__Destroy(Plain* self);
extern const CS2SX_ClassInfo Plain_ClassInfo;
void Plain_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Plain_GcKind;
void Plain_Free(Plain* self);
Plain* Plain_Retain(Plain* self);
struct Named
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    const char* f_N;
};

Named* Named_New(const char* n);
void Named_Ctor(Named* self, const char* n);
const char* Named_ToString(Named* self);

void Named__Destroy(Named* self);
extern const CS2SX_ClassInfo Named_ClassInfo;
void Named_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Named_GcKind;
void Named_Free(Named* self);
Named* Named_Retain(Named* self);
typedef struct Animal_vtable
{
    const char* (*Sound)(void* self);
} Animal_vtable;

struct Animal
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Animal_vtable* vtable;
};

Animal* Animal_New(void);
void Animal_Ctor(Animal* self);
const char* Animal_Sound(Animal* self);
const char* Animal_ToString(Animal* self);

void Animal__Destroy(Animal* self);
extern const CS2SX_ClassInfo Animal_ClassInfo;
void Animal_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Animal_GcKind;
void Animal_Free(Animal* self);
Animal* Animal_Retain(Animal* self);
struct Dog
{
    Animal base;
};

Dog* Dog_New(void);
void Dog_Ctor(Dog* self);
const char* Dog_Sound(Dog* self);

void Dog__Destroy(Dog* self);
extern const CS2SX_ClassInfo Dog_ClassInfo;
void Dog_Gc(void* self, CS2SX_GcVisit visit);
void Dog_Free(Dog* self);
Dog* Dog_Retain(Dog* self);
struct Cat
{
    Animal base;
};

Cat* Cat_New(void);
void Cat_Ctor(Cat* self);
const char* Cat_Sound(Cat* self);
const char* Cat_ToString(Cat* self);
const char* Cat_Self(Cat* self);

void Cat__Destroy(Cat* self);
extern const CS2SX_ClassInfo Cat_ClassInfo;
void Cat_Gc(void* self, CS2SX_GcVisit visit);
void Cat_Free(Cat* self);
Cat* Cat_Retain(Cat* self);
// C# value-type struct: P2
typedef struct P2 {
    int X;
    int Y;
} P2;
#ifndef CS2SX_Nullable_P2_DEFINED
#define CS2SX_Nullable_P2_DEFINED
typedef struct Nullable_P2 { P2 value; int has; } Nullable_P2;
#endif
P2 P2_New(int x, int y);
const char* P2_ToString(P2* self);
int P2_Equals(P2 a, P2 b);
#ifndef CS2SX_LIST_P2_DEFINED
#define CS2SX_LIST_P2_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(P2, P2, CS2SX_ID, CS2SX_NOREF, P2_Equals)
#endif

// C# value-type struct: Q
typedef struct Q {
    int A;
} Q;
#ifndef CS2SX_Nullable_Q_DEFINED
#define CS2SX_Nullable_Q_DEFINED
typedef struct Nullable_Q { Q value; int has; } Nullable_Q;
#endif
int Q_Equals(Q a, Q b);
#ifndef CS2SX_LIST_Q_DEFINED
#define CS2SX_LIST_Q_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Q, Q, CS2SX_ID, CS2SX_NOREF, Q_Equals)
#endif

struct ObjectStringsAndMathApp
{
    SwitchApp base;
};

void ObjectStringsAndMathApp_Init(ObjectStringsAndMathApp* self);
void ObjectStringsAndMathApp_OnInit(ObjectStringsAndMathApp* self);
void ObjectStringsAndMathApp_OnFrame(ObjectStringsAndMathApp* self);

void ObjectStringsAndMathApp_Free(ObjectStringsAndMathApp* self);


#endif
