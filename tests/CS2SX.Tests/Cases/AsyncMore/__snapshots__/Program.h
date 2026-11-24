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
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    CS2SX_Step_vtable* vtable;
};

CS2SX_Step* CS2SX_Step_New(void);
void CS2SX_Step_Ctor(CS2SX_Step* self);
/* abstract: void CS2SX_Step_cs2sx_resume(CS2SX_Step* self) */

void CS2SX_Step__Destroy(CS2SX_Step* self);
extern const CS2SX_ClassInfo CS2SX_Step_ClassInfo;
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
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
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

void CS2SX_Task__Destroy(CS2SX_Task* self);
extern const CS2SX_ClassInfo CS2SX_Task_ClassInfo;
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

void CS2SX_WhenAll__Destroy(CS2SX_WhenAll* self);
extern const CS2SX_ClassInfo CS2SX_WhenAll_ClassInfo;
void CS2SX_WhenAll_Gc(void* self, CS2SX_GcVisit visit);
void CS2SX_WhenAll_Free(CS2SX_WhenAll* self);
CS2SX_WhenAll* CS2SX_WhenAll_Retain(CS2SX_WhenAll* self);
struct CS2SX_WhenAll_int
{
    CS2SX_Step base;
    List_CS2SX_Task_int* f_Tasks;
    CS2SX_Task_intArr* f_Done;
    int f_Next;
};

CS2SX_WhenAll_int* CS2SX_WhenAll_int_New(void);
void CS2SX_WhenAll_int_Ctor(CS2SX_WhenAll_int* self);
CS2SX_Task_intArr* CS2SX_WhenAll_int_Of(CS2SX_Task_int** tasks);
CS2SX_Task_intArr* CS2SX_WhenAll_int_OfList(List_CS2SX_Task_int* tasks);
void CS2SX_WhenAll_int_cs2sx_resume(CS2SX_WhenAll_int* self);

void CS2SX_WhenAll_int__Destroy(CS2SX_WhenAll_int* self);
extern const CS2SX_ClassInfo CS2SX_WhenAll_int_ClassInfo;
void CS2SX_WhenAll_int_Gc(void* self, CS2SX_GcVisit visit);
void CS2SX_WhenAll_int_Free(CS2SX_WhenAll_int* self);
CS2SX_WhenAll_int* CS2SX_WhenAll_int_Retain(CS2SX_WhenAll_int* self);
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

void CS2SX_Task_int__Destroy(CS2SX_Task_int* self);
extern const CS2SX_ClassInfo CS2SX_Task_int_ClassInfo;
void CS2SX_Task_int_Gc(void* self, CS2SX_GcVisit visit);
void CS2SX_Task_int_Free(CS2SX_Task_int* self);
CS2SX_Task_int* CS2SX_Task_int_Retain(CS2SX_Task_int* self);
int CS2SX_Task_int_get_Result(CS2SX_Task_int* self);
struct CS2SX_Task_intArr
{
    CS2SX_Task base;
    int* f_Value;
};

CS2SX_Task_intArr* CS2SX_Task_intArr_New(void);
void CS2SX_Task_intArr_Ctor(CS2SX_Task_intArr* self);
void CS2SX_Task_intArr_SetResult(CS2SX_Task_intArr* self, int* v);
CS2SX_Task_intArr* CS2SX_Task_intArr_From(int* v);
CS2SX_Task_intArr* CS2SX_Task_intArr_Run(Func_intptr_t work);

void CS2SX_Task_intArr__Destroy(CS2SX_Task_intArr* self);
extern const CS2SX_ClassInfo CS2SX_Task_intArr_ClassInfo;
void CS2SX_Task_intArr_Gc(void* self, CS2SX_GcVisit visit);
void CS2SX_Task_intArr_Free(CS2SX_Task_intArr* self);
CS2SX_Task_intArr* CS2SX_Task_intArr_Retain(CS2SX_Task_intArr* self);
int* CS2SX_Task_intArr_get_Result(CS2SX_Task_intArr* self);
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

void CS2SX_Task_str__Destroy(CS2SX_Task_str* self);
extern const CS2SX_ClassInfo CS2SX_Task_str_ClassInfo;
void CS2SX_Task_str_Gc(void* self, CS2SX_GcVisit visit);
void CS2SX_Task_str_Free(CS2SX_Task_str* self);
CS2SX_Task_str* CS2SX_Task_str_Retain(CS2SX_Task_str* self);
const char* CS2SX_Task_str_get_Result(CS2SX_Task_str* self);
struct Resource
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    const char* f_Name;
};

extern int Resource_Open;
Resource* Resource_New(const char* name);
void Resource_Ctor(Resource* self, const char* name);
void Resource_Dispose(Resource* self);

void Resource__Destroy(Resource* self);
extern const CS2SX_ClassInfo Resource_ClassInfo;
void Resource_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Resource_GcKind;
void Resource_Free(Resource* self);
Resource* Resource_Retain(Resource* self);
struct AsyncMoreApp
{
    SwitchApp base;
    int f_ticks;
    Func_CS2SX_Task_t f_later;
    int f_done;
};

void AsyncMoreApp_Init(AsyncMoreApp* self);
CS2SX_Task_int* AsyncMoreApp_Value(int v);
CS2SX_Task_str* AsyncMoreApp_Name();
CS2SX_Task* AsyncMoreApp_Fail(const char* msg);
CS2SX_Task_str* AsyncMoreApp_Guarded(AsyncMoreApp* self);
CS2SX_Task_str* AsyncMoreApp_Rethrows(AsyncMoreApp* self);
CS2SX_Task_int* AsyncMoreApp_Loops(AsyncMoreApp* self);
CS2SX_Task* AsyncMoreApp_Script(AsyncMoreApp* self);
void AsyncMoreApp_OnInit(AsyncMoreApp* self);
void AsyncMoreApp_OnFrame(AsyncMoreApp* self);
CS2SX_Task_int* AsyncMoreApp___AsyncLambda1_L111(AsyncMoreApp* self, int x, int bonus);
CS2SX_Task* AsyncMoreApp___AsyncLambda2_L114(AsyncMoreApp* self);
void AsyncMoreApp___AsyncLambda3_L116(AsyncMoreApp* self);

void AsyncMoreApp_Free(AsyncMoreApp* self);
struct Value_Async
{
    CS2SX_Step base;
    CS2SX_Task_int* f_cs2sx_task;
    int f_cs2sx_state;
    int f_v;
    CS2SX_Task* f_cs2sx_a1;
};

Value_Async* Value_Async_New(void);
void Value_Async_Ctor(Value_Async* self);
void Value_Async_cs2sx_resume(Value_Async* self);
void Value_Async_cs2sx_run(Value_Async* self);

void Value_Async__Destroy(Value_Async* self);
extern const CS2SX_ClassInfo Value_Async_ClassInfo;
void Value_Async_Gc(void* self, CS2SX_GcVisit visit);
void Value_Async_Free(Value_Async* self);
Value_Async* Value_Async_Retain(Value_Async* self);
struct Name_Async
{
    CS2SX_Step base;
    CS2SX_Task_str* f_cs2sx_task;
    int f_cs2sx_state;
    CS2SX_Task* f_cs2sx_a1;
};

Name_Async* Name_Async_New(void);
void Name_Async_Ctor(Name_Async* self);
void Name_Async_cs2sx_resume(Name_Async* self);
void Name_Async_cs2sx_run(Name_Async* self);

void Name_Async__Destroy(Name_Async* self);
extern const CS2SX_ClassInfo Name_Async_ClassInfo;
void Name_Async_Gc(void* self, CS2SX_GcVisit visit);
void Name_Async_Free(Name_Async* self);
Name_Async* Name_Async_Retain(Name_Async* self);
struct Fail_Async
{
    CS2SX_Step base;
    CS2SX_Task* f_cs2sx_task;
    int f_cs2sx_state;
    const char* f_msg;
    CS2SX_Task* f_cs2sx_a1;
};

Fail_Async* Fail_Async_New(void);
void Fail_Async_Ctor(Fail_Async* self);
void Fail_Async_cs2sx_resume(Fail_Async* self);
void Fail_Async_cs2sx_run(Fail_Async* self);

void Fail_Async__Destroy(Fail_Async* self);
extern const CS2SX_ClassInfo Fail_Async_ClassInfo;
void Fail_Async_Gc(void* self, CS2SX_GcVisit visit);
void Fail_Async_Free(Fail_Async* self);
Fail_Async* Fail_Async_Retain(Fail_Async* self);
struct Guarded_Async
{
    CS2SX_Step base;
    AsyncMoreApp* f_cs2sx_this;
    CS2SX_Task_str* f_cs2sx_task;
    int f_cs2sx_state;
    const char* f_log;
    Exception* f_fin1;
    Exception* f_ex1;
    InvalidOperationException* f_e;
    int f_v;
    CS2SX_Task* f_cs2sx_a1;
    CS2SX_Task_int* f_cs2sx_a2;
    CS2SX_Task* f_cs2sx_a3;
};

Guarded_Async* Guarded_Async_New(void);
void Guarded_Async_Ctor(Guarded_Async* self);
void Guarded_Async_cs2sx_resume(Guarded_Async* self);
void Guarded_Async_cs2sx_run(Guarded_Async* self);

void Guarded_Async__Destroy(Guarded_Async* self);
extern const CS2SX_ClassInfo Guarded_Async_ClassInfo;
void Guarded_Async_Gc(void* self, CS2SX_GcVisit visit);
void Guarded_Async_Free(Guarded_Async* self);
Guarded_Async* Guarded_Async_Retain(Guarded_Async* self);
struct Rethrows_Async
{
    CS2SX_Step base;
    AsyncMoreApp* f_cs2sx_this;
    CS2SX_Task_str* f_cs2sx_task;
    int f_cs2sx_state;
    Exception* f_ex2;
    CS2SX_Task* f_cs2sx_a1;
    CS2SX_Task* f_cs2sx_a2;
};

Rethrows_Async* Rethrows_Async_New(void);
void Rethrows_Async_Ctor(Rethrows_Async* self);
void Rethrows_Async_cs2sx_resume(Rethrows_Async* self);
void Rethrows_Async_cs2sx_run(Rethrows_Async* self);

void Rethrows_Async__Destroy(Rethrows_Async* self);
extern const CS2SX_ClassInfo Rethrows_Async_ClassInfo;
void Rethrows_Async_Gc(void* self, CS2SX_GcVisit visit);
void Rethrows_Async_Free(Rethrows_Async* self);
Rethrows_Async* Rethrows_Async_Retain(Rethrows_Async* self);
struct Loops_Async
{
    CS2SX_Step base;
    AsyncMoreApp* f_cs2sx_this;
    CS2SX_Task_int* f_cs2sx_task;
    int f_cs2sx_state;
    int f_n;
    int f_sum;
    int f_i;
    Resource* f_r;
    CS2SX_Task_int* f_cs2sx_a1;
    CS2SX_Task_int* f_cs2sx_a2;
    CS2SX_Task_int* f_cs2sx_a3;
    CS2SX_Task* f_cs2sx_a4;
};

Loops_Async* Loops_Async_New(void);
void Loops_Async_Ctor(Loops_Async* self);
void Loops_Async_cs2sx_resume(Loops_Async* self);
void Loops_Async_cs2sx_run(Loops_Async* self);

void Loops_Async__Destroy(Loops_Async* self);
extern const CS2SX_ClassInfo Loops_Async_ClassInfo;
void Loops_Async_Gc(void* self, CS2SX_GcVisit visit);
void Loops_Async_Free(Loops_Async* self);
Loops_Async* Loops_Async_Retain(Loops_Async* self);
struct Script_Async
{
    CS2SX_Step base;
    AsyncMoreApp* f_cs2sx_this;
    CS2SX_Task* f_cs2sx_task;
    int f_cs2sx_state;
    int f_on;
    int f_aw4;
    const char* f_none;
    const char* f_aw5;
    const char* f_picked;
    int f_aw6;
    int f_pick;
    int* f_all;
    int f_bonus;
    Func_int_CS2SX_Task_int_t f_add;
    Action_t f_fire;
    CS2SX_Task_str* f_cs2sx_a1;
    CS2SX_Task_str* f_cs2sx_a2;
    CS2SX_Task_int* f_cs2sx_a3;
    CS2SX_Task_int* f_cs2sx_a4;
    CS2SX_Task_str* f_cs2sx_a5;
    CS2SX_Task_int* f_cs2sx_a6;
    CS2SX_Task_intArr* f_cs2sx_a7;
    CS2SX_Task_int* f_cs2sx_a8;
    CS2SX_Task* f_cs2sx_a9;
};

Script_Async* Script_Async_New(void);
void Script_Async_Ctor(Script_Async* self);
void Script_Async_cs2sx_resume(Script_Async* self);
void Script_Async_cs2sx_run(Script_Async* self);

void Script_Async__Destroy(Script_Async* self);
extern const CS2SX_ClassInfo Script_Async_ClassInfo;
void Script_Async_Gc(void* self, CS2SX_GcVisit visit);
void Script_Async_Free(Script_Async* self);
Script_Async* Script_Async_Retain(Script_Async* self);
struct __AsyncLambda1_L111_Async
{
    CS2SX_Step base;
    AsyncMoreApp* f_cs2sx_this;
    CS2SX_Task_int* f_cs2sx_task;
    int f_cs2sx_state;
    int x;
    int f_bonus;
    CS2SX_Task* f_cs2sx_a1;
};

__AsyncLambda1_L111_Async* __AsyncLambda1_L111_Async_New(void);
void __AsyncLambda1_L111_Async_Ctor(__AsyncLambda1_L111_Async* self);
void __AsyncLambda1_L111_Async_cs2sx_resume(__AsyncLambda1_L111_Async* self);
void __AsyncLambda1_L111_Async_cs2sx_run(__AsyncLambda1_L111_Async* self);

void __AsyncLambda1_L111_Async__Destroy(__AsyncLambda1_L111_Async* self);
extern const CS2SX_ClassInfo __AsyncLambda1_L111_Async_ClassInfo;
void __AsyncLambda1_L111_Async_Gc(void* self, CS2SX_GcVisit visit);
void __AsyncLambda1_L111_Async_Free(__AsyncLambda1_L111_Async* self);
__AsyncLambda1_L111_Async* __AsyncLambda1_L111_Async_Retain(__AsyncLambda1_L111_Async* self);
struct __AsyncLambda2_L114_Async
{
    CS2SX_Step base;
    AsyncMoreApp* f_cs2sx_this;
    CS2SX_Task* f_cs2sx_task;
    int f_cs2sx_state;
    CS2SX_Task* f_cs2sx_a1;
};

__AsyncLambda2_L114_Async* __AsyncLambda2_L114_Async_New(void);
void __AsyncLambda2_L114_Async_Ctor(__AsyncLambda2_L114_Async* self);
void __AsyncLambda2_L114_Async_cs2sx_resume(__AsyncLambda2_L114_Async* self);
void __AsyncLambda2_L114_Async_cs2sx_run(__AsyncLambda2_L114_Async* self);

void __AsyncLambda2_L114_Async__Destroy(__AsyncLambda2_L114_Async* self);
extern const CS2SX_ClassInfo __AsyncLambda2_L114_Async_ClassInfo;
void __AsyncLambda2_L114_Async_Gc(void* self, CS2SX_GcVisit visit);
void __AsyncLambda2_L114_Async_Free(__AsyncLambda2_L114_Async* self);
__AsyncLambda2_L114_Async* __AsyncLambda2_L114_Async_Retain(__AsyncLambda2_L114_Async* self);
struct __AsyncLambda3_L116_Async
{
    CS2SX_Step base;
    AsyncMoreApp* f_cs2sx_this;
    CS2SX_Task* f_cs2sx_task;
    int f_cs2sx_state;
    CS2SX_Task* f_cs2sx_a1;
};

__AsyncLambda3_L116_Async* __AsyncLambda3_L116_Async_New(void);
void __AsyncLambda3_L116_Async_Ctor(__AsyncLambda3_L116_Async* self);
void __AsyncLambda3_L116_Async_cs2sx_resume(__AsyncLambda3_L116_Async* self);
void __AsyncLambda3_L116_Async_cs2sx_run(__AsyncLambda3_L116_Async* self);

void __AsyncLambda3_L116_Async__Destroy(__AsyncLambda3_L116_Async* self);
extern const CS2SX_ClassInfo __AsyncLambda3_L116_Async_ClassInfo;
void __AsyncLambda3_L116_Async_Gc(void* self, CS2SX_GcVisit visit);
void __AsyncLambda3_L116_Async_Free(__AsyncLambda3_L116_Async* self);
__AsyncLambda3_L116_Async* __AsyncLambda3_L116_Async_Retain(__AsyncLambda3_L116_Async* self);


#endif
