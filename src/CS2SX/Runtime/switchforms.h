#pragma once
#include <switch.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <strings.h>
#include <stdbool.h>
#include <ctype.h>

extern char _cs2sx_strbuf[1024];

// ============================================================================
// Arrays (C# T[])
//
// A C# array is a plain C pointer to its first element (so a[i] is ordinary C
// indexing), preceded in memory by a 16-byte header that carries the length:
//
//     [ CS2SX_ArrHdr | e0 | e1 | ... ]
//                      ^-- T* the program works with
//
// a.Length → CS2SX_ArrLen(a) works wherever the array travels: parameters,
// fields, return values. The 16-byte header keeps the elements 16-aligned.
// Arrays are created by the transpiler via
//   CS2SX_ArrNew(n, sizeof(T))                 heap, zero-initialised (new T[n])
//   CS2SX_ArrFromInit((T[]){...}, n, sizeof(T)) heap copy of an initializer
//   CS2SX_ARR_LOCAL(T, name, n, ...)           stack array for non-escaping locals
// The magic word lets CS2SX_ArrLen return 0 instead of garbage for a pointer that
// was not created as a C# array (e.g. raw native memory).
// ============================================================================

#define CS2SX_ARR_MAGIC       0xA77A1A5Eu   // heap array (owned, freeable)
#define CS2SX_ARR_MAGIC_STACK 0xA77A57ACu   // stack array (never freed)

typedef struct CS2SX_ArrHdr
{
    u32 magic;
    s32 length;
    u64 _reserved;
} CS2SX_ArrHdr;

void* CS2SX_ArrNew(int count, size_t elemSize);

static inline void* CS2SX_ArrFromInit(const void* src, int count, size_t elemSize)
{
    void* a = CS2SX_ArrNew(count, elemSize);
    if (a && src && count > 0) memcpy(a, src, (size_t)count * elemSize);
    return a;
}

static inline int CS2SX_ArrLen(const void* a)
{
    if (!a) return 0;
    const CS2SX_ArrHdr* h = (const CS2SX_ArrHdr*)a - 1;
    return (h->magic == CS2SX_ARR_MAGIC || h->magic == CS2SX_ARR_MAGIC_STACK) ? h->length : 0;
}

/// Bounds check of a[i] like C#: null → NullReferenceException, i outside
/// [0, Length) → IndexOutOfRangeException. Pointers that are not C# arrays (no
/// magic header, e.g. native memory) are not checked.
static inline void CS2SX_ArrCheck(const void* a, int i)
{
    if (__builtin_expect(!a, 0)) CS2SX_ThrowNullRef();
    const CS2SX_ArrHdr* h = (const CS2SX_ArrHdr*)a - 1;
    if ((h->magic == CS2SX_ARR_MAGIC || h->magic == CS2SX_ARR_MAGIC_STACK)
        && __builtin_expect((unsigned)i >= (unsigned)h->length, 0))
        CS2SX_ThrowIndexOutOfRange();
}

/// a[i] with the check, still an lvalue (a[i] = v, a[i]++, &a[i]); a and i are
/// evaluated once.
/// (&(a)[0] makes fixed-size C arrays — e.g. TouchState.x — decay to a pointer.)
#define CS2SX_AT(a, i) (*({ __typeof__(&(a)[0]) _cs2sx_a = (a); int _cs2sx_i = (int)(i); \
    CS2SX_ArrCheck(_cs2sx_a, _cs2sx_i); _cs2sx_a + _cs2sx_i; }))

/// Frees a heap array created by CS2SX_ArrNew/CS2SX_ArrFromInit (NULL-safe).
void CS2SX_ArrFree(void* a);

/// Array.Resize: a new heap array of newCount elements holding the first
/// min(old, new) elements; the old array is freed if it was a heap array.
void* CS2SX_ArrResize(void* a, int newCount, size_t elemSize);

/// Stack-allocated array with a header, for locals that never leave their method.
/// Usage: CS2SX_ARR_LOCAL(int, grid, 4);  CS2SX_ARR_LOCAL(int, d, 3, 9, 4, 7);
/// The stack header uses CS2SX_ARR_MAGIC_STACK so CS2SX_ArrFree ignores it.
#define CS2SX_ARR_LOCAL(T, name, n, ...)                                           \
    struct { CS2SX_ArrHdr h; T d[(n) > 0 ? (n) : 1]; } name##_arr =               \
        { { CS2SX_ARR_MAGIC_STACK, (n), 0 }, { __VA_ARGS__ } };                    \
    T* name = name##_arr.d

/// Static (global) array with a header: CS2SX_ARR_STATIC(int, Cls_table, 8);
/// defines Cls_table as a T* to zero-initialised (or listed) elements.
#define CS2SX_ARR_STATIC(T, name, n, ...)                                          \
    static struct { CS2SX_ArrHdr h; T d[(n) > 0 ? (n) : 1]; } name##_arr =        \
        { { CS2SX_ARR_MAGIC_STACK, (n), 0 }, { __VA_ARGS__ } };                    \
    T* name = name##_arr.d

// ============================================================================
// Per-Frame String Arena
//
// Replaces the 64-slot ring buffer with a flat bump-pointer arena.
// cs2sx_frame_begin() resets the arena at the start of every game-loop frame;
// all string temporaries are valid until the next frame begins.
// Capacity: CS2SX_ARENA_SIZE bytes per frame (default 512 KB).
// Each _cs2sx_next_buf() call hands out CS2SX_STRBUF_SIZE bytes, 8-byte aligned.
// If the arena fills within a single frame the allocator wraps (same behaviour
// as the old ring-buffer overflow — silent wrap rather than a crash).
// ============================================================================

#define CS2SX_STRBUF_SIZE  1024
#define CS2SX_ARENA_SIZE   (512 * 1024)

extern char   _cs2sx_arena[CS2SX_ARENA_SIZE];
extern size_t _cs2sx_arena_pos;

static inline void cs2sx_frame_begin(void)
{
    _cs2sx_arena_pos = 0;
}

char* _cs2sx_next_buf(void);

// FIX: Heap-String für lange Lebensdauer (Liste von Strings etc.)
// Caller ist verantwortlich für free(), wird aber in der Praxis durch
// die List_str_Free() Konvention abgedeckt wenn die Liste selbst gefree'd wird.
char* _cs2sx_heap_strdup(const char* src);

// Frees a heap string returned by _cs2sx_heap_strdup. NULL-safe. Used by Sys.FreeStr
// for manual cleanup of owned string fields (the auto-destructor skips string fields,
// since they may alias static literals). Never pass a string literal here.
static inline void CS2SX_FreeStr(const char* s)
{
    if (s) free((void*)s);
}

// ============================================================================
// Action
// ============================================================================

typedef void (*Action)(void*);

// ============================================================================
// Control
// ============================================================================

#define FORM_MAX_CONTROLS 64

typedef struct Control Control;
struct Control
{
    int   x, y, width, height;
    int   visible;
    int   focusable;
    int   focused;   // moved here from Button so all focusable controls work without casts
    void* context;
    void  (*Draw)  (Control* self);
    void  (*Update)(Control* self, u64 kDown, u64 kHeld);
    void  (*Free)  (Control* self);  // virtual destructor — NULL → plain free()
};

// ============================================================================
// Label
// ============================================================================

typedef struct Label Label;
struct Label
{
    Control base;
    char    text[256];
};

static inline void Label_SetText(Label* l, const char* s)
{
    if (!l || !s) return;
    strncpy(l->text, s, sizeof(l->text) - 1);
    l->text[sizeof(l->text) - 1] = '\0';
}

static inline void Label_Draw(Control* self)
{
    if (!self || !self->visible) return;
    Label* l = (Label*)self;
    printf("\033[%d;%dH%s", self->y, self->x, l->text);
}

Label* Label_New(const char* text);

static inline void Label_Free(Label* l)
{
    if (!l) return;
    free(l);
}

// ============================================================================
// Button
// ============================================================================

typedef struct Button Button;
struct Button
{
    Control     base;
    const char* text;
    // focused moved to Control.base.focused — access via base.focused or self->focused via Control*
    Action_t    OnClick;   // C# Action (cs2sx_delegate.h)
};

void Button_Draw(Control* self);

void Button_Update(Control* self, u64 kDown, u64 kHeld);

Button* Button_New(const char* text);

// ============================================================================
// ProgressBar
// ============================================================================

typedef struct ProgressBar ProgressBar;
struct ProgressBar
{
    Control base;
    int     value;
    int     width_chars;
};

void ProgressBar_Draw(Control* self);

ProgressBar* ProgressBar_New(int width_chars);

// ============================================================================
// StringBuilder
// ============================================================================

typedef struct
{
    char* buf;
    int   length;
    int   capacity;
} StringBuilder;

StringBuilder* StringBuilder_New(int capacity);

static inline void StringBuilder_Free(StringBuilder* sb)
{
    if (!sb) return;
    free(sb->buf);
    free(sb);
}

void StringBuilder__grow(StringBuilder* sb, int needed);

StringBuilder* StringBuilder_AppendStr(StringBuilder* sb, const char* s);

StringBuilder* StringBuilder_AppendChar(StringBuilder* sb, char c);

StringBuilder* StringBuilder_AppendInt(StringBuilder* sb, int val);

StringBuilder* StringBuilder_AppendUInt(StringBuilder* sb, unsigned int val);

StringBuilder* StringBuilder_AppendFloat(StringBuilder* sb, float val);

static inline StringBuilder* StringBuilder_AppendLine(StringBuilder* sb, const char* s)
{
    StringBuilder_AppendStr(sb, s);
    StringBuilder_AppendChar(sb, '\n');
    return sb;
}

StringBuilder* StringBuilder_AppendLong(StringBuilder* sb, long long val);

StringBuilder* StringBuilder_AppendULong(StringBuilder* sb, unsigned long long val);

StringBuilder* StringBuilder_AppendDouble(StringBuilder* sb, double val);

static inline StringBuilder* StringBuilder_AppendLineInt(StringBuilder* sb, int val)
{
    StringBuilder_AppendInt(sb, val);
    StringBuilder_AppendChar(sb, '\n');
    return sb;
}

static inline StringBuilder* StringBuilder_AppendLineUInt(StringBuilder* sb, unsigned int val)
{
    StringBuilder_AppendUInt(sb, val);
    StringBuilder_AppendChar(sb, '\n');
    return sb;
}

static inline StringBuilder* StringBuilder_AppendLineLong(StringBuilder* sb, long long val)
{
    StringBuilder_AppendLong(sb, val);
    StringBuilder_AppendChar(sb, '\n');
    return sb;
}

static inline StringBuilder* StringBuilder_AppendLineULong(StringBuilder* sb, unsigned long long val)
{
    StringBuilder_AppendULong(sb, val);
    StringBuilder_AppendChar(sb, '\n');
    return sb;
}

static inline StringBuilder* StringBuilder_AppendLineFloat(StringBuilder* sb, float val)
{
    StringBuilder_AppendFloat(sb, val);
    StringBuilder_AppendChar(sb, '\n');
    return sb;
}

static inline StringBuilder* StringBuilder_AppendLineDouble(StringBuilder* sb, double val)
{
    StringBuilder_AppendDouble(sb, val);
    StringBuilder_AppendChar(sb, '\n');
    return sb;
}

static inline StringBuilder* StringBuilder_AppendLineChar(StringBuilder* sb, char c)
{
    StringBuilder_AppendChar(sb, c);
    StringBuilder_AppendChar(sb, '\n');
    return sb;
}

static inline void StringBuilder_Clear(StringBuilder* sb)
{
    if (!sb) return;
    sb->buf[0] = '\0';
    sb->length = 0;
}

static inline const char* StringBuilder_ToString(StringBuilder* sb)
{
    return sb ? sb->buf : "";
}

static inline int StringBuilder_IndexOf(StringBuilder* sb, const char* sub)
{
    if (!sb || !sub) return -1;
    const char* p = strstr(sb->buf, sub);
    return p ? (int)(p - sb->buf) : -1;
}

void StringBuilder_Insert(StringBuilder* sb, int index, const char* s);

void StringBuilder_Replace(StringBuilder* sb, const char* from, const char* to);

// ============================================================================
// String helpers
// ============================================================================

static inline const char* Int_ToString(int val)
{
    char* buf = _cs2sx_next_buf();
    snprintf(buf, CS2SX_STRBUF_SIZE, "%d", val);
    return buf;
}

static inline const char* UInt_ToString(unsigned int val)
{
    char* buf = _cs2sx_next_buf();
    snprintf(buf, CS2SX_STRBUF_SIZE, "%u", val);
    return buf;
}

/// double/float ToString() without a format, as .NET prints them (src/rt_number.c):
/// the shortest text that reads back as the same value — 2.5, 0.1, 1E+21, 1E-05,
/// NaN, Infinity. Also used for doubles in string concatenation and interpolation.
const char* Float_ToString(float val);
const char* Double_ToString(double val);
#define CS2SX_BoolStr(b) ((b) ? "True" : "False")

static inline const char* Long_ToString(long long val)
{
    char* buf = _cs2sx_next_buf();
    snprintf(buf, CS2SX_STRBUF_SIZE, "%lld", val);
    return buf;
}

static inline const char* ULong_ToString(unsigned long long val)
{
    char* buf = _cs2sx_next_buf();
    snprintf(buf, CS2SX_STRBUF_SIZE, "%llu", val);
    return buf;
}

static inline int String_IsNullOrEmpty(const char* s)
{
    return s == NULL || s[0] == '\0';
}

static inline int String_Contains(const char* haystack, const char* needle)
{
    if (!haystack || !needle) return 0;
    return strstr(haystack, needle) != NULL;
}

static inline int String_StartsWith(const char* s, const char* prefix)
{
    if (!s || !prefix) return 0;
    return strncmp(s, prefix, strlen(prefix)) == 0;
}

static inline int String_EndsWith(const char* s, const char* suffix)
{
    if (!s || !suffix) return 0;
    int sl = (int)strlen(s), fl = (int)strlen(suffix);
    return fl <= sl && strcmp(s + sl - fl, suffix) == 0;
}

const char* String_Trim(const char* s);

const char* String_TrimStart(const char* s);

const char* String_TrimEnd(const char* s);

const char* String_ToUpper(const char* s);

const char* String_ToLower(const char* s);

const char* String_Substring(const char* s, int start, int length);

const char* String_SubstringFrom(const char* s, int start);

static inline int String_IndexOf(const char* s, const char* sub)
{
    if (!s || !sub) return -1;
    const char* p = strstr(s, sub);
    return p ? (int)(p - s) : -1;
}

static inline int String_IndexOfChar(const char* s, char c)
{
    if (!s) return -1;
    const char* p = strchr(s, c);
    return p ? (int)(p - s) : -1;
}

int String_LastIndexOf(const char* s, const char* sub);

const char* String_Replace(const char* s, const char* from, const char* to);

const char* String_PadLeft(const char* s, int totalWidth, char padChar);

const char* String_PadRight(const char* s, int totalWidth, char padChar);

static inline int String_CompareTo(const char* a, const char* b)
{
    if (!a || !b) return 0;
    return strcmp(a, b);
}

int String_EqualsIgnoreCase(const char* a, const char* b);


#define String_Length(s) ((int)strlen(s))

// ============================================================================
// int.Parse / TryParse & co. (src/rt_number.c) — C# syntax and range rules:
// leading/trailing whitespace, optional sign, nothing else. Parse throws
// FormatException / OverflowException / ArgumentNullException; TryParse returns 0
// and stores 0 instead. Floating point also accepts a fraction, an exponent,
// thousands separators, "NaN" and "Infinity"; out of range gives ±Infinity.
// ============================================================================

#define CS2SX_PARSE_DECLARE(Name, T) \
    T Name##_Parse(const char* s); int Name##_TryParse(const char* s, T* out_val);
CS2SX_PARSE_DECLARE(CS2SX_Int, int)
CS2SX_PARSE_DECLARE(CS2SX_UInt, unsigned int)
CS2SX_PARSE_DECLARE(CS2SX_Long, long long)
CS2SX_PARSE_DECLARE(CS2SX_ULong, unsigned long long)
CS2SX_PARSE_DECLARE(CS2SX_Short, short)
CS2SX_PARSE_DECLARE(CS2SX_UShort, unsigned short)
CS2SX_PARSE_DECLARE(CS2SX_Byte, unsigned char)
CS2SX_PARSE_DECLARE(CS2SX_SByte, signed char)
CS2SX_PARSE_DECLARE(CS2SX_Float, float)
CS2SX_PARSE_DECLARE(CS2SX_Double, double)
CS2SX_PARSE_DECLARE(CS2SX_Bool, int)
#undef CS2SX_PARSE_DECLARE

// ============================================================================
// List<T>
// ============================================================================

#define CS2SX_LIST_INITIAL_CAP 8

/// Index check of List<T> accessors: a null list (count < 0) throws
/// NullReferenceException, an index outside [0, count) ArgumentOutOfRangeException.
static inline void CS2SX_ListCheck(int count, int index)
{
    if (__builtin_expect(count < 0, 0)) CS2SX_ThrowNullRef();
    if (__builtin_expect((unsigned)index >= (unsigned)count, 0)) CS2SX_ThrowArgOutOfRange("index");
}

#define CS2SX_LIST_DEFINE(T)                                                                        \
typedef struct { T* data; int count; int capacity; } List_##T;                                      \
static inline List_##T* List_##T##_New(void) {                                                      \
    List_##T* l = (List_##T*)malloc(sizeof(List_##T));                                              \
    if (!l) return NULL;                                                                            \
    l->data = (T*)malloc(CS2SX_LIST_INITIAL_CAP * sizeof(T));                                      \
    l->count = 0; l->capacity = CS2SX_LIST_INITIAL_CAP; return l; }                                \
static inline void List_##T##_Add(List_##T* l, T val) {                                            \
    if (!l) CS2SX_ThrowNullRef();                                                                                 \
    if (l->count >= l->capacity) {                                                                  \
        l->capacity *= 2;                                                                           \
        l->data = (T*)realloc(l->data, l->capacity * sizeof(T)); }                                 \
    l->data[l->count++] = val; }                                                                    \
static inline T    List_##T##_Get(List_##T* l, int i) {                                            \
    CS2SX_ListCheck(l ? l->count : -1, i); return l->data[i]; }                                     \
static inline void List_##T##_Set(List_##T* l, int i, T val) {                                     \
    CS2SX_ListCheck(l ? l->count : -1, i); l->data[i] = val; }                                      \
static inline void List_##T##_Insert(List_##T* l, int idx, T val) {                                \
    CS2SX_ListCheck(l ? l->count + 1 : -1, idx);                                                    \
    List_##T##_Add(l, val);                                                                         \
    for (int _i = l->count - 1; _i > idx; _i--) l->data[_i] = l->data[_i - 1];                    \
    l->data[idx] = val; }                                                                           \
static inline int  List_##T##_Count(List_##T* l)       { return l ? l->count : 0; }                \
static inline void List_##T##_Clear(List_##T* l)       { if (l) l->count = 0; }                    \
static inline void List_##T##_Free(List_##T* l)        { if (l) { free(l->data); free(l); } }      \
static inline int  List_##T##_Contains(List_##T* l, T val) {                                       \
    for (int _i = 0; _i < l->count; _i++) { if (l->data[_i] == val) return 1; } return 0; }       \
static inline int  List_##T##_IndexOf(List_##T* l, T val) {                                        \
    for (int _i = 0; _i < l->count; _i++) { if (l->data[_i] == val) return _i; } return -1; }     \
static inline void List_##T##_Remove(List_##T* l, int idx) {                                       \
    CS2SX_ListCheck(l ? l->count : -1, idx);                                                        \
    for (int _i = idx; _i < l->count - 1; _i++) l->data[_i] = l->data[_i + 1];                   \
    l->count--; }                                                                                   \
static inline void List_##T##_RemoveValue(List_##T* l, T val) {                                    \
    for (int _i = 0; _i < l->count; _i++) {                                                        \
        if (l->data[_i] == val) { List_##T##_Remove(l, _i); return; } } }                         \
static inline void List_##T##_Reverse(List_##T* l) {                                               \
    if (!l || l->count < 2) return;                                                                 \
    int _lo = 0, _hi = l->count - 1;                                                               \
    while (_lo < _hi) { T _t = l->data[_lo]; l->data[_lo] = l->data[_hi];                         \
        l->data[_hi] = _t; _lo++; _hi--; } }                                                       \
static inline void List_##T##_Sort(List_##T* l) {                                                  \
    if (!l || l->count < 2) return;                                                                 \
    for (int _i = 1; _i < l->count; _i++) {                                                        \
        T _key = l->data[_i]; int _j = _i - 1;                                                     \
        while (_j >= 0 && l->data[_j] > _key) { l->data[_j+1] = l->data[_j]; _j--; }             \
        l->data[_j+1] = _key; } }

// ── List<T> für Pointer-Typen (User-Klassen) ─────────────────────────────────
// CS2SX_LIST_DEFINE_PTR(T) erzeugt List_T mit T*-Elementen (heap-allozierte Objekte).
// Wird von _generics.h für jede List<UserClass>-Nutzung emittiert.
// Free gibt nur den Container frei, nicht die Elemente selbst.
#define CS2SX_LIST_DEFINE_PTR(T)                                                                    \
typedef struct { T** data; int count; int capacity; } List_##T;                                     \
static inline List_##T* List_##T##_New(void) {                                                      \
    List_##T* l = (List_##T*)malloc(sizeof(List_##T));                                              \
    if (!l) return NULL;                                                                            \
    l->data = (T**)malloc(CS2SX_LIST_INITIAL_CAP * sizeof(T*));                                    \
    l->count = 0; l->capacity = CS2SX_LIST_INITIAL_CAP; return l; }                                \
static inline void List_##T##_Add(List_##T* l, T* val) {                                           \
    if (!l) CS2SX_ThrowNullRef();                                                                                 \
    if (l->count >= l->capacity) {                                                                  \
        l->capacity *= 2;                                                                           \
        l->data = (T**)realloc(l->data, l->capacity * sizeof(T*)); }                               \
    l->data[l->count++] = val; }                                                                    \
static inline T*   List_##T##_Get(List_##T* l, int i) {                                            \
    CS2SX_ListCheck(l ? l->count : -1, i); return l->data[i]; }                                     \
static inline void List_##T##_Set(List_##T* l, int i, T* val) {                                    \
    CS2SX_ListCheck(l ? l->count : -1, i); l->data[i] = val; }                                      \
static inline void List_##T##_Insert(List_##T* l, int idx, T* val) {                               \
    CS2SX_ListCheck(l ? l->count + 1 : -1, idx);                                                    \
    List_##T##_Add(l, val);                                                                         \
    for (int _i = l->count - 1; _i > idx; _i--) l->data[_i] = l->data[_i - 1];                    \
    l->data[idx] = val; }                                                                           \
static inline int  List_##T##_Count(List_##T* l)       { return l ? l->count : 0; }                \
static inline void List_##T##_Clear(List_##T* l)       { if (l) l->count = 0; }                    \
static inline void List_##T##_Free(List_##T* l)        { if (l) { free(l->data); free(l); } }      \
static inline int  List_##T##_Contains(List_##T* l, T* val) {                                      \
    if (!l) CS2SX_ThrowNullRef();                                                                               \
    for (int _i = 0; _i < l->count; _i++) { if (l->data[_i] == val) return 1; } return 0; }       \
static inline int  List_##T##_IndexOf(List_##T* l, T* val) {                                       \
    if (!l) CS2SX_ThrowNullRef();                                                                              \
    for (int _i = 0; _i < l->count; _i++) { if (l->data[_i] == val) return _i; } return -1; }     \
static inline void List_##T##_Remove(List_##T* l, int idx) {                                       \
    CS2SX_ListCheck(l ? l->count : -1, idx);                                                        \
    for (int _i = idx; _i < l->count - 1; _i++) l->data[_i] = l->data[_i + 1];                   \
    l->count--; }                                                                                   \
static inline void List_##T##_RemoveValue(List_##T* l, T* val) {                                   \
    if (!l) CS2SX_ThrowNullRef();                                                                                 \
    for (int _i = 0; _i < l->count; _i++) {                                                        \
        if (l->data[_i] == val) { List_##T##_Remove(l, _i); return; } } }                         \
static inline void List_##T##_Reverse(List_##T* l) {                                               \
    if (!l || l->count < 2) return;                                                                 \
    int _lo = 0, _hi = l->count - 1;                                                               \
    while (_lo < _hi) { T* _t = l->data[_lo]; l->data[_lo] = l->data[_hi];                        \
        l->data[_hi] = _t; _lo++; _hi--; } }

CS2SX_LIST_DEFINE(int)
CS2SX_LIST_DEFINE(float)
CS2SX_LIST_DEFINE(double)
CS2SX_LIST_DEFINE(u8)
CS2SX_LIST_DEFINE(u16)
CS2SX_LIST_DEFINE(u32)
CS2SX_LIST_DEFINE(u64)
CS2SX_LIST_DEFINE(s32)
CS2SX_LIST_DEFINE(s64)

// voidptr list — fallback for List<tuple> and other opaque pointer lists
typedef void* voidptr;
CS2SX_LIST_DEFINE(voidptr)

// ============================================================================
// Stack<T> — LIFO using growable array
// ============================================================================

#define CS2SX_STACK_DEFINE(T)                                                                       \
typedef struct { T* data; int count; int capacity; } Stack_##T;                                     \
static inline Stack_##T* Stack_##T##_New(void) {                                                    \
    Stack_##T* s = (Stack_##T*)malloc(sizeof(Stack_##T));                                           \
    if (!s) return NULL;                                                                             \
    s->data = (T*)malloc(8 * sizeof(T)); s->count = 0; s->capacity = 8; return s; }                \
static inline void Stack_##T##_Push(Stack_##T* s, T val) {                                         \
    if (!s) CS2SX_ThrowNullRef();                                                                                  \
    if (s->count >= s->capacity) {                                                                   \
        s->capacity *= 2;                                                                            \
        s->data = (T*)realloc(s->data, s->capacity * sizeof(T)); }                                  \
    s->data[s->count++] = val; }                                                                    \
static inline T Stack_##T##_Pop(Stack_##T* s) {                                                     \
    if (!s) CS2SX_ThrowNullRef();                                                                    \
    if (s->count == 0) CS2SX_ThrowInvalidOp("Stack empty.");                                         \
    return s->data[--s->count]; }                                                                    \
static inline T Stack_##T##_Peek(Stack_##T* s) {                                                    \
    if (!s) CS2SX_ThrowNullRef();                                                                    \
    if (s->count == 0) CS2SX_ThrowInvalidOp("Stack empty.");                                         \
    return s->data[s->count - 1]; }                                                                  \
static inline void Stack_##T##_Clear(Stack_##T* s) { if (s) s->count = 0; }                        \
static inline void Stack_##T##_Free(Stack_##T* s) { if (s) { free(s->data); free(s); } }

CS2SX_STACK_DEFINE(int)
CS2SX_STACK_DEFINE(float)
CS2SX_STACK_DEFINE(double)

// ============================================================================
// Queue<T> — FIFO using circular buffer
// ============================================================================

#define CS2SX_QUEUE_DEFINE(T)                                                                       \
typedef struct { T* data; int head; int tail; int count; int capacity; } Queue_##T;                 \
static inline Queue_##T* Queue_##T##_New(void) {                                                    \
    Queue_##T* q = (Queue_##T*)malloc(sizeof(Queue_##T));                                           \
    if (!q) return NULL;                                                                             \
    q->data = (T*)malloc(8 * sizeof(T));                                                             \
    q->head = 0; q->tail = 0; q->count = 0; q->capacity = 8; return q; }                           \
static inline void Queue_##T##_Enqueue(Queue_##T* q, T val) {                                       \
    if (!q) CS2SX_ThrowNullRef();                                                                                   \
    if (q->count >= q->capacity) {                                                                    \
        int nc = q->capacity * 2;                                                                     \
        T* nd = (T*)malloc(nc * sizeof(T));                                                           \
        for (int _i = 0; _i < q->count; _i++)                                                        \
            nd[_i] = q->data[(q->head + _i) % q->capacity];                                          \
        free(q->data); q->data = nd; q->head = 0; q->tail = q->count; q->capacity = nc; }            \
    q->data[q->tail] = val; q->tail = (q->tail + 1) % q->capacity; q->count++; }                   \
static inline T Queue_##T##_Dequeue(Queue_##T* q) {                                                  \
    if (!q) CS2SX_ThrowNullRef();                                                                     \
    if (q->count == 0) CS2SX_ThrowInvalidOp("Queue empty.");                                          \
    T val = q->data[q->head]; q->head = (q->head + 1) % q->capacity; q->count--; return val; }      \
static inline T Queue_##T##_Peek(Queue_##T* q) {                                                     \
    if (!q) CS2SX_ThrowNullRef();                                                                     \
    if (q->count == 0) CS2SX_ThrowInvalidOp("Queue empty.");                                          \
    return q->data[q->head]; }                                                                        \
static inline void Queue_##T##_Clear(Queue_##T* q) {                                                 \
    if (q) { q->head = 0; q->tail = 0; q->count = 0; } }                                             \
static inline void Queue_##T##_Free(Queue_##T* q) { if (q) { free(q->data); free(q); } }

CS2SX_QUEUE_DEFINE(int)
CS2SX_QUEUE_DEFINE(float)
CS2SX_QUEUE_DEFINE(double)

// ============================================================================
// HashSet<T> — sorted unique array (suitable for small-to-medium sets)
// ============================================================================

#define CS2SX_HASHSET_DEFINE(T)                                                                     \
typedef struct { T* data; int count; int capacity; } HashSet_##T;                                   \
static inline HashSet_##T* HashSet_##T##_New(void) {                                                \
    HashSet_##T* s = (HashSet_##T*)malloc(sizeof(HashSet_##T));                                     \
    if (!s) return NULL;                                                                              \
    s->data = (T*)malloc(8 * sizeof(T)); s->count = 0; s->capacity = 8; return s; }                \
static inline int HashSet_##T##_Contains(HashSet_##T* s, T val) {                                   \
    if (!s) CS2SX_ThrowNullRef();                                                                                 \
    for (int _i = 0; _i < s->count; _i++) if (s->data[_i] == val) return 1; return 0; }            \
static inline int HashSet_##T##_Add(HashSet_##T* s, T val) {                                        \
    if (!s || HashSet_##T##_Contains(s, val)) return 0;                                              \
    if (s->count >= s->capacity) {                                                                    \
        s->capacity *= 2;                                                                             \
        s->data = (T*)realloc(s->data, s->capacity * sizeof(T)); }                                   \
    s->data[s->count++] = val; return 1; }                                                           \
static inline int HashSet_##T##_Remove(HashSet_##T* s, T val) {                                     \
    if (!s) CS2SX_ThrowNullRef();                                                                                 \
    for (int _i = 0; _i < s->count; _i++) if (s->data[_i] == val) {                                 \
        for (int _j = _i; _j < s->count-1; _j++) s->data[_j] = s->data[_j+1];                      \
        s->count--; return 1; } return 0; }                                                           \
static inline void HashSet_##T##_Clear(HashSet_##T* s) { if (s) s->count = 0; }                    \
static inline void HashSet_##T##_Free(HashSet_##T* s) { if (s) { free(s->data); free(s); } }      \
static inline void HashSet_##T##_UnionWith(HashSet_##T* dst, HashSet_##T* src) {                   \
    if (!dst || !src) return;                                                                        \
    for (int _i = 0; _i < src->count; _i++) HashSet_##T##_Add(dst, src->data[_i]); }               \
static inline void HashSet_##T##_IntersectWith(HashSet_##T* dst, HashSet_##T* src) {               \
    if (!dst || !src) return;                                                                        \
    int _n = 0;                                                                                      \
    for (int _i = 0; _i < dst->count; _i++)                                                         \
        if (HashSet_##T##_Contains(src, dst->data[_i])) dst->data[_n++] = dst->data[_i];           \
    dst->count = _n; }                                                                               \
static inline void HashSet_##T##_ExceptWith(HashSet_##T* dst, HashSet_##T* src) {                  \
    if (!dst || !src) return;                                                                        \
    for (int _i = 0; _i < src->count; _i++) HashSet_##T##_Remove(dst, src->data[_i]); }

CS2SX_HASHSET_DEFINE(int)
CS2SX_HASHSET_DEFINE(float)

// ── qsort comparison helpers (used by Array.Sort transpilation) ───────────────
static inline int _cs2sx_cmp_int(const void* a, const void* b)
{ return (*(const int*)a > *(const int*)b) - (*(const int*)a < *(const int*)b); }
static inline int _cs2sx_cmp_uint(const void* a, const void* b)
{ return (*(const unsigned int*)a > *(const unsigned int*)b) - (*(const unsigned int*)a < *(const unsigned int*)b); }
static inline int _cs2sx_cmp_long(const void* a, const void* b)
{ return (*(const long long*)a > *(const long long*)b) - (*(const long long*)a < *(const long long*)b); }
static inline int _cs2sx_cmp_float(const void* a, const void* b)
{ return (*(const float*)a > *(const float*)b) - (*(const float*)a < *(const float*)b); }
static inline int _cs2sx_cmp_double(const void* a, const void* b)
{ return (*(const double*)a > *(const double*)b) - (*(const double*)a < *(const double*)b); }
static inline int _cs2sx_cmp_str(const void* a, const void* b)
{ return strcmp(*(const char* const*)a, *(const char* const*)b); }

// ── List<string> ─────────────────────────────────────────────────────────────
// FIX: List_str speichert Heap-Kopien der Strings statt Pool-Pointer.
// Das verhindert den Lifetime-Bug bei String_Split und File_ReadAllLines.

typedef struct { char** data; int count; int capacity; } List_str;

List_str* List_str_New(void);

// FIX: Add nimmt const char* und legt eine Heap-Kopie an
void List_str_Add(List_str* l, const char* val);

static inline const char* List_str_Get(List_str* l, int i) {
    CS2SX_ListCheck(l ? l->count : -1, i); return l->data[i]; }
static inline int          List_str_Count(List_str* l) { return l ? l->count : 0; }
void List_str_Insert(List_str* l, int idx, const char* val);
static inline void         List_str_Set(List_str* l, int i, const char* val) {
    CS2SX_ListCheck(l ? l->count : -1, i);
    free(l->data[i]);
    l->data[i] = val ? _cs2sx_heap_strdup(val) : NULL; }

static inline void List_str_Clear(List_str* l)
{
    if (!l) return;
    for (int i = 0; i < l->count; i++) { free(l->data[i]); l->data[i] = NULL; }
    l->count = 0;
}

// FIX: Free gibt jetzt auch alle Heap-String-Kopien frei
void List_str_Free(List_str* l);

/// List<string> → C# string[] (header array). Ownership of the strings moves to
/// the array; the list container itself is released. Used where a runtime helper
/// builds a List_str but the C# API returns string[] (Split, ReadAllLines, GetFiles).
const char** CS2SX_StrArr_FromList(List_str* l);

void List_str_Remove(List_str* l, int idx);

static inline void List_str_RemoveValue(List_str* l, const char* val)
{
    for (int _i = 0; _i < l->count; _i++)
        if (l->data[_i] && strcmp(l->data[_i], val) == 0) { List_str_Remove(l, _i); return; }
}

static inline int List_str_Contains(List_str* l, const char* val)
{
    for (int _i = 0; _i < l->count; _i++)
        if (l->data[_i] && strcmp(l->data[_i], val) == 0) return 1;
    return 0;
}

static inline int List_str_IndexOf(List_str* l, const char* val)
{
    for (int _i = 0; _i < l->count; _i++)
        if (l->data[_i] && strcmp(l->data[_i], val) == 0) return _i;
    return -1;
}

static inline void List_str_Reverse(List_str* l)
{
    if (!l || l->count < 2) return;
    int lo = 0, hi = l->count - 1;
    while (lo < hi) { char* t = l->data[lo]; l->data[lo] = l->data[hi]; l->data[hi] = t; lo++; hi--; }
}

void List_str_Sort(List_str* l);

// ── String_Join / String_Split ────────────────────────────────────────────────

const char* String_Join(const char* sep, List_str* list);

/// string.Join(sep, string[]) — joins a C# string array (length from its header).
static inline const char* String_JoinArr(const char* sep, const char** arr)
{
    int n = CS2SX_ArrLen(arr);
    List_str view = { (char**)arr, n, n };   // read-only view, no copy
    return String_Join(sep, &view);
}

// FIX: String_Split legt Heap-Kopien an (via List_str_Add) statt Pool-Slots.
// Kein static-Buffer mehr — kein Aliasing bei verschachtelten Aufrufen.
List_str* String_Split(const char* s, const char* sep);

// StringSplitOptions.RemoveEmptyEntries-Variante
List_str* String_Split_RemoveEmpty(const char* s, const char* sep);

// C#'s parameterless string.Split() splits on any run of whitespace and
// drops empty entries (e.g. "  a  b ".Split() => ["a","b"]).
List_str* String_Split_Whitespace(const char* s);

// ============================================================================
// Dictionary<TKey, TValue>
// ============================================================================

#define CS2SX_DICT_INITIAL_CAP 8

#define CS2SX_DICT_DEFINE(K, V)                                                                         \
typedef struct { K* keys; V* vals; int count; int capacity; } Dict_##K##_##V;                           \
static inline Dict_##K##_##V* Dict_##K##_##V##_New(void) {                                             \
    Dict_##K##_##V* d = (Dict_##K##_##V*)malloc(sizeof(Dict_##K##_##V));                               \
    if (!d) return NULL;                                                                                \
    d->keys = (K*)malloc(CS2SX_DICT_INITIAL_CAP * sizeof(K));                                          \
    d->vals = (V*)malloc(CS2SX_DICT_INITIAL_CAP * sizeof(V));                                          \
    d->count = 0; d->capacity = CS2SX_DICT_INITIAL_CAP; return d; }                                    \
static inline int Dict_##K##_##V##_ContainsKey(Dict_##K##_##V* d, K key);                            \
static inline void Dict_##K##_##V##_Add(Dict_##K##_##V* d, K key, V val) {                             \
    if (!d) CS2SX_ThrowNullRef();                                                                       \
    if (Dict_##K##_##V##_ContainsKey(d, key)) CS2SX_ThrowDuplicateKey();                               \
    if (d->count >= d->capacity) {                                                                      \
        d->capacity *= 2;                                                                               \
        d->keys = (K*)realloc(d->keys, d->capacity * sizeof(K));                                        \
        d->vals = (V*)realloc(d->vals, d->capacity * sizeof(V)); }                                      \
    d->keys[d->count] = key; d->vals[d->count] = val; d->count++; }                                    \
static inline int Dict_##K##_##V##_ContainsKey(Dict_##K##_##V* d, K key) {                             \
    for (int _i = 0; _i < d->count; _i++) { if (d->keys[_i] == key) return 1; } return 0; }           \
static inline int Dict_##K##_##V##_TryGetValue(Dict_##K##_##V* d, K key, V* out_val) {                 \
    for (int _i = 0; _i < d->count; _i++) {                                                            \
        if (d->keys[_i] == key) { *out_val = d->vals[_i]; return 1; } } return 0; }                    \
static inline V Dict_##K##_##V##_Get(Dict_##K##_##V* d, K key) {                                       \
    V _v; memset(&_v, 0, sizeof(V));                                                                    \
    if (!d) CS2SX_ThrowNullRef();                                                                       \
    if (!Dict_##K##_##V##_TryGetValue(d, key, &_v)) CS2SX_ThrowKeyNotFound(); return _v; }             \
static inline void Dict_##K##_##V##_Set(Dict_##K##_##V* d, K key, V val) {                             \
    for (int _i = 0; _i < d->count; _i++) {                                                            \
        if (d->keys[_i] == key) { d->vals[_i] = val; return; } }                                       \
    Dict_##K##_##V##_Add(d, key, val); }                                                                \
static inline void Dict_##K##_##V##_Remove(Dict_##K##_##V* d, K key) {                                 \
    for (int _i = 0; _i < d->count; _i++) {                                                            \
        if (d->keys[_i] == key) {                                                                       \
            for (int _j = _i; _j < d->count - 1; _j++) {                                               \
                d->keys[_j] = d->keys[_j+1]; d->vals[_j] = d->vals[_j+1]; }                           \
            d->count--; return; } } }                                                                   \
static inline void Dict_##K##_##V##_Clear(Dict_##K##_##V* d) { if (d) d->count = 0; }                  \
static inline void Dict_##K##_##V##_Free(Dict_##K##_##V* d)  {                                         \
    if (d) { free(d->keys); free(d->vals); free(d); } }

#define CS2SX_DICT_DEFINE_STR_KEY(V)                                                                    \
typedef struct { const char** keys; V* vals; int count; int capacity; } Dict_str_##V;                   \
static inline Dict_str_##V* Dict_str_##V##_New(void) {                                                  \
    Dict_str_##V* d = (Dict_str_##V*)malloc(sizeof(Dict_str_##V));                                      \
    if (!d) return NULL;                                                                                \
    d->keys = (const char**)malloc(CS2SX_DICT_INITIAL_CAP * sizeof(const char*));                      \
    d->vals = (V*)malloc(CS2SX_DICT_INITIAL_CAP * sizeof(V));                                          \
    d->count = 0; d->capacity = CS2SX_DICT_INITIAL_CAP; return d; }                                    \
static inline int Dict_str_##V##_ContainsKey(Dict_str_##V* d, const char* key);                         \
static inline void Dict_str_##V##_Add(Dict_str_##V* d, const char* key, V val) {                        \
    if (!d) CS2SX_ThrowNullRef();                                                                       \
    if (!key) CS2SX_ThrowArgNull("key");                                                                \
    if (Dict_str_##V##_ContainsKey(d, key)) CS2SX_ThrowDuplicateKey();                                 \
    if (d->count >= d->capacity) {                                                                      \
        d->capacity *= 2;                                                                               \
        d->keys = (const char**)realloc(d->keys, d->capacity * sizeof(const char*));                    \
        d->vals = (V*)realloc(d->vals, d->capacity * sizeof(V)); }                                      \
    d->keys[d->count] = key; d->vals[d->count] = val; d->count++; }                                    \
static inline int Dict_str_##V##_ContainsKey(Dict_str_##V* d, const char* key) {                        \
    for (int _i = 0; _i < d->count; _i++) {                                                            \
        if (strcmp(d->keys[_i], key) == 0) return 1; } return 0; }                                     \
static inline int Dict_str_##V##_TryGetValue(Dict_str_##V* d, const char* key, V* out_val) {            \
    for (int _i = 0; _i < d->count; _i++) {                                                            \
        if (strcmp(d->keys[_i], key) == 0) { *out_val = d->vals[_i]; return 1; } } return 0; }         \
static inline V Dict_str_##V##_Get(Dict_str_##V* d, const char* key) {                                  \
    V _v; memset(&_v, 0, sizeof(V));                                                                    \
    if (!d) CS2SX_ThrowNullRef();                                                                       \
    if (!key) CS2SX_ThrowArgNull("key");                                                                \
    if (!Dict_str_##V##_TryGetValue(d, key, &_v)) CS2SX_ThrowKeyNotFound(); return _v; }              \
static inline void Dict_str_##V##_Set(Dict_str_##V* d, const char* key, V val) {                        \
    for (int _i = 0; _i < d->count; _i++) {                                                            \
        if (strcmp(d->keys[_i], key) == 0) { d->vals[_i] = val; return; } }                            \
    Dict_str_##V##_Add(d, key, val); }                                                                  \
static inline void Dict_str_##V##_Remove(Dict_str_##V* d, const char* key) {                            \
    for (int _i = 0; _i < d->count; _i++) {                                                            \
        if (strcmp(d->keys[_i], key) == 0) {                                                            \
            for (int _j = _i; _j < d->count - 1; _j++) {                                               \
                d->keys[_j] = d->keys[_j+1]; d->vals[_j] = d->vals[_j+1]; }                           \
            d->count--; return; } } }                                                                   \
static inline void Dict_str_##V##_Clear(Dict_str_##V* d) { if (d) d->count = 0; }                       \
static inline void Dict_str_##V##_Free(Dict_str_##V* d)  {                                              \
    if (d) { free(d->keys); free(d->vals); free(d); } }

CS2SX_DICT_DEFINE(int, int)
CS2SX_DICT_DEFINE(int, float)
CS2SX_DICT_DEFINE_STR_KEY(int)
CS2SX_DICT_DEFINE_STR_KEY(float)
CS2SX_DICT_DEFINE_STR_KEY(u8)
CS2SX_DICT_DEFINE_STR_KEY(u32)
CS2SX_DICT_DEFINE_STR_KEY(u64)

typedef struct { const char** keys; const char** vals; int count; int capacity; } Dict_str_str;
Dict_str_str* Dict_str_str_New(void);
void Dict_str_str_Add(Dict_str_str* d, const char* key, const char* val);
static inline int Dict_str_str_ContainsKey(Dict_str_str* d, const char* key) {
    for (int _i = 0; _i < d->count; _i++) { if (strcmp(d->keys[_i], key) == 0) return 1; } return 0;
}
static inline int Dict_str_str_TryGetValue(Dict_str_str* d, const char* key, const char** out_val) {
    for (int _i = 0; _i < d->count; _i++) {
        if (strcmp(d->keys[_i], key) == 0) { *out_val = d->vals[_i]; return 1; }
    } return 0;
}
static inline const char* Dict_str_str_Get(Dict_str_str* d, const char* key) {
    const char* v = NULL;
    if (!d) CS2SX_ThrowNullRef();
    if (!key) CS2SX_ThrowArgNull("key");
    if (!Dict_str_str_TryGetValue(d, key, &v)) CS2SX_ThrowKeyNotFound();
    return v;
}
void Dict_str_str_Set(Dict_str_str* d, const char* key, const char* val);
void Dict_str_str_Remove(Dict_str_str* d, const char* key);
static inline void Dict_str_str_Clear(Dict_str_str* d) { if (d) d->count = 0; }
static inline void Dict_str_str_Free(Dict_str_str* d) {
    if (d) { free(d->keys); free(d->vals); free(d); }
}

// ============================================================================
// switchforms.h — File I/O Abschnitt (KOMPLETT ERSETZEN ab "// File I/O")
//
// FIXES:
//   1. CS2SX_File_ReadAllText: static globaler Buffer entfernt.
//      Jetzt: eigener Heap-Buffer pro Aufruf, Größe dynamisch nach Dateigröße.
//      Caller muss NICHT free() aufrufen — Buffer wird beim nächsten Aufruf
//      freigegeben (Last-Call-Owns-Buffer Semantik, wie vorher, aber korrekt
//      weil der alte Buffer explizit freigegeben wird bevor der neue gesetzt wird).
//
//   2. CS2SX_Dir_GetFiles / GetDirectories / GetEntries:
//      static FsDirectoryEntry-Arrays entfernt — jetzt stack-alloziert.
//      Das war ein latenter Bug: verschachtelte Aufrufe (z.B. GetFiles in
//      einem foreach über GetDirectories) überschrieben sich gegenseitig.
//
//   3. CS2SX_File_ReadAllLines: ruft ReadAllText auf und tokenisiert sofort —
//      der interne Buffer gehört ReadAllLines, kein Aliasing möglich.
// ============================================================================

// ============================================================================
// File I/O
// ============================================================================

#define CS2SX_FILE_BUF_SIZE 32768

// ============================================================================
// devoptab (stdio) path support — used for USB mass storage (libusbhsfs).
// SD-card paths are fsFs-rooted ("/switch/…", no drive prefix) and use the fast
// fsFs API below. devoptab mounts carry a "<name>:" prefix ("ums0:/…", "romfs:/…")
// and are reached through the C stdio/dirent layer. cs2sx_devoptab() picks the
// path; every fs entry point dispatches on it so USB volumes "just work".
// ============================================================================
#include <dirent.h>
#include <sys/stat.h>
#include <unistd.h>

static inline int cs2sx_devoptab(const char* p) { return p && strchr(p, ':') != NULL; }

static inline void cs2sx_join(char* out, size_t cap, const char* dir, const char* name)
{
    size_t L = strlen(dir);
    if (L > 0 && dir[L - 1] == '/') snprintf(out, cap, "%s%s", dir, name);
    else                            snprintf(out, cap, "%s/%s", dir, name);
}
static inline int cs2sx_dop_isdir(const char* path)
{
    struct stat st;
    if (stat(path, &st) != 0) return 0;
    return S_ISDIR(st.st_mode) ? 1 : 0;
}
static inline int cs2sx_dop_isfile(const char* path)
{
    struct stat st;
    if (stat(path, &st) != 0) return 0;
    return S_ISDIR(st.st_mode) ? 0 : 1;
}
// mode: 0 = all entries · 1 = files only · 2 = directories only
List_str* cs2sx_dop_entries(const char* path, int mode);
int cs2sx_dop_rmtree(const char* path);
// Binary copy through stdio — works for any mix of SD ("/…") and USB ("ums0:/…"),
// since the SD card is also reachable via the default devoptab device.
int cs2sx_dop_copy(const char* src, const char* dst);

int CS2SX_File_Exists(const char* path);

int CS2SX_Dir_Exists(const char* path);

int CS2SX_Dir_Create(const char* path);

// ============================================================================
// File.ReadAllText — a new heap string per call; FileNotFoundException if missing
// (the C# string is immutable — earlier results stay valid)
//           ODER bis zum Ende der aufrufenden Anweisung.
//           Für längere Lebensdauer: _cs2sx_heap_strdup() verwenden.
// FIX: CS2SX_File_Copy macht jetzt eine eigene Heap-Kopie vor dem Write-Aufruf,
//      sodass der statische Buffer nicht durch WriteAllText überschrieben wird.
// ============================================================================

const char* CS2SX_File_ReadAllText(const char* path);

// ============================================================================
// NSP (PFS0) inspector — parses the container header WITHOUT decryption (no keys
// needed). Exposes the file list, total content size, ticket presence and a
// title-id-based type guess. One NSP "open" at a time (for an info viewer).
// ============================================================================

#define CS2SX_NSP_MAXFILES 64
extern int  g_nsp_ok;
extern char g_nsp_tid[17];
extern int  g_nsp_type;
extern int  g_nsp_nca;
extern long g_nsp_total;
extern int  g_nsp_ticket;
extern int  g_nsp_nfiles;
extern char g_nsp_name[CS2SX_NSP_MAXFILES][64];
extern long g_nsp_size[CS2SX_NSP_MAXFILES];

int cs2sx_hexv(char c);
static inline int cs2sx_ends(const char* s, const char* suf)
{
    size_t a = strlen(s), b = strlen(suf);
    return a >= b && strcmp(s + a - b, suf) == 0;
}

int CS2SX_Nsp_Open(const char* path);
static inline int         CS2SX_Nsp_Ok(void)         { return g_nsp_ok; }
static inline const char* CS2SX_Nsp_TitleId(void)    { return g_nsp_tid; }
static inline int         CS2SX_Nsp_Type(void)       { return g_nsp_type; }
static inline int         CS2SX_Nsp_NcaCount(void)   { return g_nsp_nca; }
static inline long        CS2SX_Nsp_TotalSize(void)  { return g_nsp_total; }
static inline int         CS2SX_Nsp_HasTicket(void)  { return g_nsp_ticket; }
static inline int         CS2SX_Nsp_FileCount(void)  { return g_nsp_nfiles; }
static inline const char* CS2SX_Nsp_FileName(int i)  { if (i < 0 || i >= g_nsp_nfiles) return ""; return g_nsp_name[i]; }
static inline long        CS2SX_Nsp_FileSize(int i)  { if (i < 0 || i >= g_nsp_nfiles) return 0; return g_nsp_size[i]; }

// ============================================================================
// USB mass storage via libusbhsfs (FAT/exFAT). Only active when the lib header is
// vendored into the project (else stubs). Mounted volumes appear as devoptab
// prefixes ("ums0:") — file access on those paths goes through stdio.
// IMPORTANT: the cache below is a per-TU static (like the copy engine), so ALL
// CS2SX_Usb_* calls must originate from ONE C# class (UsbManager) to stay coherent.
// ============================================================================
#if defined(__has_include)
#  if __has_include(<usbhsfs.h>)
#    include <usbhsfs.h>
#    define CS2SX_HAVE_USB 1
#  endif
#endif

#ifdef CS2SX_HAVE_USB
#define CS2SX_USB_MAXDEV 8
static int           g_usb_inited = 0;
static UsbHsFsDevice g_usb_dev[CS2SX_USB_MAXDEV];
static unsigned int  g_usb_count = 0;

static inline int CS2SX_Usb_Init(void)
{
    if (g_usb_inited) return 1;
    if (R_FAILED(usbHsFsInitialize(0))) return 0;
    g_usb_inited = 1;
    g_usb_count = usbHsFsListMountedDevices(g_usb_dev, CS2SX_USB_MAXDEV);
    return 1;
}
static inline void CS2SX_Usb_Exit(void)
{
    if (g_usb_inited) { usbHsFsExit(); g_usb_inited = 0; g_usb_count = 0; }
}
// Polls for mount/unmount each frame. Drives off the live device COUNT rather than
// only the hotplug event — libusbhsfs mounts on a background thread, so a device
// present at boot can appear a few frames after init; the event alone is fragile.
// Returns 1 if the mounted-device set changed (so the UI can refresh).
static inline int CS2SX_Usb_Poll(void)
{
    if (!g_usb_inited) return 0;
    UEvent* ev = usbHsFsGetStatusChangeUserEvent();   // drain so it doesn't accumulate
    if (ev) waitSingle(waiterForUEvent(ev), 0);
    unsigned int now = usbHsFsGetMountedDeviceCount();
    if (now != g_usb_count)
    {
        g_usb_count = usbHsFsListMountedDevices(g_usb_dev, CS2SX_USB_MAXDEV);
        return 1;
    }
    return 0;
}
static inline int CS2SX_Usb_Count(void) { return (int)g_usb_count; }
static inline int CS2SX_Usb_Inited(void) { return g_usb_inited; }
static inline const char* CS2SX_Usb_Name(int i)     // e.g. "ums0:"
{
    if (!g_usb_inited || i < 0 || i >= (int)g_usb_count) return "";
    return g_usb_dev[i].name;
}
static inline const char* CS2SX_Usb_FsType(int i)
{
    if (!g_usb_inited || i < 0 || i >= (int)g_usb_count) return "";
    return LIBUSBHSFS_FS_TYPE_STR(g_usb_dev[i].fs_type);
}
static inline long CS2SX_Usb_Capacity(int i)
{
    if (!g_usb_inited || i < 0 || i >= (int)g_usb_count) return 0;
    return (long)g_usb_dev[i].capacity;
}
static inline int CS2SX_Usb_Unmount(int i)
{
    if (!g_usb_inited || i < 0 || i >= (int)g_usb_count) return 0;
    bool ok = usbHsFsUnmountDevice(&g_usb_dev[i], true);
    g_usb_count = usbHsFsListMountedDevices(g_usb_dev, CS2SX_USB_MAXDEV);
    return ok ? 1 : 0;
}
#else   // no libusbhsfs vendored — inert stubs so mapped calls still link if referenced
static inline int         CS2SX_Usb_Init(void)        { return 0; }
static inline void        CS2SX_Usb_Exit(void)        { }
static inline int         CS2SX_Usb_Poll(void)        { return 0; }
static inline int         CS2SX_Usb_Count(void)       { return 0; }
static inline int         CS2SX_Usb_Inited(void)      { return 0; }
static inline const char* CS2SX_Usb_Name(int i)       { (void)i; return ""; }
static inline const char* CS2SX_Usb_FsType(int i)     { (void)i; return ""; }
static inline long        CS2SX_Usb_Capacity(int i)   { (void)i; return 0; }
static inline int         CS2SX_Usb_Unmount(int i)    { (void)i; return 0; }
#endif

// FIX 3: ReadAllLines macht eine eigene Kopie des Inhalts —
// kein Aliasing mit dem ReadAllText-Buffer möglich.
List_str* CS2SX_File_ReadAllLines(const char* path);

int CS2SX_File_WriteAllText(const char* path, const char* content);

int CS2SX_File_AppendAllText(const char* path, const char* content);

int CS2SX_File_Delete(const char* path);

// FIX: File.Copy liest zuerst in eine eigene Heap-Kopie, damit der interne
//      ReadAllText-Buffer nicht durch den nachfolgenden WriteAllText-Aufruf
//      invalidiert wird.
int CS2SX_File_Copy(const char* src, const char* dst);

int CS2SX_Dir_Delete(const char* path);

// Binary-safe file copy (chunked). Unlike CS2SX_File_Copy (text-based, corrupts
// non-text data) this preserves images, NRO, archives, etc. byte-for-byte.
int CS2SX_File_CopyBinary(const char* src, const char* dst);

// Reads up to maxBytes and returns a formatted hex dump (offset · hex · ascii),
// one 16-byte row per line. Handles binary data (incl. NUL bytes). "" on failure.
const char* CS2SX_File_ReadHexDump(const char* path, int maxBytes);

// ── Chunked copy (frame-stepped, for a live progress bar) ───────────────────
// Single in-flight copy context; the app is single-threaded so one is enough.
typedef struct {
    FsFileSystem fs;
    FsFile in;
    FsFile out;
    s64    size;
    s64    off;
    char*  buf;
    int    active;
} CS2SX_CopyCtx;
extern CS2SX_CopyCtx g_cs2sx_copy;

#define CS2SX_COPY_BUF (4 * 1024 * 1024)   // 4 MB working buffer

void CS2SX_File_CopyEnd(void);

// Opens src+dst for a chunked copy. Returns total size (>=0), or -1 on error.
long long CS2SX_File_CopyBegin(const char* src, const char* dst);

// Copies up to chunkBytes more. Returns bytes copied so far (== size when done),
// or -1 on error. Caller calls CopyEnd() when done/aborting.
long long CS2SX_File_CopyStep(int chunkBytes);

// File size in bytes (-1 if missing/unreadable).
long long CS2SX_File_GetSize(const char* path);

// Last-modified time as a POSIX timestamp (0 if unavailable). Use for sorting.
long long CS2SX_File_GetMTime(const char* path);

// Renames/moves a file within the SD filesystem (cheap — no data copy).
int CS2SX_File_Rename(const char* src, const char* dst);

int CS2SX_Dir_Rename(const char* src, const char* dst);

// Recursively deletes a directory and everything in it.
int CS2SX_Dir_DeleteRecursive(const char* path);

// SD-card free / total space in bytes (-1 if unavailable).
long long CS2SX_Fs_GetFreeSpace(void);

long long CS2SX_Fs_GetTotalSpace(void);

// Recursively sums file sizes under a directory using directory ENTRIES (each
// entry already carries its size — no per-file open), so it is fast even for
// folders with many files. Returns total bytes (0 if empty/unreadable).
long long _cs2sx_dirsize_rec(FsFileSystem* fs, const char* path);

long long CS2SX_Fs_DirSize(const char* path);

// ZIP archive support — provided by a project that links the miniz extern-lib
// (externLibs/miniz). Declared here so Archive.* calls compile; the symbols are
// only required when an app actually calls them.
extern int CS2SX_Zip_Extract(const char* zipPath, const char* destDir);          // → #files, -1 err
extern int CS2SX_Zip_Compress(const char* srcPath, int isDir, const char* zipPath); // → #files, -1 err

// Frame-stepped variants — Begin*, then call Step() a few times per frame until
// Busy() is false; the UI + music keep running between entries (no thread).
extern int CS2SX_Zip_BeginExtract(const char* zipPath, const char* destDir);     // → total, -1 err
extern int CS2SX_Zip_BeginCompress(const char* list, const char* zipPath);       // list: "<isDir>\t<path>\n"…
extern int CS2SX_Zip_Step(void);                                                 // 1 = more, 0 = done
extern int CS2SX_Zip_StepBudget(int maxBytes);                                   // process ~maxBytes per call
extern int CS2SX_Zip_Busy(void);
extern int CS2SX_Zip_Result(void);                                               // → #files, -1 err, -2 cancelled
extern int CS2SX_Zip_Progress(void);                                             // entries/files processed
extern int CS2SX_Zip_Total(void);
extern void CS2SX_Zip_Cancel(void);

static inline const char* CS2SX_Dir_GetCurrent(void)
{
    return "/switch";
}

// FIX 2: Kein static FsDirectoryEntry-Array mehr — stack-alloziert.
// Das verhindert Aliasing wenn GetFiles innerhalb eines foreach über
// GetDirectories aufgerufen wird.
List_str* CS2SX_Dir_GetFiles(const char* path, const char* pattern);

// FIX 2: Ebenfalls stack-alloziert
List_str* CS2SX_Dir_GetDirectories(const char* path);

// FIX 2: Ebenfalls stack-alloziert
List_str* CS2SX_Dir_GetEntries(const char* path);

const char* CS2SX_Path_GetFileName(const char* path);

const char* CS2SX_Path_GetExtension(const char* path);

const char* CS2SX_Path_GetDirectoryName(const char* path);

static inline int CS2SX_Path_IsDirectory(const char* path)
{
    // USB/devoptab: stat is reliable. SD keeps the cheap extension heuristic.
    if (cs2sx_devoptab(path)) return cs2sx_dop_isdir(path);
    return CS2SX_Path_GetExtension(path)[0] == '\0';
}

const char* CS2SX_Path_GetFileNameWithoutExt(const char* path);

static inline int CS2SX_Path_IsPathRooted(const char* path)
{
    if (!path || !path[0]) return 0;
    return path[0] == '/' || path[0] == '\\';
}

const char* CS2SX_Path_ChangeExtension(const char* path, const char* ext);

int String_LastIndexOfChar(const char* s, char c);

#define CS2SX_RETURN_BUF_SIZE 512

// ============================================================================
// Form
// ============================================================================

typedef struct Form Form;
struct Form
{
    Control* controls[FORM_MAX_CONTROLS];
    int      count;
    int      focusedIndex;
};

static inline void Form_Add(Form* form, Control* ctrl)
{
    if (!form || !ctrl || form->count >= FORM_MAX_CONTROLS) return;
    ctrl->context = form;
    form->controls[form->count++] = ctrl;
}

void Form_InitFocus(Form* form);

void Form_MoveFocus(Form* form, int direction);

void Form_DrawAll(Form* form);

void Form_UpdateAll(Form* form, u64 kDown, u64 kHeld);

void Form_Free(Form* form);

// ============================================================================
// DateTime — wraps localtime() for basic date/time access
// ============================================================================

#include <time.h>

static inline struct tm* _cs2sx_now(void)
{
    time_t t = time(NULL);
    return localtime(&t);
}

// "YYYY-MM-DD HH:MM" for a file's last-modified time ("" if unavailable).
const char* CS2SX_File_GetModifiedString(const char* path);

#define CS2SX_DateTime_Now_Year()       (_cs2sx_now()->tm_year + 1900)
#define CS2SX_DateTime_Now_Month()      (_cs2sx_now()->tm_mon  + 1)
#define CS2SX_DateTime_Now_Day()        _cs2sx_now()->tm_mday
#define CS2SX_DateTime_Now_Hour()       _cs2sx_now()->tm_hour
#define CS2SX_DateTime_Now_Minute()     _cs2sx_now()->tm_min
#define CS2SX_DateTime_Now_Second()     _cs2sx_now()->tm_sec
#define CS2SX_DateTime_Now_DayOfWeek()  _cs2sx_now()->tm_wday
#define CS2SX_DateTime_Now_DayOfYear()  _cs2sx_now()->tm_yday
#define CS2SX_DateTime_Now_Ticks()      ((long long)armGetSystemTick())

// ============================================================================
// Stopwatch — high-precision timer using armGetSystemTick()
// ============================================================================

typedef struct CS2SX_Stopwatch CS2SX_Stopwatch;
struct CS2SX_Stopwatch
{
    uint64_t start;
    uint64_t accumulated;
    bool     running;
};

CS2SX_Stopwatch* CS2SX_Stopwatch_New(void);

CS2SX_Stopwatch* CS2SX_Stopwatch_StartNew(void);

static inline void CS2SX_Stopwatch_Start(CS2SX_Stopwatch* sw)
{
    if (!sw || sw->running) return;
    sw->start   = armGetSystemTick();
    sw->running = true;
}

static inline void CS2SX_Stopwatch_Stop(CS2SX_Stopwatch* sw)
{
    if (!sw || !sw->running) return;
    sw->accumulated += armGetSystemTick() - sw->start;
    sw->running      = false;
}

void CS2SX_Stopwatch_Reset(CS2SX_Stopwatch* sw);

void CS2SX_Stopwatch_Restart(CS2SX_Stopwatch* sw);

static inline long long CS2SX_Stopwatch_ElapsedMs(CS2SX_Stopwatch* sw)
{
    if (!sw) return 0;
    uint64_t ticks = sw->accumulated + (sw->running ? armGetSystemTick() - sw->start : 0);
    return (long long)(armTicksToNs(ticks) / 1000000ULL);
}

static inline double CS2SX_Stopwatch_ElapsedMsDouble(CS2SX_Stopwatch* sw)
{
    if (!sw) return 0.0;
    uint64_t ticks = sw->accumulated + (sw->running ? armGetSystemTick() - sw->start : 0);
    return (double)armTicksToNs(ticks) / 1000000.0;
}

static inline double CS2SX_Stopwatch_ElapsedSecDouble(CS2SX_Stopwatch* sw)
{
    if (!sw) return 0.0;
    uint64_t ticks = sw->accumulated + (sw->running ? armGetSystemTick() - sw->start : 0);
    return (double)armTicksToNs(ticks) / 1000000000.0;
}

static inline long long CS2SX_Stopwatch_ElapsedTicks(CS2SX_Stopwatch* sw)
{
    if (!sw) return 0;
    uint64_t ticks = sw->accumulated + (sw->running ? armGetSystemTick() - sw->start : 0);
    return (long long)ticks;
}

static inline void CS2SX_Stopwatch_Free(CS2SX_Stopwatch* sw) { free(sw); }

// ── TimeSpan ──────────────────────────────────────────────────────────────────
// Represents a duration in 100-nanosecond ticks (same as .NET TimeSpan.Ticks).
// DateTime subtraction produces a TimeSpan; Stopwatch.Elapsed returns one too.

typedef struct { long long ticks; } CS2SX_TimeSpan;

#define CS2SX_TICKS_PER_MS       10000LL
#define CS2SX_TICKS_PER_SEC   10000000LL
#define CS2SX_TICKS_PER_MIN  600000000LL
#define CS2SX_TICKS_PER_HOUR 36000000000LL
#define CS2SX_TICKS_PER_DAY  864000000000LL

static inline CS2SX_TimeSpan CS2SX_TimeSpan_FromMs(double ms)
{ CS2SX_TimeSpan ts; ts.ticks = (long long)(ms * CS2SX_TICKS_PER_MS); return ts; }
static inline CS2SX_TimeSpan CS2SX_TimeSpan_FromSec(double s)
{ CS2SX_TimeSpan ts; ts.ticks = (long long)(s  * CS2SX_TICKS_PER_SEC); return ts; }
static inline CS2SX_TimeSpan CS2SX_TimeSpan_FromTicks(long long t)
{ CS2SX_TimeSpan ts; ts.ticks = t; return ts; }

static inline double CS2SX_TimeSpan_TotalMs(CS2SX_TimeSpan ts)
{ return (double)ts.ticks / CS2SX_TICKS_PER_MS; }
static inline double CS2SX_TimeSpan_TotalSec(CS2SX_TimeSpan ts)
{ return (double)ts.ticks / CS2SX_TICKS_PER_SEC; }
static inline double CS2SX_TimeSpan_TotalMin(CS2SX_TimeSpan ts)
{ return (double)ts.ticks / CS2SX_TICKS_PER_MIN; }
static inline double CS2SX_TimeSpan_TotalHours(CS2SX_TimeSpan ts)
{ return (double)ts.ticks / CS2SX_TICKS_PER_HOUR; }
static inline double CS2SX_TimeSpan_TotalDays(CS2SX_TimeSpan ts)
{ return (double)ts.ticks / CS2SX_TICKS_PER_DAY; }
static inline int    CS2SX_TimeSpan_Milliseconds(CS2SX_TimeSpan ts)
{ return (int)((ts.ticks / CS2SX_TICKS_PER_MS) % 1000); }
static inline int    CS2SX_TimeSpan_Seconds(CS2SX_TimeSpan ts)
{ return (int)((ts.ticks / CS2SX_TICKS_PER_SEC) % 60); }
static inline int    CS2SX_TimeSpan_Minutes(CS2SX_TimeSpan ts)
{ return (int)((ts.ticks / CS2SX_TICKS_PER_MIN) % 60); }
static inline int    CS2SX_TimeSpan_Hours(CS2SX_TimeSpan ts)
{ return (int)((ts.ticks / CS2SX_TICKS_PER_HOUR) % 24); }
static inline int    CS2SX_TimeSpan_Days(CS2SX_TimeSpan ts)
{ return (int)(ts.ticks  / CS2SX_TICKS_PER_DAY); }

// DateTime - DateTime = TimeSpan (using time_t seconds, 1-second resolution)
static inline CS2SX_TimeSpan CS2SX_DateTime_Subtract(time_t a, time_t b)
{ CS2SX_TimeSpan ts; ts.ticks = (long long)(difftime(a, b) * CS2SX_TICKS_PER_SEC); return ts; }

// TimeSpan arithmetic
static inline CS2SX_TimeSpan CS2SX_TimeSpan_Add(CS2SX_TimeSpan a, CS2SX_TimeSpan b)
{ CS2SX_TimeSpan r; r.ticks = a.ticks + b.ticks; return r; }
static inline CS2SX_TimeSpan CS2SX_TimeSpan_Sub(CS2SX_TimeSpan a, CS2SX_TimeSpan b)
{ CS2SX_TimeSpan r; r.ticks = a.ticks - b.ticks; return r; }

// ── Regex (wraps POSIX regex.h) ───────────────────────────────────────────────
typedef struct { char pattern[256]; } CS2SX_Regex;
CS2SX_Regex* CS2SX_Regex_New(const char* pattern);
static inline void CS2SX_Regex_Free(CS2SX_Regex* r) { free(r); }

// ── Regex helpers (POSIX regex.h) ────────────────────────────────────────────
// Uses POSIX regex available via musl libc on Nintendo Switch.
#include <regex.h>

int CS2SX_Regex_IsMatch(const char* input, const char* pattern);

// Escapes regex metacharacters so the input is matched literally.
// Returns a pooled buffer (valid for the current frame).
const char* CS2SX_Regex_Escape(const char* s);

void CS2SX_Regex_Match(const char* input, const char* pattern,
                                      char* out_buf, int out_size);

void CS2SX_Regex_Replace(const char* input, const char* pattern,
                                         const char* replacement,
                                         char* out_buf, int out_size);

List_str* CS2SX_Regex_Split(const char* input, const char* pattern);

