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

void CS2SX_Async_Suspend_1(int point)
{
    CS2SX_TRACE("CS2SX_Async.Suspend", "Program.cs");
}

void CS2SX_Async_Suspend_2(int point, int depth)
{
    CS2SX_TRACE("CS2SX_Async.Suspend", "Program.cs");
}

void CS2SX_Async_Label(int point, int level)
{
    CS2SX_TRACE("CS2SX_Async.Label", "Program.cs");
}

void CS2SX_Async_Jump(int point, int level)
{
    CS2SX_TRACE("CS2SX_Async.Jump", "Program.cs");
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
    CS2SX_LINE(9);
    self->f_Observed = 1;
    CS2SX_LINE(9);
    if (self->f_waiting == NULL)
    {
        CS2SX_LINE(9);
        CS2SX_COLL_STORE(List_CS2SX_Step, self->f_waiting, List_CS2SX_Step_New());
    }
    CS2SX_LINE(9);
    List_CS2SX_Step_Add(self->f_waiting, step);
}

void CS2SX_Task_Finish(CS2SX_Task* self)
{
    CS2SX_TRACE("CS2SX_Task.Finish", "Program.cs");
    CS2SX_LINE(9);
    if (self->f_IsCompleted)
    {
        CS2SX_LINE(9);
        return;
    }
    CS2SX_LINE(9);
    self->f_IsCompleted = 1;
    CS2SX_LINE(9);
    List_CS2SX_Step* w = self->f_waiting;
    CS2SX_LINE(9);
    if (w == NULL)
    {
        CS2SX_LINE(9);
        return;
    }
    CS2SX_LINE(9);
    CS2SX_COLL_STORE(List_CS2SX_Step, self->f_waiting, NULL);
    CS2SX_LINE(9);
    for (int i = 0; i < CS2SX_NN(w)->count; i++)
    {
        CS2SX_LINE(9);
        ({ __auto_type _cs2sx_r = CS2SX_NN(List_CS2SX_Step_Get(w, i)); _cs2sx_r->vtable->cs2sx_resume(_cs2sx_r); });
    }
}

void CS2SX_Task_Fail(CS2SX_Task* self, Exception* e)
{
    CS2SX_TRACE("CS2SX_Task.Fail", "Program.cs");
    CS2SX_LINE(9);
    self->f_Fault = e;
    CS2SX_LINE(9);
    CS2SX_Task_Finish(self);
}

void CS2SX_Task_Check(CS2SX_Task* self)
{
    CS2SX_TRACE("CS2SX_Task.Check", "Program.cs");
    CS2SX_LINE(9);
    self->f_Observed = 1;
    CS2SX_LINE(9);
    if (!self->f_IsCompleted)
    {
        CS2SX_LINE(9);
        CS2SX_Throw((Exception*)(CS2SX_Exc_New(&CS2SX_TI_InvalidOperationException, "Der Task ist noch nicht fertig (await verwenden).", NULL, NULL)));
    }
    CS2SX_LINE(9);
    if (self->f_Fault != NULL)
    {
        CS2SX_LINE(9);
        CS2SX_Throw(Exception_Retain((Exception*)(self->f_Fault)));
    }
}

void CS2SX_Task_Wait(CS2SX_Task* self)
{
    CS2SX_TRACE("CS2SX_Task.Wait", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_Check(self);
}

CS2SX_Task* CS2SX_Task_Delay(int milliseconds)
{
    CS2SX_TRACE("CS2SX_Task.Delay", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task* t = CS2SX_Task_New();
    CS2SX_LINE(9);
    if (milliseconds <= 0)
    {
        CS2SX_LINE(9);
        t->f_IsCompleted = 1;
        CS2SX_LINE(9);
        return t;
    }
    CS2SX_LINE(9);
    t->f_Due = CS2SX_Task_clock + milliseconds;
    CS2SX_LINE(9);
    List_CS2SX_Task_Add(CS2SX_Task_timers, t);
    CS2SX_LINE(9);
    return t;
}

CS2SX_Task* CS2SX_Task_DelaySpan(CS2SX_TimeSpan time)
{
    CS2SX_TRACE("CS2SX_Task.DelaySpan", "Program.cs");
    CS2SX_LINE(9);
    return CS2SX_Task_Delay(((int)CS2SX_TimeSpan_TotalMs(time)));
}

CS2SX_Task* CS2SX_Task_Yield()
{
    CS2SX_TRACE("CS2SX_Task.Yield", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task* t = CS2SX_Task_New();
    CS2SX_LINE(9);
    List_CS2SX_Task_Add(CS2SX_Task_timers, t);
    CS2SX_LINE(9);
    return t;
}

CS2SX_Task* CS2SX_Task_Run(Action_t work)
{
    CS2SX_TRACE("CS2SX_Task.Run", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task* t = CS2SX_Task_New();
    CS2SX_LINE(9);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(9);
            Action_Invoke(work);
            CS2SX_LINE(9);
            CS2SX_Task_Finish(t);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* e = (Exception*)_exf_0_ex;
                CS2SX_LINE(9);
                CS2SX_Task_Fail(t, e);
            }
        }
    }
    CS2SX_LINE(9);
    return t;
}

CS2SX_Task* CS2SX_Task_WhenAll(CS2SX_Task** tasks)
{
    CS2SX_TRACE("CS2SX_Task.WhenAll", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_WhenAll* all = CS2SX_WhenAll_New();
    CS2SX_LINE(9);
    for (int i = 0; i < CS2SX_ArrLen(CS2SX_NN(tasks)); i++)
    {
        CS2SX_LINE(9);
        List_CS2SX_Task_Add(all->f_Tasks, CS2SX_AT(tasks, i));
    }
    CS2SX_LINE(9);
    all->base.vtable->cs2sx_resume(all);
    CS2SX_LINE(9);
    return all->f_Done;
}

CS2SX_Task* CS2SX_Task_WhenAllList(List_CS2SX_Task* tasks)
{
    CS2SX_TRACE("CS2SX_Task.WhenAllList", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_WhenAll* all = CS2SX_WhenAll_New();
    CS2SX_LINE(9);
    for (int i = 0; i < CS2SX_NN(tasks)->count; i++)
    {
        CS2SX_LINE(9);
        List_CS2SX_Task_Add(all->f_Tasks, List_CS2SX_Task_Get(tasks, i));
    }
    CS2SX_LINE(9);
    all->base.vtable->cs2sx_resume(all);
    CS2SX_LINE(9);
    return all->f_Done;
}

void CS2SX_Task_Tick(long long nowMs)
{
    CS2SX_TRACE("CS2SX_Task.Tick", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_clock = nowMs;
    CS2SX_LINE(9);
    int i = 0;
    int n = CS2SX_NN(CS2SX_Task_timers)->count;
    CS2SX_LINE(9);
    while (i < n)
    {
        CS2SX_LINE(9);
        CS2SX_Task* t = List_CS2SX_Task_Get(CS2SX_Task_timers, i);
        CS2SX_LINE(9);
        if (CS2SX_NN(t)->f_Due <= nowMs)
        {
            CS2SX_LINE(9);
            List_CS2SX_Task_Remove(CS2SX_Task_timers, i);
            CS2SX_LINE(9);
            n--;
            CS2SX_LINE(9);
            CS2SX_Task_Finish(CS2SX_NN(t));
        }
        else
        {
            CS2SX_LINE(9);
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
    CS2SX_LINE(9);
    if (self->f_Fault != NULL && !self->f_Observed)
    {
        CS2SX_LINE(9);
        char* _sb0 = _cs2sx_next_buf();
        _sb0 = CS2SX_Format(_sb0, "Unobserved task exception: %s\n%s", ((Exception*)(self->f_Fault))->f_Message, (((Exception*)(self->f_Fault))->f_StackTrace ? ((Exception*)(self->f_Fault))->f_StackTrace : ""));
        const char* _ps_1 = _sb0;
        printf("%s\n", _ps_1 ? _ps_1 : "");
    }
    if (self->f_Fault) { Exception_Free(self->f_Fault); self->f_Fault = NULL; }
    if (self->f_waiting) { List_CS2SX_Step_Free(self->f_waiting); self->f_waiting = NULL; }
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
    CS2SX_TRACE("CS2SX_Task.get_CompletedTask", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task* t = CS2SX_Task_New();
    CS2SX_LINE(9);
    t->f_IsCompleted = 1;
    CS2SX_LINE(9);
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
    CS2SX_LINE(9);
    while (self->f_Next < CS2SX_NN(self->f_Tasks)->count)
    {
        CS2SX_LINE(9);
        CS2SX_Task* t = List_CS2SX_Task_Get(self->f_Tasks, self->f_Next);
        CS2SX_LINE(9);
        if (!CS2SX_NN(t)->f_IsCompleted)
        {
            CS2SX_LINE(9);
            CS2SX_Task_Then(CS2SX_NN(t), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(9);
            return;
        }
        CS2SX_LINE(9);
        self->f_Next++;
    }
    CS2SX_LINE(9);
    for (int i = 0; i < CS2SX_NN(self->f_Tasks)->count; i++)
    {
        CS2SX_LINE(9);
        Exception* failed = CS2SX_NN(List_CS2SX_Task_Get(self->f_Tasks, i))->f_Fault;
        CS2SX_LINE(9);
        if (failed != NULL)
        {
            CS2SX_LINE(9);
            CS2SX_NN(List_CS2SX_Task_Get(self->f_Tasks, i))->f_Observed = 1;
            CS2SX_LINE(9);
            CS2SX_Task_Fail(CS2SX_NN(self->f_Done), failed);
            CS2SX_LINE(9);
            return;
        }
    }
    CS2SX_LINE(9);
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
    if (self->f_Tasks) { List_CS2SX_Task_Free(self->f_Tasks); self->f_Tasks = NULL; }
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

void CS2SX_Task_CS2SX_Task_Ctor(CS2SX_Task_CS2SX_Task* self)
{
    CS2SX_TRACE("CS2SX_Task_CS2SX_Task..ctor", "Program.cs");
    CS2SX_Task_Ctor((CS2SX_Task*)self);
}

CS2SX_Task_CS2SX_Task* CS2SX_Task_CS2SX_Task_New(void)
{
    CS2SX_Task_CS2SX_Task* self = (CS2SX_Task_CS2SX_Task*)calloc(1, sizeof(CS2SX_Task_CS2SX_Task));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))CS2SX_Task_CS2SX_Task_Dispose;
    self->base._gc = CS2SX_Task_CS2SX_Task_Gc;
    CS2SX_Task_CS2SX_Task_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_Task_CS2SX_Task_Free);
    return self;
}

void CS2SX_Task_CS2SX_Task_SetResult(CS2SX_Task_CS2SX_Task* self, CS2SX_Task* v)
{
    CS2SX_TRACE("CS2SX_Task_CS2SX_Task.SetResult", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_ARC_STORE(CS2SX_Task, self->f_Value, CS2SX_RETAIN(CS2SX_Task, v));
    CS2SX_LINE(9);
    CS2SX_Task_Finish((CS2SX_Task*)self);
}

CS2SX_Task_CS2SX_Task* CS2SX_Task_CS2SX_Task_From(CS2SX_Task* v)
{
    CS2SX_TRACE("CS2SX_Task_CS2SX_Task.From", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_CS2SX_Task* t = CS2SX_Task_CS2SX_Task_New();
    CS2SX_LINE(9);
    CS2SX_ARC_STORE(CS2SX_Task, t->f_Value, CS2SX_RETAIN(CS2SX_Task, v));
    CS2SX_LINE(9);
    t->base.f_IsCompleted = 1;
    CS2SX_LINE(9);
    return t;
}

CS2SX_Task_CS2SX_Task* CS2SX_Task_CS2SX_Task_Run(Func_CS2SX_Task_t work)
{
    CS2SX_TRACE("CS2SX_Task_CS2SX_Task.Run", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_CS2SX_Task* t = CS2SX_Task_CS2SX_Task_New();
    CS2SX_LINE(9);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(9);
            CS2SX_Task_CS2SX_Task_SetResult(t, Func_CS2SX_Task_Invoke(work));
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* e = (Exception*)_exf_0_ex;
                CS2SX_LINE(9);
                CS2SX_Task_Fail((CS2SX_Task*)t, e);
            }
        }
    }
    CS2SX_LINE(9);
    return t;
}

void CS2SX_Task_CS2SX_Task_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Task_CS2SX_Task* self = (CS2SX_Task_CS2SX_Task*)_self; (void)self;
    if (visit) visit(self->f_Value, &CS2SX_Task_GcKind);
    else if (self->f_Value) { void* _o = self->f_Value; self->f_Value = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Task_Gc(_self, visit);
}

void CS2SX_Task_CS2SX_Task_Dispose(CS2SX_Task_CS2SX_Task* self)
{
    if (self->f_Value) { CS2SX_Task_Free(self->f_Value); self->f_Value = NULL; }
    CS2SX_Task_Dispose((CS2SX_Task*)self);
}

void CS2SX_Task_CS2SX_Task_Free(CS2SX_Task_CS2SX_Task* self)
{
    if (!self) return;
    CS2SX_Task* _o = (CS2SX_Task*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Task_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

CS2SX_Task_CS2SX_Task* CS2SX_Task_CS2SX_Task_Retain(CS2SX_Task_CS2SX_Task* self)
{
    if (self) ((CS2SX_Task*)self)->_rc++;
    return self;
}

CS2SX_Task* CS2SX_Task_CS2SX_Task_get_Result(CS2SX_Task_CS2SX_Task* self)
{
    CS2SX_TRACE("CS2SX_Task_CS2SX_Task.get_Result", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_Check((CS2SX_Task*)self);
    CS2SX_LINE(9);
    return self->f_Value;
}

static CS2SX_Step_vtable CS2SX_WhenAny_vtable_instance;

void CS2SX_WhenAny_Ctor(CS2SX_WhenAny* self)
{
    CS2SX_TRACE("CS2SX_WhenAny..ctor", "Program.cs");
    self->f_Tasks = List_CS2SX_Task_Retain(List_CS2SX_Task_New());
    self->f_Done = CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_CS2SX_Task_New());
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

CS2SX_WhenAny* CS2SX_WhenAny_New(void)
{
    CS2SX_WhenAny* self = (CS2SX_WhenAny*)calloc(1, sizeof(CS2SX_WhenAny));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))CS2SX_WhenAny_Dispose;
    self->base._gc = CS2SX_WhenAny_Gc;
    self->base.vtable = &CS2SX_WhenAny_vtable_instance;
    CS2SX_WhenAny_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_WhenAny_Free);
    return self;
}

CS2SX_Task_CS2SX_Task* CS2SX_WhenAny_Of(CS2SX_Task** tasks)
{
    CS2SX_TRACE("CS2SX_WhenAny.Of", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_WhenAny* any = CS2SX_WhenAny_New();
    CS2SX_LINE(9);
    for (int i = 0; i < CS2SX_ArrLen(CS2SX_NN(tasks)); i++)
    {
        CS2SX_LINE(9);
        List_CS2SX_Task_Add(any->f_Tasks, CS2SX_AT(tasks, i));
    }
    CS2SX_LINE(9);
    any->base.vtable->cs2sx_resume(any);
    CS2SX_LINE(9);
    if (!CS2SX_NN(any->f_Done)->base.f_IsCompleted)
    {
        CS2SX_LINE(9);
        for (int i = 0; i < CS2SX_NN(any->f_Tasks)->count; i++)
        {
            CS2SX_LINE(9);
            CS2SX_Task_Then(CS2SX_NN(List_CS2SX_Task_Get(any->f_Tasks, i)), (CS2SX_Step*)(CS2SX_Step*)any);
        }
    }
    CS2SX_LINE(9);
    return any->f_Done;
}

void CS2SX_WhenAny_cs2sx_resume(CS2SX_WhenAny* self)
{
    CS2SX_TRACE("CS2SX_WhenAny.cs2sx_resume", "Program.cs");
    CS2SX_LINE(9);
    if (CS2SX_NN(self->f_Done)->base.f_IsCompleted)
    {
        CS2SX_LINE(9);
        return;
    }
    CS2SX_LINE(9);
    for (int i = 0; i < CS2SX_NN(self->f_Tasks)->count; i++)
    {
        CS2SX_LINE(9);
        CS2SX_Task* t = List_CS2SX_Task_Get(self->f_Tasks, i);
        CS2SX_LINE(9);
        if (CS2SX_NN(t)->f_IsCompleted)
        {
            CS2SX_LINE(9);
            CS2SX_Task_CS2SX_Task_SetResult(CS2SX_NN(self->f_Done), t);
            CS2SX_LINE(9);
            return;
        }
    }
}

void CS2SX_WhenAny_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_WhenAny* self = (CS2SX_WhenAny*)_self; (void)self;
    if (visit) visit(self->f_Tasks, &List_CS2SX_Task_GcKind);
    else if (self->f_Tasks) { void* _o = self->f_Tasks; self->f_Tasks = NULL; ((void (*)(void*))List_CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_Done, &CS2SX_Task_GcKind);
    else if (self->f_Done) { void* _o = self->f_Done; self->f_Done = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void CS2SX_WhenAny_Dispose(CS2SX_WhenAny* self)
{
    if (self->f_Tasks) { List_CS2SX_Task_Free(self->f_Tasks); self->f_Tasks = NULL; }
    if (self->f_Done) { CS2SX_Task_CS2SX_Task_Free(self->f_Done); self->f_Done = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void CS2SX_WhenAny_Free(CS2SX_WhenAny* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

CS2SX_WhenAny* CS2SX_WhenAny_Retain(CS2SX_WhenAny* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable CS2SX_WhenAny_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))CS2SX_WhenAny_cs2sx_resume,
};

void CS2SX_Task_bool_Ctor(CS2SX_Task_bool* self)
{
    CS2SX_TRACE("CS2SX_Task_bool..ctor", "Program.cs");
    CS2SX_Task_Ctor((CS2SX_Task*)self);
}

CS2SX_Task_bool* CS2SX_Task_bool_New(void)
{
    CS2SX_Task_bool* self = (CS2SX_Task_bool*)calloc(1, sizeof(CS2SX_Task_bool));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))CS2SX_Task_bool_Dispose;
    self->base._gc = CS2SX_Task_bool_Gc;
    CS2SX_Task_bool_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_Task_bool_Free);
    return self;
}

void CS2SX_Task_bool_SetResult(CS2SX_Task_bool* self, int v)
{
    CS2SX_TRACE("CS2SX_Task_bool.SetResult", "Program.cs");
    CS2SX_LINE(9);
    self->f_Value = v;
    CS2SX_LINE(9);
    CS2SX_Task_Finish((CS2SX_Task*)self);
}

CS2SX_Task_bool* CS2SX_Task_bool_From(int v)
{
    CS2SX_TRACE("CS2SX_Task_bool.From", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_bool* t = CS2SX_Task_bool_New();
    CS2SX_LINE(9);
    t->f_Value = v;
    CS2SX_LINE(9);
    t->base.f_IsCompleted = 1;
    CS2SX_LINE(9);
    return t;
}

CS2SX_Task_bool* CS2SX_Task_bool_Run(Func_int_t work)
{
    CS2SX_TRACE("CS2SX_Task_bool.Run", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_bool* t = CS2SX_Task_bool_New();
    CS2SX_LINE(9);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(9);
            CS2SX_Task_bool_SetResult(t, Func_int_Invoke(work));
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* e = (Exception*)_exf_0_ex;
                CS2SX_LINE(9);
                CS2SX_Task_Fail((CS2SX_Task*)t, e);
            }
        }
    }
    CS2SX_LINE(9);
    return t;
}

void CS2SX_Task_bool_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Task_bool* self = (CS2SX_Task_bool*)_self; (void)self;
    CS2SX_Task_Gc(_self, visit);
}

void CS2SX_Task_bool_Dispose(CS2SX_Task_bool* self)
{
    CS2SX_Task_Dispose((CS2SX_Task*)self);
}

void CS2SX_Task_bool_Free(CS2SX_Task_bool* self)
{
    if (!self) return;
    CS2SX_Task* _o = (CS2SX_Task*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Task_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

CS2SX_Task_bool* CS2SX_Task_bool_Retain(CS2SX_Task_bool* self)
{
    if (self) ((CS2SX_Task*)self)->_rc++;
    return self;
}

int CS2SX_Task_bool_get_Result(CS2SX_Task_bool* self)
{
    CS2SX_TRACE("CS2SX_Task_bool.get_Result", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_Check((CS2SX_Task*)self);
    CS2SX_LINE(9);
    return self->f_Value;
}

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
    CS2SX_LINE(9);
    self->f_Value = v;
    CS2SX_LINE(9);
    CS2SX_Task_Finish((CS2SX_Task*)self);
}

CS2SX_Task_int* CS2SX_Task_int_From(int v)
{
    CS2SX_TRACE("CS2SX_Task_int.From", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_int* t = CS2SX_Task_int_New();
    CS2SX_LINE(9);
    t->f_Value = v;
    CS2SX_LINE(9);
    t->base.f_IsCompleted = 1;
    CS2SX_LINE(9);
    return t;
}

CS2SX_Task_int* CS2SX_Task_int_Run(Func_int_t work)
{
    CS2SX_TRACE("CS2SX_Task_int.Run", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_int* t = CS2SX_Task_int_New();
    CS2SX_LINE(9);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(9);
            CS2SX_Task_int_SetResult(t, Func_int_Invoke(work));
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* e = (Exception*)_exf_0_ex;
                CS2SX_LINE(9);
                CS2SX_Task_Fail((CS2SX_Task*)t, e);
            }
        }
    }
    CS2SX_LINE(9);
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
    CS2SX_TRACE("CS2SX_Task_int.get_Result", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_Check((CS2SX_Task*)self);
    CS2SX_LINE(9);
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
    CS2SX_LINE(9);
    CS2SX_STR_STORE(self->f_Value, v);
    CS2SX_LINE(9);
    CS2SX_Task_Finish((CS2SX_Task*)self);
}

CS2SX_Task_str* CS2SX_Task_str_From(const char* v)
{
    CS2SX_TRACE("CS2SX_Task_str.From", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_str* t = CS2SX_Task_str_New();
    CS2SX_LINE(9);
    CS2SX_STR_STORE(t->f_Value, v);
    CS2SX_LINE(9);
    t->base.f_IsCompleted = 1;
    CS2SX_LINE(9);
    return t;
}

CS2SX_Task_str* CS2SX_Task_str_Run(Func_str_t work)
{
    CS2SX_TRACE("CS2SX_Task_str.Run", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_str* t = CS2SX_Task_str_New();
    CS2SX_LINE(9);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(9);
            CS2SX_Task_str_SetResult(t, Func_str_Invoke(work));
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* e = (Exception*)_exf_0_ex;
                CS2SX_LINE(9);
                CS2SX_Task_Fail((CS2SX_Task*)t, e);
            }
        }
    }
    CS2SX_LINE(9);
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
    CS2SX_TRACE("CS2SX_Task_str.get_Result", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Task_Check((CS2SX_Task*)self);
    CS2SX_LINE(9);
    return self->f_Value;
}

void Source_Ctor(Source* self)
{
    CS2SX_TRACE("Source..ctor", "Program.cs");
    self->f_left = 3;
}

Source* Source_New(void)
{
    Source* self = (Source*)calloc(1, sizeof(Source));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Source_Dispose;
    self->_gc = Source_Gc;
    Source_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Source_Free);
    return self;
}

CS2SX_Task_bool* Source_Next(Source* self)
{
    CS2SX_TRACE("Source.Next", "Program.cs");
    CS2SX_LINE(15);
    Next_Async* cs2sx_sm = Next_Async_New();
    CS2SX_LINE(15);
    CS2SX_ARC_STORE(Source, cs2sx_sm->f_cs2sx_this, CS2SX_RETAIN(Source, self));
    CS2SX_LINE(15);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_bool_New()));
    CS2SX_LINE(15);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(15);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task_int* Source_Risky(Source* self, int n)
{
    CS2SX_TRACE("Source.Risky", "Program.cs");
    CS2SX_LINE(22);
    Risky_Async* cs2sx_sm = Risky_Async_New();
    CS2SX_LINE(22);
    CS2SX_ARC_STORE(Source, cs2sx_sm->f_cs2sx_this, CS2SX_RETAIN(Source, self));
    CS2SX_LINE(22);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_int_New()));
    CS2SX_LINE(22);
    cs2sx_sm->f_n = n;
    CS2SX_LINE(22);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(22);
    return cs2sx_sm->f_cs2sx_task;
}

void Source_Gc(void* _self, CS2SX_GcVisit visit)
{
    Source* self = (Source*)_self; (void)self;
}

static void Source_GcDyn(void* self, CS2SX_GcVisit visit) { ((Source*)self)->_gc(self, visit); }
const CS2SX_GcKind Source_GcKind = { offsetof(Source, _rc), Source_GcDyn, (CS2SX_FreeFn)Source_Free };

void Source_Dispose(Source* self)
{
}

void Source_Free(Source* self)
{
    if (!self) return;
    Source* _o = (Source*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Source_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Source* Source_Retain(Source* self)
{
    if (self) ((Source*)self)->_rc++;
    return self;
}

int Source_get_Total(Source* self)
{
    return 10 - self->f_left;
}

static CS2SX_Step_vtable Next_Async_vtable_instance;

void Next_Async_Ctor(Next_Async* self)
{
    CS2SX_TRACE("Next_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Next_Async* Next_Async_New(void)
{
    Next_Async* self = (Next_Async*)calloc(1, sizeof(Next_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Next_Async_Dispose;
    self->base._gc = Next_Async_Gc;
    self->base.vtable = &Next_Async_vtable_instance;
    Next_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Next_Async_Free);
    return self;
}

void Next_Async_cs2sx_resume(Next_Async* self)
{
    CS2SX_TRACE("Next_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(15);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(15);
            Next_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(15);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Next_Async_cs2sx_run(Next_Async* self)
{
    CS2SX_TRACE("Next_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(15);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    {
        CS2SX_LINE(16);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
        CS2SX_LINE(16);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(16);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(16);
            self->f_cs2sx_state = 1; return; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(16);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    CS2SX_LINE(17);
    CS2SX_NN(self->f_cs2sx_this)->f_left--;
    {
        CS2SX_LINE(18);
        CS2SX_Task_bool_SetResult(CS2SX_NN(self->f_cs2sx_task), CS2SX_NN(self->f_cs2sx_this)->f_left >= 0);
        CS2SX_LINE(18);
        return;
    }
    CS2SX_LINE(19);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

void Next_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Next_Async* self = (Next_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_this, &Source_GcKind);
    else if (self->f_cs2sx_this) { void* _o = self->f_cs2sx_this; self->f_cs2sx_this = NULL; ((void (*)(void*))Source_Free)(_o); }
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Next_Async_Dispose(Next_Async* self)
{
    if (self->f_cs2sx_this) { Source_Free(self->f_cs2sx_this); self->f_cs2sx_this = NULL; }
    if (self->f_cs2sx_task) { CS2SX_Task_bool_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Next_Async_Free(Next_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Next_Async* Next_Async_Retain(Next_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Next_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Next_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Risky_Async_vtable_instance;

void Risky_Async_Ctor(Risky_Async* self)
{
    CS2SX_TRACE("Risky_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Risky_Async* Risky_Async_New(void)
{
    Risky_Async* self = (Risky_Async*)calloc(1, sizeof(Risky_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Risky_Async_Dispose;
    self->base._gc = Risky_Async_Gc;
    self->base.vtable = &Risky_Async_vtable_instance;
    Risky_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Risky_Async_Free);
    return self;
}

void Risky_Async_cs2sx_resume(Risky_Async* self)
{
    CS2SX_TRACE("Risky_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(22);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(22);
            Risky_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(22);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Risky_Async_cs2sx_run(Risky_Async* self)
{
    CS2SX_TRACE("Risky_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(22);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    {
        CS2SX_LINE(23);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Delay(5)));
        CS2SX_LINE(23);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(23);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(23);
            self->f_cs2sx_state = 1; return; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(23);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    CS2SX_LINE(24);
    if (self->f_n < 0)
    {
        CS2SX_LINE(24);
        CS2SX_Throw((Exception*)(CS2SX_Exc_New(&CS2SX_TI_ArgumentException, "negative input", NULL, NULL)));
    }
    {
        CS2SX_LINE(25);
        CS2SX_Task_int_SetResult(CS2SX_NN(self->f_cs2sx_task), self->f_n * 2);
        CS2SX_LINE(25);
        return;
    }
    CS2SX_LINE(26);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

void Risky_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Risky_Async* self = (Risky_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_this, &Source_GcKind);
    else if (self->f_cs2sx_this) { void* _o = self->f_cs2sx_this; self->f_cs2sx_this = NULL; ((void (*)(void*))Source_Free)(_o); }
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Risky_Async_Dispose(Risky_Async* self)
{
    if (self->f_cs2sx_this) { Source_Free(self->f_cs2sx_this); self->f_cs2sx_this = NULL; }
    if (self->f_cs2sx_task) { CS2SX_Task_int_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Risky_Async_Free(Risky_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Risky_Async* Risky_Async_Retain(Risky_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Risky_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Risky_Async_cs2sx_resume,
};

void AsyncFlowApp_Init(AsyncFlowApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))AsyncFlowApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))AsyncFlowApp_OnFrame;
    self->f_source = CS2SX_RETAIN(Source, Source_New());
}

CS2SX_Task_str* AsyncFlowApp_Guarded(AsyncFlowApp* self, int n)
{
    CS2SX_TRACE("AsyncFlowApp.Guarded", "Program.cs");
    CS2SX_LINE(36);
    Guarded_Async* cs2sx_sm = Guarded_Async_New();
    CS2SX_LINE(36);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(36);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_str_New()));
    CS2SX_LINE(36);
    cs2sx_sm->f_n = n;
    CS2SX_LINE(36);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(36);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task_int* AsyncFlowApp_Nested(AsyncFlowApp* self)
{
    CS2SX_TRACE("AsyncFlowApp.Nested", "Program.cs");
    CS2SX_LINE(58);
    Nested_Async* cs2sx_sm = Nested_Async_New();
    CS2SX_LINE(58);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(58);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_int_New()));
    CS2SX_LINE(58);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(58);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task_int* AsyncFlowApp_Drain(AsyncFlowApp* self)
{
    CS2SX_TRACE("AsyncFlowApp.Drain", "Program.cs");
    CS2SX_LINE(76);
    Drain_Async* cs2sx_sm = Drain_Async_New();
    CS2SX_LINE(76);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(76);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_int_New()));
    CS2SX_LINE(76);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(76);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task* AsyncFlowApp_Lost(AsyncFlowApp* self)
{
    CS2SX_TRACE("AsyncFlowApp.Lost", "Program.cs");
    CS2SX_LINE(87);
    Lost_Async* cs2sx_sm = Lost_Async_New();
    CS2SX_LINE(87);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(87);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_New()));
    CS2SX_LINE(87);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(87);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task* AsyncFlowApp_Script(AsyncFlowApp* self)
{
    CS2SX_TRACE("AsyncFlowApp.Script", "Program.cs");
    CS2SX_LINE(93);
    Script_Async* cs2sx_sm = Script_Async_New();
    CS2SX_LINE(93);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(93);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_New()));
    CS2SX_LINE(93);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(93);
    return cs2sx_sm->f_cs2sx_task;
}

void AsyncFlowApp_OnInit(AsyncFlowApp* self)
{
    CS2SX_TRACE("AsyncFlowApp.OnInit", "Program.cs");
    CS2SX_LINE(110);
    AsyncFlowApp_Script(self);
}

void AsyncFlowApp_OnFrame(AsyncFlowApp* self)
{
    CS2SX_TRACE("AsyncFlowApp.OnFrame", "Program.cs");
    CS2SX_LINE(115);
    if (!self->f_finished)
    {
        CS2SX_LINE(115);
        return;
    }
    CS2SX_LINE(118);
    if (++self->f_since == 60)
    {
        CS2SX_LINE(118);
        printf("done\n");
    }
}

void AsyncFlowApp_Free(AsyncFlowApp* self)
{
    if (!self) return;
    if (self->f_source) { Source_Free(self->f_source); self->f_source = NULL; }
    free(self);
}

static CS2SX_Step_vtable Guarded_Async_vtable_instance;

void Guarded_Async_Ctor(Guarded_Async* self)
{
    CS2SX_TRACE("Guarded_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Guarded_Async* Guarded_Async_New(void)
{
    Guarded_Async* self = (Guarded_Async*)calloc(1, sizeof(Guarded_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Guarded_Async_Dispose;
    self->base._gc = Guarded_Async_Gc;
    self->base.vtable = &Guarded_Async_vtable_instance;
    Guarded_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Guarded_Async_Free);
    return self;
}

void Guarded_Async_cs2sx_resume(Guarded_Async* self)
{
    CS2SX_TRACE("Guarded_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(36);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(36);
            Guarded_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(36);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Guarded_Async_cs2sx_run(Guarded_Async* self)
{
    CS2SX_TRACE("Guarded_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(36);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; case 2: goto _cs2sx_r2_0; default: break; };
    CS2SX_LINE(37);
    CS2SX_STR_STORE(self->f_result, "none");
    {
        CS2SX_LINE(38);
        _cs2sx_r1_0: (void)0;
        CS2SX_LINE(38);
        _cs2sx_r2_0: (void)0;
        CS2SX_LINE(38);
        {
            Exception* _exf_1_pending = NULL;
            CS2SX_ExFrame _exf_1;
            CS2SX_ExPush(&_exf_1);
            if (setjmp(_exf_1.buf) == 0)
            {
                CS2SX_LINE(39);
                if (self->f_cs2sx_state == 1) goto _cs2sx_r1_1;
                CS2SX_LINE(39);
                if (self->f_cs2sx_state == 2) goto _cs2sx_r2_1;
                CS2SX_LINE(40);
                char* _sb0 = _cs2sx_next_buf();
                _sb0 = CS2SX_Format(_sb0, "try %d", self->f_n);
                const char* _ps_2 = _sb0;
                printf("%s\n", _ps_2 ? _ps_2 : "");
                {
                    CS2SX_LINE(41);
                    CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, Source_Risky(CS2SX_NN(CS2SX_NN(self->f_cs2sx_this)->f_source), self->f_n)));
                    CS2SX_LINE(41);
                    if (!CS2SX_NN(self->f_cs2sx_a1)->base.f_IsCompleted)
                    {
                        CS2SX_LINE(41);
                        CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)self);
                        CS2SX_LINE(41);
                        self->f_cs2sx_state = 1; CS2SX_ExPop(&_exf_1); return; _cs2sx_r1_1: self->f_cs2sx_state = 0;
                    }
                    CS2SX_LINE(41);
                    self->f_v = CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a1));
                }
                CS2SX_LINE(42);
                char* _sb1 = _cs2sx_next_buf();
                _sb1 = CS2SX_Format(_sb1, "ok %d", self->f_v);
                CS2SX_STR_STORE(self->f_result, _sb1);
                {
                    CS2SX_LINE(43);
                    CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a2, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
                    CS2SX_LINE(43);
                    if (!CS2SX_NN(self->f_cs2sx_a2)->f_IsCompleted)
                    {
                        CS2SX_LINE(43);
                        CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a2), (CS2SX_Step*)(CS2SX_Step*)self);
                        CS2SX_LINE(43);
                        self->f_cs2sx_state = 2; CS2SX_ExPop(&_exf_1); return; _cs2sx_r2_1: self->f_cs2sx_state = 0;
                    }
                    CS2SX_LINE(43);
                    CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a2));
                }
                CS2SX_LINE(44);
                char* _sb2 = _cs2sx_next_buf();
                _sb2 = CS2SX_Format(_sb2, "%s%s", self->f_result ? self->f_result : "", "!" ? "!" : "");
                CS2SX_STR_STORE(self->f_result, _sb2);
                CS2SX_ExPop(&_exf_1);
            }
            else
            {
                Exception* _exf_1_ex = _exf_1.ex;
                CS2SX_ExFrame _exf_1_c;
                CS2SX_ExPush(&_exf_1_c);
                if (setjmp(_exf_1_c.buf) == 0)
                {
                    if (CS2SX_Exc_Is(_exf_1_ex, &CS2SX_TI_ArgumentException))
                    {
                        ArgumentException* e = (ArgumentException*)_exf_1_ex;
                        CS2SX_LINE(48);
                        char* _sb3 = _cs2sx_next_buf();
                        _sb3 = CS2SX_Format(_sb3, "caught %s", ((Exception*)(e))->f_Message);
                        CS2SX_STR_STORE(self->f_result, _sb3);
                    }
                    else _exf_1_pending = _exf_1_ex;
                    CS2SX_ExPop(&_exf_1_c);
                }
                else _exf_1_pending = _exf_1_c.ex;
            }
            {
                CS2SX_LINE(52);
                char* _sb4 = _cs2sx_next_buf();
                _sb4 = CS2SX_Format(_sb4, "finally %d", self->f_n);
                const char* _ps_3 = _sb4;
                printf("%s\n", _ps_3 ? _ps_3 : "");
            }
            if (_exf_1_pending) CS2SX_Throw(_exf_1_pending);
        }
    }
    {
        CS2SX_LINE(54);
        CS2SX_Task_str_SetResult(CS2SX_NN(self->f_cs2sx_task), self->f_result);
        CS2SX_LINE(54);
        return;
    }
    CS2SX_LINE(55);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

void Guarded_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Guarded_Async* self = (Guarded_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a2, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a2) { void* _o = self->f_cs2sx_a2; self->f_cs2sx_a2 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Guarded_Async_Dispose(Guarded_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_str_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    CS2SX_StrDrop(self->f_result); self->f_result = NULL;
    if (self->f_cs2sx_a1) { CS2SX_Task_int_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    if (self->f_cs2sx_a2) { CS2SX_Task_Free(self->f_cs2sx_a2); self->f_cs2sx_a2 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Guarded_Async_Free(Guarded_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Guarded_Async* Guarded_Async_Retain(Guarded_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Guarded_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Guarded_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Nested_Async_vtable_instance;

void Nested_Async_Ctor(Nested_Async* self)
{
    CS2SX_TRACE("Nested_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Nested_Async* Nested_Async_New(void)
{
    Nested_Async* self = (Nested_Async*)calloc(1, sizeof(Nested_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Nested_Async_Dispose;
    self->base._gc = Nested_Async_Gc;
    self->base.vtable = &Nested_Async_vtable_instance;
    Nested_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Nested_Async_Free);
    return self;
}

void Nested_Async_cs2sx_resume(Nested_Async* self)
{
    CS2SX_TRACE("Nested_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(58);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(58);
            Nested_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(58);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Nested_Async_cs2sx_run(Nested_Async* self)
{
    CS2SX_TRACE("Nested_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(58);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    CS2SX_LINE(59);
    self->f_steps = 0;
    CS2SX_LINE(60);
    for (self->f_i = 0; self->f_i < 2; self->f_i++)
    {
        {
            CS2SX_LINE(62);
            _cs2sx_r1_0: (void)0;
            CS2SX_LINE(62);
            {
                CS2SX_ExFrame _exf_1;
                CS2SX_ExPush(&_exf_1);
                if (setjmp(_exf_1.buf) == 0)
                {
                    CS2SX_LINE(63);
                    if (self->f_cs2sx_state == 1) goto _cs2sx_r1_1;
                    {
                        CS2SX_LINE(64);
                        _cs2sx_r1_1: (void)0;
                        CS2SX_LINE(64);
                        {
                            Exception* _exf_2_pending = NULL;
                            CS2SX_ExFrame _exf_2;
                            CS2SX_ExPush(&_exf_2);
                            if (setjmp(_exf_2.buf) == 0)
                            {
                                CS2SX_LINE(65);
                                if (self->f_cs2sx_state == 1) goto _cs2sx_r1_2;
                                {
                                    CS2SX_LINE(66);
                                    CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, Source_Risky(CS2SX_NN(CS2SX_NN(self->f_cs2sx_this)->f_source), (self->f_i == 0 ? 1 : -1))));
                                    CS2SX_LINE(66);
                                    if (!CS2SX_NN(self->f_cs2sx_a1)->base.f_IsCompleted)
                                    {
                                        CS2SX_LINE(66);
                                        CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)self);
                                        CS2SX_LINE(66);
                                        self->f_cs2sx_state = 1; CS2SX_ExPop(&_exf_2); CS2SX_ExPop(&_exf_1); return; _cs2sx_r1_2: self->f_cs2sx_state = 0;
                                    }
                                    CS2SX_LINE(66);
                                    self->f_steps += CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a1));
                                }
                                CS2SX_ExPop(&_exf_2);
                            }
                            else
                            {
                                Exception* _exf_2_ex = _exf_2.ex;
                                _exf_2_pending = _exf_2_ex;
                            }
                            {
                                CS2SX_LINE(68);
                                self->f_steps += 100;
                            }
                            if (_exf_2_pending) CS2SX_Throw(_exf_2_pending);
                        }
                    }
                    CS2SX_ExPop(&_exf_1);
                }
                else
                {
                    Exception* _exf_1_ex = _exf_1.ex;
                    if (CS2SX_Exc_Is(_exf_1_ex, &CS2SX_TI_ArgumentException))
                    {
                        CS2SX_LINE(70);
                        self->f_steps += 1000;
                        Exception_Free(_exf_1_ex);
                    }
                    else CS2SX_Throw(_exf_1_ex);
                }
            }
        }
    }
    {
        CS2SX_LINE(72);
        CS2SX_Task_int_SetResult(CS2SX_NN(self->f_cs2sx_task), self->f_steps);
        CS2SX_LINE(72);
        return;
    }
    CS2SX_LINE(73);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

void Nested_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Nested_Async* self = (Nested_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Nested_Async_Dispose(Nested_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_int_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_int_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Nested_Async_Free(Nested_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Nested_Async* Nested_Async_Retain(Nested_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Nested_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Nested_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Drain_Async_vtable_instance;

void Drain_Async_Ctor(Drain_Async* self)
{
    CS2SX_TRACE("Drain_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Drain_Async* Drain_Async_New(void)
{
    Drain_Async* self = (Drain_Async*)calloc(1, sizeof(Drain_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Drain_Async_Dispose;
    self->base._gc = Drain_Async_Gc;
    self->base.vtable = &Drain_Async_vtable_instance;
    Drain_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Drain_Async_Free);
    return self;
}

void Drain_Async_cs2sx_resume(Drain_Async* self)
{
    CS2SX_TRACE("Drain_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(76);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(76);
            Drain_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(76);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Drain_Async_cs2sx_run(Drain_Async* self)
{
    CS2SX_TRACE("Drain_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(76);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    CS2SX_LINE(77);
    self->f_rounds = 0;
    CS2SX_LINE(78);
    while (1)
    {
        CS2SX_LINE(78);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, Source_Next(CS2SX_NN(CS2SX_NN(self->f_cs2sx_this)->f_source))));
        CS2SX_LINE(78);
        if (!CS2SX_NN(self->f_cs2sx_a1)->base.f_IsCompleted)
        {
            CS2SX_LINE(78);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)self);
            CS2SX_LINE(78);
            self->f_cs2sx_state = 1; return; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(78);
        if (!(CS2SX_Task_bool_get_Result(CS2SX_NN(self->f_cs2sx_a1))))
        {
            CS2SX_LINE(78);
            break;
        }
        {
            CS2SX_LINE(80);
            self->f_rounds++;
            CS2SX_LINE(81);
            if (self->f_rounds > 10)
            {
                CS2SX_LINE(81);
                break;
            }
        }
    }
    {
        CS2SX_LINE(83);
        CS2SX_Task_int_SetResult(CS2SX_NN(self->f_cs2sx_task), self->f_rounds);
        CS2SX_LINE(83);
        return;
    }
    CS2SX_LINE(84);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

void Drain_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Drain_Async* self = (Drain_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Drain_Async_Dispose(Drain_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_int_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_bool_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Drain_Async_Free(Drain_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Drain_Async* Drain_Async_Retain(Drain_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Drain_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Drain_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Lost_Async_vtable_instance;

void Lost_Async_Ctor(Lost_Async* self)
{
    CS2SX_TRACE("Lost_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Lost_Async* Lost_Async_New(void)
{
    Lost_Async* self = (Lost_Async*)calloc(1, sizeof(Lost_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Lost_Async_Dispose;
    self->base._gc = Lost_Async_Gc;
    self->base.vtable = &Lost_Async_vtable_instance;
    Lost_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Lost_Async_Free);
    return self;
}

void Lost_Async_cs2sx_resume(Lost_Async* self)
{
    CS2SX_TRACE("Lost_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(87);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(87);
            Lost_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(87);
                CS2SX_Task_Fail(CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Lost_Async_cs2sx_run(Lost_Async* self)
{
    CS2SX_TRACE("Lost_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(87);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    {
        CS2SX_LINE(88);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
        CS2SX_LINE(88);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(88);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(88);
            self->f_cs2sx_state = 1; return; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(88);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    CS2SX_LINE(89);
    CS2SX_Throw((Exception*)(CS2SX_Exc_New(&CS2SX_TI_InvalidOperationException, "nobody waits for this", NULL, NULL)));
    CS2SX_LINE(90);
    CS2SX_Task_Finish(CS2SX_NN(self->f_cs2sx_task));
}

void Lost_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Lost_Async* self = (Lost_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Lost_Async_Dispose(Lost_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Lost_Async_Free(Lost_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Lost_Async* Lost_Async_Retain(Lost_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Lost_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Lost_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Script_Async_vtable_instance;

void Script_Async_Ctor(Script_Async* self)
{
    CS2SX_TRACE("Script_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Script_Async* Script_Async_New(void)
{
    Script_Async* self = (Script_Async*)calloc(1, sizeof(Script_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Script_Async_Dispose;
    self->base._gc = Script_Async_Gc;
    self->base.vtable = &Script_Async_vtable_instance;
    Script_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Script_Async_Free);
    return self;
}

void Script_Async_cs2sx_resume(Script_Async* self)
{
    CS2SX_TRACE("Script_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(93);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(93);
            Script_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(93);
                CS2SX_Task_Fail(CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Script_Async_cs2sx_run(Script_Async* self)
{
    CS2SX_TRACE("Script_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(93);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; case 2: goto _cs2sx_r2_0; case 3: goto _cs2sx_r3_0; case 4: goto _cs2sx_r4_0; case 5: goto _cs2sx_r5_0; default: break; };
    {
        CS2SX_LINE(94);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, AsyncFlowApp_Guarded(CS2SX_NN(self->f_cs2sx_this), 4)));
        CS2SX_LINE(94);
        if (!CS2SX_NN(self->f_cs2sx_a1)->base.f_IsCompleted)
        {
            CS2SX_LINE(94);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)self);
            CS2SX_LINE(94);
            self->f_cs2sx_state = 1; return; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(94);
        const char* _ps_1 = CS2SX_Task_str_get_Result(CS2SX_NN(self->f_cs2sx_a1));
        printf("%s\n", _ps_1 ? _ps_1 : "");
    }
    {
        CS2SX_LINE(95);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a2, CS2SX_RETAIN(CS2SX_Task, AsyncFlowApp_Guarded(CS2SX_NN(self->f_cs2sx_this), -1)));
        CS2SX_LINE(95);
        if (!CS2SX_NN(self->f_cs2sx_a2)->base.f_IsCompleted)
        {
            CS2SX_LINE(95);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a2), (CS2SX_Step*)self);
            CS2SX_LINE(95);
            self->f_cs2sx_state = 2; return; _cs2sx_r2_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(95);
        const char* _ps_2 = CS2SX_Task_str_get_Result(CS2SX_NN(self->f_cs2sx_a2));
        printf("%s\n", _ps_2 ? _ps_2 : "");
    }
    {
        CS2SX_LINE(96);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a3, CS2SX_RETAIN(CS2SX_Task, AsyncFlowApp_Nested(CS2SX_NN(self->f_cs2sx_this))));
        CS2SX_LINE(96);
        if (!CS2SX_NN(self->f_cs2sx_a3)->base.f_IsCompleted)
        {
            CS2SX_LINE(96);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a3), (CS2SX_Step*)self);
            CS2SX_LINE(96);
            self->f_cs2sx_state = 3; return; _cs2sx_r3_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(96);
        char* _sb0 = _cs2sx_next_buf();
        _sb0 = CS2SX_Format(_sb0, "nested %d", CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a3)));
        const char* _ps_3 = _sb0;
        printf("%s\n", _ps_3 ? _ps_3 : "");
    }
    {
        CS2SX_LINE(97);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a4, CS2SX_RETAIN(CS2SX_Task, AsyncFlowApp_Drain(CS2SX_NN(self->f_cs2sx_this))));
        CS2SX_LINE(97);
        if (!CS2SX_NN(self->f_cs2sx_a4)->base.f_IsCompleted)
        {
            CS2SX_LINE(97);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a4), (CS2SX_Step*)self);
            CS2SX_LINE(97);
            self->f_cs2sx_state = 4; return; _cs2sx_r4_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(97);
        char* _sb1 = _cs2sx_next_buf();
        _sb1 = CS2SX_Format(_sb1, "drain %d %d", CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a4)), Source_get_Total(CS2SX_NN(CS2SX_NN(self->f_cs2sx_this)->f_source)));
        const char* _ps_4 = _sb1;
        printf("%s\n", _ps_4 ? _ps_4 : "");
    }
    CS2SX_LINE(99);
    CS2SX_ARC_STORE(CS2SX_Task, self->f_slow, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_DelaySpan(CS2SX_TimeSpan_FromMs(600))));
    CS2SX_LINE(100);
    CS2SX_ARC_STORE(CS2SX_Task, self->f_fast, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Delay(20)));
    {
        CS2SX_LINE(101);
        CS2SX_ARR_LOCAL(CS2SX_Task*, _pa_5, 2, self->f_slow, self->f_fast);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a5, CS2SX_RETAIN(CS2SX_Task, CS2SX_WhenAny_Of(_pa_5)));
        CS2SX_LINE(101);
        if (!CS2SX_NN(self->f_cs2sx_a5)->base.f_IsCompleted)
        {
            CS2SX_LINE(101);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a5), (CS2SX_Step*)self);
            CS2SX_LINE(101);
            self->f_cs2sx_state = 5; return; _cs2sx_r5_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(101);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_first, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_CS2SX_Task_get_Result(CS2SX_NN(self->f_cs2sx_a5))));
    }
    CS2SX_LINE(102);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "any %s %s", CS2SX_BoolStr((self->f_first == self->f_fast)), CS2SX_BoolStr(CS2SX_NN(self->f_slow)->f_IsCompleted));
    const char* _ps_6 = _sb2;
    printf("%s\n", _ps_6 ? _ps_6 : "");
    CS2SX_LINE(104);
    AsyncFlowApp_Lost(CS2SX_NN(self->f_cs2sx_this));
    CS2SX_LINE(105);
    CS2SX_NN(self->f_cs2sx_this)->f_finished = 1;
    CS2SX_LINE(106);
    CS2SX_Task_Finish(CS2SX_NN(self->f_cs2sx_task));
}

void Script_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Script_Async* self = (Script_Async*)_self; (void)self;
    if (visit) visit(self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_slow, &CS2SX_Task_GcKind);
    else if (self->f_slow) { void* _o = self->f_slow; self->f_slow = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_fast, &CS2SX_Task_GcKind);
    else if (self->f_fast) { void* _o = self->f_fast; self->f_fast = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit(self->f_first, &CS2SX_Task_GcKind);
    else if (self->f_first) { void* _o = self->f_first; self->f_first = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
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

void Script_Async_Dispose(Script_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_slow) { CS2SX_Task_Free(self->f_slow); self->f_slow = NULL; }
    if (self->f_fast) { CS2SX_Task_Free(self->f_fast); self->f_fast = NULL; }
    if (self->f_first) { CS2SX_Task_Free(self->f_first); self->f_first = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_str_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    if (self->f_cs2sx_a2) { CS2SX_Task_str_Free(self->f_cs2sx_a2); self->f_cs2sx_a2 = NULL; }
    if (self->f_cs2sx_a3) { CS2SX_Task_int_Free(self->f_cs2sx_a3); self->f_cs2sx_a3 = NULL; }
    if (self->f_cs2sx_a4) { CS2SX_Task_int_Free(self->f_cs2sx_a4); self->f_cs2sx_a4 = NULL; }
    if (self->f_cs2sx_a5) { CS2SX_Task_CS2SX_Task_Free(self->f_cs2sx_a5); self->f_cs2sx_a5 = NULL; }
    CS2SX_Step_Dispose((CS2SX_Step*)self);
}

void Script_Async_Free(Script_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Script_Async* Script_Async_Retain(Script_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Script_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Script_Async_cs2sx_resume,
};
