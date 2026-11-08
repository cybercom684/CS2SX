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
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    int f_V;
};

__Ref_int* __Ref_int_New(int v);
void __Ref_int_Ctor(__Ref_int* self, int v);

void __Ref_int_Dispose(__Ref_int* self);
void __Ref_int_Free(__Ref_int* self);
__Ref_int* __Ref_int_Retain(__Ref_int* self);


#endif
