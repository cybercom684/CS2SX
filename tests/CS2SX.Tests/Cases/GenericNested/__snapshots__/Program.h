#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct GenericNestedApp
{
    SwitchApp base;
};

void GenericNestedApp_Init(GenericNestedApp* self);
void GenericNestedApp_OnInit(GenericNestedApp* self);

void GenericNestedApp_Free(GenericNestedApp* self);


#endif
