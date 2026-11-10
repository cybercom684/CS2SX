#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

// C# value-type struct: Size
typedef struct Size {
    int W;
    int H;
} Size;
#ifndef CS2SX_Nullable_Size_DEFINED
#define CS2SX_Nullable_Size_DEFINED
typedef struct Nullable_Size { Size value; int has; } Nullable_Size;
#endif
int Size_Equals(Size a, Size b);

struct Profile
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    Nullable_int f_BestScore;
    Nullable_Mode f_LastMode;
    const char* f_Nickname;
};

Profile* Profile_New(void);
void Profile_Ctor(Profile* self);
Nullable_int Profile_Find(Profile* self, int key);
int Profile_Add(Profile* self, Nullable_int bonus);

void Profile_Dispose(Profile* self);
void Profile_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Profile_GcKind;
void Profile_Free(Profile* self);
Profile* Profile_Retain(Profile* self);
struct NullableValuesApp
{
    SwitchApp base;
};

void NullableValuesApp_Init(NullableValuesApp* self);
void NullableValuesApp_OnInit(NullableValuesApp* self);

void NullableValuesApp_Free(NullableValuesApp* self);


#endif
