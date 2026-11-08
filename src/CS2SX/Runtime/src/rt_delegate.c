// Runtime module for cs2sx_delegate.h — delegate objects and invocation lists.
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
    if (env && --((CS2SX_Env*)env)->_rc == 0)
    {
        if (((CS2SX_Env*)env)->_dtor) ((CS2SX_Env*)env)->_dtor(env);
        free(env);
    }
}

CS2SX_Delegate* CS2SX_Delegate_New(void* fn, void* target, int ownsTarget)
{
    CS2SX_Delegate* d = (CS2SX_Delegate*)calloc(1, sizeof(CS2SX_Delegate));
    if (!d) return NULL;
    d->_rc = 1;
    d->fn = fn;
    d->target = target;
    d->ownsTarget = ownsTarget;
    return d;
}

CS2SX_Delegate* CS2SX_Delegate_Retain(CS2SX_Delegate* d)
{
    if (d && d->_rc > 0) d->_rc++;
    return d;
}

void CS2SX_Delegate_Release(CS2SX_Delegate* d)
{
    while (d && d->_rc > 0 && --d->_rc == 0)
    {
        CS2SX_Delegate* next = d->next;
        if (d->ownsTarget) env_release(d->target);
        free(d);
        d = next;
    }
}

int CS2SX_Delegate_Equals(const CS2SX_Delegate* a, const CS2SX_Delegate* b)
{
    for (; a && b; a = a->next, b = b->next)
        if (a->fn != b->fn || a->target != b->target) return 0;
    return a == b;   // both lists ended
}

// Copies one entry (not its successors), sharing the bound target.
static CS2SX_Delegate* entry_copy(const CS2SX_Delegate* e)
{
    CS2SX_Delegate* c = CS2SX_Delegate_New(e->fn, e->target, e->ownsTarget);
    if (c && e->ownsTarget) env_retain(e->target);
    return c;
}

static int list_length(const CS2SX_Delegate* d)
{
    int n = 0;
    for (; d; d = d->next) n++;
    return n;
}

// New list from the entries [0, n) of `src` except those in [skipFrom, skipFrom + skipLen).
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
    return head;
}

CS2SX_Delegate* CS2SX_Delegate_Combine(CS2SX_Delegate* a, CS2SX_Delegate* b)
{
    if (!a) return CS2SX_Delegate_Retain(b);
    if (!b) return CS2SX_Delegate_Retain(a);
    return list_copy_except(a, 0, 0, b);
}

// C#: removes the LAST occurrence of b's invocation list from a's.
CS2SX_Delegate* CS2SX_Delegate_Remove(CS2SX_Delegate* a, CS2SX_Delegate* b)
{
    if (!a) return NULL;
    if (!b) return CS2SX_Delegate_Retain(a);
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
    return CS2SX_Delegate_Retain(a);
}
