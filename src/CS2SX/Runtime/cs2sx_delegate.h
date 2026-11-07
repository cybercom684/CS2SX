#pragma once
// ============================================================================
// cs2sx_delegate.h — C# delegates (Action, Func<…>, events) as objects.
//
// A delegate value is a CS2SX_Delegate*: a function plus the object it is bound
// to (a closure environment or the `this` of an instance method), and — for
// multicast delegates — the next entry of its invocation list. Every delegate
// type is a typedef of CS2SX_Delegate* (Action_t, Func_int_int_t, …) with a typed
// invoker generated next to it (Func_int_int_Invoke(d, x)), which calls each
// entry in order and returns the last result, like C#.
//
//   x => x * k            static int _lambda_3(void* _env, int x) { … }
//                         CS2SX_Delegate_New((void*)_lambda_3, env, 1)   (env: captured k)
//   () => 1               &_dg_4    (no captures: one static, never freed)
//   OnChanged             CS2SX_Delegate_New((void*)App_OnChanged, self, 0)
//   ev += h / ev -= h     ev = CS2SX_Delegate_Combine(ev, h) / CS2SX_Delegate_Remove(ev, h)
//
// Lifted functions take the bound object as their first parameter (void*), so
// instance methods `R T_M(T* self, …)` are called through the same signature.
// Delegates are immutable: Combine/Remove build new lists.
// ============================================================================

typedef struct CS2SX_Delegate CS2SX_Delegate;
struct CS2SX_Delegate
{
    int             _rc;        // < 0: static instance, never freed
    void*           fn;         // R (*)(void* target, A…)
    void*           target;     // closure env or `this`; may be NULL
    int             ownsTarget; // target is a CS2SX_Env (refcounted with the delegate)
    CS2SX_Delegate* next;       // next entry of the invocation list (owned)
};

/// Header of a closure environment (the struct of captured variables).
typedef struct CS2SX_Env { int _rc; } CS2SX_Env;

#define CS2SX_DELEGATE_STATIC(fn) { -1, (void*)(fn), NULL, 0, NULL }
#define CS2SX_ENV_NEW(T) ((T*)CS2SX_Env_New(sizeof(T)))

void* CS2SX_Env_New(size_t size);
CS2SX_Delegate* CS2SX_Delegate_New(void* fn, void* target, int ownsTarget);
CS2SX_Delegate* CS2SX_Delegate_Combine(CS2SX_Delegate* a, CS2SX_Delegate* b);
CS2SX_Delegate* CS2SX_Delegate_Remove(CS2SX_Delegate* a, CS2SX_Delegate* b);
int  CS2SX_Delegate_Equals(const CS2SX_Delegate* a, const CS2SX_Delegate* b);
CS2SX_Delegate* CS2SX_Delegate_Retain(CS2SX_Delegate* d);
void CS2SX_Delegate_Release(CS2SX_Delegate* d);

/// Invoking a null delegate throws NullReferenceException, like C#.
static inline void CS2SX_Delegate_CheckNull(const CS2SX_Delegate* d)
{
    if (!d) CS2SX_ThrowNew(&CS2SX_TI_NullReferenceException, NULL);
}

// Action (no parameters) is used by the runtime itself (Button.OnClick).
typedef CS2SX_Delegate* Action_t;
static inline void Action_Invoke(Action_t d)
{
    CS2SX_Delegate_CheckNull(d);
    for (; d; d = d->next) ((void (*)(void*))d->fn)(d->target);
}
