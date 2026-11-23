#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

#ifndef CS2SX_LIST_State_DEFINED
#define CS2SX_LIST_State_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(State, State, CS2SX_ID, CS2SX_NOREF, CS2SX_EQ_MEM)
#endif
// C# value-type struct: Point
typedef struct Point {
    int X;
    int Y;
} Point;
#ifndef CS2SX_Nullable_Point_DEFINED
#define CS2SX_Nullable_Point_DEFINED
typedef struct Nullable_Point { Point value; int has; } Nullable_Point;
#endif
Point Point_New(int x, int y);
int Point_ManhattanLength(Point* self);
int Point_Equals(Point a, Point b);
#ifndef CS2SX_LIST_Point_DEFINED
#define CS2SX_LIST_Point_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Point, Point, CS2SX_ID, CS2SX_NOREF, Point_Equals)
#endif

struct Score
{
    int _rc;
    const char* f_Player;
    int f_Points;
};

Score* Score_New(const char* Player, int Points);
void Score_Free(Score* self);
Score* Score_Retain(Score* self);
extern const CS2SX_ClassInfo Score_ClassInfo;
int Score_Equals(Score* a, Score* b);
const char* Score_ToString(Score* self);

struct StructsRecordsEnumsApp
{
    SwitchApp base;
    State f_state;
};

void StructsRecordsEnumsApp_Init(StructsRecordsEnumsApp* self);
void StructsRecordsEnumsApp_OnInit(StructsRecordsEnumsApp* self);

void StructsRecordsEnumsApp_Free(StructsRecordsEnumsApp* self);


#endif
