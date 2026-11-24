#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_interfaces.h"

struct C
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    /* implements IB */
    /* implements IA */
};

C* C_New(void);
void C_Ctor(C* self);
const char* C_ToString(C* self);
const char* C_Hello(C* self);

void C__Destroy(C* self);
extern const CS2SX_ClassInfo C_ClassInfo;
void C_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind C_GcKind;
void C_Free(C* self);
C* C_Retain(C* self);
const char* C_get_Name(C* self);
int C_get_N(C* self);
extern IB_vtable C_IB_vtable_instance;
C* C_Retain(C* self);
void C_Free(C* self);
extern const CS2SX_GcKind C_GcKind;
extern const CS2SX_ClassInfo C_ClassInfo;
static inline IB* C_as_IB(C* self)
{
    return (IB*)CS2SX_Iface_Wrap(sizeof(IB), &C_IB_vtable_instance, self, (void* (*)(void*))C_Retain, (CS2SX_FreeFn)C_Free, &C_GcKind, &C_ClassInfo);
}
extern IA_vtable C_IA_vtable_instance;
C* C_Retain(C* self);
void C_Free(C* self);
extern const CS2SX_GcKind C_GcKind;
extern const CS2SX_ClassInfo C_ClassInfo;
static inline IA* C_as_IA(C* self)
{
    return (IA*)CS2SX_Iface_Wrap(sizeof(IA), &C_IA_vtable_instance, self, (void* (*)(void*))C_Retain, (CS2SX_FreeFn)C_Free, &C_GcKind, &C_ClassInfo);
}

struct D
{
    C base;
};

D* D_New(void);
void D_Ctor(D* self);

void D__Destroy(D* self);
extern const CS2SX_ClassInfo D_ClassInfo;
void D_Gc(void* self, CS2SX_GcVisit visit);
void D_Free(D* self);
D* D_Retain(D* self);
extern IB_vtable C_IB_vtable_instance;
C* C_Retain(C* self);
void C_Free(C* self);
extern const CS2SX_GcKind C_GcKind;
extern const CS2SX_ClassInfo D_ClassInfo;
static inline IB* D_as_IB(D* self)
{
    return (IB*)CS2SX_Iface_Wrap(sizeof(IB), &C_IB_vtable_instance, self, (void* (*)(void*))C_Retain, (CS2SX_FreeFn)C_Free, &C_GcKind, &D_ClassInfo);
}
extern IA_vtable C_IA_vtable_instance;
C* C_Retain(C* self);
void C_Free(C* self);
extern const CS2SX_GcKind C_GcKind;
extern const CS2SX_ClassInfo D_ClassInfo;
static inline IA* D_as_IA(D* self)
{
    return (IA*)CS2SX_Iface_Wrap(sizeof(IA), &C_IA_vtable_instance, self, (void* (*)(void*))C_Retain, (CS2SX_FreeFn)C_Free, &C_GcKind, &D_ClassInfo);
}

struct Item
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    const char* f_Label;
};

Item* Item_New(const char* l);
void Item_Ctor(Item* self, const char* l);

void Item__Destroy(Item* self);
extern const CS2SX_ClassInfo Item_ClassInfo;
void Item_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Item_GcKind;
void Item_Free(Item* self);
Item* Item_Retain(Item* self);
struct Holder
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    /* implements IHolder */
    Item* f_Thing;
    const char* f_Tag;
};

Holder* Holder_New(void);
void Holder_Ctor(Holder* self);

void Holder__Destroy(Holder* self);
extern const CS2SX_ClassInfo Holder_ClassInfo;
void Holder_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Holder_GcKind;
void Holder_Free(Holder* self);
Holder* Holder_Retain(Holder* self);
extern IHolder_vtable Holder_IHolder_vtable_instance;
Holder* Holder_Retain(Holder* self);
void Holder_Free(Holder* self);
extern const CS2SX_GcKind Holder_GcKind;
extern const CS2SX_ClassInfo Holder_ClassInfo;
static inline IHolder* Holder_as_IHolder(Holder* self)
{
    return (IHolder*)CS2SX_Iface_Wrap(sizeof(IHolder), &Holder_IHolder_vtable_instance, self, (void* (*)(void*))Holder_Retain, (CS2SX_FreeFn)Holder_Free, &Holder_GcKind, &Holder_ClassInfo);
}

// C# value-type struct: Vec
typedef struct Vec {
    int X;
} Vec;
#ifndef CS2SX_Nullable_Vec_DEFINED
#define CS2SX_Nullable_Vec_DEFINED
typedef struct Nullable_Vec { Vec value; int has; } Nullable_Vec;
#endif
int Vec_get_Twice(Vec* self);
int Vec_Equals(Vec a, Vec b);
#ifndef CS2SX_LIST_Vec_DEFINED
#define CS2SX_LIST_Vec_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Vec, Vec, CS2SX_ID, CS2SX_NOREF, Vec_Equals)
#endif

// C# value-type struct: Pair_int
typedef struct Pair_int {
    int A;
} Pair_int;
#ifndef CS2SX_Nullable_Pair_int_DEFINED
#define CS2SX_Nullable_Pair_int_DEFINED
typedef struct Nullable_Pair_int { Pair_int value; int has; } Nullable_Pair_int;
#endif
const char* Pair_int_Note(Pair_int* self);
int Pair_int_Equals(Pair_int a, Pair_int b);
#ifndef CS2SX_LIST_Pair_int_DEFINED
#define CS2SX_LIST_Pair_int_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Pair_int, Pair_int, CS2SX_ID, CS2SX_NOREF, Pair_int_Equals)
#endif

struct ReviewFixesApp
{
    SwitchApp base;
};

void ReviewFixesApp_Init(ReviewFixesApp* self);
int ReviewFixesApp_Dbl(Vec* v);
int ReviewFixesApp_Dbl2(Vec v);
int ReviewFixesApp_IsBig(int x);
void ReviewFixesApp_OnInit(ReviewFixesApp* self);
void ReviewFixesApp_OnFrame(ReviewFixesApp* self);

void ReviewFixesApp_Free(ReviewFixesApp* self);


#endif
