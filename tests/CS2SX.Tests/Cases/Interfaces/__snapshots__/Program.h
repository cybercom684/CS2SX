#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_interfaces.h"
#include "_generics.h"

struct Bullet
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    /* implements IUpdatable */
    float f_Y;
};

Bullet* Bullet_New(void);
void Bullet_Ctor(Bullet* self);
void Bullet_Update(Bullet* self, float dt);
int Bullet_IsAlive(Bullet* self);

void Bullet__Destroy(Bullet* self);
extern const CS2SX_ClassInfo Bullet_ClassInfo;
void Bullet_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Bullet_GcKind;
void Bullet_Free(Bullet* self);
Bullet* Bullet_Retain(Bullet* self);
extern IUpdatable_vtable Bullet_IUpdatable_vtable_instance;
Bullet* Bullet_Retain(Bullet* self);
void Bullet_Free(Bullet* self);
extern const CS2SX_GcKind Bullet_GcKind;
extern const CS2SX_ClassInfo Bullet_ClassInfo;
static inline IUpdatable* Bullet_as_IUpdatable(Bullet* self)
{
    return (IUpdatable*)CS2SX_Iface_Wrap(sizeof(IUpdatable), &Bullet_IUpdatable_vtable_instance, self, (void* (*)(void*))Bullet_Retain, (CS2SX_FreeFn)Bullet_Free, &Bullet_GcKind, &Bullet_ClassInfo);
}

struct Timer
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    /* implements IUpdatable */
    float f_Left;
};

Timer* Timer_New(void);
void Timer_Ctor(Timer* self);
void Timer_Update(Timer* self, float dt);
int Timer_IsAlive(Timer* self);

void Timer__Destroy(Timer* self);
extern const CS2SX_ClassInfo Timer_ClassInfo;
void Timer_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Timer_GcKind;
void Timer_Free(Timer* self);
Timer* Timer_Retain(Timer* self);
extern IUpdatable_vtable Timer_IUpdatable_vtable_instance;
Timer* Timer_Retain(Timer* self);
void Timer_Free(Timer* self);
extern const CS2SX_GcKind Timer_GcKind;
extern const CS2SX_ClassInfo Timer_ClassInfo;
static inline IUpdatable* Timer_as_IUpdatable(Timer* self)
{
    return (IUpdatable*)CS2SX_Iface_Wrap(sizeof(IUpdatable), &Timer_IUpdatable_vtable_instance, self, (void* (*)(void*))Timer_Retain, (CS2SX_FreeFn)Timer_Free, &Timer_GcKind, &Timer_ClassInfo);
}

struct InterfacesApp
{
    SwitchApp base;
    List_IUpdatable* f_things;
};

void InterfacesApp_Init(InterfacesApp* self);
void InterfacesApp_OnInit(InterfacesApp* self);
void InterfacesApp_OnFrame(InterfacesApp* self);

void InterfacesApp_Free(InterfacesApp* self);


#endif
