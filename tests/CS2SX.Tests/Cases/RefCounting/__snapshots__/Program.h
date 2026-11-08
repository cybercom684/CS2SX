#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct Counter
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
};

extern int Counter_Disposed;
Counter* Counter_New(void);
void Counter_Ctor(Counter* self);

void Counter_Dispose(Counter* self);
void Counter_Free(Counter* self);
Counter* Counter_Retain(Counter* self);
struct Node
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    int f_V;
};

Node* Node_New(int v);
void Node_Ctor(Node* self, int v);

void Node_Dispose(Node* self);
void Node_Free(Node* self);
Node* Node_Retain(Node* self);
struct Shape
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
};

Shape* Shape_New(void);
void Shape_Ctor(Shape* self);

void Shape_Dispose(Shape* self);
void Shape_Free(Shape* self);
Shape* Shape_Retain(Shape* self);
struct Circle
{
    Shape base;
    Node* f_Center;
};

Circle* Circle_New(void);
void Circle_Ctor(Circle* self);

void Circle_Dispose(Circle* self);
void Circle_Free(Circle* self);
Circle* Circle_Retain(Circle* self);
struct Holder
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    Node* f_Kept;
};

Holder* Holder_New(void);
void Holder_Ctor(Holder* self);

void Holder_Dispose(Holder* self);
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
