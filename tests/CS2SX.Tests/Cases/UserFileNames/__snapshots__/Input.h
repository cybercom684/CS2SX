#pragma once
#ifndef CS2SX_INPUT_H
#define CS2SX_INPUT_H

#include "_forward.h"

struct InputMapper
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    int f_Presses;
};

InputMapper* InputMapper_New(void);
void InputMapper_Ctor(InputMapper* self);
void InputMapper_Poll(InputMapper* self);

void InputMapper_Dispose(InputMapper* self);
void InputMapper_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind InputMapper_GcKind;
void InputMapper_Free(InputMapper* self);
InputMapper* InputMapper_Retain(InputMapper* self);


#endif
