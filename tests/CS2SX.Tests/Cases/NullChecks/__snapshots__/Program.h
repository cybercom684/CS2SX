#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_interfaces.h"

typedef struct Node_vtable
{
    const char* (*Kind)(void* self);
} Node_vtable;

struct Node
{
    Node_vtable* vtable;
    int _rc;
    int f_Value;
    Node* f_Next;
    const char* f_Name;
};

Node* Node_New(void);
void Node_Ctor(Node* self);
int Node_Get(Node* self);
const char* Node_Kind(Node* self);

void Node_Free(Node* self);
Node* Node_Retain(Node* self);
struct Square
{
    int _rc;
    /* implements IShape */
};

Square* Square_New(void);
void Square_Ctor(Square* self);
int Square_Area(Square* self);

void Square_Free(Square* self);
Square* Square_Retain(Square* self);
extern IShape_vtable Square_IShape_vtable_instance;
static inline IShape* Square_as_IShape(Square* self)
{
    IShape* _iface = (IShape*)malloc(sizeof(IShape));
    if (!_iface) return NULL;
    _iface->vtable = &Square_IShape_vtable_instance;
    _iface->obj = self;
    return _iface;
}

struct NullChecksApp
{
    SwitchApp base;
};

void NullChecksApp_Init(NullChecksApp* self);
const char* NullChecksApp_Try(Action_t action);
void NullChecksApp_OnInit(NullChecksApp* self);

void NullChecksApp_Free(NullChecksApp* self);


#endif
