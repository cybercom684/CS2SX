#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct Game
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    int* f_Data;
};

Game* Game_New(void);
void Game_Ctor(Game* self);
int Game_Read(Game* self, int i);
void Game_Step(Game* self, int i);

void Game_Dispose(Game* self);
void Game_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Game_GcKind;
void Game_Free(Game* self);
Game* Game_Retain(Game* self);
struct StackTracesApp
{
    SwitchApp base;
};

void StackTracesApp_Init(StackTracesApp* self);
void StackTracesApp_OnInit(StackTracesApp* self);
void StackTracesApp_Helper();

void StackTracesApp_Free(StackTracesApp* self);


#endif
