#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct Item
{
    int _rc;
    const char* f_Name;
    int f_Count;
};

Item* Item_New(const char* name, int count);

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
