#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct Point
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    int f_X;
    int f_Y;
    const char* f_Tag;
};

Point* Point_New_0(void);
void Point_Ctor_0(Point* self);
Point* Point_New_1(int v);
void Point_Ctor_1(Point* self, int v);
Point* Point_New_2(int x, int y);
void Point_Ctor_2(Point* self, int x, int y);
Point* Point_New_3(int x, int y, const char* tag);
void Point_Ctor_3(Point* self, int x, int y, const char* tag);

void Point_Dispose(Point* self);
void Point_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Point_GcKind;
void Point_Free(Point* self);
Point* Point_Retain(Point* self);
struct Point3
{
    Point base;
    int f_Z;
};

Point3* Point3_New_3(int x, int y, int z);
void Point3_Ctor_3(Point3* self, int x, int y, int z);
Point3* Point3_New_1(int v);
void Point3_Ctor_1(Point3* self, int v);

void Point3_Dispose(Point3* self);
void Point3_Gc(void* self, CS2SX_GcVisit visit);
void Point3_Free(Point3* self);
Point3* Point3_Retain(Point3* self);
struct ParseError
{
    Exception base;
    int f_Position;
};

extern const CS2SX_TypeInfo CS2SX_TI_ParseError;
ParseError* ParseError_New(const char* message, int position);
void ParseError_Ctor(ParseError* self, const char* message, int position);

void ParseError_Free(ParseError* self);
struct Counter
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    Action_int_t f_Changed;
    int value;
};

Counter* Counter_New(void);
void Counter_Ctor(Counter* self);
void Counter_Add(Counter* self, int n);

void Counter_Dispose(Counter* self);
void Counter_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Counter_GcKind;
void Counter_Free(Counter* self);
Counter* Counter_Retain(Counter* self);
struct LanguagePhase4App
{
    SwitchApp base;
    int f_sum;
};

void LanguagePhase4App_Init(LanguagePhase4App* self);
int LanguagePhase4App_Parse(const char* s);
const char* LanguagePhase4App_Classify(const char* s);
const char* LanguagePhase4App_Rethrow();
void LanguagePhase4App_OnChangedA(LanguagePhase4App* self, int v);
void LanguagePhase4App_OnChangedB(LanguagePhase4App* self, int v);
void LanguagePhase4App_OnInit(LanguagePhase4App* self);

void LanguagePhase4App_Free(LanguagePhase4App* self);


#endif
