#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct Item
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const char* f_Name;
    int f_Count;
};

Item* Item_New(const char* name, int count);
void Item_Ctor(Item* self, const char* name, int count);

void Item_Dispose(Item* self);
void Item_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Item_GcKind;
void Item_Free(Item* self);
Item* Item_Retain(Item* self);
struct GenericsCollectionsApp
{
    SwitchApp base;
};

void GenericsCollectionsApp_Init(GenericsCollectionsApp* self);
void GenericsCollectionsApp_OnInit(GenericsCollectionsApp* self);

void GenericsCollectionsApp_Free(GenericsCollectionsApp* self);


#endif
