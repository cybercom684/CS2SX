#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_interfaces.h"
#include "_generics.h"

struct Bullet
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    /* implements IUpdatable */
    float f_Y;
};

Bullet* Bullet_New(void);
void Bullet_Ctor(Bullet* self);
void Bullet_Update(Bullet* self, float dt);
int Bullet_IsAlive(Bullet* self);

void Bullet_Dispose(Bullet* self);
void Bullet_Free(Bullet* self);
Bullet* Bullet_Retain(Bullet* self);
extern IUpdatable_vtable Bullet_IUpdatable_vtable_instance;
static inline IUpdatable* Bullet_as_IUpdatable(Bullet* self)
{
    IUpdatable* _iface = (IUpdatable*)malloc(sizeof(IUpdatable));
    if (!_iface) return NULL;
    _iface->vtable = &Bullet_IUpdatable_vtable_instance;
    _iface->obj = self;
    return _iface;
}

struct Timer
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    /* implements IUpdatable */
    float f_Left;
};

Timer* Timer_New(void);
void Timer_Ctor(Timer* self);
void Timer_Update(Timer* self, float dt);
int Timer_IsAlive(Timer* self);

void Timer_Dispose(Timer* self);
void Timer_Free(Timer* self);
Timer* Timer_Retain(Timer* self);
extern IUpdatable_vtable Timer_IUpdatable_vtable_instance;
static inline IUpdatable* Timer_as_IUpdatable(Timer* self)
{
    IUpdatable* _iface = (IUpdatable*)malloc(sizeof(IUpdatable));
    if (!_iface) return NULL;
    _iface->vtable = &Timer_IUpdatable_vtable_instance;
    _iface->obj = self;
    return _iface;
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
