#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct Board
{
    int _rc;
    int* f_Cells;
};

Board* Board_New(int n);
void Board_Ctor(Board* self, int n);
int Board_Sum(int* values);
int Board_Count(int* values);
int* Board_Doubled(Board* self);

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
