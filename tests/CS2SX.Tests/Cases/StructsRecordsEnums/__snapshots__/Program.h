#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

// C# value-type struct: Point
typedef struct Point Point;
int Point_ManhattanLength(Point* self);

struct Score
{
    int _rc;
    const char* f_Player;
    int f_Points;
};

Score* Score_New(const char* Player, int Points);
void Score_Free(Score* self);
Score* Score_Retain(Score* self);

struct StructsRecordsEnumsApp
{
    SwitchApp base;
    State f_state;
};

void StructsRecordsEnumsApp_Init(StructsRecordsEnumsApp* self);
void StructsRecordsEnumsApp_OnInit(StructsRecordsEnumsApp* self);

void StructsRecordsEnumsApp_Free(StructsRecordsEnumsApp* self);


#endif
