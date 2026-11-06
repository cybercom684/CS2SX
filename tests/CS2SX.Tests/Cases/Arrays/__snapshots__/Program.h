#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct ArraysApp
{
    SwitchApp base;
    float* f_weights;
};

extern int* ArraysApp_s_table;
extern const char** ArraysApp_s_names;
void ArraysApp_Init(ArraysApp* self);
void ArraysApp_OnInit(ArraysApp* self);

void ArraysApp_Free(ArraysApp* self);


#endif
