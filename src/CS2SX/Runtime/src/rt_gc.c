// Cycle collection for reference-counted objects (see CS2SX_GcKind in switchforms.h).
//
// Trial deletion (Bacon & Rajan) with the bookkeeping in a side table instead of
// the objects: the real reference counts are never changed while looking.
//   1. Every object reachable from a suspect gets an entry with a copy of its
//      count; every followed reference subtracts one from its target's copy.
//   2. An entry whose copy stays above zero is referenced from outside the graph:
//      it is alive, and so is everything it reaches.
//   3. The rest is garbage. It is pinned, its followed references are cut, and
//      unpinning lets plain reference counting free it.
#include "cs2sx_prelude.h"

typedef struct
{
    void* obj;                  // NULL = empty, GC_TOMB = removed
    const CS2SX_GcKind* kind;
    int count;                  // trial count (collection table only)
    int live;
} GcEntry;

typedef struct { GcEntry* e; int cap; int used; int count; } GcTable;

#define GC_TOMB ((void*)(uintptr_t)1)

static GcTable s_suspects;
static int s_collecting;
static unsigned s_frames;

static unsigned gc_hash(void* p)
{
    uintptr_t x = (uintptr_t)p >> 4;
    x ^= x >> 17; x *= 0x9E3779B1u; x ^= x >> 15;
    return (unsigned)x;
}

static GcEntry* gc_find(GcTable* t, void* obj)
{
    if (t->cap == 0) return NULL;
    unsigned mask = (unsigned)t->cap - 1;
    for (unsigned i = gc_hash(obj) & mask; ; i = (i + 1) & mask)
    {
        GcEntry* e = &t->e[i];
        if (e->obj == obj) return e;
        if (e->obj == NULL) return NULL;
    }
}

static int gc_grow(GcTable* t)
{
    int cap = t->cap ? t->cap : 64;
    if (t->count * 2 >= cap) cap *= 2;   // else: only tombstones to drop
    GcEntry* old = t->e;
    int oldCap = t->cap;
    GcEntry* e = (GcEntry*)calloc((size_t)cap, sizeof(GcEntry));
    if (!e) return 0;
    t->e = e; t->cap = cap; t->used = 0; t->count = 0;
    unsigned mask = (unsigned)cap - 1;
    for (int k = 0; k < oldCap; k++)
    {
        if (old[k].obj == NULL || old[k].obj == GC_TOMB) continue;
        unsigned i = gc_hash(old[k].obj) & mask;
        while (e[i].obj) i = (i + 1) & mask;
        e[i] = old[k];
        t->used++; t->count++;
    }
    free(old);
    return 1;
}

/// The entry of obj, created when missing (*added = 1). NULL when out of memory.
static GcEntry* gc_put(GcTable* t, void* obj, const CS2SX_GcKind* kind, int* added)
{
    *added = 0;
    GcEntry* e = gc_find(t, obj);
    if (e) return e;
    if ((t->used + 1) * 4 > t->cap * 3 && !gc_grow(t)) return NULL;
    unsigned mask = (unsigned)t->cap - 1;
    unsigned i = gc_hash(obj) & mask;
    while (t->e[i].obj) i = (i + 1) & mask;   // tombstones are not reused: keeps find simple
    e = &t->e[i];
    e->obj = obj; e->kind = kind; e->count = 0; e->live = 0;
    t->used++; t->count++;
    *added = 1;
    return e;
}

static void gc_clear(GcTable* t)
{
    free(t->e);
    t->e = NULL; t->cap = 0; t->used = 0; t->count = 0;
}

void CS2SX_GcLeaf(void* self, CS2SX_GcVisit visit) { (void)self; (void)visit; }

void CS2SX_GcSuspect(void* obj, const CS2SX_GcKind* kind)
{
    if (!obj) return;
    int added;
    gc_put(&s_suspects, obj, kind, &added);
}

void CS2SX_GcForget(void* obj)
{
    if (s_suspects.count == 0) return;
    GcEntry* e = gc_find(&s_suspects, obj);
    if (!e) return;
    e->obj = GC_TOMB;
    s_suspects.count--;
}

// ── One collection ──────────────────────────────────────────────────────────
static GcTable s_graph;
static void** s_work;
static int s_work_count, s_work_cap;
static int s_failed;

static void gc_push(void* obj)
{
    if (s_work_count == s_work_cap)
    {
        int cap = s_work_cap ? s_work_cap * 2 : 256;
        void** w = (void**)realloc(s_work, (size_t)cap * sizeof(void*));
        if (!w) { s_failed = 1; return; }
        s_work = w; s_work_cap = cap;
    }
    s_work[s_work_count++] = obj;
}

static int gc_rc(void* obj, const CS2SX_GcKind* kind)
{
    return *(int*)((char*)obj + kind->rc_offset);
}

static GcEntry* gc_node(void* obj, const CS2SX_GcKind* kind)
{
    int added;
    GcEntry* e = gc_put(&s_graph, obj, kind, &added);
    if (!e) { s_failed = 1; return NULL; }
    if (added) { e->count = gc_rc(obj, kind); gc_push(obj); }
    return e;
}

static void gc_visit_subtract(void* child, const CS2SX_GcKind* kind)
{
    if (!child) return;
    GcEntry* e = gc_node(child, kind);
    if (e) e->count--;
}

static void gc_visit_live(void* child, const CS2SX_GcKind* kind)
{
    (void)kind;
    if (!child) return;
    GcEntry* e = gc_find(&s_graph, child);
    if (e && !e->live) { e->live = 1; gc_push(child); }
}

int CS2SX_GcCollect(void)
{
    if (s_collecting || s_suspects.count == 0) return 0;
    s_collecting = 1;
    s_failed = 0;
    s_work_count = 0;

    // 1. The graph below the suspects, with the references from inside subtracted.
    GcTable suspects = s_suspects;
    memset(&s_suspects, 0, sizeof(s_suspects));
    for (int i = 0; i < suspects.cap; i++)
        if (suspects.e[i].obj && suspects.e[i].obj != GC_TOMB)
            gc_node(suspects.e[i].obj, suspects.e[i].kind);
    gc_clear(&suspects);
    while (s_work_count > 0 && !s_failed)
    {
        void* obj = s_work[--s_work_count];
        // (The table may move while children are added: look the kind up first.)
        const CS2SX_GcKind* kind = gc_find(&s_graph, obj)->kind;
        kind->gc(obj, gc_visit_subtract);
    }

    // 2. Referenced from outside → alive, with everything it reaches.
    int freed = 0;
    if (!s_failed)
    {
        s_work_count = 0;
        for (int i = 0; i < s_graph.cap && !s_failed; i++)
        {
            GcEntry* e = &s_graph.e[i];
            if (e->obj && e->count > 0) { e->live = 1; gc_push(e->obj); }
        }
        while (s_work_count > 0 && !s_failed)
        {
            void* obj = s_work[--s_work_count];
            gc_find(&s_graph, obj)->kind->gc(obj, gc_visit_live);
        }
    }

    // 3. The rest is garbage: pin, cut the references, unpin.
    if (!s_failed)
    {
        for (int i = 0; i < s_graph.cap; i++)
        {
            GcEntry* e = &s_graph.e[i];
            if (!e->obj || e->live) continue;
            (*(int*)((char*)e->obj + e->kind->rc_offset))++;
            freed++;
        }
        if (freed > 0)
        {
            for (int i = 0; i < s_graph.cap; i++)
            {
                GcEntry* e = &s_graph.e[i];
                if (e->obj && !e->live) e->kind->gc(e->obj, NULL);
            }
            for (int i = 0; i < s_graph.cap; i++)
            {
                GcEntry* e = &s_graph.e[i];
                if (e->obj && !e->live) e->kind->release(e->obj);
            }
        }
    }

    gc_clear(&s_graph);
    s_work_count = 0;
    s_collecting = 0;
    return freed;
}

void CS2SX_GcFrame(void)
{
    // Looking costs time proportional to what the suspects reach, so not every
    // frame — unless many have piled up.
    s_frames++;
    if (s_suspects.count == 0) return;
    if ((s_frames & 15) != 0 && s_suspects.count < 4096) return;
    // (Between frames nothing on the stack points at objects: what the cut
    // references released can go right away.)
    if (CS2SX_GcCollect() > 0) CS2SX_DrainReleases();
}
