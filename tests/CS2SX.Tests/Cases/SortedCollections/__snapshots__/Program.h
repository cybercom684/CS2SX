#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct Task1
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    const char* f_Name;
    int f_Weight;
};

Task1* Task1_New(const char* name, int weight);
void Task1_Ctor(Task1* self, const char* name, int weight);
int Task1_CompareTo(Task1* self, Task1* other);

void Task1__Destroy(Task1* self);
extern const CS2SX_ClassInfo Task1_ClassInfo;
void Task1_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Task1_GcKind;
void Task1_Free(Task1* self);
Task1* Task1_Retain(Task1* self);
struct SortedCollectionsApp
{
    SwitchApp base;
    CS2SX_SortedDictionary_str_int* f_scores;
};

void SortedCollectionsApp_Init(SortedCollectionsApp* self);
void SortedCollectionsApp_OnInit(SortedCollectionsApp* self);
void SortedCollectionsApp_OnFrame(SortedCollectionsApp* self);

void SortedCollectionsApp_Free(SortedCollectionsApp* self);


#endif
