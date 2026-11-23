#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

#ifndef CS2SX_LIST_Rank_DEFINED
#define CS2SX_LIST_Rank_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Rank, Rank, CS2SX_ID, CS2SX_NOREF, CS2SX_EQ_MEM)
#endif
struct Item
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    const char* f_Name;
    int f_Count;
};

Item* Item_New(void);
void Item_Ctor(Item* self);

void Item__Destroy(Item* self);
extern const CS2SX_ClassInfo Item_ClassInfo;
void Item_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Item_GcKind;
void Item_Free(Item* self);
Item* Item_Retain(Item* self);
struct SaveGame
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    int f_Lives;
    const char* f_Player;
    int f_Level;
    double f_Time;
    int f_Hard;
    Rank f_Rank;
    Nullable_int f_Best;
    List_Item* f_Items;
    int* f_Scores;
    Dict_str_int* f_Flags;
    float f_Health;
    const char* f_Secret;
    const char* f_Note;
};

SaveGame* SaveGame_New(void);
void SaveGame_Ctor(SaveGame* self);

void SaveGame__Destroy(SaveGame* self);
extern const CS2SX_ClassInfo SaveGame_ClassInfo;
void SaveGame_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind SaveGame_GcKind;
void SaveGame_Free(SaveGame* self);
SaveGame* SaveGame_Retain(SaveGame* self);
struct Point
{
    int _rc;
    int f_X;
    int f_Y;
};

Point* Point_New(int X, int Y);
void Point_Free(Point* self);
Point* Point_Retain(Point* self);
extern const CS2SX_ClassInfo Point_ClassInfo;

struct Card
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Rank f_Rank;
    Rank f_Plain;
};

Card* Card_New(void);
void Card_Ctor(Card* self);

void Card__Destroy(Card* self);
extern const CS2SX_ClassInfo Card_ClassInfo;
void Card_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Card_GcKind;
void Card_Free(Card* self);
Card* Card_Retain(Card* self);
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
#ifndef CS2SX_LIST_Size_DEFINED
#define CS2SX_LIST_Size_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Size, Size, CS2SX_ID, CS2SX_NOREF, Size_Equals)
#endif

struct JsonApp
{
    SwitchApp base;
};

void JsonApp_Init(JsonApp* self);
void JsonApp_OnInit(JsonApp* self);
void JsonApp_OnFrame(JsonApp* self);

void JsonApp_Free(JsonApp* self);


#endif
