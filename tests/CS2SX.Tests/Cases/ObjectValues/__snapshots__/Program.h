#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_interfaces.h"
#include "_generics.h"

#ifndef CS2SX_LIST_Rank_DEFINED
#define CS2SX_LIST_Rank_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Rank, Rank, CS2SX_ID, CS2SX_NOREF, CS2SX_EQ_MEM)
#endif
struct Foo
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    int f_A;
};

Foo* Foo_New(void);
void Foo_Ctor(Foo* self);

void Foo__Destroy(Foo* self);
extern const CS2SX_ClassInfo Foo_ClassInfo;
void Foo_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Foo_GcKind;
void Foo_Free(Foo* self);
Foo* Foo_Retain(Foo* self);
struct Bar
{
    Foo base;
    /* implements IShape */
};

Bar* Bar_New(void);
void Bar_Ctor(Bar* self);
int Bar_Size(Bar* self);

void Bar__Destroy(Bar* self);
extern const CS2SX_ClassInfo Bar_ClassInfo;
void Bar_Gc(void* self, CS2SX_GcVisit visit);
void Bar_Free(Bar* self);
Bar* Bar_Retain(Bar* self);
extern IShape_vtable Bar_IShape_vtable_instance;
Foo* Foo_Retain(Foo* self);
void Foo_Free(Foo* self);
extern const CS2SX_GcKind Foo_GcKind;
extern const CS2SX_ClassInfo Bar_ClassInfo;
static inline IShape* Bar_as_IShape(Bar* self)
{
    return (IShape*)CS2SX_Iface_Wrap(sizeof(IShape), &Bar_IShape_vtable_instance, self, (void* (*)(void*))Foo_Retain, (CS2SX_FreeFn)Foo_Free, &Foo_GcKind, &Bar_ClassInfo);
}

struct Other
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
};

Other* Other_New(void);
void Other_Ctor(Other* self);

void Other__Destroy(Other* self);
extern const CS2SX_ClassInfo Other_ClassInfo;
void Other_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Other_GcKind;
void Other_Free(Other* self);
Other* Other_Retain(Other* self);
struct ObjectValuesApp
{
    SwitchApp base;
    void* f_held;
    List_voidptr* f_bag;
    Dict_str_voidptr* f_props;
    int f_frame;
};

void ObjectValuesApp_Init(ObjectValuesApp* self);
const char* ObjectValuesApp_Describe(void* o);
void* ObjectValuesApp_Pick(int i);
void ObjectValuesApp_OnInit(ObjectValuesApp* self);
void ObjectValuesApp_OnFrame(ObjectValuesApp* self);

void ObjectValuesApp_Free(ObjectValuesApp* self);


#endif
