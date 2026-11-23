#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

void CS2SX_Yield_Begin(int points);
void CS2SX_Yield_Return(int point);

typedef struct CS2SX_Iter_vtable
{
    int (*MoveNext)(void* self);
} CS2SX_Iter_vtable;

struct CS2SX_Iter
{
    CS2SX_Iter_vtable* vtable;
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
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
    CS2SX_Iter_int_vtable* vtable;
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
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
#ifndef CS2SX_LIST_Kind_DEFINED
#define CS2SX_LIST_Kind_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Kind, Kind, CS2SX_ID, CS2SX_NOREF, CS2SX_EQ_MEM)
#endif
struct Pair
{
    int _rc;
    int f_X;
    int f_Y;
};

Pair* Pair_New(int X, int Y);
void Pair_Free(Pair* self);
Pair* Pair_Retain(Pair* self);
extern const CS2SX_ClassInfo Pair_ClassInfo;
int Pair_Equals(Pair* a, Pair* b);
const char* Pair_ToString(Pair* self);

struct Hero
{
    int _rc;
    const char* f_Name;
    int f_Level;
    Kind f_Element;
    Pair* f_Pos;
    int f_Ready;
};

Hero* Hero_New(const char* Name, int Level, Kind Element, Pair* Pos);
void Hero_Free(Hero* self);
Hero* Hero_Retain(Hero* self);
extern const CS2SX_ClassInfo Hero_ClassInfo;
int Hero_Equals(Hero* a, Hero* b);
const char* Hero_ToString(Hero* self);

struct Deck
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    List_int* f_cards;
};

Deck* Deck_New(void);
void Deck_Ctor(Deck* self);
CS2SX_Iter_int* Deck_GetEnumerator(Deck* self);

void Deck__Destroy(Deck* self);
extern const CS2SX_ClassInfo Deck_ClassInfo;
void Deck_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Deck_GcKind;
void Deck_Free(Deck* self);
Deck* Deck_Retain(Deck* self);
struct GetEnumerator_Iter
{
    CS2SX_Iter_int base;
    Deck* f_cs2sx_this;
    int f_cs2sx_state;
    int f_c;
    List_int* f_cs2sx_c1;
    int f_cs2sx_i1;
};

GetEnumerator_Iter* GetEnumerator_Iter_New(void);
void GetEnumerator_Iter_Ctor(GetEnumerator_Iter* self);
int GetEnumerator_Iter_MoveNext(GetEnumerator_Iter* self);

void GetEnumerator_Iter__Destroy(GetEnumerator_Iter* self);
extern const CS2SX_ClassInfo GetEnumerator_Iter_ClassInfo;
void GetEnumerator_Iter_Gc(void* self, CS2SX_GcVisit visit);
void GetEnumerator_Iter_Free(GetEnumerator_Iter* self);
GetEnumerator_Iter* GetEnumerator_Iter_Retain(GetEnumerator_Iter* self);
struct Node
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    int f_V;
};

Node* Node_New(int v);
void Node_Ctor(Node* self, int v);
int Node_Twice(Node* self);
const char* Node_Name(Node* self, const char* prefix);

void Node__Destroy(Node* self);
extern const CS2SX_ClassInfo Node_ClassInfo;
void Node_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Node_GcKind;
void Node_Free(Node* self);
Node* Node_Retain(Node* self);
struct RecordsAndFormatsApp
{
    SwitchApp base;
};

void RecordsAndFormatsApp_Init(RecordsAndFormatsApp* self);
void RecordsAndFormatsApp_OnInit(RecordsAndFormatsApp* self);
void RecordsAndFormatsApp_OnFrame(RecordsAndFormatsApp* self);

void RecordsAndFormatsApp_Free(RecordsAndFormatsApp* self);


#endif
