#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

typedef struct CS2SX_Step_vtable
{
    void (*cs2sx_resume)(void* self);
} CS2SX_Step_vtable;

struct CS2SX_Step
{
    CS2SX_Step_vtable* vtable;
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
};

CS2SX_Step* CS2SX_Step_New(void);
void CS2SX_Step_Ctor(CS2SX_Step* self);
/* abstract: void CS2SX_Step_cs2sx_resume(CS2SX_Step* self) */

void CS2SX_Step_Dispose(CS2SX_Step* self);
void CS2SX_Step_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind CS2SX_Step_GcKind;
void CS2SX_Step_Free(CS2SX_Step* self);
CS2SX_Step* CS2SX_Step_Retain(CS2SX_Step* self);
void CS2SX_Async_Begin(int points);
void CS2SX_Async_Suspend_1(int point);
void CS2SX_Async_Suspend_2(int point, int depth);
void CS2SX_Async_Label(int point, int level);
void CS2SX_Async_Jump(int point, int level);

struct CS2SX_Task
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    int f_IsCompleted;
    Exception* f_Fault;
    long long f_Due;
    int f_Observed;
    List_CS2SX_Step* f_waiting;
};

extern List_CS2SX_Task* CS2SX_Task_timers;
extern long long CS2SX_Task_clock;
CS2SX_Task* CS2SX_Task_New(void);
void CS2SX_Task_Ctor(CS2SX_Task* self);
void CS2SX_Task_Then(CS2SX_Task* self, CS2SX_Step* step);
void CS2SX_Task_Finish(CS2SX_Task* self);
void CS2SX_Task_Fail(CS2SX_Task* self, Exception* e);
void CS2SX_Task_Check(CS2SX_Task* self);
void CS2SX_Task_Wait(CS2SX_Task* self);
CS2SX_Task* CS2SX_Task_Delay(int milliseconds);
CS2SX_Task* CS2SX_Task_DelaySpan(CS2SX_TimeSpan time);
CS2SX_Task* CS2SX_Task_Yield();
CS2SX_Task* CS2SX_Task_Run(Action_t work);
CS2SX_Task* CS2SX_Task_WhenAll(CS2SX_Task** tasks);
CS2SX_Task* CS2SX_Task_WhenAllList(List_CS2SX_Task* tasks);
void CS2SX_Task_Tick(long long nowMs);

void CS2SX_Task_Dispose(CS2SX_Task* self);
void CS2SX_Task_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind CS2SX_Task_GcKind;
void CS2SX_Task_Free(CS2SX_Task* self);
CS2SX_Task* CS2SX_Task_Retain(CS2SX_Task* self);
int CS2SX_Task_get_IsFaulted(CS2SX_Task* self);
int CS2SX_Task_get_IsCompletedSuccessfully(CS2SX_Task* self);
CS2SX_Task* CS2SX_Task_get_CompletedTask();
struct CS2SX_WhenAll
{
    CS2SX_Step base;
    List_CS2SX_Task* f_Tasks;
    CS2SX_Task* f_Done;
    int f_Next;
};

CS2SX_WhenAll* CS2SX_WhenAll_New(void);
void CS2SX_WhenAll_Ctor(CS2SX_WhenAll* self);
void CS2SX_WhenAll_cs2sx_resume(CS2SX_WhenAll* self);

void CS2SX_WhenAll_Dispose(CS2SX_WhenAll* self);
void CS2SX_WhenAll_Gc(void* self, CS2SX_GcVisit visit);
void CS2SX_WhenAll_Free(CS2SX_WhenAll* self);
CS2SX_WhenAll* CS2SX_WhenAll_Retain(CS2SX_WhenAll* self);
struct CS2SX_Task_CS2SX_Task
{
    CS2SX_Task base;
    CS2SX_Task* f_Value;
};

CS2SX_Task_CS2SX_Task* CS2SX_Task_CS2SX_Task_New(void);
void CS2SX_Task_CS2SX_Task_Ctor(CS2SX_Task_CS2SX_Task* self);
void CS2SX_Task_CS2SX_Task_SetResult(CS2SX_Task_CS2SX_Task* self, CS2SX_Task* v);
CS2SX_Task_CS2SX_Task* CS2SX_Task_CS2SX_Task_From(CS2SX_Task* v);
CS2SX_Task_CS2SX_Task* CS2SX_Task_CS2SX_Task_Run(Func_CS2SX_Task_t work);

void CS2SX_Task_CS2SX_Task_Dispose(CS2SX_Task_CS2SX_Task* self);
void CS2SX_Task_CS2SX_Task_Gc(void* self, CS2SX_GcVisit visit);
void CS2SX_Task_CS2SX_Task_Free(CS2SX_Task_CS2SX_Task* self);
CS2SX_Task_CS2SX_Task* CS2SX_Task_CS2SX_Task_Retain(CS2SX_Task_CS2SX_Task* self);
CS2SX_Task* CS2SX_Task_CS2SX_Task_get_Result(CS2SX_Task_CS2SX_Task* self);
struct CS2SX_WhenAny
{
    CS2SX_Step base;
    List_CS2SX_Task* f_Tasks;
    CS2SX_Task_CS2SX_Task* f_Done;
};

CS2SX_WhenAny* CS2SX_WhenAny_New(void);
void CS2SX_WhenAny_Ctor(CS2SX_WhenAny* self);
CS2SX_Task_CS2SX_Task* CS2SX_WhenAny_Of(CS2SX_Task** tasks);
void CS2SX_WhenAny_cs2sx_resume(CS2SX_WhenAny* self);

void CS2SX_WhenAny_Dispose(CS2SX_WhenAny* self);
void CS2SX_WhenAny_Gc(void* self, CS2SX_GcVisit visit);
void CS2SX_WhenAny_Free(CS2SX_WhenAny* self);
CS2SX_WhenAny* CS2SX_WhenAny_Retain(CS2SX_WhenAny* self);
struct CS2SX_Task_bool
{
    CS2SX_Task base;
    int f_Value;
};

CS2SX_Task_bool* CS2SX_Task_bool_New(void);
void CS2SX_Task_bool_Ctor(CS2SX_Task_bool* self);
void CS2SX_Task_bool_SetResult(CS2SX_Task_bool* self, int v);
CS2SX_Task_bool* CS2SX_Task_bool_From(int v);
CS2SX_Task_bool* CS2SX_Task_bool_Run(Func_int_t work);

void CS2SX_Task_bool_Dispose(CS2SX_Task_bool* self);
void CS2SX_Task_bool_Gc(void* self, CS2SX_GcVisit visit);
void CS2SX_Task_bool_Free(CS2SX_Task_bool* self);
CS2SX_Task_bool* CS2SX_Task_bool_Retain(CS2SX_Task_bool* self);
int CS2SX_Task_bool_get_Result(CS2SX_Task_bool* self);
struct CS2SX_Task_int
{
    CS2SX_Task base;
    int f_Value;
};

CS2SX_Task_int* CS2SX_Task_int_New(void);
void CS2SX_Task_int_Ctor(CS2SX_Task_int* self);
void CS2SX_Task_int_SetResult(CS2SX_Task_int* self, int v);
CS2SX_Task_int* CS2SX_Task_int_From(int v);
CS2SX_Task_int* CS2SX_Task_int_Run(Func_int_t work);

void CS2SX_Task_int_Dispose(CS2SX_Task_int* self);
void CS2SX_Task_int_Gc(void* self, CS2SX_GcVisit visit);
void CS2SX_Task_int_Free(CS2SX_Task_int* self);
CS2SX_Task_int* CS2SX_Task_int_Retain(CS2SX_Task_int* self);
int CS2SX_Task_int_get_Result(CS2SX_Task_int* self);
struct CS2SX_Task_str
{
    CS2SX_Task base;
    const char* f_Value;
};

CS2SX_Task_str* CS2SX_Task_str_New(void);
void CS2SX_Task_str_Ctor(CS2SX_Task_str* self);
void CS2SX_Task_str_SetResult(CS2SX_Task_str* self, const char* v);
CS2SX_Task_str* CS2SX_Task_str_From(const char* v);
CS2SX_Task_str* CS2SX_Task_str_Run(Func_str_t work);

void CS2SX_Task_str_Dispose(CS2SX_Task_str* self);
void CS2SX_Task_str_Gc(void* self, CS2SX_GcVisit visit);
void CS2SX_Task_str_Free(CS2SX_Task_str* self);
CS2SX_Task_str* CS2SX_Task_str_Retain(CS2SX_Task_str* self);
const char* CS2SX_Task_str_get_Result(CS2SX_Task_str* self);
struct Source
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    int f_left;
};

Source* Source_New(void);
void Source_Ctor(Source* self);
CS2SX_Task_bool* Source_Next(Source* self);
CS2SX_Task_int* Source_Risky(Source* self, int n);

void Source_Dispose(Source* self);
void Source_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Source_GcKind;
void Source_Free(Source* self);
Source* Source_Retain(Source* self);
int Source_get_Total(Source* self);
struct Next_Async
{
    CS2SX_Step base;
    Source* f_cs2sx_this;
    CS2SX_Task_bool* f_cs2sx_task;
    int f_cs2sx_state;
    CS2SX_Task* f_cs2sx_a1;
};

Next_Async* Next_Async_New(void);
void Next_Async_Ctor(Next_Async* self);
void Next_Async_cs2sx_resume(Next_Async* self);
void Next_Async_cs2sx_run(Next_Async* self);

void Next_Async_Dispose(Next_Async* self);
void Next_Async_Gc(void* self, CS2SX_GcVisit visit);
void Next_Async_Free(Next_Async* self);
Next_Async* Next_Async_Retain(Next_Async* self);
struct Risky_Async
{
    CS2SX_Step base;
    Source* f_cs2sx_this;
    CS2SX_Task_int* f_cs2sx_task;
    int f_cs2sx_state;
    int f_n;
    CS2SX_Task* f_cs2sx_a1;
};

Risky_Async* Risky_Async_New(void);
void Risky_Async_Ctor(Risky_Async* self);
void Risky_Async_cs2sx_resume(Risky_Async* self);
void Risky_Async_cs2sx_run(Risky_Async* self);

void Risky_Async_Dispose(Risky_Async* self);
void Risky_Async_Gc(void* self, CS2SX_GcVisit visit);
void Risky_Async_Free(Risky_Async* self);
Risky_Async* Risky_Async_Retain(Risky_Async* self);
struct AsyncFlowApp
{
    SwitchApp base;
    Source* f_source;
    int f_finished;
    int f_since;
};

void AsyncFlowApp_Init(AsyncFlowApp* self);
CS2SX_Task_str* AsyncFlowApp_Guarded(AsyncFlowApp* self, int n);
CS2SX_Task_int* AsyncFlowApp_Nested(AsyncFlowApp* self);
CS2SX_Task_int* AsyncFlowApp_Drain(AsyncFlowApp* self);
CS2SX_Task* AsyncFlowApp_Lost(AsyncFlowApp* self);
CS2SX_Task* AsyncFlowApp_Script(AsyncFlowApp* self);
void AsyncFlowApp_OnInit(AsyncFlowApp* self);
void AsyncFlowApp_OnFrame(AsyncFlowApp* self);

void AsyncFlowApp_Free(AsyncFlowApp* self);
struct Guarded_Async
{
    CS2SX_Step base;
    AsyncFlowApp* f_cs2sx_this;
    CS2SX_Task_str* f_cs2sx_task;
    int f_cs2sx_state;
    int f_n;
    const char* f_result;
    int f_v;
    CS2SX_Task_int* f_cs2sx_a1;
    CS2SX_Task* f_cs2sx_a2;
};

Guarded_Async* Guarded_Async_New(void);
void Guarded_Async_Ctor(Guarded_Async* self);
void Guarded_Async_cs2sx_resume(Guarded_Async* self);
void Guarded_Async_cs2sx_run(Guarded_Async* self);

void Guarded_Async_Dispose(Guarded_Async* self);
void Guarded_Async_Gc(void* self, CS2SX_GcVisit visit);
void Guarded_Async_Free(Guarded_Async* self);
Guarded_Async* Guarded_Async_Retain(Guarded_Async* self);
struct Nested_Async
{
    CS2SX_Step base;
    AsyncFlowApp* f_cs2sx_this;
    CS2SX_Task_int* f_cs2sx_task;
    int f_cs2sx_state;
    int f_steps;
    int f_i;
    CS2SX_Task_int* f_cs2sx_a1;
};

Nested_Async* Nested_Async_New(void);
void Nested_Async_Ctor(Nested_Async* self);
void Nested_Async_cs2sx_resume(Nested_Async* self);
void Nested_Async_cs2sx_run(Nested_Async* self);

void Nested_Async_Dispose(Nested_Async* self);
void Nested_Async_Gc(void* self, CS2SX_GcVisit visit);
void Nested_Async_Free(Nested_Async* self);
Nested_Async* Nested_Async_Retain(Nested_Async* self);
struct Drain_Async
{
    CS2SX_Step base;
    AsyncFlowApp* f_cs2sx_this;
    CS2SX_Task_int* f_cs2sx_task;
    int f_cs2sx_state;
    int f_rounds;
    CS2SX_Task_bool* f_cs2sx_a1;
};

Drain_Async* Drain_Async_New(void);
void Drain_Async_Ctor(Drain_Async* self);
void Drain_Async_cs2sx_resume(Drain_Async* self);
void Drain_Async_cs2sx_run(Drain_Async* self);

void Drain_Async_Dispose(Drain_Async* self);
void Drain_Async_Gc(void* self, CS2SX_GcVisit visit);
void Drain_Async_Free(Drain_Async* self);
Drain_Async* Drain_Async_Retain(Drain_Async* self);
struct Lost_Async
{
    CS2SX_Step base;
    AsyncFlowApp* f_cs2sx_this;
    CS2SX_Task* f_cs2sx_task;
    int f_cs2sx_state;
    CS2SX_Task* f_cs2sx_a1;
};

Lost_Async* Lost_Async_New(void);
void Lost_Async_Ctor(Lost_Async* self);
void Lost_Async_cs2sx_resume(Lost_Async* self);
void Lost_Async_cs2sx_run(Lost_Async* self);

void Lost_Async_Dispose(Lost_Async* self);
void Lost_Async_Gc(void* self, CS2SX_GcVisit visit);
void Lost_Async_Free(Lost_Async* self);
Lost_Async* Lost_Async_Retain(Lost_Async* self);
struct Script_Async
{
    CS2SX_Step base;
    AsyncFlowApp* f_cs2sx_this;
    CS2SX_Task* f_cs2sx_task;
    int f_cs2sx_state;
    CS2SX_Task* f_slow;
    CS2SX_Task* f_fast;
    CS2SX_Task* f_first;
    CS2SX_Task_str* f_cs2sx_a1;
    CS2SX_Task_str* f_cs2sx_a2;
    CS2SX_Task_int* f_cs2sx_a3;
    CS2SX_Task_int* f_cs2sx_a4;
    CS2SX_Task_CS2SX_Task* f_cs2sx_a5;
};

Script_Async* Script_Async_New(void);
void Script_Async_Ctor(Script_Async* self);
void Script_Async_cs2sx_resume(Script_Async* self);
void Script_Async_cs2sx_run(Script_Async* self);

void Script_Async_Dispose(Script_Async* self);
void Script_Async_Gc(void* self, CS2SX_GcVisit visit);
void Script_Async_Free(Script_Async* self);
Script_Async* Script_Async_Retain(Script_Async* self);


#endif
