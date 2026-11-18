#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct Board
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    int* f_Cells;
};

Board* Board_New(int n);
void Board_Ctor(Board* self, int n);
int Board_Sum(int* values);
int Board_Count(int* values);
int* Board_Doubled(Board* self);

void Board__Destroy(Board* self);
extern const CS2SX_ClassInfo Board_ClassInfo;
void Board_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Board_GcKind;
void Board_Free(Board* self);
Board* Board_Retain(Board* self);
struct ArraysAdvancedApp
{
    SwitchApp base;
};

void ArraysAdvancedApp_Init(ArraysAdvancedApp* self);
void ArraysAdvancedApp_OnInit(ArraysAdvancedApp* self);

void ArraysAdvancedApp_Free(ArraysAdvancedApp* self);


#endif
