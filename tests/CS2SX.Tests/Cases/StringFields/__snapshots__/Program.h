#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct Label2
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const char* f_Name;
    const char* f_Text;
};

Label2* Label2_New(void);
void Label2_Ctor(Label2* self);

void Label2_Dispose(Label2* self);
void Label2_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Label2_GcKind;
void Label2_Free(Label2* self);
Label2* Label2_Retain(Label2* self);
struct StringFieldsApp
{
    SwitchApp base;
    const char* f_status;
    const char* f_log;
    Label2* f_label;
    int f_frame;
};

extern const char* StringFieldsApp_s_last;
void StringFieldsApp_Init(StringFieldsApp* self);
void StringFieldsApp_OnFrame(StringFieldsApp* self);

void StringFieldsApp_Free(StringFieldsApp* self);


#endif
