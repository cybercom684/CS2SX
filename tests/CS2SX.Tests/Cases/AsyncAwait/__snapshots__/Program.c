#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

static CS2SX_Step_vtable CS2SX_Step_vtable_instance =
{
};

void CS2SX_Step_Ctor(CS2SX_Step* self)
{
    CS2SX_TRACE("CS2SX_Step..ctor", "Program.cs");
}

CS2SX_Step* CS2SX_Step_New(void)
{
    CS2SX_Step* self = (CS2SX_Step*)calloc(1, sizeof(CS2SX_Step));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))CS2SX_Step_Dispose;
    self->_gc = CS2SX_Step_Gc;
    self->vtable = &CS2SX_Step_vtable_instance;
    CS2SX_Step_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_Step_Free);
    return self;
}

void CS2SX_Step_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Step* self = (CS2SX_Step*)_self; (void)self;
}

static void CS2SX_Step_GcDyn(void* self, CS2SX_GcVisit visit) { ((CS2SX_Step*)self)->_gc(self, visit); }
const CS2SX_GcKind CS2SX_Step_GcKind = { offsetof(CS2SX_Step, _rc), CS2SX_Step_GcDyn, (CS2SX_FreeFn)CS2SX_Step_Free };

void CS2SX_Step_Dispose(CS2SX_Step* self)
{
}

void CS2SX_Step_Free(CS2SX_Step* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

CS2SX_Step* CS2SX_Step_Retain(CS2SX_Step* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

void CS2SX_Async_Begin(int points)
{
    CS2SX_TRACE("CS2SX_Async.Begin", "Program.cs");
}

void CS2SX_Async_Suspend(int point)
{
    CS2SX_TRACE("CS2SX_Async.Suspend", "Program.cs");
}

List_CS2SX_Task* CS2SX_Task_timers;
__attribute__((constructor)) static void CS2SX_Task_timers_Init(void)
{
    CS2SX_Task_timers = List_CS2SX_Task_Retain(List_CS2SX_Task_New());
}
long long CS2SX_Task_clock;
void CS2SX_Task_Ctor(CS2SX_Task* self)
{
    CS2SX_TRACE("CS2SX_Task..ctor", "Program.cs");
}

CS2SX_Task* CS2SX_Task_New(void)
{
    CS2SX_Task* self = (CS2SX_Task*)calloc(1, sizeof(CS2SX_Task));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))CS2SX_Task_Dispose;
    self->_gc = CS2SX_Task_Gc;
    CS2SX_Task_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_Task_Free);
    return self;
}

void CS2SX_Task_Then(CS2SX_Task* self, CS2SX_Step* step)
{
    CS2SX_TRACE("CS2SX_Task.Then", "Program.cs");
    CS2SX_LINE(8);
    if (self->f_waiting == NULL)
    {
        CS2SX_LINE(8);
        CS2SX_COLL_STORE(List_CS2SX_Step, self->f_waiting, List_CS2SX_Step_New());
    }
    CS2SX_LINE(8);
    List_CS2SX_Step_Add(self->f_waiting, step);
}

void CS2SX_Task_Finish(CS2SX_Task* self)
{
    CS2SX_TRACE("CS2SX_Task.Finish", "Program.cs");
    CS2SX_LINE(8);
    if (self->f_IsCompleted)
    {
        CS2SX_LINE(8);
        return;
    }
    CS2SX_LINE(8);
    self->f_IsCompleted = 1;
    CS2SX_LINE(8);
    List_CS2SX_Step* w = self->f_waiting;
    CS2SX_LINE(8);
    if (w == NULL)
    {
        CS2SX_LINE(8);
        return;
    }
    CS2SX_LINE(8);
    CS2SX_COLL_STORE(List_CS2SX_Step, self->f_waiting, NULL);
    CS2SX_LINE(8);
    for (int i = 0; i < CS2SX_NN(w)->count; i++)
    {
        CS2SX_LINE(8);
        ({ __auto_type _cs2sx_r = CS2SX_NN(List_CS2SX_Step_Get(w, i)); _cs2sx_r->vtable->cs2sx_resume(_cs2sx_r); });
    }
}

void CS2SX_Task_Fail(CS2SX_Task* self, Exception* e)
{
    CS2SX_TRACE("CS2SX_Task.Fail", "Program.cs");
    CS2SX_LINE(8);
    self->f_Fault = e;
    CS2SX_LINE(8);
    CS2SX_Task_Finish(self);
}

void CS2SX_Task_Check(CS2SX_Task* self)
{
    CS2SX_TRACE("CS2SX_Task.Check", "Program.cs");
    CS2SX_LINE(8);
    if (!self->f_IsCompleted)
    {
        CS2SX_LINE(8);
        CS2SX_Throw((Exception*)(CS2SX_Exc_New(&CS2SX_TI_InvalidOperationException, "Der Task ist noch nicht fertig (await verwenden).", NULL, NULL)));
    }
    CS2SX_LINE(8);
    if (self->f_Fault != NULL)
    {
        CS2SX_LINE(8);
        CS2SX_Throw(Exception_Retain((Exception*)(self->f_Fault)));
    }
}

void CS2SX_Task_Wait(CS2SX_Task* self)
{
    CS2SX_TRACE("CS2SX_Task.Wait", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_Task_Check(self);
}

CS2SX_Task* CS2SX_Task_Delay(int milliseconds)
{
    CS2SX_TRACE("CS2SX_Task.Delay", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_Task* t = CS2SX_Task_New();
    CS2SX_LINE(8);
    if (milliseconds <= 0)
    {
        CS2SX_LINE(8);
        t->f_IsCompleted = 1;
        CS2SX_LINE(8);
        return t;
    }
    CS2SX_LINE(8);
    t->f_Due = CS2SX_Task_clock + milliseconds;
    CS2SX_LINE(8);
    List_CS2SX_Task_Add(CS2SX_Task_timers, t);
    CS2SX_LINE(8);
    return t;
}

CS2SX_Task* CS2SX_Task_Yield()
{
    CS2SX_TRACE("CS2SX_Task.Yield", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_Task* t = CS2SX_Task_New();
    CS2SX_LINE(8);
    List_CS2SX_Task_Add(CS2SX_Task_timers, t);
    CS2SX_LINE(8);
    return t;
}

CS2SX_Task* CS2SX_Task_Run(Action_t work)
{
    CS2SX_TRACE("CS2SX_Task.Run", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_Task* t = CS2SX_Task_New();
    CS2SX_LINE(8);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(8);
            Action_Invoke(work);
            CS2SX_LINE(8);
            CS2SX_Task_Finish(t);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* e = (Exception*)_exf_0_ex;
                CS2SX_LINE(8);
                CS2SX_Task_Fail(t, e);
            }
        }
    }
    CS2SX_LINE(8);
    return t;
}

CS2SX_Task* CS2SX_Task_WhenAll(CS2SX_Task** tasks)
{
    CS2SX_TRACE("CS2SX_Task.WhenAll", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_WhenAll* all = CS2SX_WhenAll_New();
    CS2SX_LINE(8);
    for (int i = 0; i < CS2SX_ArrLen(CS2SX_NN(tasks)); i++)
    {
        CS2SX_LINE(8);
        List_CS2SX_Task_Add(all->f_Tasks, CS2SX_AT(tasks, i));
    }
    CS2SX_LINE(8);
    all->base.vtable->cs2sx_resume(all);
    CS2SX_LINE(8);
    return all->f_Done;
}

CS2SX_Task* CS2SX_Task_WhenAllList(List_CS2SX_Task* tasks)
{
    CS2SX_TRACE("CS2SX_Task.WhenAllList", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_WhenAll* all = CS2SX_WhenAll_New();
    CS2SX_LINE(8);
    for (int i = 0; i < CS2SX_NN(tasks)->count; i++)
    {
        CS2SX_LINE(8);
        List_CS2SX_Task_Add(all->f_Tasks, List_CS2SX_Task_Get(tasks, i));
    }
    CS2SX_LINE(8);
    all->base.vtable->cs2sx_resume(all);
    CS2SX_LINE(8);
    return all->f_Done;
}

void CS2SX_Task_Tick(long long nowMs)
{
    CS2SX_TRACE("CS2SX_Task.Tick", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_Task_clock = nowMs;
    CS2SX_LINE(8);
    int i = 0;
    int n = CS2SX_NN(CS2SX_Task_timers)->count;
    CS2SX_LINE(8);
    while (i < n)
    {
        CS2SX_LINE(8);
        CS2SX_Task* t = List_CS2SX_Task_Get(CS2SX_Task_timers, i);
        CS2SX_LINE(8);
        if (CS2SX_NN(t)->f_Due <= nowMs)
        {
            CS2SX_LINE(8);
            List_CS2SX_Task_Remove(CS2SX_Task_timers, i);
            CS2SX_LINE(8);
            n--;
            CS2SX_LINE(8);
            CS2SX_Task_Finish(CS2SX_NN(t));
        }
        else
        {
            CS2SX_LINE(8);
            i++;
        }
    }
}

void CS2SX_Task_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Task* self = (CS2SX_Task*)_self; (void)self;
    if (visit) visit(self->f_waiting, &List_CS2SX_Step_GcKind);
    else if (self->f_waiting) { void* _o = self->f_waiting; self->f_waiting = NULL; ((void (*)(void*))List_CS2SX_Step_Free)(_o); }
}

static void CS2SX_Task_GcDyn(void* self, CS2SX_GcVisit visit) { ((CS2SX_Task*)self)->_gc(self, visit); }
const CS2SX_GcKind CS2SX_Task_GcKind = { offsetof(CS2SX_Task, _rc), CS2SX_Task_GcDyn, (CS2SX_FreeFn)CS2SX_Task_Free };

void CS2SX_Task_Dispose(CS2SX_Task* self)
{
    if (self->f_Fault) { Exception_Free(self->f_Fault); self->f_Fault = NULL; }
}

void CS2SX_Task_Free(CS2SX_Task* self)
{
    if (!self) return;
    CS2SX_Task* _o = (CS2SX_Task*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Task_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

CS2SX_Task* CS2SX_Task_Retain(CS2SX_Task* self)
{
    if (self) ((CS2SX_Task*)self)->_rc++;
    return self;
}

int CS2SX_Task_get_IsFaulted(CS2SX_Task* self)
{
    return self->f_Fault != NULL;
}

int CS2SX_Task_get_IsCompletedSuccessfully(CS2SX_Task* self)
{
    return self->f_IsCompleted && self->f_Fault == NULL;
}

CS2SX_Task* CS2SX_Task_get_CompletedTask()
{
    CS2SX_LINE(8);
    CS2SX_Task* t = CS2SX_Task_New();
    CS2SX_LINE(8);
    t->f_IsCompleted = 1;
    CS2SX_LINE(8);
    return t;
}

static CS2SX_Step_vtable CS2SX_WhenAll_vtable_instance;

void CS2SX_WhenAll_Ctor(CS2SX_WhenAll* self)
{
    CS2SX_TRACE("CS2SX_WhenAll..ctor", "Program.cs");
    self->f_Tasks = List_CS2SX_Task_Retain(List_CS2SX_Task_New());
    self->f_Done = CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_New());
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

CS2SX_WhenAll* CS2SX_WhenAll_New(void)
{
    CS2SX_WhenAll* self = (CS2SX_WhenAll*)calloc(1, sizeof(CS2SX_WhenAll));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))CS2SX_WhenAll_Dispose;
    self->base._gc = CS2SX_WhenAll_Gc;
    self->base.vtable = &CS2SX_WhenAll_vtable_instance;
    CS2SX_WhenAll_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_WhenAll_Free);
    return self;
}

void CS2SX_WhenAll_cs2sx_resume(CS2SX_WhenAll* self)
{
    CS2SX_TRACE("CS2SX_WhenAll.cs2sx_resume", "Program.cs");
    CS2SX_LINE(8);
    while (self->f_Next < CS2SX_NN(self->f_Tasks)->count)
    {
        CS2SX_LINE(8);
        CS2SX_Task* t = List_CS2SX_Task_Get(self->f_Tasks, self->f_Next);
        CS2SX_LINE(8);
        if (!CS2SX_NN(t)->f_IsCompleted)
        {
            CS2SX_LINE(8);
            CS2SX_Task_Then(CS2SX_NN(t), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(8);
            return;
        }
        CS2SX_LINE(8);
        self->f_Next++;
    }
    CS2SX_LINE(8);
    for (int i = 0; i < CS2SX_NN(self->f_Tasks)->count; i++)
    {
        CS2SX_LINE(8);
        Exception* failed = CS2SX_NN(List_CS2SX_Task_Get(self->f_Tasks, i))->f_Fault;
        CS2SX_LINE(8);
        if (failed != NULL)
        {
            CS2SX_LINE(8);
            CS2SX_Task_Fail(CS2SX_NN(self->f_Done), failed);
            CS2SX_LINE(8);
            return;
        }
    }
    CS2SX_LINE(8);
    CS2SX_Task_Finish(CS2SX_NN(self->f_Done));
}

void CS2SX_WhenAll_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_WhenAll* self = (CS2SX_WhenAll*)_self; (void)self;
    if (visit) visit(self->f_Tasks, &List_CS2SX_Task_GcKind);
    else if (self->f_Tasks) { void* _o = self->f_Tasks; self->f_Tasks = NULL; ((void (*)(void*))List_CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_Done, &CS2SX_Task_GcKind);
    else if (self->f_Done) { void* _o = self->f_Done; self->f_Done = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void CS2SX_WhenAll_Dispose(CS2SX_WhenAll* self)
{
    if (self->f_Done) { CS2SX_Task_Free(self->f_Done); self->f_Done = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void CS2SX_WhenAll_Free(CS2SX_WhenAll* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

CS2SX_WhenAll* CS2SX_WhenAll_Retain(CS2SX_WhenAll* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable CS2SX_WhenAll_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))CS2SX_WhenAll_cs2sx_resume,
};

void CS2SX_Task_int_Ctor(CS2SX_Task_int* self)
{
    CS2SX_TRACE("CS2SX_Task_int..ctor", "Program.cs");
    CS2SX_Task_Ctor((CS2SX_Task*)self);
}

CS2SX_Task_int* CS2SX_Task_int_New(void)
{
    CS2SX_Task_int* self = (CS2SX_Task_int*)calloc(1, sizeof(CS2SX_Task_int));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))CS2SX_Task_int_Dispose;
    self->base._gc = CS2SX_Task_int_Gc;
    CS2SX_Task_int_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_Task_int_Free);
    return self;
}

void CS2SX_Task_int_SetResult(CS2SX_Task_int* self, int v)
{
    CS2SX_TRACE("CS2SX_Task_int.SetResult", "Program.cs");
    CS2SX_LINE(8);
    self->f_Value = v;
    CS2SX_LINE(8);
    CS2SX_Task_Finish((CS2SX_Task*)self);
}

CS2SX_Task_int* CS2SX_Task_int_From(int v)
{
    CS2SX_TRACE("CS2SX_Task_int.From", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_Task_int* t = CS2SX_Task_int_New();
    CS2SX_LINE(8);
    t->f_Value = v;
    CS2SX_LINE(8);
    t->base.f_IsCompleted = 1;
    CS2SX_LINE(8);
    return t;
}

CS2SX_Task_int* CS2SX_Task_int_Run(Func_int_t work)
{
    CS2SX_TRACE("CS2SX_Task_int.Run", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_Task_int* t = CS2SX_Task_int_New();
    CS2SX_LINE(8);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(8);
            CS2SX_Task_int_SetResult(t, Func_int_Invoke(work));
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* e = (Exception*)_exf_0_ex;
                CS2SX_LINE(8);
                CS2SX_Task_Fail((CS2SX_Task*)t, e);
            }
        }
    }
    CS2SX_LINE(8);
    return t;
}

void CS2SX_Task_int_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Task_int* self = (CS2SX_Task_int*)_self; (void)self;
    CS2SX_Task_Gc(_self, visit);
}

void CS2SX_Task_int_Dispose(CS2SX_Task_int* self)
{
    CS2SX_Task_Dispose((CS2SX_Task*)self);
}

void CS2SX_Task_int_Free(CS2SX_Task_int* self)
{
    if (!self) return;
    CS2SX_Task* _o = (CS2SX_Task*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Task_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

CS2SX_Task_int* CS2SX_Task_int_Retain(CS2SX_Task_int* self)
{
    if (self) ((CS2SX_Task*)self)->_rc++;
    return self;
}

int CS2SX_Task_int_get_Result(CS2SX_Task_int* self)
{
    CS2SX_LINE(8);
    CS2SX_Task_Check((CS2SX_Task*)self);
    CS2SX_LINE(8);
    return self->f_Value;
}

void CS2SX_Task_str_Ctor(CS2SX_Task_str* self)
{
    CS2SX_TRACE("CS2SX_Task_str..ctor", "Program.cs");
    CS2SX_Task_Ctor((CS2SX_Task*)self);
}

CS2SX_Task_str* CS2SX_Task_str_New(void)
{
    CS2SX_Task_str* self = (CS2SX_Task_str*)calloc(1, sizeof(CS2SX_Task_str));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))CS2SX_Task_str_Dispose;
    self->base._gc = CS2SX_Task_str_Gc;
    CS2SX_Task_str_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_Task_str_Free);
    return self;
}

void CS2SX_Task_str_SetResult(CS2SX_Task_str* self, const char* v)
{
    CS2SX_TRACE("CS2SX_Task_str.SetResult", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_STR_STORE(self->f_Value, v);
    CS2SX_LINE(8);
    CS2SX_Task_Finish((CS2SX_Task*)self);
}

CS2SX_Task_str* CS2SX_Task_str_From(const char* v)
{
    CS2SX_TRACE("CS2SX_Task_str.From", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_Task_str* t = CS2SX_Task_str_New();
    CS2SX_LINE(8);
    CS2SX_STR_STORE(t->f_Value, v);
    CS2SX_LINE(8);
    t->base.f_IsCompleted = 1;
    CS2SX_LINE(8);
    return t;
}

CS2SX_Task_str* CS2SX_Task_str_Run(Func_str_t work)
{
    CS2SX_TRACE("CS2SX_Task_str.Run", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_Task_str* t = CS2SX_Task_str_New();
    CS2SX_LINE(8);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(8);
            CS2SX_Task_str_SetResult(t, Func_str_Invoke(work));
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* e = (Exception*)_exf_0_ex;
                CS2SX_LINE(8);
                CS2SX_Task_Fail((CS2SX_Task*)t, e);
            }
        }
    }
    CS2SX_LINE(8);
    return t;
}

void CS2SX_Task_str_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Task_str* self = (CS2SX_Task_str*)_self; (void)self;
    CS2SX_Task_Gc(_self, visit);
}

void CS2SX_Task_str_Dispose(CS2SX_Task_str* self)
{
    CS2SX_StrDrop(self->f_Value); self->f_Value = NULL;
    CS2SX_Task_Dispose((CS2SX_Task*)self);
}

void CS2SX_Task_str_Free(CS2SX_Task_str* self)
{
    if (!self) return;
    CS2SX_Task* _o = (CS2SX_Task*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Task_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

CS2SX_Task_str* CS2SX_Task_str_Retain(CS2SX_Task_str* self)
{
    if (self) ((CS2SX_Task*)self)->_rc++;
    return self;
}

const char* CS2SX_Task_str_get_Result(CS2SX_Task_str* self)
{
    CS2SX_LINE(8);
    CS2SX_Task_Check((CS2SX_Task*)self);
    CS2SX_LINE(8);
    return self->f_Value;
}

void Loader_Ctor(Loader* self)
{
    CS2SX_TRACE("Loader..ctor", "Program.cs");
}

Loader* Loader_New(void)
{
    Loader* self = (Loader*)calloc(1, sizeof(Loader));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Loader_Dispose;
    self->_gc = Loader_Gc;
    Loader_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Loader_Free);
    return self;
}

CS2SX_Task_int* Loader_Fetch(Loader* self, int id)
{
    CS2SX_TRACE("Loader.Fetch", "Program.cs");
    CS2SX_LINE(14);
    Fetch_Async* cs2sx_sm = Fetch_Async_New();
    CS2SX_LINE(14);
    CS2SX_ARC_STORE(Loader, cs2sx_sm->f_cs2sx_this, CS2SX_RETAIN(Loader, self));
    CS2SX_LINE(14);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_int_New()));
    CS2SX_LINE(14);
    cs2sx_sm->f_id = id;
    CS2SX_LINE(14);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(14);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task_str* Loader_Name(Loader* self, int id)
{
    CS2SX_TRACE("Loader.Name", "Program.cs");
    CS2SX_LINE(21);
    Name_Async* cs2sx_sm = Name_Async_New();
    CS2SX_LINE(21);
    CS2SX_ARC_STORE(Loader, cs2sx_sm->f_cs2sx_this, CS2SX_RETAIN(Loader, self));
    CS2SX_LINE(21);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_str_New()));
    CS2SX_LINE(21);
    cs2sx_sm->f_id = id;
    CS2SX_LINE(21);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(21);
    return cs2sx_sm->f_cs2sx_task;
}

void Loader_Gc(void* _self, CS2SX_GcVisit visit)
{
    Loader* self = (Loader*)_self; (void)self;
}

static void Loader_GcDyn(void* self, CS2SX_GcVisit visit) { ((Loader*)self)->_gc(self, visit); }
const CS2SX_GcKind Loader_GcKind = { offsetof(Loader, _rc), Loader_GcDyn, (CS2SX_FreeFn)Loader_Free };

void Loader_Dispose(Loader* self)
{
}

void Loader_Free(Loader* self)
{
    if (!self) return;
    Loader* _o = (Loader*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Loader_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Loader* Loader_Retain(Loader* self)
{
    if (self) ((Loader*)self)->_rc++;
    return self;
}

int Loader_get_Calls(Loader* self)
{
    return self->f_calls;
}

static CS2SX_Step_vtable Fetch_Async_vtable_instance;

void Fetch_Async_Ctor(Fetch_Async* self)
{
    CS2SX_TRACE("Fetch_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Fetch_Async* Fetch_Async_New(void)
{
    Fetch_Async* self = (Fetch_Async*)calloc(1, sizeof(Fetch_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Fetch_Async_Dispose;
    self->base._gc = Fetch_Async_Gc;
    self->base.vtable = &Fetch_Async_vtable_instance;
    Fetch_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Fetch_Async_Free);
    return self;
}

void Fetch_Async_cs2sx_resume(Fetch_Async* self)
{
    CS2SX_TRACE("Fetch_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(14);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(14);
            Fetch_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(14);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Fetch_Async_cs2sx_run(Fetch_Async* self)
{
    CS2SX_TRACE("Fetch_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(14);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_resume_1; default: break; };
    CS2SX_LINE(15);
    CS2SX_NN(self->f_cs2sx_this)->f_calls++;
    {
        CS2SX_LINE(16);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Delay(30)));
        CS2SX_LINE(16);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(16);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(16);
            self->f_cs2sx_state = 1; return; _cs2sx_resume_1: (void)0;
        }
        CS2SX_LINE(16);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    {
        CS2SX_LINE(17);
        CS2SX_Task_int_SetResult(CS2SX_NN(self->f_cs2sx_task), self->f_id * 10);
        CS2SX_LINE(17);
        return;
    }
    CS2SX_LINE(18);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

void Fetch_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Fetch_Async* self = (Fetch_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_this, &Loader_GcKind);
    else if (self->f_cs2sx_this) { void* _o = self->f_cs2sx_this; self->f_cs2sx_this = NULL; ((void (*)(void*))Loader_Free)(_o); }
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Fetch_Async_Dispose(Fetch_Async* self)
{
    if (self->f_cs2sx_this) { Loader_Free(self->f_cs2sx_this); self->f_cs2sx_this = NULL; }
    if (self->f_cs2sx_task) { CS2SX_Task_int_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Fetch_Async_Free(Fetch_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Fetch_Async* Fetch_Async_Retain(Fetch_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Fetch_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Fetch_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Name_Async_vtable_instance;

void Name_Async_Ctor(Name_Async* self)
{
    CS2SX_TRACE("Name_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Name_Async* Name_Async_New(void)
{
    Name_Async* self = (Name_Async*)calloc(1, sizeof(Name_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Name_Async_Dispose;
    self->base._gc = Name_Async_Gc;
    self->base.vtable = &Name_Async_vtable_instance;
    Name_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Name_Async_Free);
    return self;
}

void Name_Async_cs2sx_resume(Name_Async* self)
{
    CS2SX_TRACE("Name_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(21);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(21);
            Name_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(21);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Name_Async_cs2sx_run(Name_Async* self)
{
    CS2SX_TRACE("Name_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(21);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_resume_1; default: break; };
    {
        CS2SX_LINE(22);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, Loader_Fetch(CS2SX_NN(self->f_cs2sx_this), self->f_id)));
        CS2SX_LINE(22);
        if (!CS2SX_NN(self->f_cs2sx_a1)->base.f_IsCompleted)
        {
            CS2SX_LINE(22);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)self);
            CS2SX_LINE(22);
            self->f_cs2sx_state = 1; return; _cs2sx_resume_1: (void)0;
        }
        CS2SX_LINE(22);
        self->f_v = CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a1));
    }
    {
        CS2SX_LINE(23);
        char* _sb0 = _cs2sx_next_buf();
        snprintf(_sb0, CS2SX_STRBUF_SIZE, "item%d", self->f_v);
        char* _sb1 = _cs2sx_next_buf();
        snprintf(_sb1, CS2SX_STRBUF_SIZE, "item%d", self->f_v);
        char* _sb2 = _cs2sx_next_buf();
        snprintf(_sb2, CS2SX_STRBUF_SIZE, "item%d", self->f_v);
        CS2SX_Task_str_SetResult(CS2SX_NN(self->f_cs2sx_task), _sb2);
        CS2SX_LINE(23);
        return;
    }
    CS2SX_LINE(24);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

void Name_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Name_Async* self = (Name_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_this, &Loader_GcKind);
    else if (self->f_cs2sx_this) { void* _o = self->f_cs2sx_this; self->f_cs2sx_this = NULL; ((void (*)(void*))Loader_Free)(_o); }
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Name_Async_Dispose(Name_Async* self)
{
    if (self->f_cs2sx_this) { Loader_Free(self->f_cs2sx_this); self->f_cs2sx_this = NULL; }
    if (self->f_cs2sx_task) { CS2SX_Task_str_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_int_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Name_Async_Free(Name_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Name_Async* Name_Async_Retain(Name_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Name_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Name_Async_cs2sx_resume,
};

void AsyncAwaitApp_Init(AsyncAwaitApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))AsyncAwaitApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))AsyncAwaitApp_OnFrame;
    self->f_loader = CS2SX_RETAIN(Loader, Loader_New());
}

CS2SX_Task_str* AsyncAwaitApp_Greet(const char* who)
{
    CS2SX_TRACE("AsyncAwaitApp.Greet", "Program.cs");
    CS2SX_LINE(41);
    Greet_Async* cs2sx_sm = Greet_Async_New();
    CS2SX_LINE(41);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_str_New()));
    CS2SX_LINE(41);
    CS2SX_STR_STORE(cs2sx_sm->f_who, who);
    CS2SX_LINE(41);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(41);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task_int* AsyncAwaitApp_Twice(AsyncAwaitApp* self, int x)
{
    CS2SX_TRACE("AsyncAwaitApp.Twice", "Program.cs");
    CS2SX_LINE(46);
    Twice_Async* cs2sx_sm = Twice_Async_New();
    CS2SX_LINE(46);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(46);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_int_New()));
    CS2SX_LINE(46);
    cs2sx_sm->x = x;
    CS2SX_LINE(46);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(46);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task_int* AsyncAwaitApp_Sum(AsyncAwaitApp* self, List_int* ids)
{
    CS2SX_TRACE("AsyncAwaitApp.Sum", "Program.cs");
    CS2SX_LINE(49);
    Sum_Async* cs2sx_sm = Sum_Async_New();
    CS2SX_LINE(49);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(49);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_int_New()));
    CS2SX_LINE(49);
    CS2SX_COLL_STORE(List_int, cs2sx_sm->f_ids, ids);
    CS2SX_LINE(49);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(49);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task* AsyncAwaitApp_Sequence(AsyncAwaitApp* self, const char* who)
{
    CS2SX_TRACE("AsyncAwaitApp.Sequence", "Program.cs");
    CS2SX_LINE(64);
    Sequence_Async* cs2sx_sm = Sequence_Async_New();
    CS2SX_LINE(64);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(64);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_New()));
    CS2SX_LINE(64);
    CS2SX_STR_STORE(cs2sx_sm->f_who, who);
    CS2SX_LINE(64);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(64);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task* AsyncAwaitApp_Fails(AsyncAwaitApp* self)
{
    CS2SX_TRACE("AsyncAwaitApp.Fails", "Program.cs");
    CS2SX_LINE(80);
    Fails_Async* cs2sx_sm = Fails_Async_New();
    CS2SX_LINE(80);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(80);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_New()));
    CS2SX_LINE(80);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(80);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task* AsyncAwaitApp_Outer(AsyncAwaitApp* self)
{
    CS2SX_TRACE("AsyncAwaitApp.Outer", "Program.cs");
    CS2SX_LINE(86);
    Outer_Async* cs2sx_sm = Outer_Async_New();
    CS2SX_LINE(86);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(86);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_New()));
    CS2SX_LINE(86);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(86);
    return cs2sx_sm->f_cs2sx_task;
}

void AsyncAwaitApp_Fire(AsyncAwaitApp* self)
{
    CS2SX_TRACE("AsyncAwaitApp.Fire", "Program.cs");
    CS2SX_LINE(92);
    Fire_Async* cs2sx_sm = Fire_Async_New();
    CS2SX_LINE(92);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(92);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_New()));
    CS2SX_LINE(92);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
}

CS2SX_Task* AsyncAwaitApp_Pulse(AsyncAwaitApp* self)
{
    CS2SX_TRACE("AsyncAwaitApp.Pulse", "Program.cs");
    CS2SX_LINE(100);
    Pulse_Async* cs2sx_sm = Pulse_Async_New();
    CS2SX_LINE(100);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(100);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_New()));
    CS2SX_LINE(100);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(100);
    return cs2sx_sm->f_cs2sx_task;
}

void AsyncAwaitApp_OnInit(AsyncAwaitApp* self)
{
    CS2SX_TRACE("AsyncAwaitApp.OnInit", "Program.cs");
    CS2SX_LINE(107);
    printf("init\n");
    CS2SX_LINE(108);
    CS2SX_ARC_STORE(CS2SX_Task, self->f_seq, CS2SX_RETAIN(CS2SX_Task, AsyncAwaitApp_Sequence(self, "sw")));
    CS2SX_LINE(109);
    List_int* _list_0 = List_int_New();
    List_int_Add(_list_0, 1);
    List_int_Add(_list_0, 2);
    List_int_Add(_list_0, 3);
    CS2SX_ARC_STORE(CS2SX_Task, self->f_sum, CS2SX_RETAIN(CS2SX_Task, AsyncAwaitApp_Sum(self, _list_0)));
    CS2SX_LINE(110);
    CS2SX_ARC_STORE(CS2SX_Task, self->f_outer, CS2SX_RETAIN(CS2SX_Task, AsyncAwaitApp_Outer(self)));
    CS2SX_LINE(111);
    CS2SX_ARR_LOCAL(CS2SX_Task*, _pa_1, 2, self->f_seq, (CS2SX_Task*)self->f_sum);
    CS2SX_ARC_STORE(CS2SX_Task, self->f_all, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_WhenAll(_pa_1)));
    CS2SX_LINE(112);
    AsyncAwaitApp_Fire(self);
    CS2SX_LINE(113);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "started %s %d", CS2SX_BoolStr(CS2SX_NN(self->f_sum)->base.f_IsCompleted), Loader_get_Calls(CS2SX_NN(self->f_loader)));
    const char* _ps_2 = _sb0;
    printf("%s\n", _ps_2 ? _ps_2 : "");
}

void AsyncAwaitApp_OnFrame(AsyncAwaitApp* self)
{
    CS2SX_TRACE("AsyncAwaitApp.OnFrame", "Program.cs");
    CS2SX_LINE(123);
    if (!self->f_fired)
    {
        CS2SX_LINE(123);
        return;
    }
    CS2SX_LINE(124);
    if (!self->f_reported)
    {
        CS2SX_LINE(126);
        self->f_reported = 1;
        CS2SX_LINE(127);
        char* _sb1 = _cs2sx_next_buf();
        snprintf(_sb1, CS2SX_STRBUF_SIZE, "sum %d calls %d", CS2SX_Task_int_get_Result(CS2SX_NN(self->f_sum)), Loader_get_Calls(CS2SX_NN(self->f_loader)));
        const char* _ps_3 = _sb1;
        printf("%s\n", _ps_3 ? _ps_3 : "");
        CS2SX_LINE(128);
        char* _sb2 = _cs2sx_next_buf();
        snprintf(_sb2, CS2SX_STRBUF_SIZE, "outer %s %s", CS2SX_BoolStr(CS2SX_NN(self->f_outer)->f_IsCompleted), CS2SX_BoolStr(CS2SX_Task_get_IsFaulted(CS2SX_NN(self->f_outer))));
        const char* _ps_4 = _sb2;
        printf("%s\n", _ps_4 ? _ps_4 : "");
        CS2SX_LINE(129);
        {
            CS2SX_ExFrame _exf_5;
            CS2SX_ExPush(&_exf_5);
            if (setjmp(_exf_5.buf) == 0)
            {
                CS2SX_LINE(129);
                CS2SX_Task_Wait(CS2SX_NN(self->f_outer));
                CS2SX_ExPop(&_exf_5);
            }
            else
            {
                Exception* _exf_5_ex = _exf_5.ex;
                if (CS2SX_Exc_Is(_exf_5_ex, &CS2SX_TI_InvalidOperationException))
                {
                    InvalidOperationException* e = (InvalidOperationException*)_exf_5_ex;
                    CS2SX_LINE(130);
                    char* _sb3 = _cs2sx_next_buf();
                    snprintf(_sb3, CS2SX_STRBUF_SIZE, "caught %s", ((Exception*)(e))->f_Message);
                    const char* _ps_6 = _sb3;
                    printf("%s\n", _ps_6 ? _ps_6 : "");
                }
                else CS2SX_Throw(_exf_5_ex);
            }
        }
        CS2SX_LINE(131);
        char* _sb4 = _cs2sx_next_buf();
        snprintf(_sb4, CS2SX_STRBUF_SIZE, "all %s %s", CS2SX_BoolStr(CS2SX_NN(self->f_all)->f_IsCompleted), CS2SX_BoolStr(CS2SX_Task_get_IsCompletedSuccessfully(CS2SX_NN(self->f_seq))));
        const char* _ps_7 = _sb4;
        printf("%s\n", _ps_7 ? _ps_7 : "");
        CS2SX_LINE(132);
        return;
    }
    CS2SX_LINE(136);
    AsyncAwaitApp_Pulse(self);
    CS2SX_LINE(137);
    self->f_since++;
    CS2SX_LINE(138);
    if (self->f_since == 40)
    {
        CS2SX_LINE(138);
        self->f_base = CS2SX_HeapUsed();
    }
    CS2SX_LINE(139);
    if (self->f_since == 300)
    {
        CS2SX_LINE(141);
        long long grown = CS2SX_HeapUsed() - self->f_base;
        CS2SX_LINE(142);
        char* _sb5 = _cs2sx_next_buf();
        snprintf(_sb5, CS2SX_STRBUF_SIZE, "stable %s %s", CS2SX_BoolStr((grown < 8192)), CS2SX_BoolStr((self->f_pulses > 250)));
        const char* _ps_8 = _sb5;
        printf("%s\n", _ps_8 ? _ps_8 : "");
        CS2SX_LINE(143);
        printf("done\n");
    }
}

void AsyncAwaitApp_Free(AsyncAwaitApp* self)
{
    if (!self) return;
    if (self->f_loader) { Loader_Free(self->f_loader); self->f_loader = NULL; }
    if (self->f_seq) { CS2SX_Task_Free(self->f_seq); self->f_seq = NULL; }
    if (self->f_sum) { CS2SX_Task_int_Free(self->f_sum); self->f_sum = NULL; }
    if (self->f_outer) { CS2SX_Task_Free(self->f_outer); self->f_outer = NULL; }
    if (self->f_all) { CS2SX_Task_Free(self->f_all); self->f_all = NULL; }
    free(self);
}

static CS2SX_Step_vtable Greet_Async_vtable_instance;

void Greet_Async_Ctor(Greet_Async* self)
{
    CS2SX_TRACE("Greet_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Greet_Async* Greet_Async_New(void)
{
    Greet_Async* self = (Greet_Async*)calloc(1, sizeof(Greet_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Greet_Async_Dispose;
    self->base._gc = Greet_Async_Gc;
    self->base.vtable = &Greet_Async_vtable_instance;
    Greet_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Greet_Async_Free);
    return self;
}

void Greet_Async_cs2sx_resume(Greet_Async* self)
{
    CS2SX_TRACE("Greet_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(41);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(41);
            Greet_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(41);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Greet_Async_cs2sx_run(Greet_Async* self)
{
    CS2SX_TRACE("Greet_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(41);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_resume_1; default: break; };
    {
        CS2SX_LINE(42);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
        CS2SX_LINE(42);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(42);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(42);
            self->f_cs2sx_state = 1; return; _cs2sx_resume_1: (void)0;
        }
        CS2SX_LINE(42);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    {
        CS2SX_LINE(43);
        char* _sb0 = _cs2sx_next_buf();
        snprintf(_sb0, CS2SX_STRBUF_SIZE, "hi %s", self->f_who);
        char* _sb1 = _cs2sx_next_buf();
        snprintf(_sb1, CS2SX_STRBUF_SIZE, "hi %s", self->f_who);
        char* _sb2 = _cs2sx_next_buf();
        snprintf(_sb2, CS2SX_STRBUF_SIZE, "hi %s", self->f_who);
        CS2SX_Task_str_SetResult(CS2SX_NN(self->f_cs2sx_task), _sb2);
        CS2SX_LINE(43);
        return;
    }
    CS2SX_LINE(44);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

void Greet_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Greet_Async* self = (Greet_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Greet_Async_Dispose(Greet_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_str_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    CS2SX_StrDrop(self->f_who); self->f_who = NULL;
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Greet_Async_Free(Greet_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Greet_Async* Greet_Async_Retain(Greet_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Greet_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Greet_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Twice_Async_vtable_instance;

void Twice_Async_Ctor(Twice_Async* self)
{
    CS2SX_TRACE("Twice_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Twice_Async* Twice_Async_New(void)
{
    Twice_Async* self = (Twice_Async*)calloc(1, sizeof(Twice_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Twice_Async_Dispose;
    self->base._gc = Twice_Async_Gc;
    self->base.vtable = &Twice_Async_vtable_instance;
    Twice_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Twice_Async_Free);
    return self;
}

void Twice_Async_cs2sx_resume(Twice_Async* self)
{
    CS2SX_TRACE("Twice_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(46);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(46);
            Twice_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(46);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Twice_Async_cs2sx_run(Twice_Async* self)
{
    CS2SX_TRACE("Twice_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(46);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_resume_1; default: break; };
    CS2SX_LINE(46);
    CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, Loader_Fetch(CS2SX_NN(CS2SX_NN(self->f_cs2sx_this)->f_loader), self->x)));
    CS2SX_LINE(46);
    if (!CS2SX_NN(self->f_cs2sx_a1)->base.f_IsCompleted)
    {
        CS2SX_LINE(46);
        CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)self);
        CS2SX_LINE(46);
        self->f_cs2sx_state = 1; return; _cs2sx_resume_1: (void)0;
    }
    CS2SX_LINE(46);
    CS2SX_Task_int_SetResult(CS2SX_NN(self->f_cs2sx_task), CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a1)) * 2);
}

void Twice_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Twice_Async* self = (Twice_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Twice_Async_Dispose(Twice_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_int_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_int_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Twice_Async_Free(Twice_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Twice_Async* Twice_Async_Retain(Twice_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Twice_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Twice_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Sum_Async_vtable_instance;

void Sum_Async_Ctor(Sum_Async* self)
{
    CS2SX_TRACE("Sum_Async..ctor", "Program.cs");
    self->f_ids = List_int_Retain(List_int_New());
    self->f_cs2sx_c1 = List_int_Retain(List_int_New());
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Sum_Async* Sum_Async_New(void)
{
    Sum_Async* self = (Sum_Async*)calloc(1, sizeof(Sum_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Sum_Async_Dispose;
    self->base._gc = Sum_Async_Gc;
    self->base.vtable = &Sum_Async_vtable_instance;
    Sum_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Sum_Async_Free);
    return self;
}

void Sum_Async_cs2sx_resume(Sum_Async* self)
{
    CS2SX_TRACE("Sum_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(49);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(49);
            Sum_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(49);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Sum_Async_cs2sx_run(Sum_Async* self)
{
    CS2SX_TRACE("Sum_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(49);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_resume_1; case 2: goto _cs2sx_resume_2; default: break; };
    CS2SX_LINE(50);
    self->f_total = 0;
    {
        CS2SX_LINE(51);
        CS2SX_COLL_STORE(List_int, self->f_cs2sx_c1, self->f_ids);
        CS2SX_LINE(51);
        for (self->f_cs2sx_i1 = 0; self->f_cs2sx_i1 < CS2SX_NN(self->f_cs2sx_c1)->count; self->f_cs2sx_i1++)
        {
            CS2SX_LINE(52);
            self->f_id = List_int_Get(self->f_cs2sx_c1, self->f_cs2sx_i1);
            {
                CS2SX_LINE(53);
                CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, Loader_Fetch(CS2SX_NN(CS2SX_NN(self->f_cs2sx_this)->f_loader), self->f_id)));
                CS2SX_LINE(53);
                if (!CS2SX_NN(self->f_cs2sx_a1)->base.f_IsCompleted)
                {
                    CS2SX_LINE(53);
                    CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)self);
                    CS2SX_LINE(53);
                    self->f_cs2sx_state = 1; return; _cs2sx_resume_1: (void)0;
                }
                CS2SX_LINE(53);
                self->f_total += CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a1));
            }
        }
    }
    CS2SX_LINE(55);
    for (self->f_i = 0; self->f_i < 2; self->f_i++)
    {
        {
            CS2SX_LINE(57);
            CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a2, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
            CS2SX_LINE(57);
            if (!CS2SX_NN(self->f_cs2sx_a2)->f_IsCompleted)
            {
                CS2SX_LINE(57);
                CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a2), (CS2SX_Step*)(CS2SX_Step*)self);
                CS2SX_LINE(57);
                self->f_cs2sx_state = 2; return; _cs2sx_resume_2: (void)0;
            }
            CS2SX_LINE(57);
            CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a2));
        }
        CS2SX_LINE(58);
        self->f_total++;
    }
    {
        CS2SX_LINE(60);
        CS2SX_Task_int_SetResult(CS2SX_NN(self->f_cs2sx_task), self->f_total);
        CS2SX_LINE(60);
        return;
    }
    CS2SX_LINE(61);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

void Sum_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Sum_Async* self = (Sum_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a2, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a2) { void* _o = self->f_cs2sx_a2; self->f_cs2sx_a2 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Sum_Async_Dispose(Sum_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_int_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_int_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    if (self->f_cs2sx_a2) { CS2SX_Task_Free(self->f_cs2sx_a2); self->f_cs2sx_a2 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Sum_Async_Free(Sum_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Sum_Async* Sum_Async_Retain(Sum_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Sum_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Sum_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Sequence_Async_vtable_instance;

void Sequence_Async_Ctor(Sequence_Async* self)
{
    CS2SX_TRACE("Sequence_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Sequence_Async* Sequence_Async_New(void)
{
    Sequence_Async* self = (Sequence_Async*)calloc(1, sizeof(Sequence_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Sequence_Async_Dispose;
    self->base._gc = Sequence_Async_Gc;
    self->base.vtable = &Sequence_Async_vtable_instance;
    Sequence_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Sequence_Async_Free);
    return self;
}

void Sequence_Async_cs2sx_resume(Sequence_Async* self)
{
    CS2SX_TRACE("Sequence_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(64);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(64);
            Sequence_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(64);
                CS2SX_Task_Fail(CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Sequence_Async_cs2sx_run(Sequence_Async* self)
{
    CS2SX_TRACE("Sequence_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(64);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_resume_1; case 2: goto _cs2sx_resume_2; case 3: goto _cs2sx_resume_3; case 4: goto _cs2sx_resume_4; case 5: goto _cs2sx_resume_5; default: break; };
    CS2SX_LINE(65);
    printf("seq start\n");
    {
        CS2SX_LINE(66);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, AsyncAwaitApp_Greet(self->f_who)));
        CS2SX_LINE(66);
        if (!CS2SX_NN(self->f_cs2sx_a1)->base.f_IsCompleted)
        {
            CS2SX_LINE(66);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)self);
            CS2SX_LINE(66);
            self->f_cs2sx_state = 1; return; _cs2sx_resume_1: (void)0;
        }
        CS2SX_LINE(66);
        CS2SX_STR_STORE(self->f_greeting, CS2SX_Task_str_get_Result(CS2SX_NN(self->f_cs2sx_a1)));
    }
    {
        CS2SX_LINE(67);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a2, CS2SX_RETAIN(CS2SX_Task, Loader_Name(CS2SX_NN(CS2SX_NN(self->f_cs2sx_this)->f_loader), 1)));
        CS2SX_LINE(67);
        if (!CS2SX_NN(self->f_cs2sx_a2)->base.f_IsCompleted)
        {
            CS2SX_LINE(67);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a2), (CS2SX_Step*)self);
            CS2SX_LINE(67);
            self->f_cs2sx_state = 2; return; _cs2sx_resume_2: (void)0;
        }
        CS2SX_LINE(67);
        CS2SX_STR_STORE(self->f_a, CS2SX_Task_str_get_Result(CS2SX_NN(self->f_cs2sx_a2)));
    }
    CS2SX_LINE(68);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "seq %s %s", self->f_greeting, self->f_a);
    const char* _ps_1 = _sb0;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(69);
    if (strlen(CS2SX_NN(self->f_a)) > 3)
    {
        {
            CS2SX_LINE(71);
            CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a3, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Delay(20)));
            CS2SX_LINE(71);
            if (!CS2SX_NN(self->f_cs2sx_a3)->f_IsCompleted)
            {
                CS2SX_LINE(71);
                CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a3), (CS2SX_Step*)(CS2SX_Step*)self);
                CS2SX_LINE(71);
                self->f_cs2sx_state = 3; return; _cs2sx_resume_3: (void)0;
            }
            CS2SX_LINE(71);
            CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a3));
        }
        CS2SX_LINE(72);
        printf("seq waited\n");
    }
    CS2SX_LINE(74);
    CS2SX_ARC_STORE(CS2SX_Task, self->f_ready, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_int_From(5)));
    {
        CS2SX_LINE(75);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a4, CS2SX_RETAIN(CS2SX_Task, self->f_ready));
        CS2SX_LINE(75);
        if (!CS2SX_NN(self->f_cs2sx_a4)->base.f_IsCompleted)
        {
            CS2SX_LINE(75);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a4), (CS2SX_Step*)self);
            CS2SX_LINE(75);
            self->f_cs2sx_state = 4; return; _cs2sx_resume_4: (void)0;
        }
        CS2SX_LINE(75);
        self->f_five = CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a4));
    }
    {
        CS2SX_LINE(76);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a5, CS2SX_RETAIN(CS2SX_Task, AsyncAwaitApp_Twice(CS2SX_NN(self->f_cs2sx_this), 2)));
        CS2SX_LINE(76);
        if (!CS2SX_NN(self->f_cs2sx_a5)->base.f_IsCompleted)
        {
            CS2SX_LINE(76);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a5), (CS2SX_Step*)self);
            CS2SX_LINE(76);
            self->f_cs2sx_state = 5; return; _cs2sx_resume_5: (void)0;
        }
        CS2SX_LINE(76);
        char* _sb1 = _cs2sx_next_buf();
        snprintf(_sb1, CS2SX_STRBUF_SIZE, "seq end %d %d", self->f_five, CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a5)));
        const char* _ps_2 = _sb1;
        printf("%s\n", _ps_2 ? _ps_2 : "");
    }
    CS2SX_LINE(77);
    CS2SX_Task_Finish(CS2SX_NN(self->f_cs2sx_task));
}

void Sequence_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Sequence_Async* self = (Sequence_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_ready, &CS2SX_Task_GcKind);
    else if (self->f_ready) { void* _o = self->f_ready; self->f_ready = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a2, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a2) { void* _o = self->f_cs2sx_a2; self->f_cs2sx_a2 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a3, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a3) { void* _o = self->f_cs2sx_a3; self->f_cs2sx_a3 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a4, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a4) { void* _o = self->f_cs2sx_a4; self->f_cs2sx_a4 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a5, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a5) { void* _o = self->f_cs2sx_a5; self->f_cs2sx_a5 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Sequence_Async_Dispose(Sequence_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    CS2SX_StrDrop(self->f_who); self->f_who = NULL;
    CS2SX_StrDrop(self->f_greeting); self->f_greeting = NULL;
    CS2SX_StrDrop(self->f_a); self->f_a = NULL;
    if (self->f_ready) { CS2SX_Task_int_Free(self->f_ready); self->f_ready = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_str_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    if (self->f_cs2sx_a2) { CS2SX_Task_str_Free(self->f_cs2sx_a2); self->f_cs2sx_a2 = NULL; }
    if (self->f_cs2sx_a3) { CS2SX_Task_Free(self->f_cs2sx_a3); self->f_cs2sx_a3 = NULL; }
    if (self->f_cs2sx_a4) { CS2SX_Task_int_Free(self->f_cs2sx_a4); self->f_cs2sx_a4 = NULL; }
    if (self->f_cs2sx_a5) { CS2SX_Task_int_Free(self->f_cs2sx_a5); self->f_cs2sx_a5 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Sequence_Async_Free(Sequence_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Sequence_Async* Sequence_Async_Retain(Sequence_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Sequence_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Sequence_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Fails_Async_vtable_instance;

void Fails_Async_Ctor(Fails_Async* self)
{
    CS2SX_TRACE("Fails_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Fails_Async* Fails_Async_New(void)
{
    Fails_Async* self = (Fails_Async*)calloc(1, sizeof(Fails_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Fails_Async_Dispose;
    self->base._gc = Fails_Async_Gc;
    self->base.vtable = &Fails_Async_vtable_instance;
    Fails_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Fails_Async_Free);
    return self;
}

void Fails_Async_cs2sx_resume(Fails_Async* self)
{
    CS2SX_TRACE("Fails_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(80);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(80);
            Fails_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(80);
                CS2SX_Task_Fail(CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Fails_Async_cs2sx_run(Fails_Async* self)
{
    CS2SX_TRACE("Fails_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(80);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_resume_1; default: break; };
    {
        CS2SX_LINE(81);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Delay(10)));
        CS2SX_LINE(81);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(81);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(81);
            self->f_cs2sx_state = 1; return; _cs2sx_resume_1: (void)0;
        }
        CS2SX_LINE(81);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    CS2SX_LINE(82);
    CS2SX_Throw((Exception*)(CS2SX_Exc_New(&CS2SX_TI_InvalidOperationException, "boom", NULL, NULL)));
    CS2SX_LINE(83);
    CS2SX_Task_Finish(CS2SX_NN(self->f_cs2sx_task));
}

void Fails_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Fails_Async* self = (Fails_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Fails_Async_Dispose(Fails_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Fails_Async_Free(Fails_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Fails_Async* Fails_Async_Retain(Fails_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Fails_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Fails_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Outer_Async_vtable_instance;

void Outer_Async_Ctor(Outer_Async* self)
{
    CS2SX_TRACE("Outer_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Outer_Async* Outer_Async_New(void)
{
    Outer_Async* self = (Outer_Async*)calloc(1, sizeof(Outer_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Outer_Async_Dispose;
    self->base._gc = Outer_Async_Gc;
    self->base.vtable = &Outer_Async_vtable_instance;
    Outer_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Outer_Async_Free);
    return self;
}

void Outer_Async_cs2sx_resume(Outer_Async* self)
{
    CS2SX_TRACE("Outer_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(86);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(86);
            Outer_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(86);
                CS2SX_Task_Fail(CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Outer_Async_cs2sx_run(Outer_Async* self)
{
    CS2SX_TRACE("Outer_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(86);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_resume_1; default: break; };
    {
        CS2SX_LINE(87);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, AsyncAwaitApp_Fails(CS2SX_NN(self->f_cs2sx_this))));
        CS2SX_LINE(87);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(87);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(87);
            self->f_cs2sx_state = 1; return; _cs2sx_resume_1: (void)0;
        }
        CS2SX_LINE(87);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    CS2SX_LINE(88);
    printf("not reached\n");
    CS2SX_LINE(89);
    CS2SX_Task_Finish(CS2SX_NN(self->f_cs2sx_task));
}

void Outer_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Outer_Async* self = (Outer_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Outer_Async_Dispose(Outer_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Outer_Async_Free(Outer_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Outer_Async* Outer_Async_Retain(Outer_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Outer_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Outer_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Fire_Async_vtable_instance;

void Fire_Async_Ctor(Fire_Async* self)
{
    CS2SX_TRACE("Fire_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Fire_Async* Fire_Async_New(void)
{
    Fire_Async* self = (Fire_Async*)calloc(1, sizeof(Fire_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Fire_Async_Dispose;
    self->base._gc = Fire_Async_Gc;
    self->base.vtable = &Fire_Async_vtable_instance;
    Fire_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Fire_Async_Free);
    return self;
}

void Fire_Async_cs2sx_resume(Fire_Async* self)
{
    CS2SX_TRACE("Fire_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(92);
    Fire_Async_cs2sx_run(self);
}

void Fire_Async_cs2sx_run(Fire_Async* self)
{
    CS2SX_TRACE("Fire_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(92);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_resume_1; case 2: goto _cs2sx_resume_2; default: break; };
    {
        CS2SX_LINE(93);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Delay(50)));
        CS2SX_LINE(93);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(93);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(93);
            self->f_cs2sx_state = 1; return; _cs2sx_resume_1: (void)0;
        }
        CS2SX_LINE(93);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    {
        CS2SX_LINE(94);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a2, CS2SX_RETAIN(CS2SX_Task, CS2SX_NN(self->f_cs2sx_this)->f_all));
        CS2SX_LINE(94);
        if (!CS2SX_NN(self->f_cs2sx_a2)->f_IsCompleted)
        {
            CS2SX_LINE(94);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a2), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(94);
            self->f_cs2sx_state = 2; return; _cs2sx_resume_2: (void)0;
        }
        CS2SX_LINE(94);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a2));
    }
    CS2SX_LINE(95);
    printf("fire\n");
    CS2SX_LINE(96);
    CS2SX_NN(self->f_cs2sx_this)->f_fired = 1;
    CS2SX_LINE(97);
    CS2SX_Task_Finish(CS2SX_NN(self->f_cs2sx_task));
}

void Fire_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Fire_Async* self = (Fire_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a2, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a2) { void* _o = self->f_cs2sx_a2; self->f_cs2sx_a2 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Fire_Async_Dispose(Fire_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    if (self->f_cs2sx_a2) { CS2SX_Task_Free(self->f_cs2sx_a2); self->f_cs2sx_a2 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Fire_Async_Free(Fire_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Fire_Async* Fire_Async_Retain(Fire_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Fire_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Fire_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Pulse_Async_vtable_instance;

void Pulse_Async_Ctor(Pulse_Async* self)
{
    CS2SX_TRACE("Pulse_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Pulse_Async* Pulse_Async_New(void)
{
    Pulse_Async* self = (Pulse_Async*)calloc(1, sizeof(Pulse_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Pulse_Async_Dispose;
    self->base._gc = Pulse_Async_Gc;
    self->base.vtable = &Pulse_Async_vtable_instance;
    Pulse_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Pulse_Async_Free);
    return self;
}

void Pulse_Async_cs2sx_resume(Pulse_Async* self)
{
    CS2SX_TRACE("Pulse_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(100);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(100);
            Pulse_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(100);
                CS2SX_Task_Fail(CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Pulse_Async_cs2sx_run(Pulse_Async* self)
{
    CS2SX_TRACE("Pulse_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(100);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_resume_1; default: break; };
    {
        CS2SX_LINE(101);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
        CS2SX_LINE(101);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(101);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(101);
            self->f_cs2sx_state = 1; return; _cs2sx_resume_1: (void)0;
        }
        CS2SX_LINE(101);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    CS2SX_LINE(102);
    CS2SX_NN(self->f_cs2sx_this)->f_pulses++;
    CS2SX_LINE(103);
    CS2SX_Task_Finish(CS2SX_NN(self->f_cs2sx_task));
}

void Pulse_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Pulse_Async* self = (Pulse_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Pulse_Async_Dispose(Pulse_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Pulse_Async_Free(Pulse_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Pulse_Async* Pulse_Async_Retain(Pulse_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Pulse_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Pulse_Async_cs2sx_resume,
};
