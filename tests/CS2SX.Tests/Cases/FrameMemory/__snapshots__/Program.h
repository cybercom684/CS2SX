#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_interfaces.h"
#include "_generics.h"

struct Counter
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
};

extern int Counter_Freed;
Counter* Counter_New(void);
void Counter_Ctor(Counter* self);

void Counter_Dispose(Counter* self);
void Counter_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Counter_GcKind;
void Counter_Free(Counter* self);
Counter* Counter_Retain(Counter* self);
struct Item
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    int f_V;
};

Item* Item_New(int v);
void Item_Ctor(Item* self, int v);

void Item_Dispose(Item* self);
void Item_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Item_GcKind;
void Item_Free(Item* self);
Item* Item_Retain(Item* self);
struct Wrapped
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    /* implements IHas */
    int f_V;
};

Wrapped* Wrapped_New(int v);
void Wrapped_Ctor(Wrapped* self, int v);
int Wrapped_Get(Wrapped* self);

void Wrapped_Dispose(Wrapped* self);
void Wrapped_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Wrapped_GcKind;
void Wrapped_Free(Wrapped* self);
Wrapped* Wrapped_Retain(Wrapped* self);
extern IHas_vtable Wrapped_IHas_vtable_instance;
static inline IHas* Wrapped_as_IHas(Wrapped* self)
{
    IHas* _iface = (IHas*)malloc(sizeof(IHas));
    if (!_iface) return NULL;
    _iface->vtable = &Wrapped_IHas_vtable_instance;
    _iface->obj = self;
    return _iface;
}

// C# value-type struct: Pair
typedef struct Pair {
    Item* A;
} Pair;
#ifndef CS2SX_Nullable_Pair_DEFINED
#define CS2SX_Nullable_Pair_DEFINED
typedef struct Nullable_Pair { Pair value; int has; } Nullable_Pair;
#endif
int Pair_Equals(Pair a, Pair b);

struct FrameMemoryApp
{
    SwitchApp base;
    Dict_str_Item* f_dict;
    Stack_Item* f_stack;
    Queue_Item* f_queue;
    HashSet_Item* f_set;
    Item** f_arr;
    Box_Item* f_box;
    Pair f_pair;
    IHas* f_iface;
    Func_int_t f_fn;
    Item* f_field;
    int f_frame;
};

void FrameMemoryApp_Init(FrameMemoryApp* self);
int FrameMemoryApp_Garbage();
void FrameMemoryApp_OnFrame(FrameMemoryApp* self);

void FrameMemoryApp_Free(FrameMemoryApp* self);


#endif
