#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

int Util_Max_int(int a, int b);
float Util_Max_float(float a, float b);
const char* Util_Max_str(const char* a, const char* b);
void Util_Swap_int(int* a, int* b);
Node* Util_First_Node(List_Node* items);
int Util_Larger_int(int a, int b);
int Util_OrDefault_int(List_int* items);
int Util_CountWhere_int(List_int* items, Func_int_int_t pred);

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
// C# value-type struct: Pair
typedef struct Pair {
    int A;
    int B;
} Pair;
#ifndef CS2SX_Nullable_Pair_DEFINED
#define CS2SX_Nullable_Pair_DEFINED
typedef struct Nullable_Pair { Pair value; int has; } Nullable_Pair;
#endif
const char* Pair_Pick_str(const char* a, const char* b, int first);
int Pair_Pick_int(int a, int b, int first);
int Pair_Equals(Pair a, Pair b);

struct GenericMethodsApp
{
    SwitchApp base;
};

void GenericMethodsApp_Init(GenericMethodsApp* self);
void GenericMethodsApp_OnInit(GenericMethodsApp* self);
List_Node* GenericMethodsApp_Repeat_Node(Node* value, int n);

void GenericMethodsApp_Free(GenericMethodsApp* self);


#endif
