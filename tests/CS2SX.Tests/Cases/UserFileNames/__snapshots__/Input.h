#pragma once
#ifndef CS2SX_INPUT_H
#define CS2SX_INPUT_H

#include "_forward.h"

struct InputMapper
{
    int _rc;
    int f_Presses;
};

InputMapper* InputMapper_New(void);
void InputMapper_Ctor(InputMapper* self);
void InputMapper_Poll(InputMapper* self);

void InputMapper_Free(InputMapper* self);
InputMapper* InputMapper_Retain(InputMapper* self);


#endif
