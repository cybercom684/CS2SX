#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_interfaces.h"
#include "_generics.h"

struct Plain
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    /* implements IFormatter */
};

Plain* Plain_New(void);
void Plain_Ctor(Plain* self);
const char* Plain_Format_int(Plain* self, int value);
const char* Plain_Format_str(Plain* self, const char* value);

void Plain__Destroy(Plain* self);
extern const CS2SX_ClassInfo Plain_ClassInfo;
void Plain_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Plain_GcKind;
void Plain_Free(Plain* self);
Plain* Plain_Retain(Plain* self);
extern IFormatter_vtable Plain_IFormatter_vtable_instance;
Plain* Plain_Retain(Plain* self);
void Plain_Free(Plain* self);
extern const CS2SX_GcKind Plain_GcKind;
extern const CS2SX_ClassInfo Plain_ClassInfo;
static inline IFormatter* Plain_as_IFormatter(Plain* self)
{
    return (IFormatter*)CS2SX_Iface_Wrap(sizeof(IFormatter), &Plain_IFormatter_vtable_instance, self, (void* (*)(void*))Plain_Retain, (CS2SX_FreeFn)Plain_Free, &Plain_GcKind, &Plain_ClassInfo);
}

struct Bracket
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    /* implements IFormatter */
};

Bracket* Bracket_New(void);
void Bracket_Ctor(Bracket* self);
const char* Bracket_Format_int(Bracket* self, int value);
const char* Bracket_Format_str(Bracket* self, const char* value);

void Bracket__Destroy(Bracket* self);
extern const CS2SX_ClassInfo Bracket_ClassInfo;
void Bracket_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Bracket_GcKind;
void Bracket_Free(Bracket* self);
Bracket* Bracket_Retain(Bracket* self);
extern IFormatter_vtable Bracket_IFormatter_vtable_instance;
Bracket* Bracket_Retain(Bracket* self);
void Bracket_Free(Bracket* self);
extern const CS2SX_GcKind Bracket_GcKind;
extern const CS2SX_ClassInfo Bracket_ClassInfo;
static inline IFormatter* Bracket_as_IFormatter(Bracket* self)
{
    return (IFormatter*)CS2SX_Iface_Wrap(sizeof(IFormatter), &Bracket_IFormatter_vtable_instance, self, (void* (*)(void*))Bracket_Retain, (CS2SX_FreeFn)Bracket_Free, &Bracket_GcKind, &Bracket_ClassInfo);
}

typedef struct Picker_vtable
{
    int (*Pick_int)(void* self, int a, int b);
    const char* (*Pick_str)(void* self, const char* a, const char* b);
    int (*Echo_int)(void* self, int x);
} Picker_vtable;

struct Picker
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Picker_vtable* vtable;
};

Picker* Picker_New(void);
void Picker_Ctor(Picker* self);
/* abstract: int Picker_Pick_int(Picker* self, int a, int b) */
/* abstract: const char* Picker_Pick_str(Picker* self, const char* a, const char* b) */
int Picker_Echo_int(Picker* self, int x);

void Picker__Destroy(Picker* self);
extern const CS2SX_ClassInfo Picker_ClassInfo;
void Picker_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Picker_GcKind;
void Picker_Free(Picker* self);
Picker* Picker_Retain(Picker* self);
struct FirstPicker
{
    Picker base;
};

FirstPicker* FirstPicker_New(void);
void FirstPicker_Ctor(FirstPicker* self);
int FirstPicker_Pick_int(FirstPicker* self, int a, int b);
const char* FirstPicker_Pick_str(FirstPicker* self, const char* a, const char* b);

void FirstPicker__Destroy(FirstPicker* self);
extern const CS2SX_ClassInfo FirstPicker_ClassInfo;
void FirstPicker_Gc(void* self, CS2SX_GcVisit visit);
void FirstPicker_Free(FirstPicker* self);
FirstPicker* FirstPicker_Retain(FirstPicker* self);
struct SecondPicker
{
    Picker base;
};

SecondPicker* SecondPicker_New(void);
void SecondPicker_Ctor(SecondPicker* self);
int SecondPicker_Pick_int(SecondPicker* self, int a, int b);
const char* SecondPicker_Pick_str(SecondPicker* self, const char* a, const char* b);
int SecondPicker_Echo_int(SecondPicker* self, int x);

void SecondPicker__Destroy(SecondPicker* self);
extern const CS2SX_ClassInfo SecondPicker_ClassInfo;
void SecondPicker_Gc(void* self, CS2SX_GcVisit visit);
void SecondPicker_Free(SecondPicker* self);
SecondPicker* SecondPicker_Retain(SecondPicker* self);
// C# value-type struct: Pair_int
typedef struct Pair_int {
    int A;
    int B;
} Pair_int;
#ifndef CS2SX_Nullable_Pair_int_DEFINED
#define CS2SX_Nullable_Pair_int_DEFINED
typedef struct Nullable_Pair_int { Pair_int value; int has; } Nullable_Pair_int;
#endif
Pair_int Pair_int_New(int a, int b);
Pair_int Pair_int_Swapped(Pair_int* self);
int Pair_int_Equals(Pair_int a, Pair_int b);
#ifndef CS2SX_LIST_Pair_int_DEFINED
#define CS2SX_LIST_Pair_int_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Pair_int, Pair_int, CS2SX_ID, CS2SX_NOREF, Pair_int_Equals)
#endif

// C# value-type struct: Pair_str
typedef struct Pair_str {
    const char* A;
    const char* B;
} Pair_str;
#ifndef CS2SX_Nullable_Pair_str_DEFINED
#define CS2SX_Nullable_Pair_str_DEFINED
typedef struct Nullable_Pair_str { Pair_str value; int has; } Nullable_Pair_str;
#endif
Pair_str Pair_str_New(const char* a, const char* b);
Pair_str Pair_str_Swapped(Pair_str* self);
int Pair_str_Equals(Pair_str a, Pair_str b);
void Pair_str_Keep(Pair_str* s);
void Pair_str_Drop(Pair_str* s);
void Pair_str_Gc(Pair_str* s, CS2SX_GcVisit visit);
static inline Pair_str Pair_str_KeepV(Pair_str v) { Pair_str_Keep(&v); return v; }
static inline void Pair_str_DropV(Pair_str v) { Pair_str_Drop(&v); }
#ifndef CS2SX_LIST_Pair_str_DEFINED
#define CS2SX_LIST_Pair_str_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Pair_str, Pair_str, Pair_str_KeepV, Pair_str_DropV, Pair_str_Equals)
#endif

// C# value-type struct: Slot_str_int
typedef struct Slot_str_int {
    const char* Key;
    int Value;
} Slot_str_int;
#ifndef CS2SX_Nullable_Slot_str_int_DEFINED
#define CS2SX_Nullable_Slot_str_int_DEFINED
typedef struct Nullable_Slot_str_int { Slot_str_int value; int has; } Nullable_Slot_str_int;
#endif
int Slot_str_int_Equals(Slot_str_int a, Slot_str_int b);
void Slot_str_int_Keep(Slot_str_int* s);
void Slot_str_int_Drop(Slot_str_int* s);
void Slot_str_int_Gc(Slot_str_int* s, CS2SX_GcVisit visit);
static inline Slot_str_int Slot_str_int_KeepV(Slot_str_int v) { Slot_str_int_Keep(&v); return v; }
static inline void Slot_str_int_DropV(Slot_str_int v) { Slot_str_int_Drop(&v); }
#ifndef CS2SX_LIST_Slot_str_int_DEFINED
#define CS2SX_LIST_Slot_str_int_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Slot_str_int, Slot_str_int, Slot_str_int_KeepV, Slot_str_int_DropV, Slot_str_int_Equals)
#endif

struct GenericTypesApp
{
    SwitchApp base;
};

void GenericTypesApp_Init(GenericTypesApp* self);
void GenericTypesApp_OnInit(GenericTypesApp* self);

void GenericTypesApp_Free(GenericTypesApp* self);


#endif
