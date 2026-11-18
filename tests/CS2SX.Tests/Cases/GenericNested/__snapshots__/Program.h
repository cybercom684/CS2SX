#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

// C# value-type struct: Pair_int
typedef struct Pair_int {
    int A;
} Pair_int;
#ifndef CS2SX_Nullable_Pair_int_DEFINED
#define CS2SX_Nullable_Pair_int_DEFINED
typedef struct Nullable_Pair_int { Pair_int value; int has; } Nullable_Pair_int;
#endif
Pair_int Pair_int_New(int a);
const char* Pair_int_Map_str(Pair_int* self, Func_int_str_t f);
int Pair_int_Map_int(Pair_int* self, Func_int_int_t f);
int Pair_int_Equals(Pair_int a, Pair_int b);
#ifndef CS2SX_LIST_Pair_int_DEFINED
#define CS2SX_LIST_Pair_int_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Pair_int, Pair_int, CS2SX_ID, CS2SX_NOREF, Pair_int_Equals)
#endif

struct GenericNestedApp
{
    SwitchApp base;
};

void GenericNestedApp_Init(GenericNestedApp* self);
void GenericNestedApp_OnInit(GenericNestedApp* self);

void GenericNestedApp_Free(GenericNestedApp* self);


#endif
