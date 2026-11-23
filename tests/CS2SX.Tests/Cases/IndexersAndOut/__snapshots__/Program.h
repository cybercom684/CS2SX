#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct Item
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    const char* f_Name;
};

Item* Item_New(const char* n);
void Item_Ctor(Item* self, const char* n);

void Item__Destroy(Item* self);
extern const CS2SX_ClassInfo Item_ClassInfo;
void Item_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Item_GcKind;
void Item_Free(Item* self);
Item* Item_Retain(Item* self);
struct Bag
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    List_int* f_items;
};

Bag* Bag_New(void);
void Bag_Ctor(Bag* self);
void Bag_Add(Bag* self, int x);
int Bag_TryFirst(Bag* self, int* v);
void Bag_Swap(Bag* self, int* a, int* b);
int Bag_Find(Bag* self, const char* name, Item** found);

void Bag__Destroy(Bag* self);
extern const CS2SX_ClassInfo Bag_ClassInfo;
void Bag_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Bag_GcKind;
void Bag_Free(Bag* self);
Bag* Bag_Retain(Bag* self);
int Bag_get_Count(Bag* self);
int Bag_get(Bag* self, int i);
void Bag_set(Bag* self, int i, int value);
struct Grid
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Dict_str_str* f_cells;
};

Grid* Grid_New(void);
void Grid_Ctor(Grid* self);

void Grid__Destroy(Grid* self);
extern const CS2SX_ClassInfo Grid_ClassInfo;
void Grid_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Grid_GcKind;
void Grid_Free(Grid* self);
Grid* Grid_Retain(Grid* self);
const char* Grid_get(Grid* self, const char* key);
void Grid_set(Grid* self, const char* key, const char* value);
struct BigBag
{
    Bag base;
};

BigBag* BigBag_New(void);
void BigBag_Ctor(BigBag* self);

void BigBag__Destroy(BigBag* self);
extern const CS2SX_ClassInfo BigBag_ClassInfo;
void BigBag_Gc(void* self, CS2SX_GcVisit visit);
void BigBag_Free(BigBag* self);
BigBag* BigBag_Retain(BigBag* self);
struct IndexersAndOutApp
{
    SwitchApp base;
    Bag* f_field;
};

void IndexersAndOutApp_Init(IndexersAndOutApp* self);
int IndexersAndOutApp_Mine(IndexersAndOutApp* self, const char** s);
void IndexersAndOutApp_OnInit(IndexersAndOutApp* self);
void IndexersAndOutApp_OnFrame(IndexersAndOutApp* self);

void IndexersAndOutApp_Free(IndexersAndOutApp* self);


#endif
