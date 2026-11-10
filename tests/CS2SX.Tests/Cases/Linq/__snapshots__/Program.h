#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct Enemy
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const char* f_Name;
    int f_Hp;
};

Enemy* Enemy_New(const char* name, int hp);
void Enemy_Ctor(Enemy* self, const char* name, int hp);

void Enemy_Dispose(Enemy* self);
void Enemy_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Enemy_GcKind;
void Enemy_Free(Enemy* self);
Enemy* Enemy_Retain(Enemy* self);
struct LinqApp
{
    SwitchApp base;
};

void LinqApp_Init(LinqApp* self);
void LinqApp_OnInit(LinqApp* self);

void LinqApp_Free(LinqApp* self);


#endif
