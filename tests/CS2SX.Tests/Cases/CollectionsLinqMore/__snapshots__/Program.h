#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct Item
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    const char* f_Name;
    int f_Score;
};

Item* Item_New(const char* n, int s);
void Item_Ctor(Item* self, const char* n, int s);

void Item__Destroy(Item* self);
extern const CS2SX_ClassInfo Item_ClassInfo;
void Item_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Item_GcKind;
void Item_Free(Item* self);
Item* Item_Retain(Item* self);
struct CollectionsLinqMoreApp
{
    SwitchApp base;
};

void CollectionsLinqMoreApp_Init(CollectionsLinqMoreApp* self);
int CollectionsLinqMoreApp_SumAll(int* xs);
void CollectionsLinqMoreApp_OnInit(CollectionsLinqMoreApp* self);
void CollectionsLinqMoreApp_OnFrame(CollectionsLinqMoreApp* self);

void CollectionsLinqMoreApp_Free(CollectionsLinqMoreApp* self);


#endif
