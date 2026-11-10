#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct Inventory
{
    int _rc;
    List_str* f_Items;
    int f_Slots;
};

Inventory* Inventory_New(List_str* Items, int Slots);
void Inventory_Free(Inventory* self);
Inventory* Inventory_Retain(Inventory* self);

struct Bag
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    List_int* f_Values;
    StringBuilder* f_Log;
    Dict_str_int* f_Counts;
};

Bag* Bag_New(void);
void Bag_Ctor(Bag* self);

void Bag_Dispose(Bag* self);
void Bag_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Bag_GcKind;
void Bag_Free(Bag* self);
Bag* Bag_Retain(Bag* self);
struct CollectionMemoryApp
{
    SwitchApp base;
    List_int* f_scores;
    Bag* f_bag;
    Inventory* f_inv;
    Func_int_t f_counter;
    List_int** f_grid;
    long long f_base;
    int f_frame;
};

void CollectionMemoryApp_Init(CollectionMemoryApp* self);
void CollectionMemoryApp_OnFrame(CollectionMemoryApp* self);

void CollectionMemoryApp_Free(CollectionMemoryApp* self);


#endif
