#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

#define K_R (6.0f)
#define K_H (3.0)
#define K_Big (1E+30)
#define K_N (((double)NAN))

struct IndexOfAndConstantsApp
{
    SwitchApp base;
};

#define IndexOfAndConstantsApp_R (6.0f)
#define IndexOfAndConstantsApp_D (3.0)
#define IndexOfAndConstantsApp_F2 (2.5f)
#define IndexOfAndConstantsApp_S ((float)(4))
void IndexOfAndConstantsApp_Init(IndexOfAndConstantsApp* self);
void IndexOfAndConstantsApp_OnInit(IndexOfAndConstantsApp* self);
void IndexOfAndConstantsApp_OnFrame(IndexOfAndConstantsApp* self);

void IndexOfAndConstantsApp_Free(IndexOfAndConstantsApp* self);


#endif
