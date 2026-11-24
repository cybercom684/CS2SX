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
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Node_vtable* vtable;
    int f_Value;
    Node* f_Next;
    const char* f_Name;
};

Node* Node_New(void);
void Node_Ctor(Node* self);
int Node_Get(Node* self);
const char* Node_Kind(Node* self);

void Node__Destroy(Node* self);
extern const CS2SX_ClassInfo Node_ClassInfo;
void Node_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Node_GcKind;
void Node_Free(Node* self);
Node* Node_Retain(Node* self);
struct Square
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    /* implements IShape */
};

Square* Square_New(void);
void Square_Ctor(Square* self);
int Square_Area(Square* self);

void Square__Destroy(Square* self);
extern const CS2SX_ClassInfo Square_ClassInfo;
void Square_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Square_GcKind;
void Square_Free(Square* self);
Square* Square_Retain(Square* self);
extern IShape_vtable Square_IShape_vtable_instance;
Square* Square_Retain(Square* self);
void Square_Free(Square* self);
extern const CS2SX_GcKind Square_GcKind;
extern const CS2SX_ClassInfo Square_ClassInfo;
static inline IShape* Square_as_IShape(Square* self)
{
    return (IShape*)CS2SX_Iface_Wrap(sizeof(IShape), &Square_IShape_vtable_instance, self, (void* (*)(void*))Square_Retain, (CS2SX_FreeFn)Square_Free, &Square_GcKind, &Square_ClassInfo);
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
