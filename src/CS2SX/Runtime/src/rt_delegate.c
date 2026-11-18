// Runtime module for cs2sx_delegate.h — delegate objects and invocation lists.
//
// Delegates are reference-counted like objects: a new delegate (and every list
// Combine/Remove builds) belongs to the current frame and is released at the next
// one unless something keeps it (a field, a collection, a closure: they retain).
// Each entry owns its target: a closure environment, or the object an instance
// method is bound to.
#include "cs2sx_prelude.h"

void* CS2SX_Env_New(size_t size)
{
    CS2SX_Env* env = (CS2SX_Env*)calloc(1, size);
    if (env) env->_rc = 1;
    return env;
}

static void env_retain(void* env)
{
    if (env) ((CS2SX_Env*)env)->_rc++;
}

static void env_release(void* env)
{
    if (!env) return;
    CS2SX_Env* e = (CS2SX_Env*)env;
    if (--e->_rc > 0) { if (e->_gc) CS2SX_GcSuspect(e, &CS2SX_Env_GcKind); return; }
    CS2SX_GcForget(e);
    if (e->_dtor) e->_dtor(env);
    free(env);
}

static void env_gc(void* env, CS2SX_GcVisit visit)
{
    CS2SX_Env* e = (CS2SX_Env*)env;
    if (e->_gc) e->_gc(env, visit);
}

const CS2SX_GcKind CS2SX_Env_GcKind = { offsetof(CS2SX_Env, _rc), env_gc, env_release };

// One entry, not owned by the frame (the list head is).
static CS2SX_Delegate* entry_new(void* fn, void* target, int ownsTarget)
{
    CS2SX_Delegate* d = (CS2SX_Delegate*)calloc(1, sizeof(CS2SX_Delegate));
    if (!d) return NULL;
    d->_rc = 1;
    d->fn = fn;
    d->target = target;
    d->ownsTarget = ownsTarget;
    return d;
}

static void target_retain(const CS2SX_Delegate* e)
{
    if (e->ownsTarget == 1) env_retain(e->target);
    else if (e->ownsTarget == 2 && e->targetRetain) e->targetRetain(e->target);
}

static void target_release(CS2SX_Delegate* e)
{
    if (e->ownsTarget == 1) env_release(e->target);
    else if (e->ownsTarget == 2 && e->targetFree) e->targetFree(e->target);
    e->target = NULL;
    e->ownsTarget = 0;
}

CS2SX_Delegate* CS2SX_Delegate_New(void* fn, void* target, int ownsTarget)
{
    CS2SX_Delegate* d = entry_new(fn, target, ownsTarget);
    CS2SX_ReleaseLater(d, (CS2SX_FreeFn)CS2SX_Delegate_Release);   // the frame's reference
    return d;
}

CS2SX_Delegate* CS2SX_Delegate_NewBound(void* fn, void* target, void* (*retainFn)(void*),
                                        CS2SX_FreeFn freeFn, const CS2SX_GcKind* kind)
{
    CS2SX_Delegate* d = entry_new(fn, target, target && freeFn ? 2 : 0);
    if (!d) return NULL;
    if (d->ownsTarget == 2)
    {
        d->targetRetain = retainFn;
        d->targetFree = freeFn;
        d->targetKind = kind;
        if (retainFn) retainFn(target);
    }
    CS2SX_ReleaseLater(d, (CS2SX_FreeFn)CS2SX_Delegate_Release);
    return d;
}

CS2SX_Delegate* CS2SX_Delegate_Retain(CS2SX_Delegate* d)
{
    if (d && d->_rc > 0) d->_rc++;
    return d;
}

void CS2SX_Delegate_Release(CS2SX_Delegate* d)
{
    while (d && d->_rc > 0)
    {
        if (--d->_rc > 0)
        {
            if (d->ownsTarget) CS2SX_GcSuspect(d, &CS2SX_Delegate_GcKind);
            return;
        }
        CS2SX_GcForget(d);
        CS2SX_Delegate* next = d->next;
        target_release(d);
        free(d);
        d = next;
    }
}

void CS2SX_Delegate_Drop(CS2SX_Delegate* d)
{
    CS2SX_ReleaseLater(d, (CS2SX_FreeFn)CS2SX_Delegate_Release);
}

// Cycle collection: an entry refers to its target and to the next entry.
static void delegate_gc(void* self, CS2SX_GcVisit visit)
{
    CS2SX_Delegate* d = (CS2SX_Delegate*)self;
    if (visit)
    {
        if (d->next) visit(d->next, &CS2SX_Delegate_GcKind);
        if (d->ownsTarget == 1) visit(d->target, &CS2SX_Env_GcKind);
        else if (d->ownsTarget == 2 && d->targetKind) visit(d->target, d->targetKind);
        return;
    }
    CS2SX_Delegate* next = d->next;
    d->next = NULL;
    target_release(d);
    CS2SX_Delegate_Release(next);
}

const CS2SX_GcKind CS2SX_Delegate_GcKind = { offsetof(CS2SX_Delegate, _rc), delegate_gc,
                                             (CS2SX_FreeFn)CS2SX_Delegate_Release };

int CS2SX_Delegate_Equals(const CS2SX_Delegate* a, const CS2SX_Delegate* b)
{
    for (; a && b; a = a->next, b = b->next)
        if (a->fn != b->fn || a->target != b->target) return 0;
    return a == b;   // both lists ended
}

// Copies one entry (not its successors), sharing (and retaining) the bound target.
static CS2SX_Delegate* entry_copy(const CS2SX_Delegate* e)
{
    CS2SX_Delegate* c = entry_new(e->fn, e->target, e->ownsTarget);
    if (!c) return NULL;
    c->targetRetain = e->targetRetain;
    c->targetFree = e->targetFree;
    c->targetKind = e->targetKind;
    target_retain(c);
    return c;
}

static int list_length(const CS2SX_Delegate* d)
{
    int n = 0;
    for (; d; d = d->next) n++;
    return n;
}

// New list (owned by the frame) from the entries of `src` except those in
// [skipFrom, skipFrom + skipLen), followed by the entries of `tail`.
static CS2SX_Delegate* list_copy_except(const CS2SX_Delegate* src, int skipFrom, int skipLen,
                                        const CS2SX_Delegate* tail)
{
    CS2SX_Delegate* head = NULL;
    CS2SX_Delegate** link = &head;
    int i = 0;
    for (const CS2SX_Delegate* e = src; e; e = e->next, i++)
    {
        if (i >= skipFrom && i < skipFrom + skipLen) continue;
        *link = entry_copy(e);
        if (!*link) break;
        link = &(*link)->next;
    }
    for (const CS2SX_Delegate* e = tail; e; e = e->next)
    {
        *link = entry_copy(e);
        if (!*link) break;
        link = &(*link)->next;
    }
    CS2SX_ReleaseLater(head, (CS2SX_FreeFn)CS2SX_Delegate_Release);
    return head;
}

CS2SX_Delegate* CS2SX_Delegate_Combine(CS2SX_Delegate* a, CS2SX_Delegate* b)
{
    if (!a) return b;
    if (!b) return a;
    return list_copy_except(a, 0, 0, b);
}

// C#: removes the LAST occurrence of b's invocation list from a's.
CS2SX_Delegate* CS2SX_Delegate_Remove(CS2SX_Delegate* a, CS2SX_Delegate* b)
{
    if (!a) return NULL;
    if (!b) return a;
    int na = list_length(a), nb = list_length(b);
    for (int start = na - nb; start >= 0; start--)
    {
        const CS2SX_Delegate* e = a;
        for (int i = 0; i < start; i++) e = e->next;
        const CS2SX_Delegate* x = e;
        const CS2SX_Delegate* y = b;
        while (y && x->fn == y->fn && x->target == y->target) { x = x->next; y = y->next; }
        if (!y)
            return na == nb ? NULL : list_copy_except(a, start, nb, NULL);
    }
    return a;
}

// ── Interface values ────────────────────────────────────────────────────────
// Class_as_IFace allocates {vtable, obj, _rc, …}; the value owns the object.

void* CS2SX_Iface_Retain(void* iface)
{
    if (iface) ((CS2SX_IfaceHdr*)iface)->_rc++;
    return iface;
}

void CS2SX_Iface_Free(void* iface)
{
    if (!iface) return;
    CS2SX_IfaceHdr* h = (CS2SX_IfaceHdr*)iface;
    if (--h->_rc > 0) { if (h->kind) CS2SX_GcSuspect(h, &CS2SX_Iface_GcKind); return; }
    CS2SX_GcForget(h);
    if (h->release) h->release(h->obj);
    free(h);
}

void CS2SX_Iface_Drop(void* iface)
{
    CS2SX_ReleaseLater(iface, CS2SX_Iface_Free);
}

static void iface_gc(void* self, CS2SX_GcVisit visit)
{
    CS2SX_IfaceHdr* h = (CS2SX_IfaceHdr*)self;
    if (visit) { if (h->kind) visit(h->obj, h->kind); return; }
    if (h->release && h->obj) { CS2SX_FreeFn r = h->release; h->release = NULL; r(h->obj); }
}

const CS2SX_GcKind CS2SX_Iface_GcKind = { offsetof(CS2SX_IfaceHdr, _rc), iface_gc, CS2SX_Iface_Free };

void* CS2SX_Iface_Wrap(size_t size, void* vtable, void* obj, void* (*retainFn)(void*),
                       CS2SX_FreeFn freeFn, const CS2SX_GcKind* kind, const CS2SX_ClassInfo* cls)
{
    CS2SX_IfaceHdr* h = (CS2SX_IfaceHdr*)calloc(1, size);
    if (!h) return NULL;
    h->vtable = vtable;
    h->obj = obj;
    h->_rc = 1;
    h->cls = cls;
    if (obj && freeFn)
    {
        if (retainFn) retainFn(obj);
        h->release = freeFn;
        h->kind = kind;
    }
    CS2SX_ReleaseLater(h, CS2SX_Iface_Free);   // the frame's reference
    return h;
}
