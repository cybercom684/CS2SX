#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct Counter
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
};

extern int Counter_Disposed;
Counter* Counter_New(void);
void Counter_Ctor(Counter* self);

void Counter__Destroy(Counter* self);
extern const CS2SX_ClassInfo Counter_ClassInfo;
void Counter_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Counter_GcKind;
void Counter_Free(Counter* self);
Counter* Counter_Retain(Counter* self);
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

void Node__Destroy(Node* self);
extern const CS2SX_ClassInfo Node_ClassInfo;
void Node_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Node_GcKind;
void Node_Free(Node* self);
Node* Node_Retain(Node* self);
struct Shape
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
};

Shape* Shape_New(void);
void Shape_Ctor(Shape* self);

void Shape__Destroy(Shape* self);
extern const CS2SX_ClassInfo Shape_ClassInfo;
void Shape_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Shape_GcKind;
void Shape_Free(Shape* self);
Shape* Shape_Retain(Shape* self);
struct Circle
{
    Shape base;
    Node* f_Center;
};

Circle* Circle_New(void);
void Circle_Ctor(Circle* self);

void Circle__Destroy(Circle* self);
extern const CS2SX_ClassInfo Circle_ClassInfo;
void Circle_Gc(void* self, CS2SX_GcVisit visit);
void Circle_Free(Circle* self);
Circle* Circle_Retain(Circle* self);
struct Holder
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Node* f_Kept;
};

Holder* Holder_New(void);
void Holder_Ctor(Holder* self);

void Holder__Destroy(Holder* self);
extern const CS2SX_ClassInfo Holder_ClassInfo;
void Holder_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Holder_GcKind;
void Holder_Free(Holder* self);
Holder* Holder_Retain(Holder* self);
struct Pair
{
    int _rc;
    Node* f_First;
    int f_Weight;
};

Pair* Pair_New(Node* First, int Weight);
void Pair_Free(Pair* self);
Pair* Pair_Retain(Pair* self);
extern const CS2SX_ClassInfo Pair_ClassInfo;
int Pair_Equals(Pair* a, Pair* b);
const char* Pair_ToString(Pair* self);

struct RefCountingApp
{
    SwitchApp base;
    List_Node* f_nodes;
    List_Shape* f_shapes;
    Node* f_kept;
    Holder* f_holder;
    Pair* f_pair;
    int f_frame;
};

void RefCountingApp_Init(RefCountingApp* self);
void RefCountingApp_OnInit(RefCountingApp* self);
void RefCountingApp_OnFrame(RefCountingApp* self);

void RefCountingApp_Free(RefCountingApp* self);


#endif
