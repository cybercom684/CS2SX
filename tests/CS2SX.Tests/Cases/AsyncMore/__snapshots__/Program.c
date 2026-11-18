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
    self->_dtor = (void (*)(void*))CS2SX_Step__Destroy;
    self->_gc = CS2SX_Step_Gc;
    self->_class = &CS2SX_Step_ClassInfo;
    self->vtable = &CS2SX_Step_vtable_instance;
    CS2SX_Step_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_Step_Free);
    return self;
}

const CS2SX_ClassInfo CS2SX_Step_ClassInfo = { "CS2SX_Step", NULL };

void CS2SX_Step_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Step* self = (CS2SX_Step*)_self; (void)self;
}

static void CS2SX_Step_GcDyn(void* self, CS2SX_GcVisit visit) { ((CS2SX_Step*)self)->_gc(self, visit); }
const CS2SX_GcKind CS2SX_Step_GcKind = { offsetof(CS2SX_Step, _rc), CS2SX_Step_GcDyn, (CS2SX_FreeFn)CS2SX_Step_Free };

void CS2SX_Step__Destroy(CS2SX_Step* self)
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
    self->_dtor = (void (*)(void*))CS2SX_Task__Destroy;
    self->_gc = CS2SX_Task_Gc;
    self->_class = &CS2SX_Task_ClassInfo;
    CS2SX_Task_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_Task_Free);
    return self;
}

void CS2SX_Task_Then(CS2SX_Task* self, CS2SX_Step* step)
{
    CS2SX_TRACE("CS2SX_Task.Then", "Program.cs");
    CS2SX_LINE(8);
    self->f_Observed = 1;
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
    self->f_Observed = 1;
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

CS2SX_Task* CS2SX_Task_DelaySpan(CS2SX_TimeSpan time)
{
    CS2SX_TRACE("CS2SX_Task.DelaySpan", "Program.cs");
    CS2SX_LINE(8);
    return CS2SX_Task_Delay(((int)CS2SX_TimeSpan_TotalMs(time)));
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

const CS2SX_ClassInfo CS2SX_Task_ClassInfo = { "CS2SX_Task", NULL };

void CS2SX_Task_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Task* self = (CS2SX_Task*)_self; (void)self;
    if (visit) visit((void*)self->f_waiting, &List_CS2SX_Step_GcKind);
    else if (self->f_waiting) { void* _o = (void*)self->f_waiting; self->f_waiting = NULL; ((void (*)(void*))List_CS2SX_Step_Free)(_o); }
}

static void CS2SX_Task_GcDyn(void* self, CS2SX_GcVisit visit) { ((CS2SX_Task*)self)->_gc(self, visit); }
const CS2SX_GcKind CS2SX_Task_GcKind = { offsetof(CS2SX_Task, _rc), CS2SX_Task_GcDyn, (CS2SX_FreeFn)CS2SX_Task_Free };

void CS2SX_Task__Destroy(CS2SX_Task* self)
{
    CS2SX_LINE(8);
    if (self->f_Fault != NULL && !self->f_Observed)
    {
        CS2SX_LINE(8);
        char* _sb0 = _cs2sx_next_buf();
        _sb0 = CS2SX_Format(_sb0, "Unobserved task exception: %s\n%s", ((Exception*)(self->f_Fault))->f_Message, (((Exception*)(self->f_Fault))->f_StackTrace ? ((Exception*)(self->f_Fault))->f_StackTrace : ""));
        const char* _ps_1 = _sb0;
        printf("%s\n", _ps_1 ? _ps_1 : "");
    }
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
    self->base._dtor = (void (*)(void*))CS2SX_WhenAll__Destroy;
    self->base._gc = CS2SX_WhenAll_Gc;
    self->base._class = &CS2SX_WhenAll_ClassInfo;
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
            CS2SX_NN(List_CS2SX_Task_Get(self->f_Tasks, i))->f_Observed = 1;
            CS2SX_LINE(8);
            CS2SX_Task_Fail(CS2SX_NN(self->f_Done), failed);
            CS2SX_LINE(8);
            return;
        }
    }
    CS2SX_LINE(8);
    CS2SX_Task_Finish(CS2SX_NN(self->f_Done));
}

const CS2SX_ClassInfo CS2SX_WhenAll_ClassInfo = { "CS2SX_WhenAll", &CS2SX_Step_ClassInfo };

void CS2SX_WhenAll_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_WhenAll* self = (CS2SX_WhenAll*)_self; (void)self;
    if (visit) visit((void*)self->f_Tasks, &List_CS2SX_Task_GcKind);
    else if (self->f_Tasks) { void* _o = (void*)self->f_Tasks; self->f_Tasks = NULL; ((void (*)(void*))List_CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_Done, &CS2SX_Task_GcKind);
    else if (self->f_Done) { void* _o = (void*)self->f_Done; self->f_Done = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void CS2SX_WhenAll__Destroy(CS2SX_WhenAll* self)
{
    if (self->f_Tasks) { List_CS2SX_Task_Free(self->f_Tasks); self->f_Tasks = NULL; }
    if (self->f_Done) { CS2SX_Task_Free(self->f_Done); self->f_Done = NULL; }
    CS2SX_Step__Destroy((CS2SX_Step*)self);
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

static CS2SX_Step_vtable CS2SX_WhenAll_int_vtable_instance;

void CS2SX_WhenAll_int_Ctor(CS2SX_WhenAll_int* self)
{
    CS2SX_TRACE("CS2SX_WhenAll_int..ctor", "Program.cs");
    self->f_Tasks = List_CS2SX_Task_int_Retain(List_CS2SX_Task_int_New());
    self->f_Done = CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_intArr_New());
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

CS2SX_WhenAll_int* CS2SX_WhenAll_int_New(void)
{
    CS2SX_WhenAll_int* self = (CS2SX_WhenAll_int*)calloc(1, sizeof(CS2SX_WhenAll_int));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))CS2SX_WhenAll_int__Destroy;
    self->base._gc = CS2SX_WhenAll_int_Gc;
    self->base._class = &CS2SX_WhenAll_int_ClassInfo;
    self->base.vtable = &CS2SX_WhenAll_int_vtable_instance;
    CS2SX_WhenAll_int_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_WhenAll_int_Free);
    return self;
}

CS2SX_Task_intArr* CS2SX_WhenAll_int_Of(CS2SX_Task_int** tasks)
{
    CS2SX_TRACE("CS2SX_WhenAll_int.Of", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_WhenAll_int* all = CS2SX_WhenAll_int_New();
    CS2SX_LINE(8);
    for (int i = 0; i < CS2SX_ArrLen(CS2SX_NN(tasks)); i++)
    {
        CS2SX_LINE(8);
        List_CS2SX_Task_int_Add(all->f_Tasks, CS2SX_AT(tasks, i));
    }
    CS2SX_LINE(8);
    all->base.vtable->cs2sx_resume(all);
    CS2SX_LINE(8);
    return all->f_Done;
}

CS2SX_Task_intArr* CS2SX_WhenAll_int_OfList(List_CS2SX_Task_int* tasks)
{
    CS2SX_TRACE("CS2SX_WhenAll_int.OfList", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_WhenAll_int* all = CS2SX_WhenAll_int_New();
    CS2SX_LINE(8);
    for (int i = 0; i < CS2SX_NN(tasks)->count; i++)
    {
        CS2SX_LINE(8);
        List_CS2SX_Task_int_Add(all->f_Tasks, List_CS2SX_Task_int_Get(tasks, i));
    }
    CS2SX_LINE(8);
    all->base.vtable->cs2sx_resume(all);
    CS2SX_LINE(8);
    return all->f_Done;
}

void CS2SX_WhenAll_int_cs2sx_resume(CS2SX_WhenAll_int* self)
{
    CS2SX_TRACE("CS2SX_WhenAll_int.cs2sx_resume", "Program.cs");
    CS2SX_LINE(8);
    while (self->f_Next < CS2SX_NN(self->f_Tasks)->count)
    {
        CS2SX_LINE(8);
        CS2SX_Task_int* t = List_CS2SX_Task_int_Get(self->f_Tasks, self->f_Next);
        CS2SX_LINE(8);
        if (!CS2SX_NN(t)->base.f_IsCompleted)
        {
            CS2SX_LINE(8);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(t), (CS2SX_Step*)self);
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
        Exception* failed = CS2SX_NN(List_CS2SX_Task_int_Get(self->f_Tasks, i))->base.f_Fault;
        CS2SX_LINE(8);
        if (failed != NULL)
        {
            CS2SX_LINE(8);
            CS2SX_NN(List_CS2SX_Task_int_Get(self->f_Tasks, i))->base.f_Observed = 1;
            CS2SX_LINE(8);
            CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_Done), failed);
            CS2SX_LINE(8);
            return;
        }
    }
    CS2SX_LINE(8);
    int* results = (int*)CS2SX_ArrNew(CS2SX_NN(self->f_Tasks)->count, sizeof(int));
    CS2SX_LINE(8);
    for (int i = 0; i < CS2SX_NN(self->f_Tasks)->count; i++)
    {
        CS2SX_LINE(8);
        CS2SX_AT(results, i) = CS2SX_NN(List_CS2SX_Task_int_Get(self->f_Tasks, i))->f_Value;
    }
    CS2SX_LINE(8);
    CS2SX_Task_intArr_SetResult(CS2SX_NN(self->f_Done), results);
}

const CS2SX_ClassInfo CS2SX_WhenAll_int_ClassInfo = { "CS2SX_WhenAll_int", &CS2SX_Step_ClassInfo };

void CS2SX_WhenAll_int_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_WhenAll_int* self = (CS2SX_WhenAll_int*)_self; (void)self;
    if (visit) visit((void*)self->f_Tasks, &List_CS2SX_Task_int_GcKind);
    else if (self->f_Tasks) { void* _o = (void*)self->f_Tasks; self->f_Tasks = NULL; ((void (*)(void*))List_CS2SX_Task_int_Free)(_o); }
    if (visit) visit((void*)self->f_Done, &CS2SX_Task_GcKind);
    else if (self->f_Done) { void* _o = (void*)self->f_Done; self->f_Done = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void CS2SX_WhenAll_int__Destroy(CS2SX_WhenAll_int* self)
{
    if (self->f_Tasks) { List_CS2SX_Task_int_Free(self->f_Tasks); self->f_Tasks = NULL; }
    if (self->f_Done) { CS2SX_Task_intArr_Free(self->f_Done); self->f_Done = NULL; }
    CS2SX_Step__Destroy((CS2SX_Step*)self);
}

void CS2SX_WhenAll_int_Free(CS2SX_WhenAll_int* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

CS2SX_WhenAll_int* CS2SX_WhenAll_int_Retain(CS2SX_WhenAll_int* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable CS2SX_WhenAll_int_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))CS2SX_WhenAll_int_cs2sx_resume,
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
    self->base._dtor = (void (*)(void*))CS2SX_Task_int__Destroy;
    self->base._gc = CS2SX_Task_int_Gc;
    self->base._class = &CS2SX_Task_int_ClassInfo;
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

const CS2SX_ClassInfo CS2SX_Task_int_ClassInfo = { "CS2SX_Task_int", &CS2SX_Task_ClassInfo };

void CS2SX_Task_int_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Task_int* self = (CS2SX_Task_int*)_self; (void)self;
    CS2SX_Task_Gc(_self, visit);
}

void CS2SX_Task_int__Destroy(CS2SX_Task_int* self)
{
    CS2SX_Task__Destroy((CS2SX_Task*)self);
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
    CS2SX_LINE(8);
    CS2SX_Task_Check((CS2SX_Task*)self);
    CS2SX_LINE(8);
    return self->f_Value;
}

void CS2SX_Task_intArr_Ctor(CS2SX_Task_intArr* self)
{
    CS2SX_TRACE("CS2SX_Task_intArr..ctor", "Program.cs");
    CS2SX_Task_Ctor((CS2SX_Task*)self);
}

CS2SX_Task_intArr* CS2SX_Task_intArr_New(void)
{
    CS2SX_Task_intArr* self = (CS2SX_Task_intArr*)calloc(1, sizeof(CS2SX_Task_intArr));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))CS2SX_Task_intArr__Destroy;
    self->base._gc = CS2SX_Task_intArr_Gc;
    self->base._class = &CS2SX_Task_intArr_ClassInfo;
    CS2SX_Task_intArr_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_Task_intArr_Free);
    return self;
}

void CS2SX_Task_intArr_SetResult(CS2SX_Task_intArr* self, int* v)
{
    CS2SX_TRACE("CS2SX_Task_intArr.SetResult", "Program.cs");
    CS2SX_LINE(8);
    self->f_Value = v;
    CS2SX_LINE(8);
    CS2SX_Task_Finish((CS2SX_Task*)self);
}

CS2SX_Task_intArr* CS2SX_Task_intArr_From(int* v)
{
    CS2SX_TRACE("CS2SX_Task_intArr.From", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_Task_intArr* t = CS2SX_Task_intArr_New();
    CS2SX_LINE(8);
    t->f_Value = v;
    CS2SX_LINE(8);
    t->base.f_IsCompleted = 1;
    CS2SX_LINE(8);
    return t;
}

CS2SX_Task_intArr* CS2SX_Task_intArr_Run(Func_intptr_t work)
{
    CS2SX_TRACE("CS2SX_Task_intArr.Run", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_Task_intArr* t = CS2SX_Task_intArr_New();
    CS2SX_LINE(8);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(8);
            CS2SX_Task_intArr_SetResult(t, Func_intptr_Invoke(work));
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

const CS2SX_ClassInfo CS2SX_Task_intArr_ClassInfo = { "CS2SX_Task_intArr", &CS2SX_Task_ClassInfo };

void CS2SX_Task_intArr_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Task_intArr* self = (CS2SX_Task_intArr*)_self; (void)self;
    CS2SX_Task_Gc(_self, visit);
}

void CS2SX_Task_intArr__Destroy(CS2SX_Task_intArr* self)
{
    CS2SX_ArrFree(self->f_Value); self->f_Value = NULL;
    CS2SX_Task__Destroy((CS2SX_Task*)self);
}

void CS2SX_Task_intArr_Free(CS2SX_Task_intArr* self)
{
    if (!self) return;
    CS2SX_Task* _o = (CS2SX_Task*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Task_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

CS2SX_Task_intArr* CS2SX_Task_intArr_Retain(CS2SX_Task_intArr* self)
{
    if (self) ((CS2SX_Task*)self)->_rc++;
    return self;
}

int* CS2SX_Task_intArr_get_Result(CS2SX_Task_intArr* self)
{
    CS2SX_TRACE("CS2SX_Task_intArr.get_Result", "Program.cs");
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
    self->base._dtor = (void (*)(void*))CS2SX_Task_str__Destroy;
    self->base._gc = CS2SX_Task_str_Gc;
    self->base._class = &CS2SX_Task_str_ClassInfo;
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

const CS2SX_ClassInfo CS2SX_Task_str_ClassInfo = { "CS2SX_Task_str", &CS2SX_Task_ClassInfo };

void CS2SX_Task_str_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Task_str* self = (CS2SX_Task_str*)_self; (void)self;
    CS2SX_Task_Gc(_self, visit);
}

void CS2SX_Task_str__Destroy(CS2SX_Task_str* self)
{
    CS2SX_StrDrop(self->f_Value); self->f_Value = NULL;
    CS2SX_Task__Destroy((CS2SX_Task*)self);
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
    CS2SX_LINE(8);
    CS2SX_Task_Check((CS2SX_Task*)self);
    CS2SX_LINE(8);
    return self->f_Value;
}

int Resource_Open;
void Resource_Ctor(Resource* self, const char* name)
{
    CS2SX_TRACE("Resource..ctor", "Program.cs");
    CS2SX_LINE(12);
    CS2SX_STR_STORE(self->f_Name, name);
    CS2SX_LINE(12);
    Resource_Open++;
}

Resource* Resource_New(const char* name)
{
    Resource* self = (Resource*)calloc(1, sizeof(Resource));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Resource__Destroy;
    self->_gc = Resource_Gc;
    self->_class = &Resource_ClassInfo;
    Resource_Ctor(self, name);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Resource_Free);
    return self;
}

void Resource_Dispose(Resource* self)
{
    CS2SX_TRACE("Resource.Dispose", "Program.cs");
    CS2SX_LINE(13);
    Resource_Open--;
    CS2SX_LINE(13);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "dispose %s", self->f_Name);
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
}

const CS2SX_ClassInfo Resource_ClassInfo = { "Resource", NULL };

void Resource_Gc(void* _self, CS2SX_GcVisit visit)
{
    Resource* self = (Resource*)_self; (void)self;
}

static void Resource_GcDyn(void* self, CS2SX_GcVisit visit) { ((Resource*)self)->_gc(self, visit); }
const CS2SX_GcKind Resource_GcKind = { offsetof(Resource, _rc), Resource_GcDyn, (CS2SX_FreeFn)Resource_Free };

void Resource__Destroy(Resource* self)
{
    CS2SX_StrDrop(self->f_Name); self->f_Name = NULL;
}

void Resource_Free(Resource* self)
{
    if (!self) return;
    Resource* _o = (Resource*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Resource_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Resource* Resource_Retain(Resource* self)
{
    if (self) ((Resource*)self)->_rc++;
    return self;
}

void AsyncMoreApp_Init(AsyncMoreApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))AsyncMoreApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))AsyncMoreApp_OnFrame;
}

CS2SX_Task_int* AsyncMoreApp_Value(int v)
{
    CS2SX_TRACE("AsyncMoreApp.Value", "Program.cs");
    CS2SX_LINE(23);
    Value_Async* cs2sx_sm = Value_Async_New();
    CS2SX_LINE(23);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_int_New()));
    CS2SX_LINE(23);
    cs2sx_sm->f_v = v;
    CS2SX_LINE(23);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(23);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task_str* AsyncMoreApp_Name()
{
    CS2SX_TRACE("AsyncMoreApp.Name", "Program.cs");
    CS2SX_LINE(29);
    Name_Async* cs2sx_sm = Name_Async_New();
    CS2SX_LINE(29);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_str_New()));
    CS2SX_LINE(29);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(29);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task* AsyncMoreApp_Fail(const char* msg)
{
    CS2SX_TRACE("AsyncMoreApp.Fail", "Program.cs");
    CS2SX_LINE(35);
    Fail_Async* cs2sx_sm = Fail_Async_New();
    CS2SX_LINE(35);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_New()));
    CS2SX_LINE(35);
    CS2SX_STR_STORE(cs2sx_sm->f_msg, msg);
    CS2SX_LINE(35);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(35);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task_str* AsyncMoreApp_Guarded(AsyncMoreApp* self)
{
    CS2SX_TRACE("AsyncMoreApp.Guarded", "Program.cs");
    CS2SX_LINE(41);
    Guarded_Async* cs2sx_sm = Guarded_Async_New();
    CS2SX_LINE(41);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(41);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_str_New()));
    CS2SX_LINE(41);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(41);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task_str* AsyncMoreApp_Rethrows(AsyncMoreApp* self)
{
    CS2SX_TRACE("AsyncMoreApp.Rethrows", "Program.cs");
    CS2SX_LINE(65);
    Rethrows_Async* cs2sx_sm = Rethrows_Async_New();
    CS2SX_LINE(65);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(65);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_str_New()));
    CS2SX_LINE(65);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(65);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task_int* AsyncMoreApp_Loops(AsyncMoreApp* self)
{
    CS2SX_TRACE("AsyncMoreApp.Loops", "Program.cs");
    CS2SX_LINE(80);
    Loops_Async* cs2sx_sm = Loops_Async_New();
    CS2SX_LINE(80);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(80);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_int_New()));
    CS2SX_LINE(80);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(80);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task* AsyncMoreApp_Script(AsyncMoreApp* self)
{
    CS2SX_TRACE("AsyncMoreApp.Script", "Program.cs");
    CS2SX_LINE(95);
    Script_Async* cs2sx_sm = Script_Async_New();
    CS2SX_LINE(95);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(95);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_New()));
    CS2SX_LINE(95);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(95);
    return cs2sx_sm->f_cs2sx_task;
}

void AsyncMoreApp_OnInit(AsyncMoreApp* self)
{
    CS2SX_TRACE("AsyncMoreApp.OnInit", "Program.cs");
    CS2SX_LINE(120);
    AsyncMoreApp_Script(self);
}

void AsyncMoreApp_OnFrame(AsyncMoreApp* self)
{
    CS2SX_TRACE("AsyncMoreApp.OnFrame", "Program.cs");
    CS2SX_LINE(124);
    self->f_ticks++;
    CS2SX_LINE(125);
    if (self->f_done)
    {
        CS2SX_LINE(125);
        self->f_done = 0;
        CS2SX_LINE(125);
        printf("done\n");
    }
}

CS2SX_Task_int* AsyncMoreApp___AsyncLambda1_3269(AsyncMoreApp* self, int x, int bonus)
{
    CS2SX_TRACE("AsyncMoreApp.__AsyncLambda1_3269", "Program.cs");
    CS2SX_LINE(127);
    __AsyncLambda1_3269_Async* cs2sx_sm = __AsyncLambda1_3269_Async_New();
    CS2SX_LINE(127);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(127);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_int_New()));
    CS2SX_LINE(127);
    cs2sx_sm->x = x;
    CS2SX_LINE(127);
    cs2sx_sm->f_bonus = bonus;
    CS2SX_LINE(127);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(127);
    return cs2sx_sm->f_cs2sx_task;
}

CS2SX_Task* AsyncMoreApp___AsyncLambda2_3418(AsyncMoreApp* self)
{
    CS2SX_TRACE("AsyncMoreApp.__AsyncLambda2_3418", "Program.cs");
    CS2SX_LINE(127);
    __AsyncLambda2_3418_Async* cs2sx_sm = __AsyncLambda2_3418_Async_New();
    CS2SX_LINE(127);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(127);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_New()));
    CS2SX_LINE(127);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
    CS2SX_LINE(127);
    return cs2sx_sm->f_cs2sx_task;
}

void AsyncMoreApp___AsyncLambda3_3601(AsyncMoreApp* self)
{
    CS2SX_TRACE("AsyncMoreApp.__AsyncLambda3_3601", "Program.cs");
    CS2SX_LINE(127);
    __AsyncLambda3_3601_Async* cs2sx_sm = __AsyncLambda3_3601_Async_New();
    CS2SX_LINE(127);
    cs2sx_sm->f_cs2sx_this = self;
    CS2SX_LINE(127);
    CS2SX_ARC_STORE(CS2SX_Task, cs2sx_sm->f_cs2sx_task, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_New()));
    CS2SX_LINE(127);
    cs2sx_sm->base.vtable->cs2sx_resume(cs2sx_sm);
}

void AsyncMoreApp_Free(AsyncMoreApp* self)
{
    if (!self) return;
    if (self->f_later) { CS2SX_Delegate_Release(self->f_later); self->f_later = NULL; }
    free(self);
}

static CS2SX_Step_vtable Value_Async_vtable_instance;

void Value_Async_Ctor(Value_Async* self)
{
    CS2SX_TRACE("Value_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Value_Async* Value_Async_New(void)
{
    Value_Async* self = (Value_Async*)calloc(1, sizeof(Value_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Value_Async__Destroy;
    self->base._gc = Value_Async_Gc;
    self->base._class = &Value_Async_ClassInfo;
    self->base.vtable = &Value_Async_vtable_instance;
    Value_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Value_Async_Free);
    return self;
}

void Value_Async_cs2sx_resume(Value_Async* self)
{
    CS2SX_TRACE("Value_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(23);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(23);
            Value_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(23);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Value_Async_cs2sx_run(Value_Async* self)
{
    CS2SX_TRACE("Value_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(23);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    {
        CS2SX_LINE(24);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
        CS2SX_LINE(24);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(24);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(24);
            self->f_cs2sx_state = 1; return; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(24);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    {
        CS2SX_LINE(25);
        CS2SX_Task_int_SetResult(CS2SX_NN(self->f_cs2sx_task), self->f_v);
        CS2SX_LINE(25);
        return;
    }
    CS2SX_LINE(26);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

const CS2SX_ClassInfo Value_Async_ClassInfo = { "Value_Async", &CS2SX_Step_ClassInfo };

void Value_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Value_Async* self = (Value_Async*)_self; (void)self;
    if (visit) visit((void*)self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = (void*)self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = (void*)self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Value_Async__Destroy(Value_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_int_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step__Destroy((CS2SX_Step*)self);
}

void Value_Async_Free(Value_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Value_Async* Value_Async_Retain(Value_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Value_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Value_Async_cs2sx_resume,
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
    self->base._dtor = (void (*)(void*))Name_Async__Destroy;
    self->base._gc = Name_Async_Gc;
    self->base._class = &Name_Async_ClassInfo;
    self->base.vtable = &Name_Async_vtable_instance;
    Name_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Name_Async_Free);
    return self;
}

void Name_Async_cs2sx_resume(Name_Async* self)
{
    CS2SX_TRACE("Name_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(29);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(29);
            Name_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(29);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Name_Async_cs2sx_run(Name_Async* self)
{
    CS2SX_TRACE("Name_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(29);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    {
        CS2SX_LINE(30);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
        CS2SX_LINE(30);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(30);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(30);
            self->f_cs2sx_state = 1; return; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(30);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    {
        CS2SX_LINE(31);
        CS2SX_Task_str_SetResult(CS2SX_NN(self->f_cs2sx_task), "name");
        CS2SX_LINE(31);
        return;
    }
    CS2SX_LINE(32);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

const CS2SX_ClassInfo Name_Async_ClassInfo = { "Name_Async", &CS2SX_Step_ClassInfo };

void Name_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Name_Async* self = (Name_Async*)_self; (void)self;
    if (visit) visit((void*)self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = (void*)self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = (void*)self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Name_Async__Destroy(Name_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_str_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step__Destroy((CS2SX_Step*)self);
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

static CS2SX_Step_vtable Fail_Async_vtable_instance;

void Fail_Async_Ctor(Fail_Async* self)
{
    CS2SX_TRACE("Fail_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Fail_Async* Fail_Async_New(void)
{
    Fail_Async* self = (Fail_Async*)calloc(1, sizeof(Fail_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Fail_Async__Destroy;
    self->base._gc = Fail_Async_Gc;
    self->base._class = &Fail_Async_ClassInfo;
    self->base.vtable = &Fail_Async_vtable_instance;
    Fail_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Fail_Async_Free);
    return self;
}

void Fail_Async_cs2sx_resume(Fail_Async* self)
{
    CS2SX_TRACE("Fail_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(35);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(35);
            Fail_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(35);
                CS2SX_Task_Fail(CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Fail_Async_cs2sx_run(Fail_Async* self)
{
    CS2SX_TRACE("Fail_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(35);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    {
        CS2SX_LINE(36);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
        CS2SX_LINE(36);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(36);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(36);
            self->f_cs2sx_state = 1; return; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(36);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    CS2SX_LINE(37);
    CS2SX_Throw((Exception*)(CS2SX_Exc_New(&CS2SX_TI_InvalidOperationException, self->f_msg, NULL, NULL)));
    CS2SX_LINE(38);
    CS2SX_Task_Finish(CS2SX_NN(self->f_cs2sx_task));
}

const CS2SX_ClassInfo Fail_Async_ClassInfo = { "Fail_Async", &CS2SX_Step_ClassInfo };

void Fail_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Fail_Async* self = (Fail_Async*)_self; (void)self;
    if (visit) visit((void*)self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = (void*)self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = (void*)self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Fail_Async__Destroy(Fail_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    CS2SX_StrDrop(self->f_msg); self->f_msg = NULL;
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step__Destroy((CS2SX_Step*)self);
}

void Fail_Async_Free(Fail_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Fail_Async* Fail_Async_Retain(Fail_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Fail_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Fail_Async_cs2sx_resume,
};

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
    self->base._dtor = (void (*)(void*))Guarded_Async__Destroy;
    self->base._gc = Guarded_Async_Gc;
    self->base._class = &Guarded_Async_ClassInfo;
    self->base.vtable = &Guarded_Async_vtable_instance;
    Guarded_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Guarded_Async_Free);
    return self;
}

void Guarded_Async_cs2sx_resume(Guarded_Async* self)
{
    CS2SX_TRACE("Guarded_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(41);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(41);
            Guarded_Async_cs2sx_run(self);
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

void Guarded_Async_cs2sx_run(Guarded_Async* self)
{
    CS2SX_TRACE("Guarded_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(41);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; case 2: goto _cs2sx_r2_0; case 3: goto _cs2sx_r3_0; default: break; };
    CS2SX_LINE(42);
    CS2SX_STR_STORE(self->f_log, "");
    CS2SX_LINE(43);
    self->f_fin1 = NULL;
    {
        CS2SX_LINE(43);
        _cs2sx_r1_0: (void)0;
        CS2SX_LINE(43);
        _cs2sx_r2_0: (void)0;
        CS2SX_LINE(43);
        {
            CS2SX_ExFrame _exf_1;
            CS2SX_ExPush(&_exf_1);
            if (setjmp(_exf_1.buf) == 0)
            {
                CS2SX_LINE(43);
                if (self->f_cs2sx_state == 1) goto _cs2sx_r1_1;
                CS2SX_LINE(43);
                if (self->f_cs2sx_state == 2) goto _cs2sx_r2_1;
                CS2SX_LINE(43);
                self->f_ex1 = NULL;
                {
                    CS2SX_LINE(43);
                    _cs2sx_r1_1: (void)0;
                    CS2SX_LINE(43);
                    {
                        CS2SX_ExFrame _exf_2;
                        CS2SX_ExPush(&_exf_2);
                        if (setjmp(_exf_2.buf) == 0)
                        {
                            CS2SX_LINE(44);
                            if (self->f_cs2sx_state == 1) goto _cs2sx_r1_2;
                            {
                                CS2SX_LINE(45);
                                CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, AsyncMoreApp_Fail("a")));
                                CS2SX_LINE(45);
                                if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
                                {
                                    CS2SX_LINE(45);
                                    CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
                                    CS2SX_LINE(45);
                                    self->f_cs2sx_state = 1; CS2SX_ExPop(&_exf_2); CS2SX_ExPop(&_exf_1); return; _cs2sx_r1_2: self->f_cs2sx_state = 0;
                                }
                                CS2SX_LINE(45);
                                CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
                            }
                            CS2SX_ExPop(&_exf_2);
                        }
                        else
                        {
                            Exception* _exf_2_ex = _exf_2.ex;
                            if (1)
                            {
                                Exception* __ex1__c = (Exception*)_exf_2_ex;
                                CS2SX_LINE(47);
                                self->f_ex1 = __ex1__c;
                            }
                        }
                    }
                }
                CS2SX_LINE(47);
                if (CS2SX_Exc_Is((Exception*)(self->f_ex1), &CS2SX_TI_ArgumentException))
                {
                    CS2SX_LINE(49);
                    char* _sb0 = _cs2sx_next_buf();
                    _sb0 = CS2SX_Format(_sb0, "%s%s", self->f_log ? self->f_log : "", "wrong " ? "wrong " : "");
                    CS2SX_STR_STORE(self->f_log, _sb0);
                }
                else if (CS2SX_Exc_Is((Exception*)(self->f_ex1), &CS2SX_TI_InvalidOperationException))
                {
                    CS2SX_LINE(52);
                    self->f_e = ((InvalidOperationException*)self->f_ex1);
                    {
                        CS2SX_LINE(53);
                        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a2, CS2SX_RETAIN(CS2SX_Task, AsyncMoreApp_Value(5)));
                        CS2SX_LINE(53);
                        if (!CS2SX_NN(self->f_cs2sx_a2)->base.f_IsCompleted)
                        {
                            CS2SX_LINE(53);
                            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a2), (CS2SX_Step*)self);
                            CS2SX_LINE(53);
                            self->f_cs2sx_state = 2; CS2SX_ExPop(&_exf_1); return; _cs2sx_r2_1: self->f_cs2sx_state = 0;
                        }
                        CS2SX_LINE(53);
                        self->f_v = CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a2));
                    }
                    CS2SX_LINE(54);
                    char* _sb1 = _cs2sx_next_buf();
                    _sb1 = CS2SX_Format(_sb1, "caught %s%d", ((Exception*)(self->f_e))->f_Message, self->f_v);
                    char* _sb2 = _cs2sx_next_buf();
                    _sb2 = CS2SX_Format(_sb2, "%s%s", self->f_log ? self->f_log : "", _sb1 ? _sb1 : "");
                    CS2SX_STR_STORE(self->f_log, _sb2);
                }
                else if (self->f_ex1 != NULL)
                {
                    CS2SX_LINE(55);
                    CS2SX_Throw(Exception_Retain((Exception*)(self->f_ex1)));
                }
                CS2SX_ExPop(&_exf_1);
            }
            else
            {
                Exception* _exf_1_ex = _exf_1.ex;
                if (1)
                {
                    Exception* __fin1__c = (Exception*)_exf_1_ex;
                    CS2SX_LINE(55);
                    self->f_fin1 = __fin1__c;
                }
            }
        }
    }
    {
        {
            CS2SX_LINE(58);
            CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a3, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
            CS2SX_LINE(58);
            if (!CS2SX_NN(self->f_cs2sx_a3)->f_IsCompleted)
            {
                CS2SX_LINE(58);
                CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a3), (CS2SX_Step*)(CS2SX_Step*)self);
                CS2SX_LINE(58);
                self->f_cs2sx_state = 3; return; _cs2sx_r3_0: self->f_cs2sx_state = 0;
            }
            CS2SX_LINE(58);
            CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a3));
        }
        CS2SX_LINE(59);
        char* _sb3 = _cs2sx_next_buf();
        _sb3 = CS2SX_Format(_sb3, "%s%s", self->f_log ? self->f_log : "", " finally" ? " finally" : "");
        CS2SX_STR_STORE(self->f_log, _sb3);
    }
    CS2SX_LINE(60);
    if (self->f_fin1 != NULL)
    {
        CS2SX_LINE(60);
        CS2SX_Throw(Exception_Retain((Exception*)(self->f_fin1)));
    }
    {
        CS2SX_LINE(61);
        CS2SX_Task_str_SetResult(CS2SX_NN(self->f_cs2sx_task), self->f_log);
        CS2SX_LINE(61);
        return;
    }
    CS2SX_LINE(62);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

const CS2SX_ClassInfo Guarded_Async_ClassInfo = { "Guarded_Async", &CS2SX_Step_ClassInfo };

void Guarded_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Guarded_Async* self = (Guarded_Async*)_self; (void)self;
    if (visit) visit((void*)self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = (void*)self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = (void*)self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a2, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a2) { void* _o = (void*)self->f_cs2sx_a2; self->f_cs2sx_a2 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a3, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a3) { void* _o = (void*)self->f_cs2sx_a3; self->f_cs2sx_a3 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Guarded_Async__Destroy(Guarded_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_str_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    CS2SX_StrDrop(self->f_log); self->f_log = NULL;
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    if (self->f_cs2sx_a2) { CS2SX_Task_int_Free(self->f_cs2sx_a2); self->f_cs2sx_a2 = NULL; }
    if (self->f_cs2sx_a3) { CS2SX_Task_Free(self->f_cs2sx_a3); self->f_cs2sx_a3 = NULL; }
    CS2SX_Step__Destroy((CS2SX_Step*)self);
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

static CS2SX_Step_vtable Rethrows_Async_vtable_instance;

void Rethrows_Async_Ctor(Rethrows_Async* self)
{
    CS2SX_TRACE("Rethrows_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Rethrows_Async* Rethrows_Async_New(void)
{
    Rethrows_Async* self = (Rethrows_Async*)calloc(1, sizeof(Rethrows_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Rethrows_Async__Destroy;
    self->base._gc = Rethrows_Async_Gc;
    self->base._class = &Rethrows_Async_ClassInfo;
    self->base.vtable = &Rethrows_Async_vtable_instance;
    Rethrows_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Rethrows_Async_Free);
    return self;
}

void Rethrows_Async_cs2sx_resume(Rethrows_Async* self)
{
    CS2SX_TRACE("Rethrows_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(65);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(65);
            Rethrows_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(65);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Rethrows_Async_cs2sx_run(Rethrows_Async* self)
{
    CS2SX_TRACE("Rethrows_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(65);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; case 2: goto _cs2sx_r2_0; default: break; };
    {
        CS2SX_LINE(66);
        _cs2sx_r1_0: (void)0;
        CS2SX_LINE(66);
        _cs2sx_r2_0: (void)0;
        CS2SX_LINE(66);
        {
            CS2SX_ExFrame _exf_1;
            CS2SX_ExPush(&_exf_1);
            if (setjmp(_exf_1.buf) == 0)
            {
                CS2SX_LINE(67);
                if (self->f_cs2sx_state == 1) goto _cs2sx_r1_1;
                CS2SX_LINE(67);
                if (self->f_cs2sx_state == 2) goto _cs2sx_r2_1;
                CS2SX_LINE(68);
                self->f_ex2 = NULL;
                {
                    CS2SX_LINE(68);
                    _cs2sx_r1_1: (void)0;
                    CS2SX_LINE(68);
                    {
                        CS2SX_ExFrame _exf_2;
                        CS2SX_ExPush(&_exf_2);
                        if (setjmp(_exf_2.buf) == 0)
                        {
                            CS2SX_LINE(68);
                            if (self->f_cs2sx_state == 1) goto _cs2sx_r1_2;
                            {
                                CS2SX_LINE(68);
                                CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, AsyncMoreApp_Fail("b")));
                                CS2SX_LINE(68);
                                if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
                                {
                                    CS2SX_LINE(68);
                                    CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
                                    CS2SX_LINE(68);
                                    self->f_cs2sx_state = 1; CS2SX_ExPop(&_exf_2); CS2SX_ExPop(&_exf_1); return; _cs2sx_r1_2: self->f_cs2sx_state = 0;
                                }
                                CS2SX_LINE(68);
                                CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
                            }
                            CS2SX_ExPop(&_exf_2);
                        }
                        else
                        {
                            Exception* _exf_2_ex = _exf_2.ex;
                            if (1)
                            {
                                Exception* __ex2__c = (Exception*)_exf_2_ex;
                                CS2SX_LINE(69);
                                self->f_ex2 = __ex2__c;
                            }
                        }
                    }
                }
                CS2SX_LINE(69);
                if (CS2SX_Exc_Is((Exception*)(self->f_ex2), &CS2SX_TI_InvalidOperationException))
                {
                    {
                        CS2SX_LINE(71);
                        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a2, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
                        CS2SX_LINE(71);
                        if (!CS2SX_NN(self->f_cs2sx_a2)->f_IsCompleted)
                        {
                            CS2SX_LINE(71);
                            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a2), (CS2SX_Step*)(CS2SX_Step*)self);
                            CS2SX_LINE(71);
                            self->f_cs2sx_state = 2; CS2SX_ExPop(&_exf_1); return; _cs2sx_r2_1: self->f_cs2sx_state = 0;
                        }
                        CS2SX_LINE(71);
                        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a2));
                    }
                    CS2SX_LINE(72);
                    CS2SX_Throw(Exception_Retain((Exception*)(self->f_ex2)));
                }
                else if (self->f_ex2 != NULL)
                {
                    CS2SX_LINE(73);
                    CS2SX_Throw(Exception_Retain((Exception*)(self->f_ex2)));
                }
                CS2SX_ExPop(&_exf_1);
            }
            else
            {
                Exception* _exf_1_ex = _exf_1.ex;
                if (1)
                {
                    Exception* e = (Exception*)_exf_1_ex;
                    {
                        CS2SX_LINE(75);
                        char* _sb0 = _cs2sx_next_buf();
                        _sb0 = CS2SX_Format(_sb0, "outer %s", ((Exception*)(e))->f_Message);
                        char* _sb1 = _cs2sx_next_buf();
                        _sb1 = CS2SX_Format(_sb1, "outer %s", ((Exception*)(e))->f_Message);
                        char* _sb2 = _cs2sx_next_buf();
                        _sb2 = CS2SX_Format(_sb2, "outer %s", ((Exception*)(e))->f_Message);
                        CS2SX_Task_str_SetResult(CS2SX_NN(self->f_cs2sx_task), _sb2);
                        CS2SX_LINE(75);
                        return;
                    }
                }
            }
        }
    }
    {
        CS2SX_LINE(76);
        CS2SX_Task_str_SetResult(CS2SX_NN(self->f_cs2sx_task), "none");
        CS2SX_LINE(76);
        return;
    }
    CS2SX_LINE(77);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

const CS2SX_ClassInfo Rethrows_Async_ClassInfo = { "Rethrows_Async", &CS2SX_Step_ClassInfo };

void Rethrows_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Rethrows_Async* self = (Rethrows_Async*)_self; (void)self;
    if (visit) visit((void*)self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = (void*)self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = (void*)self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a2, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a2) { void* _o = (void*)self->f_cs2sx_a2; self->f_cs2sx_a2 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Rethrows_Async__Destroy(Rethrows_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_str_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    if (self->f_cs2sx_a2) { CS2SX_Task_Free(self->f_cs2sx_a2); self->f_cs2sx_a2 = NULL; }
    CS2SX_Step__Destroy((CS2SX_Step*)self);
}

void Rethrows_Async_Free(Rethrows_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Rethrows_Async* Rethrows_Async_Retain(Rethrows_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Rethrows_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Rethrows_Async_cs2sx_resume,
};

static CS2SX_Step_vtable Loops_Async_vtable_instance;

void Loops_Async_Ctor(Loops_Async* self)
{
    CS2SX_TRACE("Loops_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

Loops_Async* Loops_Async_New(void)
{
    Loops_Async* self = (Loops_Async*)calloc(1, sizeof(Loops_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Loops_Async__Destroy;
    self->base._gc = Loops_Async_Gc;
    self->base._class = &Loops_Async_ClassInfo;
    self->base.vtable = &Loops_Async_vtable_instance;
    Loops_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Loops_Async_Free);
    return self;
}

void Loops_Async_cs2sx_resume(Loops_Async* self)
{
    CS2SX_TRACE("Loops_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(80);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(80);
            Loops_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(80);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void Loops_Async_cs2sx_run(Loops_Async* self)
{
    CS2SX_TRACE("Loops_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(80);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; case 2: goto _cs2sx_r2_0; case 3: goto _cs2sx_r3_0; case 4: goto _cs2sx_r4_0; default: break; };
    CS2SX_LINE(81);
    self->f_n = 0;
    CS2SX_LINE(82);
    while (1)
    {
        {
            CS2SX_LINE(82);
            self->f_n++;
        }
        {
            CS2SX_LINE(82);
            CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, AsyncMoreApp_Value(self->f_n)));
            CS2SX_LINE(82);
            if (!CS2SX_NN(self->f_cs2sx_a1)->base.f_IsCompleted)
            {
                CS2SX_LINE(82);
                CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)self);
                CS2SX_LINE(82);
                self->f_cs2sx_state = 1; return; _cs2sx_r1_0: self->f_cs2sx_state = 0;
            }
            CS2SX_LINE(82);
            if (!(CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a1)) < 3))
            {
                CS2SX_LINE(82);
                break;
            }
        }
    }
    CS2SX_LINE(83);
    self->f_sum = 0;
    {
        {
            CS2SX_LINE(84);
            CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a2, CS2SX_RETAIN(CS2SX_Task, AsyncMoreApp_Value(1)));
            CS2SX_LINE(84);
            if (!CS2SX_NN(self->f_cs2sx_a2)->base.f_IsCompleted)
            {
                CS2SX_LINE(84);
                CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a2), (CS2SX_Step*)self);
                CS2SX_LINE(84);
                self->f_cs2sx_state = 2; return; _cs2sx_r2_0: self->f_cs2sx_state = 0;
            }
            CS2SX_LINE(84);
            self->f_i = CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a2));
        }
        CS2SX_LINE(84);
        while (1)
        {
            {
                CS2SX_LINE(84);
                CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a3, CS2SX_RETAIN(CS2SX_Task, AsyncMoreApp_Value(3)));
                CS2SX_LINE(84);
                if (!CS2SX_NN(self->f_cs2sx_a3)->base.f_IsCompleted)
                {
                    CS2SX_LINE(84);
                    CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a3), (CS2SX_Step*)self);
                    CS2SX_LINE(84);
                    self->f_cs2sx_state = 3; return; _cs2sx_r3_0: self->f_cs2sx_state = 0;
                }
                CS2SX_LINE(84);
                if (!(self->f_i <= CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a3))))
                {
                    CS2SX_LINE(84);
                    break;
                }
            }
            CS2SX_LINE(85);
            self->f_sum += self->f_i;
            CS2SX_LINE(85);
            self->f_i++;
        }
    }
    {
        CS2SX_LINE(86);
        CS2SX_ARC_STORE(Resource, self->f_r, CS2SX_RETAIN(Resource, Resource_New("r1")));
        {
            CS2SX_LINE(86);
            _cs2sx_r4_0: (void)0;
            CS2SX_LINE(86);
            {
                Exception* _exf_1_pending = NULL;
                CS2SX_ExFrame _exf_1;
                CS2SX_ExPush(&_exf_1);
                if (setjmp(_exf_1.buf) == 0)
                {
                    CS2SX_LINE(87);
                    if (self->f_cs2sx_state == 4) goto _cs2sx_r4_1;
                    {
                        CS2SX_LINE(88);
                        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a4, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
                        CS2SX_LINE(88);
                        if (!CS2SX_NN(self->f_cs2sx_a4)->f_IsCompleted)
                        {
                            CS2SX_LINE(88);
                            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a4), (CS2SX_Step*)(CS2SX_Step*)self);
                            CS2SX_LINE(88);
                            self->f_cs2sx_state = 4; CS2SX_ExPop(&_exf_1); return; _cs2sx_r4_1: self->f_cs2sx_state = 0;
                        }
                        CS2SX_LINE(88);
                        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a4));
                    }
                    CS2SX_LINE(89);
                    self->f_sum += strlen(CS2SX_NN(CS2SX_NN(self->f_r)->f_Name));
                    CS2SX_ExPop(&_exf_1);
                }
                else
                {
                    Exception* _exf_1_ex = _exf_1.ex;
                    _exf_1_pending = _exf_1_ex;
                }
                {
                    CS2SX_LINE(90);
                    (self->f_r != NULL ? (void)(Resource_Dispose(self->f_r), self->f_r = NULL) : (void)0);
                }
                if (_exf_1_pending) CS2SX_Throw(_exf_1_pending);
            }
        }
    }
    {
        CS2SX_LINE(91);
        CS2SX_Task_int_SetResult(CS2SX_NN(self->f_cs2sx_task), self->f_n * 100 + self->f_sum);
        CS2SX_LINE(91);
        return;
    }
    CS2SX_LINE(92);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

const CS2SX_ClassInfo Loops_Async_ClassInfo = { "Loops_Async", &CS2SX_Step_ClassInfo };

void Loops_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Loops_Async* self = (Loops_Async*)_self; (void)self;
    if (visit) visit((void*)self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = (void*)self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_r, &Resource_GcKind);
    else if (self->f_r) { void* _o = (void*)self->f_r; self->f_r = NULL; ((void (*)(void*))Resource_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = (void*)self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a2, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a2) { void* _o = (void*)self->f_cs2sx_a2; self->f_cs2sx_a2 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a3, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a3) { void* _o = (void*)self->f_cs2sx_a3; self->f_cs2sx_a3 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a4, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a4) { void* _o = (void*)self->f_cs2sx_a4; self->f_cs2sx_a4 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Loops_Async__Destroy(Loops_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_int_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_r) { Resource_Free(self->f_r); self->f_r = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_int_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    if (self->f_cs2sx_a2) { CS2SX_Task_int_Free(self->f_cs2sx_a2); self->f_cs2sx_a2 = NULL; }
    if (self->f_cs2sx_a3) { CS2SX_Task_int_Free(self->f_cs2sx_a3); self->f_cs2sx_a3 = NULL; }
    if (self->f_cs2sx_a4) { CS2SX_Task_Free(self->f_cs2sx_a4); self->f_cs2sx_a4 = NULL; }
    CS2SX_Step__Destroy((CS2SX_Step*)self);
}

void Loops_Async_Free(Loops_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Loops_Async* Loops_Async_Retain(Loops_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable Loops_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))Loops_Async_cs2sx_resume,
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
    self->base._dtor = (void (*)(void*))Script_Async__Destroy;
    self->base._gc = Script_Async_Gc;
    self->base._class = &Script_Async_ClassInfo;
    self->base.vtable = &Script_Async_vtable_instance;
    Script_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Script_Async_Free);
    return self;
}

void Script_Async_cs2sx_resume(Script_Async* self)
{
    CS2SX_TRACE("Script_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(95);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(95);
            Script_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(95);
                CS2SX_Task_Fail(CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

struct _cap_0
{
    CS2SX_Env _hdr;
    Script_Async* self;
};

static CS2SX_Task_int* _lambda_0(void* _env, int x)
{
    CS2SX_TRACE("Script_Async.<lambda>", "Program.cs");
    struct _cap_0* _c = (struct _cap_0*)_env;
    Script_Async* self = _c->self;
    return AsyncMoreApp___AsyncLambda1_3269(CS2SX_NN(self->f_cs2sx_this), x, self->f_bonus);
}

static void _envdtor_0(void* _e)
{
    struct _cap_0* _c = (struct _cap_0*)_e;
    CS2SX_ReleaseLater((void*)_c->self, (CS2SX_FreeFn)CS2SX_Step_Free);
}

static void _envgc_0(void* _e, CS2SX_GcVisit visit)
{
    struct _cap_0* _c = (struct _cap_0*)_e;
    if (visit) visit((void*)_c->self, &CS2SX_Step_GcKind);
    else if (_c->self) { void* _o = (void*)_c->self; _c->self = NULL; ((CS2SX_FreeFn)CS2SX_Step_Free)(_o); }
    (void)_c; (void)visit;
}

struct _cap_1
{
    CS2SX_Env _hdr;
    Script_Async* self;
};

static CS2SX_Task* _lambda_1(void* _env)
{
    CS2SX_TRACE("Script_Async.<lambda>", "Program.cs");
    struct _cap_1* _c = (struct _cap_1*)_env;
    Script_Async* self = _c->self;
    return AsyncMoreApp___AsyncLambda2_3418(CS2SX_NN(self->f_cs2sx_this));
}

static void _envdtor_1(void* _e)
{
    struct _cap_1* _c = (struct _cap_1*)_e;
    CS2SX_ReleaseLater((void*)_c->self, (CS2SX_FreeFn)CS2SX_Step_Free);
}

static void _envgc_1(void* _e, CS2SX_GcVisit visit)
{
    struct _cap_1* _c = (struct _cap_1*)_e;
    if (visit) visit((void*)_c->self, &CS2SX_Step_GcKind);
    else if (_c->self) { void* _o = (void*)_c->self; _c->self = NULL; ((CS2SX_FreeFn)CS2SX_Step_Free)(_o); }
    (void)_c; (void)visit;
}

struct _cap_2
{
    CS2SX_Env _hdr;
    Script_Async* self;
};

static void _lambda_2(void* _env)
{
    CS2SX_TRACE("Script_Async.<lambda>", "Program.cs");
    struct _cap_2* _c = (struct _cap_2*)_env;
    Script_Async* self = _c->self;
    AsyncMoreApp___AsyncLambda3_3601(CS2SX_NN(self->f_cs2sx_this));
}

static void _envdtor_2(void* _e)
{
    struct _cap_2* _c = (struct _cap_2*)_e;
    CS2SX_ReleaseLater((void*)_c->self, (CS2SX_FreeFn)CS2SX_Step_Free);
}

static void _envgc_2(void* _e, CS2SX_GcVisit visit)
{
    struct _cap_2* _c = (struct _cap_2*)_e;
    if (visit) visit((void*)_c->self, &CS2SX_Step_GcKind);
    else if (_c->self) { void* _o = (void*)_c->self; _c->self = NULL; ((CS2SX_FreeFn)CS2SX_Step_Free)(_o); }
    (void)_c; (void)visit;
}

void Script_Async_cs2sx_run(Script_Async* self)
{
    CS2SX_TRACE("Script_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(95);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; case 2: goto _cs2sx_r2_0; case 3: goto _cs2sx_r3_0; case 4: goto _cs2sx_r4_0; case 5: goto _cs2sx_r5_0; case 6: goto _cs2sx_r6_0; case 7: goto _cs2sx_r7_0; case 8: goto _cs2sx_r8_0; case 9: goto _cs2sx_r9_0; default: break; };
    {
        CS2SX_LINE(96);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, AsyncMoreApp_Guarded(CS2SX_NN(self->f_cs2sx_this))));
        CS2SX_LINE(96);
        if (!CS2SX_NN(self->f_cs2sx_a1)->base.f_IsCompleted)
        {
            CS2SX_LINE(96);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)self);
            CS2SX_LINE(96);
            self->f_cs2sx_state = 1; return; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(96);
        const char* _ps_1 = CS2SX_Task_str_get_Result(CS2SX_NN(self->f_cs2sx_a1));
        printf("%s\n", _ps_1 ? _ps_1 : "");
    }
    {
        CS2SX_LINE(97);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a2, CS2SX_RETAIN(CS2SX_Task, AsyncMoreApp_Rethrows(CS2SX_NN(self->f_cs2sx_this))));
        CS2SX_LINE(97);
        if (!CS2SX_NN(self->f_cs2sx_a2)->base.f_IsCompleted)
        {
            CS2SX_LINE(97);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a2), (CS2SX_Step*)self);
            CS2SX_LINE(97);
            self->f_cs2sx_state = 2; return; _cs2sx_r2_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(97);
        const char* _ps_2 = CS2SX_Task_str_get_Result(CS2SX_NN(self->f_cs2sx_a2));
        printf("%s\n", _ps_2 ? _ps_2 : "");
    }
    {
        CS2SX_LINE(98);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a3, CS2SX_RETAIN(CS2SX_Task, AsyncMoreApp_Loops(CS2SX_NN(self->f_cs2sx_this))));
        CS2SX_LINE(98);
        if (!CS2SX_NN(self->f_cs2sx_a3)->base.f_IsCompleted)
        {
            CS2SX_LINE(98);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a3), (CS2SX_Step*)self);
            CS2SX_LINE(98);
            self->f_cs2sx_state = 3; return; _cs2sx_r3_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(98);
        char* _sb0 = _cs2sx_next_buf();
        _sb0 = CS2SX_Format(_sb0, "loops %d %d", CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a3)), Resource_Open);
        const char* _ps_3 = _sb0;
        printf("%s\n", _ps_3 ? _ps_3 : "");
    }
    CS2SX_LINE(100);
    self->f_on = 1;
    CS2SX_LINE(101);
    self->f_aw4 = self->f_on;
    CS2SX_LINE(101);
    if (self->f_aw4)
    {
        CS2SX_LINE(101);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a4, CS2SX_RETAIN(CS2SX_Task, AsyncMoreApp_Value(1)));
        CS2SX_LINE(101);
        if (!CS2SX_NN(self->f_cs2sx_a4)->base.f_IsCompleted)
        {
            CS2SX_LINE(101);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a4), (CS2SX_Step*)self);
            CS2SX_LINE(101);
            self->f_cs2sx_state = 4; return; _cs2sx_r4_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(101);
        self->f_aw4 = CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a4)) == 1;
    }
    CS2SX_LINE(101);
    if (self->f_aw4)
    {
        CS2SX_LINE(101);
        printf("and\n");
    }
    CS2SX_LINE(102);
    CS2SX_STR_STORE(self->f_none, NULL);
    CS2SX_LINE(103);
    CS2SX_STR_STORE(self->f_aw5, self->f_none);
    CS2SX_LINE(103);
    if (self->f_aw5 == NULL)
    {
        CS2SX_LINE(103);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a5, CS2SX_RETAIN(CS2SX_Task, AsyncMoreApp_Name()));
        CS2SX_LINE(103);
        if (!CS2SX_NN(self->f_cs2sx_a5)->base.f_IsCompleted)
        {
            CS2SX_LINE(103);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a5), (CS2SX_Step*)self);
            CS2SX_LINE(103);
            self->f_cs2sx_state = 5; return; _cs2sx_r5_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(103);
        CS2SX_STR_STORE(self->f_aw5, CS2SX_Task_str_get_Result(CS2SX_NN(self->f_cs2sx_a5)));
    }
    CS2SX_LINE(103);
    CS2SX_STR_STORE(self->f_picked, self->f_aw5);
    CS2SX_LINE(104);
    if (self->f_on)
    {
        CS2SX_LINE(104);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a6, CS2SX_RETAIN(CS2SX_Task, AsyncMoreApp_Value(7)));
        CS2SX_LINE(104);
        if (!CS2SX_NN(self->f_cs2sx_a6)->base.f_IsCompleted)
        {
            CS2SX_LINE(104);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a6), (CS2SX_Step*)self);
            CS2SX_LINE(104);
            self->f_cs2sx_state = 6; return; _cs2sx_r6_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(104);
        self->f_aw6 = CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a6));
    }
    else
    {
        CS2SX_LINE(104);
        self->f_aw6 = 0;
    }
    CS2SX_LINE(104);
    self->f_pick = self->f_aw6;
    CS2SX_LINE(105);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "%s %d", self->f_picked, self->f_pick);
    const char* _ps_4 = _sb1;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    {
        CS2SX_LINE(107);
        CS2SX_ARR_LOCAL(CS2SX_Task_int*, _pa_5, 3, AsyncMoreApp_Value(1), AsyncMoreApp_Value(2), AsyncMoreApp_Value(3));
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a7, CS2SX_RETAIN(CS2SX_Task, CS2SX_WhenAll_int_Of(_pa_5)));
        CS2SX_LINE(107);
        if (!CS2SX_NN(self->f_cs2sx_a7)->base.f_IsCompleted)
        {
            CS2SX_LINE(107);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a7), (CS2SX_Step*)self);
            CS2SX_LINE(107);
            self->f_cs2sx_state = 7; return; _cs2sx_r7_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(107);
        self->f_all = CS2SX_Task_intArr_get_Result(CS2SX_NN(self->f_cs2sx_a7));
    }
    CS2SX_LINE(108);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "all %d %d", CS2SX_ArrLen(CS2SX_NN(self->f_all)), (CS2SX_AT(self->f_all, 0) + CS2SX_AT(self->f_all, 1) + CS2SX_AT(self->f_all, 2)));
    const char* _ps_6 = _sb2;
    printf("%s\n", _ps_6 ? _ps_6 : "");
    CS2SX_LINE(110);
    self->f_bonus = 10;
    CS2SX_LINE(111);
    struct _cap_0* _env_0 = CS2SX_ENV_NEW(struct _cap_0);
    _env_0->self = self; ((void* (*)(void*))CS2SX_Step_Retain)((void*)_env_0->self);
    _env_0->_hdr._dtor = _envdtor_0;
    _env_0->_hdr._gc = _envgc_0;
    CS2SX_RC_STORE(CS2SX_Delegate_Retain, CS2SX_Delegate_Release, self->f_add, CS2SX_Delegate_New((void*)_lambda_0, _env_0, 1));
    {
        CS2SX_LINE(112);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a8, CS2SX_RETAIN(CS2SX_Task, Func_int_CS2SX_Task_int_Invoke(self->f_add, 5)));
        CS2SX_LINE(112);
        if (!CS2SX_NN(self->f_cs2sx_a8)->base.f_IsCompleted)
        {
            CS2SX_LINE(112);
            CS2SX_Task_Then((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_a8), (CS2SX_Step*)self);
            CS2SX_LINE(112);
            self->f_cs2sx_state = 8; return; _cs2sx_r8_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(112);
        char* _sb3 = _cs2sx_next_buf();
        _sb3 = CS2SX_Format(_sb3, "lambda %d", CS2SX_Task_int_get_Result(CS2SX_NN(self->f_cs2sx_a8)));
        const char* _ps_7 = _sb3;
        printf("%s\n", _ps_7 ? _ps_7 : "");
    }
    CS2SX_LINE(114);
    struct _cap_1* _env_1 = CS2SX_ENV_NEW(struct _cap_1);
    _env_1->self = self; ((void* (*)(void*))CS2SX_Step_Retain)((void*)_env_1->self);
    _env_1->_hdr._dtor = _envdtor_1;
    _env_1->_hdr._gc = _envgc_1;
    CS2SX_RC_STORE(CS2SX_Delegate_Retain, CS2SX_Delegate_Release, CS2SX_NN(self->f_cs2sx_this)->f_later, CS2SX_Delegate_New((void*)_lambda_1, _env_1, 1));
    {
        CS2SX_LINE(115);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a9, CS2SX_RETAIN(CS2SX_Task, Func_CS2SX_Task_Invoke(CS2SX_NN(self->f_cs2sx_this)->f_later)));
        CS2SX_LINE(115);
        if (!CS2SX_NN(self->f_cs2sx_a9)->f_IsCompleted)
        {
            CS2SX_LINE(115);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a9), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(115);
            self->f_cs2sx_state = 9; return; _cs2sx_r9_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(115);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a9));
    }
    CS2SX_LINE(116);
    struct _cap_2* _env_2 = CS2SX_ENV_NEW(struct _cap_2);
    _env_2->self = self; ((void* (*)(void*))CS2SX_Step_Retain)((void*)_env_2->self);
    _env_2->_hdr._dtor = _envdtor_2;
    _env_2->_hdr._gc = _envgc_2;
    CS2SX_RC_STORE(CS2SX_Delegate_Retain, CS2SX_Delegate_Release, self->f_fire, CS2SX_Delegate_New((void*)_lambda_2, _env_2, 1));
    CS2SX_LINE(117);
    Action_Invoke(self->f_fire);
    CS2SX_LINE(118);
    CS2SX_Task_Finish(CS2SX_NN(self->f_cs2sx_task));
}

const CS2SX_ClassInfo Script_Async_ClassInfo = { "Script_Async", &CS2SX_Step_ClassInfo };

void Script_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    Script_Async* self = (Script_Async*)_self; (void)self;
    if (visit) visit((void*)self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = (void*)self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_add, &CS2SX_Delegate_GcKind);
    else if (self->f_add) { void* _o = (void*)self->f_add; self->f_add = NULL; ((void (*)(void*))CS2SX_Delegate_Release)(_o); }
    if (visit) visit((void*)self->f_fire, &CS2SX_Delegate_GcKind);
    else if (self->f_fire) { void* _o = (void*)self->f_fire; self->f_fire = NULL; ((void (*)(void*))CS2SX_Delegate_Release)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = (void*)self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a2, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a2) { void* _o = (void*)self->f_cs2sx_a2; self->f_cs2sx_a2 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a3, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a3) { void* _o = (void*)self->f_cs2sx_a3; self->f_cs2sx_a3 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a4, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a4) { void* _o = (void*)self->f_cs2sx_a4; self->f_cs2sx_a4 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a5, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a5) { void* _o = (void*)self->f_cs2sx_a5; self->f_cs2sx_a5 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a6, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a6) { void* _o = (void*)self->f_cs2sx_a6; self->f_cs2sx_a6 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a7, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a7) { void* _o = (void*)self->f_cs2sx_a7; self->f_cs2sx_a7 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a8, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a8) { void* _o = (void*)self->f_cs2sx_a8; self->f_cs2sx_a8 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a9, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a9) { void* _o = (void*)self->f_cs2sx_a9; self->f_cs2sx_a9 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void Script_Async__Destroy(Script_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    CS2SX_StrDrop(self->f_none); self->f_none = NULL;
    CS2SX_StrDrop(self->f_aw5); self->f_aw5 = NULL;
    CS2SX_StrDrop(self->f_picked); self->f_picked = NULL;
    CS2SX_ArrFree(self->f_all); self->f_all = NULL;
    if (self->f_add) { CS2SX_Delegate_Release(self->f_add); self->f_add = NULL; }
    if (self->f_fire) { CS2SX_Delegate_Release(self->f_fire); self->f_fire = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_str_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    if (self->f_cs2sx_a2) { CS2SX_Task_str_Free(self->f_cs2sx_a2); self->f_cs2sx_a2 = NULL; }
    if (self->f_cs2sx_a3) { CS2SX_Task_int_Free(self->f_cs2sx_a3); self->f_cs2sx_a3 = NULL; }
    if (self->f_cs2sx_a4) { CS2SX_Task_int_Free(self->f_cs2sx_a4); self->f_cs2sx_a4 = NULL; }
    if (self->f_cs2sx_a5) { CS2SX_Task_str_Free(self->f_cs2sx_a5); self->f_cs2sx_a5 = NULL; }
    if (self->f_cs2sx_a6) { CS2SX_Task_int_Free(self->f_cs2sx_a6); self->f_cs2sx_a6 = NULL; }
    if (self->f_cs2sx_a7) { CS2SX_Task_intArr_Free(self->f_cs2sx_a7); self->f_cs2sx_a7 = NULL; }
    if (self->f_cs2sx_a8) { CS2SX_Task_int_Free(self->f_cs2sx_a8); self->f_cs2sx_a8 = NULL; }
    if (self->f_cs2sx_a9) { CS2SX_Task_Free(self->f_cs2sx_a9); self->f_cs2sx_a9 = NULL; }
    CS2SX_Step__Destroy((CS2SX_Step*)self);
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

static CS2SX_Step_vtable __AsyncLambda1_3269_Async_vtable_instance;

void __AsyncLambda1_3269_Async_Ctor(__AsyncLambda1_3269_Async* self)
{
    CS2SX_TRACE("__AsyncLambda1_3269_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

__AsyncLambda1_3269_Async* __AsyncLambda1_3269_Async_New(void)
{
    __AsyncLambda1_3269_Async* self = (__AsyncLambda1_3269_Async*)calloc(1, sizeof(__AsyncLambda1_3269_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))__AsyncLambda1_3269_Async__Destroy;
    self->base._gc = __AsyncLambda1_3269_Async_Gc;
    self->base._class = &__AsyncLambda1_3269_Async_ClassInfo;
    self->base.vtable = &__AsyncLambda1_3269_Async_vtable_instance;
    __AsyncLambda1_3269_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)__AsyncLambda1_3269_Async_Free);
    return self;
}

void __AsyncLambda1_3269_Async_cs2sx_resume(__AsyncLambda1_3269_Async* self)
{
    CS2SX_TRACE("__AsyncLambda1_3269_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(127);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(127);
            __AsyncLambda1_3269_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(127);
                CS2SX_Task_Fail((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void __AsyncLambda1_3269_Async_cs2sx_run(__AsyncLambda1_3269_Async* self)
{
    CS2SX_TRACE("__AsyncLambda1_3269_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(127);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    {
        CS2SX_LINE(127);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
        CS2SX_LINE(127);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(127);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(127);
            self->f_cs2sx_state = 1; return; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(127);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    {
        CS2SX_LINE(127);
        CS2SX_Task_int_SetResult(CS2SX_NN(self->f_cs2sx_task), self->x + self->f_bonus);
        CS2SX_LINE(127);
        return;
    }
    CS2SX_LINE(127);
    CS2SX_Task_Finish((CS2SX_Task*)CS2SX_NN(self->f_cs2sx_task));
}

const CS2SX_ClassInfo __AsyncLambda1_3269_Async_ClassInfo = { "__AsyncLambda1_3269_Async", &CS2SX_Step_ClassInfo };

void __AsyncLambda1_3269_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    __AsyncLambda1_3269_Async* self = (__AsyncLambda1_3269_Async*)_self; (void)self;
    if (visit) visit((void*)self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = (void*)self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = (void*)self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void __AsyncLambda1_3269_Async__Destroy(__AsyncLambda1_3269_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_int_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step__Destroy((CS2SX_Step*)self);
}

void __AsyncLambda1_3269_Async_Free(__AsyncLambda1_3269_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

__AsyncLambda1_3269_Async* __AsyncLambda1_3269_Async_Retain(__AsyncLambda1_3269_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable __AsyncLambda1_3269_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))__AsyncLambda1_3269_Async_cs2sx_resume,
};

static CS2SX_Step_vtable __AsyncLambda2_3418_Async_vtable_instance;

void __AsyncLambda2_3418_Async_Ctor(__AsyncLambda2_3418_Async* self)
{
    CS2SX_TRACE("__AsyncLambda2_3418_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

__AsyncLambda2_3418_Async* __AsyncLambda2_3418_Async_New(void)
{
    __AsyncLambda2_3418_Async* self = (__AsyncLambda2_3418_Async*)calloc(1, sizeof(__AsyncLambda2_3418_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))__AsyncLambda2_3418_Async__Destroy;
    self->base._gc = __AsyncLambda2_3418_Async_Gc;
    self->base._class = &__AsyncLambda2_3418_Async_ClassInfo;
    self->base.vtable = &__AsyncLambda2_3418_Async_vtable_instance;
    __AsyncLambda2_3418_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)__AsyncLambda2_3418_Async_Free);
    return self;
}

void __AsyncLambda2_3418_Async_cs2sx_resume(__AsyncLambda2_3418_Async* self)
{
    CS2SX_TRACE("__AsyncLambda2_3418_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(127);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(127);
            __AsyncLambda2_3418_Async_cs2sx_run(self);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* cs2sx_e = (Exception*)_exf_0_ex;
                CS2SX_LINE(127);
                CS2SX_Task_Fail(CS2SX_NN(self->f_cs2sx_task), cs2sx_e);
            }
        }
    }
}

void __AsyncLambda2_3418_Async_cs2sx_run(__AsyncLambda2_3418_Async* self)
{
    CS2SX_TRACE("__AsyncLambda2_3418_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(127);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    {
        CS2SX_LINE(127);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Delay(30)));
        CS2SX_LINE(127);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(127);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(127);
            self->f_cs2sx_state = 1; return; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(127);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    CS2SX_LINE(127);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "later %s", CS2SX_BoolStr((CS2SX_NN(self->f_cs2sx_this)->f_ticks > 0)));
    const char* _ps_1 = _sb0;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(127);
    CS2SX_Task_Finish(CS2SX_NN(self->f_cs2sx_task));
}

const CS2SX_ClassInfo __AsyncLambda2_3418_Async_ClassInfo = { "__AsyncLambda2_3418_Async", &CS2SX_Step_ClassInfo };

void __AsyncLambda2_3418_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    __AsyncLambda2_3418_Async* self = (__AsyncLambda2_3418_Async*)_self; (void)self;
    if (visit) visit((void*)self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = (void*)self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = (void*)self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void __AsyncLambda2_3418_Async__Destroy(__AsyncLambda2_3418_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step__Destroy((CS2SX_Step*)self);
}

void __AsyncLambda2_3418_Async_Free(__AsyncLambda2_3418_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

__AsyncLambda2_3418_Async* __AsyncLambda2_3418_Async_Retain(__AsyncLambda2_3418_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable __AsyncLambda2_3418_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))__AsyncLambda2_3418_Async_cs2sx_resume,
};

static CS2SX_Step_vtable __AsyncLambda3_3601_Async_vtable_instance;

void __AsyncLambda3_3601_Async_Ctor(__AsyncLambda3_3601_Async* self)
{
    CS2SX_TRACE("__AsyncLambda3_3601_Async..ctor", "Program.cs");
    CS2SX_Step_Ctor((CS2SX_Step*)self);
}

__AsyncLambda3_3601_Async* __AsyncLambda3_3601_Async_New(void)
{
    __AsyncLambda3_3601_Async* self = (__AsyncLambda3_3601_Async*)calloc(1, sizeof(__AsyncLambda3_3601_Async));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))__AsyncLambda3_3601_Async__Destroy;
    self->base._gc = __AsyncLambda3_3601_Async_Gc;
    self->base._class = &__AsyncLambda3_3601_Async_ClassInfo;
    self->base.vtable = &__AsyncLambda3_3601_Async_vtable_instance;
    __AsyncLambda3_3601_Async_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)__AsyncLambda3_3601_Async_Free);
    return self;
}

void __AsyncLambda3_3601_Async_cs2sx_resume(__AsyncLambda3_3601_Async* self)
{
    CS2SX_TRACE("__AsyncLambda3_3601_Async.cs2sx_resume", "Program.cs");
    CS2SX_LINE(127);
    __AsyncLambda3_3601_Async_cs2sx_run(self);
}

void __AsyncLambda3_3601_Async_cs2sx_run(__AsyncLambda3_3601_Async* self)
{
    CS2SX_TRACE("__AsyncLambda3_3601_Async.cs2sx_run", "Program.cs");
    CS2SX_LINE(127);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    {
        CS2SX_LINE(127);
        CS2SX_ARC_STORE(CS2SX_Task, self->f_cs2sx_a1, CS2SX_RETAIN(CS2SX_Task, CS2SX_Task_Yield()));
        CS2SX_LINE(127);
        if (!CS2SX_NN(self->f_cs2sx_a1)->f_IsCompleted)
        {
            CS2SX_LINE(127);
            CS2SX_Task_Then(CS2SX_NN(self->f_cs2sx_a1), (CS2SX_Step*)(CS2SX_Step*)self);
            CS2SX_LINE(127);
            self->f_cs2sx_state = 1; return; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(127);
        CS2SX_Task_Check(CS2SX_NN(self->f_cs2sx_a1));
    }
    CS2SX_LINE(127);
    CS2SX_NN(self->f_cs2sx_this)->f_done = 1;
    CS2SX_LINE(127);
    CS2SX_Task_Finish(CS2SX_NN(self->f_cs2sx_task));
}

const CS2SX_ClassInfo __AsyncLambda3_3601_Async_ClassInfo = { "__AsyncLambda3_3601_Async", &CS2SX_Step_ClassInfo };

void __AsyncLambda3_3601_Async_Gc(void* _self, CS2SX_GcVisit visit)
{
    __AsyncLambda3_3601_Async* self = (__AsyncLambda3_3601_Async*)_self; (void)self;
    if (visit) visit((void*)self->f_cs2sx_task, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_task) { void* _o = (void*)self->f_cs2sx_task; self->f_cs2sx_task = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    if (visit) visit((void*)self->f_cs2sx_a1, &CS2SX_Task_GcKind);
    else if (self->f_cs2sx_a1) { void* _o = (void*)self->f_cs2sx_a1; self->f_cs2sx_a1 = NULL; ((void (*)(void*))CS2SX_Task_Free)(_o); }
    CS2SX_Step_Gc(_self, visit);
}

void __AsyncLambda3_3601_Async__Destroy(__AsyncLambda3_3601_Async* self)
{
    if (self->f_cs2sx_task) { CS2SX_Task_Free(self->f_cs2sx_task); self->f_cs2sx_task = NULL; }
    if (self->f_cs2sx_a1) { CS2SX_Task_Free(self->f_cs2sx_a1); self->f_cs2sx_a1 = NULL; }
    CS2SX_Step__Destroy((CS2SX_Step*)self);
}

void __AsyncLambda3_3601_Async_Free(__AsyncLambda3_3601_Async* self)
{
    if (!self) return;
    CS2SX_Step* _o = (CS2SX_Step*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Step_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

__AsyncLambda3_3601_Async* __AsyncLambda3_3601_Async_Retain(__AsyncLambda3_3601_Async* self)
{
    if (self) ((CS2SX_Step*)self)->_rc++;
    return self;
}

static CS2SX_Step_vtable __AsyncLambda3_3601_Async_vtable_instance =
{
    .cs2sx_resume = (void(*)(void*))__AsyncLambda3_3601_Async_cs2sx_resume,
};
