#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct ClosuresApp
{
    SwitchApp base;
    Action_t f_later;
};

void ClosuresApp_Init(ClosuresApp* self);
void ClosuresApp_OnInit(ClosuresApp* self);
void ClosuresApp_Make(ClosuresApp* self, int start);

void ClosuresApp_Free(ClosuresApp* self);
struct __Ref_int
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    int f_V;
};

__Ref_int* __Ref_int_New(int v);
void __Ref_int_Ctor(__Ref_int* self, int v);

void __Ref_int__Destroy(__Ref_int* self);
extern const CS2SX_ClassInfo __Ref_int_ClassInfo;
void __Ref_int_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind __Ref_int_GcKind;
void __Ref_int_Free(__Ref_int* self);
__Ref_int* __Ref_int_Retain(__Ref_int* self);


#endif
