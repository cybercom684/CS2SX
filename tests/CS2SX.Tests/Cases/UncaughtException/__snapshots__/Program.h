#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct Enemy
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Enemy* f_Target;
    int f_Hp;
};

Enemy* Enemy_New(void);
void Enemy_Ctor(Enemy* self);
void Enemy_Hit(Enemy* self);

void Enemy__Destroy(Enemy* self);
extern const CS2SX_ClassInfo Enemy_ClassInfo;
void Enemy_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Enemy_GcKind;
void Enemy_Free(Enemy* self);
Enemy* Enemy_Retain(Enemy* self);
struct UncaughtExceptionApp
{
    SwitchApp base;
    Enemy* f_enemy;
    int f_frame;
};

void UncaughtExceptionApp_Init(UncaughtExceptionApp* self);
void UncaughtExceptionApp_OnFrame(UncaughtExceptionApp* self);

void UncaughtExceptionApp_Free(UncaughtExceptionApp* self);


#endif
