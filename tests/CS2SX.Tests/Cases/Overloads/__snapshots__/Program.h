#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct Shape
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    const char* f_Name;
};

Shape* Shape_New(void);
void Shape_Ctor(Shape* self);

void Shape__Destroy(Shape* self);
extern const CS2SX_ClassInfo Shape_ClassInfo;
void Shape_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Shape_GcKind;
void Shape_Free(Shape* self);
Shape* Shape_Retain(Shape* self);
struct Printer
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    int f_calls;
};

Printer* Printer_New(void);
void Printer_Ctor(Printer* self);
const char* Printer_Show_1_int(Printer* self, int value);
const char* Printer_Show_1_float(Printer* self, float value);
const char* Printer_Show_1_str(Printer* self, const char* value);
const char* Printer_Show_1_Shape(Printer* self, Shape* value);
const char* Printer_Show_1_intArr(Printer* self, int* values);
const char* Printer_Show_1_List_int(Printer* self, List_int* values);
const char* Printer_Show_2_int_int(Printer* self, int a, int b);
const char* Printer_Show_2_int_str(Printer* self, int a, const char* b);
const char* Printer_Show_0(Printer* self);
int Printer_Twice_1_int(int x);
const char* Printer_Twice_1_str(const char* x);
const char* Printer_All(Printer* self);

void Printer__Destroy(Printer* self);
extern const CS2SX_ClassInfo Printer_ClassInfo;
void Printer_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Printer_GcKind;
void Printer_Free(Printer* self);
Printer* Printer_Retain(Printer* self);
int Printer_get_Calls(Printer* self);
struct OverloadsApp
{
    SwitchApp base;
};

void OverloadsApp_Init(OverloadsApp* self);
double OverloadsApp_Area_1_double(double side);
int OverloadsApp_Area_1_int(int side);
void OverloadsApp_OnInit(OverloadsApp* self);

void OverloadsApp_Free(OverloadsApp* self);


#endif
