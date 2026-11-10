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
void CS2SX_Async_Suspend(int point);

struct CS2SX_Task
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    int f_IsCompleted;
    Exception* f_Fault;
    long long f_Due;
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
struct Loader
{
    int _rc;
    void (*_dtor)(void* self);   // _Dispose of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    int f_calls;
};

Loader* Loader_New(void);
void Loader_Ctor(Loader* self);
CS2SX_Task_int* Loader_Fetch(Loader* self, int id);
CS2SX_Task_str* Loader_Name(Loader* self, int id);

void Loader_Dispose(Loader* self);
void Loader_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Loader_GcKind;
void Loader_Free(Loader* self);
Loader* Loader_Retain(Loader* self);
int Loader_get_Calls(Loader* self);
struct Fetch_Async
{
    CS2SX_Step base;
    Loader* f_cs2sx_this;
    CS2SX_Task_int* f_cs2sx_task;
    int f_cs2sx_state;
    int f_id;
    CS2SX_Task* f_cs2sx_a1;
};

Fetch_Async* Fetch_Async_New(void);
void Fetch_Async_Ctor(Fetch_Async* self);
void Fetch_Async_cs2sx_resume(Fetch_Async* self);
void Fetch_Async_cs2sx_run(Fetch_Async* self);

void Fetch_Async_Dispose(Fetch_Async* self);
void Fetch_Async_Gc(void* self, CS2SX_GcVisit visit);
void Fetch_Async_Free(Fetch_Async* self);
Fetch_Async* Fetch_Async_Retain(Fetch_Async* self);
struct Name_Async
{
    CS2SX_Step base;
    Loader* f_cs2sx_this;
    CS2SX_Task_str* f_cs2sx_task;
    int f_cs2sx_state;
    int f_id;
    int f_v;
    CS2SX_Task_int* f_cs2sx_a1;
};

Name_Async* Name_Async_New(void);
void Name_Async_Ctor(Name_Async* self);
void Name_Async_cs2sx_resume(Name_Async* self);
void Name_Async_cs2sx_run(Name_Async* self);

void Name_Async_Dispose(Name_Async* self);
void Name_Async_Gc(void* self, CS2SX_GcVisit visit);
void Name_Async_Free(Name_Async* self);
Name_Async* Name_Async_Retain(Name_Async* self);
struct AsyncAwaitApp
{
    SwitchApp base;
    Loader* f_loader;
    CS2SX_Task* f_seq;
    CS2SX_Task_int* f_sum;
    CS2SX_Task* f_outer;
    CS2SX_Task* f_all;
    int f_fired;
    int f_reported;
    int f_pulses;
    int f_since;
    long long f_base;
};

void AsyncAwaitApp_Init(AsyncAwaitApp* self);
CS2SX_Task_str* AsyncAwaitApp_Greet(const char* who);
CS2SX_Task_int* AsyncAwaitApp_Twice(AsyncAwaitApp* self, int x);
CS2SX_Task_int* AsyncAwaitApp_Sum(AsyncAwaitApp* self, List_int* ids);
CS2SX_Task* AsyncAwaitApp_Sequence(AsyncAwaitApp* self, const char* who);
CS2SX_Task* AsyncAwaitApp_Fails(AsyncAwaitApp* self);
CS2SX_Task* AsyncAwaitApp_Outer(AsyncAwaitApp* self);
void AsyncAwaitApp_Fire(AsyncAwaitApp* self);
CS2SX_Task* AsyncAwaitApp_Pulse(AsyncAwaitApp* self);
void AsyncAwaitApp_OnInit(AsyncAwaitApp* self);
void AsyncAwaitApp_OnFrame(AsyncAwaitApp* self);

void AsyncAwaitApp_Free(AsyncAwaitApp* self);
struct Greet_Async
{
    CS2SX_Step base;
    CS2SX_Task_str* f_cs2sx_task;
    int f_cs2sx_state;
    const char* f_who;
    CS2SX_Task* f_cs2sx_a1;
};

Greet_Async* Greet_Async_New(void);
void Greet_Async_Ctor(Greet_Async* self);
void Greet_Async_cs2sx_resume(Greet_Async* self);
void Greet_Async_cs2sx_run(Greet_Async* self);

void Greet_Async_Dispose(Greet_Async* self);
void Greet_Async_Gc(void* self, CS2SX_GcVisit visit);
void Greet_Async_Free(Greet_Async* self);
Greet_Async* Greet_Async_Retain(Greet_Async* self);
struct Twice_Async
{
    CS2SX_Step base;
    AsyncAwaitApp* f_cs2sx_this;
    CS2SX_Task_int* f_cs2sx_task;
    int f_cs2sx_state;
    int x;
    CS2SX_Task_int* f_cs2sx_a1;
};

Twice_Async* Twice_Async_New(void);
void Twice_Async_Ctor(Twice_Async* self);
void Twice_Async_cs2sx_resume(Twice_Async* self);
void Twice_Async_cs2sx_run(Twice_Async* self);

void Twice_Async_Dispose(Twice_Async* self);
void Twice_Async_Gc(void* self, CS2SX_GcVisit visit);
void Twice_Async_Free(Twice_Async* self);
Twice_Async* Twice_Async_Retain(Twice_Async* self);
struct Sum_Async
{
    CS2SX_Step base;
    AsyncAwaitApp* f_cs2sx_this;
    CS2SX_Task_int* f_cs2sx_task;
    int f_cs2sx_state;
    List_int* f_ids;
    int f_total;
    int f_i;
    int f_id;
    CS2SX_Task_int* f_cs2sx_a1;
    CS2SX_Task* f_cs2sx_a2;
    List_int* f_cs2sx_c1;
    int f_cs2sx_i1;
};

Sum_Async* Sum_Async_New(void);
void Sum_Async_Ctor(Sum_Async* self);
void Sum_Async_cs2sx_resume(Sum_Async* self);
void Sum_Async_cs2sx_run(Sum_Async* self);

void Sum_Async_Dispose(Sum_Async* self);
void Sum_Async_Gc(void* self, CS2SX_GcVisit visit);
void Sum_Async_Free(Sum_Async* self);
Sum_Async* Sum_Async_Retain(Sum_Async* self);
struct Sequence_Async
{
    CS2SX_Step base;
    AsyncAwaitApp* f_cs2sx_this;
    CS2SX_Task* f_cs2sx_task;
    int f_cs2sx_state;
    const char* f_who;
    const char* f_greeting;
    const char* f_a;
    CS2SX_Task_int* f_ready;
    int f_five;
    CS2SX_Task_str* f_cs2sx_a1;
    CS2SX_Task_str* f_cs2sx_a2;
    CS2SX_Task* f_cs2sx_a3;
    CS2SX_Task_int* f_cs2sx_a4;
    CS2SX_Task_int* f_cs2sx_a5;
};

Sequence_Async* Sequence_Async_New(void);
void Sequence_Async_Ctor(Sequence_Async* self);
void Sequence_Async_cs2sx_resume(Sequence_Async* self);
void Sequence_Async_cs2sx_run(Sequence_Async* self);

void Sequence_Async_Dispose(Sequence_Async* self);
void Sequence_Async_Gc(void* self, CS2SX_GcVisit visit);
void Sequence_Async_Free(Sequence_Async* self);
Sequence_Async* Sequence_Async_Retain(Sequence_Async* self);
struct Fails_Async
{
    CS2SX_Step base;
    AsyncAwaitApp* f_cs2sx_this;
    CS2SX_Task* f_cs2sx_task;
    int f_cs2sx_state;
    CS2SX_Task* f_cs2sx_a1;
};

Fails_Async* Fails_Async_New(void);
void Fails_Async_Ctor(Fails_Async* self);
void Fails_Async_cs2sx_resume(Fails_Async* self);
void Fails_Async_cs2sx_run(Fails_Async* self);

void Fails_Async_Dispose(Fails_Async* self);
void Fails_Async_Gc(void* self, CS2SX_GcVisit visit);
void Fails_Async_Free(Fails_Async* self);
Fails_Async* Fails_Async_Retain(Fails_Async* self);
struct Outer_Async
{
    CS2SX_Step base;
    AsyncAwaitApp* f_cs2sx_this;
    CS2SX_Task* f_cs2sx_task;
    int f_cs2sx_state;
    CS2SX_Task* f_cs2sx_a1;
};

Outer_Async* Outer_Async_New(void);
void Outer_Async_Ctor(Outer_Async* self);
void Outer_Async_cs2sx_resume(Outer_Async* self);
void Outer_Async_cs2sx_run(Outer_Async* self);

void Outer_Async_Dispose(Outer_Async* self);
void Outer_Async_Gc(void* self, CS2SX_GcVisit visit);
void Outer_Async_Free(Outer_Async* self);
Outer_Async* Outer_Async_Retain(Outer_Async* self);
struct Fire_Async
{
    CS2SX_Step base;
    AsyncAwaitApp* f_cs2sx_this;
    CS2SX_Task* f_cs2sx_task;
    int f_cs2sx_state;
    CS2SX_Task* f_cs2sx_a1;
    CS2SX_Task* f_cs2sx_a2;
};

Fire_Async* Fire_Async_New(void);
void Fire_Async_Ctor(Fire_Async* self);
void Fire_Async_cs2sx_resume(Fire_Async* self);
void Fire_Async_cs2sx_run(Fire_Async* self);

void Fire_Async_Dispose(Fire_Async* self);
void Fire_Async_Gc(void* self, CS2SX_GcVisit visit);
void Fire_Async_Free(Fire_Async* self);
Fire_Async* Fire_Async_Retain(Fire_Async* self);
struct Pulse_Async
{
    CS2SX_Step base;
    AsyncAwaitApp* f_cs2sx_this;
    CS2SX_Task* f_cs2sx_task;
    int f_cs2sx_state;
    CS2SX_Task* f_cs2sx_a1;
};

Pulse_Async* Pulse_Async_New(void);
void Pulse_Async_Ctor(Pulse_Async* self);
void Pulse_Async_cs2sx_resume(Pulse_Async* self);
void Pulse_Async_cs2sx_run(Pulse_Async* self);

void Pulse_Async_Dispose(Pulse_Async* self);
void Pulse_Async_Gc(void* self, CS2SX_GcVisit visit);
void Pulse_Async_Free(Pulse_Async* self);
Pulse_Async* Pulse_Async_Retain(Pulse_Async* self);


#endif
