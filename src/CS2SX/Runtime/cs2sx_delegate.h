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
//
// Delegates are reference-counted: a new one belongs to the current frame (it is
// released at the next one); whatever keeps it — a field, a collection, a closure
// — takes a reference (CS2SX_Delegate_Retain) and gives it up again
// (CS2SX_Delegate_Release / _Drop). Each entry owns its target.
// ============================================================================

typedef void (*CS2SX_FreeFn)(void* obj);
struct CS2SX_GcKind;

/// Run-time type of an object of a reference-counted class (x is T, x as T):
/// each class has one (T_ClassInfo), linked to its base class.
typedef struct CS2SX_ClassInfo
{
    const char* name;
    const struct CS2SX_ClassInfo* base;
} CS2SX_ClassInfo;

static inline int CS2SX_IsA(const CS2SX_ClassInfo* t, const CS2SX_ClassInfo* target)
{
    for (; t; t = t->base) if (t == target) return 1;
    return 0;
}

typedef struct CS2SX_Delegate CS2SX_Delegate;
struct CS2SX_Delegate
{
    int             _rc;        // < 0: static instance, never freed
    void*           fn;         // R (*)(void* target, A…)
    void*           target;     // closure env or `this`; may be NULL
    int             ownsTarget; // 0: no; 1: a CS2SX_Env; 2: an object (targetRetain/targetFree)
    CS2SX_Delegate* next;       // next entry of the invocation list (owned)
    void*         (*targetRetain)(void*);
    CS2SX_FreeFn    targetFree;
    const struct CS2SX_GcKind* targetKind;   // cycle collection: kind of the object
};

/// Header of a closure environment (the struct of captured variables).
/// _dtor releases the captured objects; _gc visits them (cycle collection).
typedef struct CS2SX_Env
{
    int _rc;
    void (*_dtor)(void* env);
    void (*_gc)(void* env, void (*visit)(void* child, const struct CS2SX_GcKind* kind));
} CS2SX_Env;

#define CS2SX_DELEGATE_STATIC(fn) { -1, (void*)(fn), NULL, 0, NULL, NULL, NULL, NULL }
#define CS2SX_ENV_NEW(T) ((T*)CS2SX_Env_New(sizeof(T)))

void* CS2SX_Env_New(size_t size);
CS2SX_Delegate* CS2SX_Delegate_New(void* fn, void* target, int ownsTarget);
/// An instance method bound to `target`, which the delegate keeps alive.
CS2SX_Delegate* CS2SX_Delegate_NewBound(void* fn, void* target, void* (*retainFn)(void*),
                                        CS2SX_FreeFn freeFn, const struct CS2SX_GcKind* kind);
CS2SX_Delegate* CS2SX_Delegate_Combine(CS2SX_Delegate* a, CS2SX_Delegate* b);
CS2SX_Delegate* CS2SX_Delegate_Remove(CS2SX_Delegate* a, CS2SX_Delegate* b);
int  CS2SX_Delegate_Equals(const CS2SX_Delegate* a, const CS2SX_Delegate* b);
CS2SX_Delegate* CS2SX_Delegate_Retain(CS2SX_Delegate* d);
void CS2SX_Delegate_Release(CS2SX_Delegate* d);
/// Gives up a reference at the next frame (collections: the value stays valid).
void CS2SX_Delegate_Drop(CS2SX_Delegate* d);
extern const struct CS2SX_GcKind CS2SX_Delegate_GcKind;
extern const struct CS2SX_GcKind CS2SX_Env_GcKind;

// ── Interface values ──────────────────────────────────────────────────────────
// An interface value (IFace*) is a small object {vtable, obj, …} made by
// Class_as_IFace: reference-counted like a delegate, and it keeps its object alive.
typedef struct CS2SX_IfaceHdr
{
    void* vtable;
    void* obj;
    int   _rc;
    CS2SX_FreeFn release;                   // gives up the object; NULL: not owned
    const struct CS2SX_GcKind* kind;        // cycle collection: kind of the object
    const CS2SX_ClassInfo* cls;             // run-time type of the object (iface is T)
} CS2SX_IfaceHdr;

void* CS2SX_Iface_Wrap(size_t size, void* vtable, void* obj, void* (*retainFn)(void*),
                       CS2SX_FreeFn freeFn, const struct CS2SX_GcKind* kind, const CS2SX_ClassInfo* cls);
void* CS2SX_Iface_Retain(void* iface);
void  CS2SX_Iface_Free(void* iface);
void  CS2SX_Iface_Drop(void* iface);
extern const struct CS2SX_GcKind CS2SX_Iface_GcKind;

/// Stores v into the field lv: takes a reference to v, gives up the previous
/// value at the next frame. For delegates and interface values. lv is evaluated
/// once; the value of the expression is v.
void CS2SX_ReleaseLater(void* obj, CS2SX_FreeFn free_fn);
#define CS2SX_RC_STORE(RetainFn, FreeFn, lv, v) ({ __typeof__(&(lv)) _cs2sx_rp = &(lv); \
    __typeof__(*_cs2sx_rp) _cs2sx_rn = (__typeof__(*_cs2sx_rp))(v); RetainFn(_cs2sx_rn); \
    __typeof__(*_cs2sx_rp) _cs2sx_ro = *_cs2sx_rp; *_cs2sx_rp = _cs2sx_rn; \
    CS2SX_ReleaseLater((void*)_cs2sx_ro, (CS2SX_FreeFn)FreeFn); _cs2sx_rn; })

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
