#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_interfaces.h"
#include "_generics.h"

struct Sq
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    /* implements IShape */
    double f_Side;
};

Sq* Sq_New(void);
void Sq_Ctor(Sq* self);
int Sq_Corners(Sq* self);
const char* Sq_Describe(Sq* self);

void Sq__Destroy(Sq* self);
extern const CS2SX_ClassInfo Sq_ClassInfo;
void Sq_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Sq_GcKind;
void Sq_Free(Sq* self);
Sq* Sq_Retain(Sq* self);
double Sq_get_Area(Sq* self);
const char* Sq_get_Name(Sq* self);
extern IShape_vtable Sq_IShape_vtable_instance;
Sq* Sq_Retain(Sq* self);
void Sq_Free(Sq* self);
extern const CS2SX_GcKind Sq_GcKind;
extern const CS2SX_ClassInfo Sq_ClassInfo;
static inline IShape* Sq_as_IShape(Sq* self)
{
    return (IShape*)CS2SX_Iface_Wrap(sizeof(IShape), &Sq_IShape_vtable_instance, self, (void* (*)(void*))Sq_Retain, (CS2SX_FreeFn)Sq_Free, &Sq_GcKind, &Sq_ClassInfo);
}

struct Circle
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    /* implements IShape */
    double f_Area;
    const char* f_Name;
};

Circle* Circle_New(void);
void Circle_Ctor(Circle* self);
const char* Circle_Describe(Circle* self);
int Circle_Corners(Circle* self);

void Circle__Destroy(Circle* self);
extern const CS2SX_ClassInfo Circle_ClassInfo;
void Circle_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Circle_GcKind;
void Circle_Free(Circle* self);
Circle* Circle_Retain(Circle* self);
extern IShape_vtable Circle_IShape_vtable_instance;
Circle* Circle_Retain(Circle* self);
void Circle_Free(Circle* self);
extern const CS2SX_GcKind Circle_GcKind;
extern const CS2SX_ClassInfo Circle_ClassInfo;
static inline IShape* Circle_as_IShape(Circle* self)
{
    return (IShape*)CS2SX_Iface_Wrap(sizeof(IShape), &Circle_IShape_vtable_instance, self, (void* (*)(void*))Circle_Retain, (CS2SX_FreeFn)Circle_Free, &Circle_GcKind, &Circle_ClassInfo);
}

struct Doc
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    /* implements INamed */
    const char* f_Title;
};

Doc* Doc_New(void);
void Doc_Ctor(Doc* self);

void Doc__Destroy(Doc* self);
extern const CS2SX_ClassInfo Doc_ClassInfo;
void Doc_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Doc_GcKind;
void Doc_Free(Doc* self);
Doc* Doc_Retain(Doc* self);
extern INamed_vtable Doc_INamed_vtable_instance;
Doc* Doc_Retain(Doc* self);
void Doc_Free(Doc* self);
extern const CS2SX_GcKind Doc_GcKind;
extern const CS2SX_ClassInfo Doc_ClassInfo;
static inline INamed* Doc_as_INamed(Doc* self)
{
    return (INamed*)CS2SX_Iface_Wrap(sizeof(INamed), &Doc_INamed_vtable_instance, self, (void* (*)(void*))Doc_Retain, (CS2SX_FreeFn)Doc_Free, &Doc_GcKind, &Doc_ClassInfo);
}

struct IntRepo
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    /* implements IRepo_int */
    List_int* f_l;
};

IntRepo* IntRepo_New(void);
void IntRepo_Ctor(IntRepo* self);
void IntRepo_Put(IntRepo* self, int x);
int IntRepo_Get(IntRepo* self, int i);

void IntRepo__Destroy(IntRepo* self);
extern const CS2SX_ClassInfo IntRepo_ClassInfo;
void IntRepo_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind IntRepo_GcKind;
void IntRepo_Free(IntRepo* self);
IntRepo* IntRepo_Retain(IntRepo* self);
int IntRepo_get_Count(IntRepo* self);
extern IRepo_int_vtable IntRepo_IRepo_int_vtable_instance;
IntRepo* IntRepo_Retain(IntRepo* self);
void IntRepo_Free(IntRepo* self);
extern const CS2SX_GcKind IntRepo_GcKind;
extern const CS2SX_ClassInfo IntRepo_ClassInfo;
static inline IRepo_int* IntRepo_as_IRepo_int(IntRepo* self)
{
    return (IRepo_int*)CS2SX_Iface_Wrap(sizeof(IRepo_int), &IntRepo_IRepo_int_vtable_instance, self, (void* (*)(void*))IntRepo_Retain, (CS2SX_FreeFn)IntRepo_Free, &IntRepo_GcKind, &IntRepo_ClassInfo);
}

struct StrRepo
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    /* implements IRepo_str */
    List_str* f_l;
};

StrRepo* StrRepo_New(void);
void StrRepo_Ctor(StrRepo* self);
void StrRepo_Put(StrRepo* self, const char* x);
const char* StrRepo_Get(StrRepo* self, int i);

void StrRepo__Destroy(StrRepo* self);
extern const CS2SX_ClassInfo StrRepo_ClassInfo;
void StrRepo_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind StrRepo_GcKind;
void StrRepo_Free(StrRepo* self);
StrRepo* StrRepo_Retain(StrRepo* self);
int StrRepo_get_Count(StrRepo* self);
extern IRepo_str_vtable StrRepo_IRepo_str_vtable_instance;
StrRepo* StrRepo_Retain(StrRepo* self);
void StrRepo_Free(StrRepo* self);
extern const CS2SX_GcKind StrRepo_GcKind;
extern const CS2SX_ClassInfo StrRepo_ClassInfo;
static inline IRepo_str* StrRepo_as_IRepo_str(StrRepo* self)
{
    return (IRepo_str*)CS2SX_Iface_Wrap(sizeof(IRepo_str), &StrRepo_IRepo_str_vtable_instance, self, (void* (*)(void*))StrRepo_Retain, (CS2SX_FreeFn)StrRepo_Free, &StrRepo_GcKind, &StrRepo_ClassInfo);
}

typedef struct Box_int_vtable
{
    const char* (*Show)(void* self);
} Box_int_vtable;

struct Box_int
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Box_int_vtable* vtable;
    int f_Value;
};

Box_int* Box_int_New(int v);
void Box_int_Ctor(Box_int* self, int v);
/* abstract: const char* Box_int_Show(Box_int* self) */
int Box_int_Get(Box_int* self);

void Box_int__Destroy(Box_int* self);
extern const CS2SX_ClassInfo Box_int_ClassInfo;
void Box_int_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Box_int_GcKind;
void Box_int_Free(Box_int* self);
Box_int* Box_int_Retain(Box_int* self);
typedef struct Box_str_vtable
{
    const char* (*Show)(void* self);
} Box_str_vtable;

struct Box_str
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Box_str_vtable* vtable;
    const char* f_Value;
};

Box_str* Box_str_New(const char* v);
void Box_str_Ctor(Box_str* self, const char* v);
/* abstract: const char* Box_str_Show(Box_str* self) */
const char* Box_str_Get(Box_str* self);

void Box_str__Destroy(Box_str* self);
extern const CS2SX_ClassInfo Box_str_ClassInfo;
void Box_str_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Box_str_GcKind;
void Box_str_Free(Box_str* self);
Box_str* Box_str_Retain(Box_str* self);
struct IntBox
{
    Box_int base;
};

IntBox* IntBox_New(int v);
void IntBox_Ctor(IntBox* self, int v);
const char* IntBox_Show(IntBox* self);

void IntBox__Destroy(IntBox* self);
extern const CS2SX_ClassInfo IntBox_ClassInfo;
void IntBox_Gc(void* self, CS2SX_GcVisit visit);
void IntBox_Free(IntBox* self);
IntBox* IntBox_Retain(IntBox* self);
struct StrBox
{
    Box_str base;
};

StrBox* StrBox_New(const char* v);
void StrBox_Ctor(StrBox* self, const char* v);
const char* StrBox_Show(StrBox* self);

void StrBox__Destroy(StrBox* self);
extern const CS2SX_ClassInfo StrBox_ClassInfo;
void StrBox_Gc(void* self, CS2SX_GcVisit visit);
void StrBox_Free(StrBox* self);
StrBox* StrBox_Retain(StrBox* self);
// C# value-type struct: Vec
typedef struct Vec {
    int X;
    int Y;
} Vec;
#ifndef CS2SX_Nullable_Vec_DEFINED
#define CS2SX_Nullable_Vec_DEFINED
typedef struct Nullable_Vec { Vec value; int has; } Nullable_Vec;
#endif
int Vec_Longer(Vec* self, Vec o);
int Vec_get_Len2(Vec* self);
int Vec_Equals(Vec a, Vec b);
#ifndef CS2SX_LIST_Vec_DEFINED
#define CS2SX_LIST_Vec_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Vec, Vec, CS2SX_ID, CS2SX_NOREF, Vec_Equals)
#endif

extern const char* Cfg_Title;
extern int Cfg_Max;

struct Timer2
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    CS2SX_DateTime f_Start;
    CS2SX_TimeSpan f_Limit;
};

Timer2* Timer2_New(void);
void Timer2_Ctor(Timer2* self);

void Timer2__Destroy(Timer2* self);
extern const CS2SX_ClassInfo Timer2_ClassInfo;
void Timer2_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Timer2_GcKind;
void Timer2_Free(Timer2* self);
Timer2* Timer2_Retain(Timer2* self);
CS2SX_DateTime Timer2_get_End(Timer2* self);
struct TypesAndDatesApp
{
    SwitchApp base;
};

void TypesAndDatesApp_Init(TypesAndDatesApp* self);
int TypesAndDatesApp_Total(IRepo_int* r);
void TypesAndDatesApp_OnInit(TypesAndDatesApp* self);
void TypesAndDatesApp_OnFrame(TypesAndDatesApp* self);

void TypesAndDatesApp_Free(TypesAndDatesApp* self);


#endif
