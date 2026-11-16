#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct Note
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const char* f_Text;
    const char* f_Title;
};

Note* Note_New(void);
void Note_Ctor(Note* self);

void Note_Dispose(Note* self);
void Note_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Note_GcKind;
void Note_Free(Note* self);
Note* Note_Retain(Note* self);
struct StringsLongApp
{
    SwitchApp base;
    const char* f_kept;
    List_str* f_list;
    Dict_str_str* f_map;
    const char** f_array;
    Note* f_note;
    int f_frame;
};

void StringsLongApp_Init(StringsLongApp* self);
const char* StringsLongApp_Repeat(const char* part, int times);
void StringsLongApp_OnFrame(StringsLongApp* self);

void StringsLongApp_Free(StringsLongApp* self);


#endif
