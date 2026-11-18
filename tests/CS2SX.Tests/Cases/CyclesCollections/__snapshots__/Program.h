#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct Hub
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Hub* f_Owner;
    Dict_str_Hub* f_ByName;
    Stack_Hub* f_History;
    Queue_Hub* f_Pending;
    HashSet_Hub* f_Seen;
};

extern int Hub_Alive;
Hub* Hub_New(void);
void Hub_Ctor(Hub* self);
Hub* Hub_Child(Hub* self);

void Hub__Destroy(Hub* self);
extern const CS2SX_ClassInfo Hub_ClassInfo;
void Hub_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Hub_GcKind;
void Hub_Free(Hub* self);
Hub* Hub_Retain(Hub* self);
struct CyclesCollectionsApp
{
    SwitchApp base;
    Hub* f_kept;
    long long f_base;
    int f_frame;
};

void CyclesCollectionsApp_Init(CyclesCollectionsApp* self);
Hub* CyclesCollectionsApp_Build();
void CyclesCollectionsApp_OnFrame(CyclesCollectionsApp* self);

void CyclesCollectionsApp_Free(CyclesCollectionsApp* self);


#endif
