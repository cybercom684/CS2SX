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

// ── object: boxes and the functions every `object` value goes through ──────────

void* CS2SX_Obj_Retain(void* o)
{
    if (o) ((CS2SX_ObjHdr*)o)->_rc++;
    return o;
}

void CS2SX_Obj_Release(void* o)
{
    if (!o) return;
    CS2SX_ObjHdr* h = (CS2SX_ObjHdr*)o;
    if (--h->_rc > 0) { CS2SX_GcSuspect(h, &CS2SX_Obj_GcKind); return; }
    CS2SX_GcForget(h);
    h->_dtor(h);
    free(h);
}

void CS2SX_Obj_Drop(void* o)
{
    CS2SX_ReleaseLater(o, CS2SX_Obj_Release);
}

static void obj_gc(void* self, CS2SX_GcVisit visit)
{
    CS2SX_ObjHdr* h = (CS2SX_ObjHdr*)self;
    if (h->_gc) h->_gc(self, visit);
}

const CS2SX_GcKind CS2SX_Obj_GcKind = { offsetof(CS2SX_ObjHdr, _rc), obj_gc, CS2SX_Obj_Release };

static const char* box_str_int(void* self)    { return Long_ToString(((CS2SX_Box*)self)->v.i); }
static const char* box_str_uint(void* self)   { return ULong_ToString(((CS2SX_Box*)self)->v.u); }
static const char* box_str_double(void* self) { return Double_ToString(((CS2SX_Box*)self)->v.d); }
static const char* box_str_single(void* self) { return Float_ToString((float)((CS2SX_Box*)self)->v.d); }
static const char* box_str_bool(void* self)   { return ((CS2SX_Box*)self)->v.i ? "True" : "False"; }
static const char* box_str_string(void* self) { return ((CS2SX_Box*)self)->v.s; }
static const char* box_str_char(void* self)
{
    char* s = CS2SX_StrAlloc(1);
    s[0] = (char)((CS2SX_Box*)self)->v.i;
    s[1] = 0;
    return s;
}

const CS2SX_ClassInfo CS2SX_Box_String  = { "System.String",  NULL, box_str_string };
const CS2SX_ClassInfo CS2SX_Box_Boolean = { "System.Boolean", NULL, box_str_bool };
const CS2SX_ClassInfo CS2SX_Box_Char    = { "System.Char",    NULL, box_str_char };
const CS2SX_ClassInfo CS2SX_Box_SByte   = { "System.SByte",   NULL, box_str_int };
const CS2SX_ClassInfo CS2SX_Box_Byte    = { "System.Byte",    NULL, box_str_int };
const CS2SX_ClassInfo CS2SX_Box_Int16   = { "System.Int16",   NULL, box_str_int };
const CS2SX_ClassInfo CS2SX_Box_UInt16  = { "System.UInt16",  NULL, box_str_int };
const CS2SX_ClassInfo CS2SX_Box_Int32   = { "System.Int32",   NULL, box_str_int };
const CS2SX_ClassInfo CS2SX_Box_UInt32  = { "System.UInt32",  NULL, box_str_int };
const CS2SX_ClassInfo CS2SX_Box_Int64   = { "System.Int64",   NULL, box_str_int };
const CS2SX_ClassInfo CS2SX_Box_UInt64  = { "System.UInt64",  NULL, box_str_uint };
const CS2SX_ClassInfo CS2SX_Box_Single  = { "System.Single",  NULL, box_str_single };
const CS2SX_ClassInfo CS2SX_Box_Double  = { "System.Double",  NULL, box_str_double };

static void box_dtor(void* self)
{
    CS2SX_Box* b = (CS2SX_Box*)self;
    if (b->h._class == &CS2SX_Box_String) free(b->v.s);
}

static CS2SX_Box* box_new(const CS2SX_ClassInfo* type)
{
    CS2SX_Box* b = (CS2SX_Box*)calloc(1, sizeof(CS2SX_Box));
    if (!b) CS2SX_ThrowNew(&CS2SX_TI_OutOfMemoryException, NULL);
    b->h._rc = 1;
    b->h._dtor = box_dtor;
    b->h._gc = CS2SX_GcLeaf;
    b->h._class = type;
    CS2SX_ReleaseLater(b, CS2SX_Obj_Release);   // the frame's reference
    return b;
}

void* CS2SX_Box_I(const CS2SX_ClassInfo* type, long long v)  { CS2SX_Box* b = box_new(type); b->v.i = v; return b; }
void* CS2SX_Box_U(const CS2SX_ClassInfo* type, unsigned long long v) { CS2SX_Box* b = box_new(type); b->v.u = v; return b; }
void* CS2SX_Box_D(const CS2SX_ClassInfo* type, double v)     { CS2SX_Box* b = box_new(type); b->v.d = v; return b; }

void* CS2SX_Box_Str(const char* s)
{
    if (!s) return NULL;
    CS2SX_Box* b = box_new(&CS2SX_Box_String);
    size_t n = strlen(s) + 1;
    b->v.s = (char*)malloc(n);
    if (!b->v.s) CS2SX_ThrowNew(&CS2SX_TI_OutOfMemoryException, NULL);
    memcpy(b->v.s, s, n);
    return b;
}

static void obj_cast_fail(void* o, const CS2SX_ClassInfo* type)
{
    char msg[160];
    snprintf(msg, sizeof msg, "Unable to cast object of type '%s' to type '%s'.",
             ((CS2SX_ObjHdr*)o)->_class ? ((CS2SX_ObjHdr*)o)->_class->name : "?", type->name);
    CS2SX_ThrowNew(&CS2SX_TI_InvalidCastException, msg);
}

void* CS2SX_Obj_Cast(void* o, const CS2SX_ClassInfo* type)
{
    if (o && !CS2SX_Obj_Is(o, type)) obj_cast_fail(o, type);
    return o;
}

static CS2SX_Box* unbox(void* o, const CS2SX_ClassInfo* type)
{
    if (!o) CS2SX_ThrowNew(&CS2SX_TI_NullReferenceException, NULL);
    if (((CS2SX_ObjHdr*)o)->_class != type) obj_cast_fail(o, type);
    return (CS2SX_Box*)o;
}

long long          CS2SX_Unbox_I(void* o, const CS2SX_ClassInfo* type) { return unbox(o, type)->v.i; }
unsigned long long CS2SX_Unbox_U(void* o, const CS2SX_ClassInfo* type) { return unbox(o, type)->v.u; }
double             CS2SX_Unbox_D(void* o, const CS2SX_ClassInfo* type) { return unbox(o, type)->v.d; }

const char* CS2SX_Unbox_Str(void* o)
{
    if (!o) return NULL;
    if (((CS2SX_ObjHdr*)o)->_class != &CS2SX_Box_String) obj_cast_fail(o, &CS2SX_Box_String);
    return ((CS2SX_Box*)o)->v.s;
}

const char* CS2SX_Obj_ToString(void* o)
{
    if (!o) return "";
    const CS2SX_ClassInfo* t = ((CS2SX_ObjHdr*)o)->_class;
    if (!t) return "Object";
    return t->str ? t->str(o) : t->name;
}
