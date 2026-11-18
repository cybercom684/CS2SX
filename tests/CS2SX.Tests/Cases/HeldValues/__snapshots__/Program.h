#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_interfaces.h"
#include "_generics.h"

struct Item
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    int f_V;
};

extern int Item_Alive;
Item* Item_New(void);
void Item_Ctor(Item* self);
int Item_Get(Item* self);

void Item__Destroy(Item* self);
extern const CS2SX_ClassInfo Item_ClassInfo;
void Item_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Item_GcKind;
void Item_Free(Item* self);
Item* Item_Retain(Item* self);
struct Thing
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    /* implements IThing */
};

extern int Thing_Alive;
Thing* Thing_New(void);
void Thing_Ctor(Thing* self);
int Thing_Value(Thing* self);

void Thing__Destroy(Thing* self);
extern const CS2SX_ClassInfo Thing_ClassInfo;
void Thing_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Thing_GcKind;
void Thing_Free(Thing* self);
Thing* Thing_Retain(Thing* self);
extern IThing_vtable Thing_IThing_vtable_instance;
Thing* Thing_Retain(Thing* self);
void Thing_Free(Thing* self);
extern const CS2SX_GcKind Thing_GcKind;
extern const CS2SX_ClassInfo Thing_ClassInfo;
static inline IThing* Thing_as_IThing(Thing* self)
{
    return (IThing*)CS2SX_Iface_Wrap(sizeof(IThing), &Thing_IThing_vtable_instance, self, (void* (*)(void*))Thing_Retain, (CS2SX_FreeFn)Thing_Free, &Thing_GcKind, &Thing_ClassInfo);
}

// C# value-type struct: Slot
typedef struct Slot {
    Item* Held;
    const char* Label;
    int N;
} Slot;
#ifndef CS2SX_Nullable_Slot_DEFINED
#define CS2SX_Nullable_Slot_DEFINED
typedef struct Nullable_Slot { Slot value; int has; } Nullable_Slot;
#endif
int Slot_Equals(Slot a, Slot b);
Item* Item_Retain(Item* self);
void Item_Free(Item* self);
extern const CS2SX_GcKind Item_GcKind;
void Slot_Keep(Slot* s);
void Slot_Drop(Slot* s);
void Slot_Gc(Slot* s, CS2SX_GcVisit visit);
static inline Slot Slot_KeepV(Slot v) { Slot_Keep(&v); return v; }
static inline void Slot_DropV(Slot v) { Slot_Drop(&v); }
#ifndef CS2SX_LIST_Slot_DEFINED
#define CS2SX_LIST_Slot_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Slot, Slot, Slot_KeepV, Slot_DropV, Slot_Equals)
#endif

struct Holder
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Func_int_t f_Getter;
    Action_t f_Changed;
    IThing* f_Thing;
    Slot f_Slot;
    _Tuple2_Item_int f_Pair;
    List_Func_int_t* f_Calls;
    List_IThing* f_Things;
    List_Slot* f_Slots;
    int f_Count;
};

extern int Holder_Alive;
Holder* Holder_New(void);
void Holder_Ctor(Holder* self);
void Holder_Wire(Holder* self);
void Holder_Fire(Holder* self);

void Holder__Destroy(Holder* self);
extern const CS2SX_ClassInfo Holder_ClassInfo;
void Holder_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Holder_GcKind;
void Holder_Free(Holder* self);
Holder* Holder_Retain(Holder* self);
struct HeldValuesApp
{
    SwitchApp base;
    Holder* f_h;
    long long f_base;
    int f_frame;
    int f_sink;
};

void HeldValuesApp_Init(HeldValuesApp* self);
void HeldValuesApp_Churn(HeldValuesApp* self);
void HeldValuesApp_OnFrame(HeldValuesApp* self);

void HeldValuesApp_Free(HeldValuesApp* self);


#endif
