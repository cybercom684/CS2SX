#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct Punkt
{
    int _rc;
    int f_X;
    int f_Y;
};

Punkt* Punkt_New(int X, int Y);
void Punkt_Free(Punkt* self);
Punkt* Punkt_Retain(Punkt* self);

struct LanguageSugarApp
{
    SwitchApp base;
    int f_calls;
    Punkt* f_origin;
};

void LanguageSugarApp_Init(LanguageSugarApp* self);
int LanguageSugarApp_Apply(Func_int_int_t op, int v);
void LanguageSugarApp_OnInit(LanguageSugarApp* self);
int LanguageSugarApp_Sq2(int x);
int LanguageSugarApp_OnInit__Sq(LanguageSugarApp* self, int x);
int LanguageSugarApp_OnInit__Fact(LanguageSugarApp* self, int n);
void LanguageSugarApp_OnInit__Count(LanguageSugarApp* self);
int LanguageSugarApp_OnInit__SqPlus(LanguageSugarApp* self, int x);

void LanguageSugarApp_Free(LanguageSugarApp* self);


#endif
