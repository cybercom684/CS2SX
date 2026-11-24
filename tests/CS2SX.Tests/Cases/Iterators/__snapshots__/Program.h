#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

void CS2SX_Yield_Begin(int points);
void CS2SX_Yield_Return(int point);

typedef struct CS2SX_Iter_vtable
{
    int (*MoveNext)(void* self);
} CS2SX_Iter_vtable;

struct CS2SX_Iter
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    CS2SX_Iter_vtable* vtable;
    void* f_Current;
};

CS2SX_Iter* CS2SX_Iter_New(void);
void CS2SX_Iter_Ctor(CS2SX_Iter* self);
/* abstract: int CS2SX_Iter_MoveNext(CS2SX_Iter* self) */
void CS2SX_Iter_Reset(CS2SX_Iter* self);
void CS2SX_Iter_Dispose(CS2SX_Iter* self);

void CS2SX_Iter__Destroy(CS2SX_Iter* self);
extern const CS2SX_ClassInfo CS2SX_Iter_ClassInfo;
void CS2SX_Iter_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind CS2SX_Iter_GcKind;
void CS2SX_Iter_Free(CS2SX_Iter* self);
CS2SX_Iter* CS2SX_Iter_Retain(CS2SX_Iter* self);
typedef struct CS2SX_Iter_int_vtable
{
    int (*MoveNext)(void* self);
} CS2SX_Iter_int_vtable;

struct CS2SX_Iter_int
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    CS2SX_Iter_int_vtable* vtable;
    int f_Current;
};

CS2SX_Iter_int* CS2SX_Iter_int_New(void);
void CS2SX_Iter_int_Ctor(CS2SX_Iter_int* self);
/* abstract: int CS2SX_Iter_int_MoveNext(CS2SX_Iter_int* self) */
void CS2SX_Iter_int_Reset(CS2SX_Iter_int* self);
void CS2SX_Iter_int_Dispose(CS2SX_Iter_int* self);

void CS2SX_Iter_int__Destroy(CS2SX_Iter_int* self);
extern const CS2SX_ClassInfo CS2SX_Iter_int_ClassInfo;
void CS2SX_Iter_int_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind CS2SX_Iter_int_GcKind;
void CS2SX_Iter_int_Free(CS2SX_Iter_int* self);
CS2SX_Iter_int* CS2SX_Iter_int_Retain(CS2SX_Iter_int* self);
typedef struct CS2SX_Iter_str_vtable
{
    int (*MoveNext)(void* self);
} CS2SX_Iter_str_vtable;

struct CS2SX_Iter_str
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    CS2SX_Iter_str_vtable* vtable;
    const char* f_Current;
};

CS2SX_Iter_str* CS2SX_Iter_str_New(void);
void CS2SX_Iter_str_Ctor(CS2SX_Iter_str* self);
/* abstract: int CS2SX_Iter_str_MoveNext(CS2SX_Iter_str* self) */
void CS2SX_Iter_str_Reset(CS2SX_Iter_str* self);
void CS2SX_Iter_str_Dispose(CS2SX_Iter_str* self);

void CS2SX_Iter_str__Destroy(CS2SX_Iter_str* self);
extern const CS2SX_ClassInfo CS2SX_Iter_str_ClassInfo;
void CS2SX_Iter_str_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind CS2SX_Iter_str_GcKind;
void CS2SX_Iter_str_Free(CS2SX_Iter_str* self);
CS2SX_Iter_str* CS2SX_Iter_str_Retain(CS2SX_Iter_str* self);
struct Tree
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    int f_Value;
    Tree* f_Left;
    Tree* f_Right;
};

Tree* Tree_New(int v, Tree* l, Tree* r);
void Tree_Ctor(Tree* self, int v, Tree* l, Tree* r);

void Tree__Destroy(Tree* self);
extern const CS2SX_ClassInfo Tree_ClassInfo;
void Tree_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Tree_GcKind;
void Tree_Free(Tree* self);
Tree* Tree_Retain(Tree* self);
List_int* Tree_get_InOrder(Tree* self);
struct Blinker
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    const char* f_Name;
    int f_Steps;
};

Blinker* Blinker_New(void);
void Blinker_Ctor(Blinker* self);
CS2SX_Iter_int* Blinker_Run(Blinker* self, int times);

void Blinker__Destroy(Blinker* self);
extern const CS2SX_ClassInfo Blinker_ClassInfo;
void Blinker_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Blinker_GcKind;
void Blinker_Free(Blinker* self);
Blinker* Blinker_Retain(Blinker* self);
struct Run_Iter
{
    CS2SX_Iter_int base;
    Blinker* f_cs2sx_this;
    int f_cs2sx_state;
    int f_times;
    const char* f_label;
    int f_i;
};

Run_Iter* Run_Iter_New(void);
void Run_Iter_Ctor(Run_Iter* self);
int Run_Iter_MoveNext(Run_Iter* self);

void Run_Iter__Destroy(Run_Iter* self);
extern const CS2SX_ClassInfo Run_Iter_ClassInfo;
void Run_Iter_Gc(void* self, CS2SX_GcVisit visit);
void Run_Iter_Free(Run_Iter* self);
Run_Iter* Run_Iter_Retain(Run_Iter* self);
struct IteratorsApp
{
    SwitchApp base;
    CS2SX_Iter_int* f_co;
    CS2SX_Iter* f_wait;
    int f_frame;
};

void IteratorsApp_Init(IteratorsApp* self);
List_int* IteratorsApp_Evens(int max);
List_str* IteratorsApp_Words();
CS2SX_Iter_str* IteratorsApp_Over(List_str* items, const char* sep);
CS2SX_Iter_str* IteratorsApp_FromSequence();
CS2SX_Iter* IteratorsApp_Wait(int frames);
void IteratorsApp_OnInit(IteratorsApp* self);
void IteratorsApp_OnFrame(IteratorsApp* self);

void IteratorsApp_Free(IteratorsApp* self);
struct Over_Iter
{
    CS2SX_Iter_str base;
    int f_cs2sx_state;
    List_str* f_items;
    const char* f_sep;
    const char* f_s;
    List_str* f_cs2sx_c1;
    int f_cs2sx_i1;
};

Over_Iter* Over_Iter_New(void);
void Over_Iter_Ctor(Over_Iter* self);
int Over_Iter_MoveNext(Over_Iter* self);

void Over_Iter__Destroy(Over_Iter* self);
extern const CS2SX_ClassInfo Over_Iter_ClassInfo;
void Over_Iter_Gc(void* self, CS2SX_GcVisit visit);
void Over_Iter_Free(Over_Iter* self);
Over_Iter* Over_Iter_Retain(Over_Iter* self);
struct FromSequence_Iter
{
    CS2SX_Iter_str base;
    int f_cs2sx_state;
    const char* f_up;
    const char* f_w;
    List_str* f_cs2sx_c1;
    int f_cs2sx_i1;
};

FromSequence_Iter* FromSequence_Iter_New(void);
void FromSequence_Iter_Ctor(FromSequence_Iter* self);
int FromSequence_Iter_MoveNext(FromSequence_Iter* self);

void FromSequence_Iter__Destroy(FromSequence_Iter* self);
extern const CS2SX_ClassInfo FromSequence_Iter_ClassInfo;
void FromSequence_Iter_Gc(void* self, CS2SX_GcVisit visit);
void FromSequence_Iter_Free(FromSequence_Iter* self);
FromSequence_Iter* FromSequence_Iter_Retain(FromSequence_Iter* self);
struct Wait_Iter
{
    CS2SX_Iter base;
    int f_cs2sx_state;
    int f_frames;
};

Wait_Iter* Wait_Iter_New(void);
void Wait_Iter_Ctor(Wait_Iter* self);
int Wait_Iter_MoveNext(Wait_Iter* self);

void Wait_Iter__Destroy(Wait_Iter* self);
extern const CS2SX_ClassInfo Wait_Iter_ClassInfo;
void Wait_Iter_Gc(void* self, CS2SX_GcVisit visit);
void Wait_Iter_Free(Wait_Iter* self);
Wait_Iter* Wait_Iter_Retain(Wait_Iter* self);


#endif
