#pragma once
#include <switch.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>

extern char _cs2sx_strbuf[512];

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
    void* context;
    void  (*Draw)  (Control* self);
    void  (*Update)(Control* self, u64 kDown, u64 kHeld);
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

static inline Label* Label_New(const char* text)
{
    Label* l = (Label*)malloc(sizeof(Label));
    if (!l) return NULL;
    memset(l, 0, sizeof(Label));
    l->base.visible = 1;
    l->base.focusable = 0;
    l->base.Draw = Label_Draw;
    Label_SetText(l, text);
    return l;
}

// ============================================================================
// Button
// ============================================================================

typedef struct Button Button;
struct Button
{
    Control     base;
    const char* text;
    int         focused;
    void        (*OnClick)(void* context);
};

static inline void Button_Draw(Control* self)
{
    if (!self || !self->visible) return;
    Button* b = (Button*)self;
    const char* t = b->text ? b->text : "";
    if (b->focused)
        printf("\033[%d;%dH> %s <", self->y, self->x, t);
    else
        printf("\033[%d;%dH  %s  ", self->y, self->x, t);
}

static inline void Button_Update(Control* self, u64 kDown, u64 kHeld)
{
    (void)kHeld;
    if (!self) return;
    Button* b = (Button*)self;
    if (b->focused && (kDown & HidNpadButton_A) && b->OnClick)
        b->OnClick(self->context);
}

static inline Button* Button_New(const char* text)
{
    Button* b = (Button*)malloc(sizeof(Button));
    if (!b) return NULL;
    memset(b, 0, sizeof(Button));
    b->base.visible = 1;
    b->base.focusable = 1;
    b->base.Draw = Button_Draw;
    b->base.Update = Button_Update;
    b->text = text;
    return b;
}

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

static inline void ProgressBar_Draw(Control* self)
{
    if (!self || !self->visible) return;
    ProgressBar* pb = (ProgressBar*)self;
    int          fill = (pb->value * pb->width_chars) / 100;
    printf("\033[%d;%dH[", self->y, self->x);
    for (int i = 0; i < pb->width_chars; i++)
        printf(i < fill ? "#" : "-");
    printf("] %3d%%", pb->value);
}

static inline ProgressBar* ProgressBar_New(int width_chars)
{
    ProgressBar* pb = (ProgressBar*)malloc(sizeof(ProgressBar));
    if (!pb) return NULL;
    memset(pb, 0, sizeof(ProgressBar));
    pb->base.visible = 1;
    pb->base.focusable = 0;
    pb->base.Draw = ProgressBar_Draw;
    pb->width_chars = width_chars > 0 ? width_chars : 20;
    return pb;
}

// ============================================================================
// StringBuilder
// ============================================================================

typedef struct
{
    char* buf;
    int   length;
    int   capacity;
} StringBuilder;

static inline StringBuilder* StringBuilder_New(int capacity)
{
    if (capacity <= 0) capacity = 256;
    StringBuilder* sb = (StringBuilder*)malloc(sizeof(StringBuilder));
    if (!sb) return NULL;
    sb->buf = (char*)malloc(capacity);
    if (!sb->buf) { free(sb); return NULL; }
    sb->buf[0] = '\0';
    sb->length = 0;
    sb->capacity = capacity;
    return sb;
}

static inline void StringBuilder_Free(StringBuilder* sb)
{
    if (!sb) return;
    free(sb->buf);
    free(sb);
}

static inline void StringBuilder__grow(StringBuilder* sb, int needed)
{
    if (sb->length + needed + 1 <= sb->capacity) return;
    int cap = sb->capacity * 2;
    while (cap < sb->length + needed + 1) cap *= 2;
    sb->buf = (char*)realloc(sb->buf, cap);
    sb->capacity = cap;
}

static inline void StringBuilder_AppendStr(StringBuilder* sb, const char* s)
{
    if (!sb || !s) return;
    int len = (int)strlen(s);
    StringBuilder__grow(sb, len);
    memcpy(sb->buf + sb->length, s, len + 1);
    sb->length += len;
}

static inline void StringBuilder_AppendChar(StringBuilder* sb, char c)
{
    if (!sb) return;
    StringBuilder__grow(sb, 1);
    sb->buf[sb->length++] = c;
    sb->buf[sb->length] = '\0';
}

static inline void StringBuilder_AppendInt(StringBuilder* sb, int val)
{
    char tmp[32];
    snprintf(tmp, sizeof(tmp), "%d", val);
    StringBuilder_AppendStr(sb, tmp);
}

static inline void StringBuilder_AppendUInt(StringBuilder* sb, unsigned int val)
{
    char tmp[32];
    snprintf(tmp, sizeof(tmp), "%u", val);
    StringBuilder_AppendStr(sb, tmp);
}

static inline void StringBuilder_AppendFloat(StringBuilder* sb, float val)
{
    char tmp[32];
    snprintf(tmp, sizeof(tmp), "%f", val);
    StringBuilder_AppendStr(sb, tmp);
}

static inline void StringBuilder_AppendLine(StringBuilder* sb, const char* s)
{
    StringBuilder_AppendStr(sb, s);
    StringBuilder_AppendChar(sb, '\n');
}

static inline void StringBuilder_AppendLineInt(StringBuilder* sb, int val)
{
    StringBuilder_AppendInt(sb, val);
    StringBuilder_AppendChar(sb, '\n');
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

// ============================================================================
// String helpers
// ============================================================================

static inline const char* Int_ToString(int val)
{
    snprintf(_cs2sx_strbuf, 512, "%d", val);
    return _cs2sx_strbuf;
}

static inline const char* UInt_ToString(unsigned int val)
{
    snprintf(_cs2sx_strbuf, 512, "%u", val);
    return _cs2sx_strbuf;
}

static inline const char* Float_ToString(float val)
{
    snprintf(_cs2sx_strbuf, 512, "%f", val);
    return _cs2sx_strbuf;
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

#define String_Length(s) ((int)strlen(s))

// ============================================================================
// List<T>
// ============================================================================

#define CS2SX_LIST_INITIAL_CAP 8

#define CS2SX_LIST_DEFINE(T)                                                                        \
typedef struct { T* data; int count; int capacity; } List_##T;                                      \
static inline List_##T* List_##T##_New(void) {                                                      \
    List_##T* l = (List_##T*)malloc(sizeof(List_##T));                                              \
    if (!l) return NULL;                                                                            \
    l->data = (T*)malloc(CS2SX_LIST_INITIAL_CAP * sizeof(T));                                      \
    l->count = 0; l->capacity = CS2SX_LIST_INITIAL_CAP; return l; }                                \
static inline void List_##T##_Add(List_##T* l, T val) {                                            \
    if (!l) return;                                                                                 \
    if (l->count >= l->capacity) {                                                                  \
        l->capacity *= 2;                                                                           \
        l->data = (T*)realloc(l->data, l->capacity * sizeof(T)); }                                 \
    l->data[l->count++] = val; }                                                                    \
static inline T    List_##T##_Get(List_##T* l, int i) { return l->data[i]; }                       \
static inline int  List_##T##_Count(List_##T* l)       { return l ? l->count : 0; }                \
static inline void List_##T##_Clear(List_##T* l)       { if (l) l->count = 0; }                    \
static inline void List_##T##_Free(List_##T* l)        { if (l) { free(l->data); free(l); } }      \
static inline int  List_##T##_Contains(List_##T* l, T val) {                                       \
    for (int _i = 0; _i < l->count; _i++) { if (l->data[_i] == val) return 1; } return 0; }       \
static inline void List_##T##_Remove(List_##T* l, int idx) {                                       \
    if (!l || idx < 0 || idx >= l->count) return;                                                  \
    for (int _i = idx; _i < l->count - 1; _i++) l->data[_i] = l->data[_i + 1];                   \
    l->count--; }                                                                                   \
static inline void List_##T##_RemoveValue(List_##T* l, T val) {                                    \
    for (int _i = 0; _i < l->count; _i++) {                                                        \
        if (l->data[_i] == val) { List_##T##_Remove(l, _i); return; } } }

#define CS2SX_LIST_DEFINE_PTR(T)                                                                    \
typedef struct { T** data; int count; int capacity; } List_##T;                                     \
static inline List_##T* List_##T##_New(void) {                                                      \
    List_##T* l = (List_##T*)malloc(sizeof(List_##T));                                              \
    if (!l) return NULL;                                                                            \
    l->data = (T**)malloc(CS2SX_LIST_INITIAL_CAP * sizeof(T*));                                    \
    l->count = 0; l->capacity = CS2SX_LIST_INITIAL_CAP; return l; }                                \
static inline void List_##T##_Add(List_##T* l, T* val) {                                           \
    if (!l) return;                                                                                 \
    if (l->count >= l->capacity) {                                                                  \
        l->capacity *= 2;                                                                           \
        l->data = (T**)realloc(l->data, l->capacity * sizeof(T*)); }                               \
    l->data[l->count++] = val; }                                                                    \
static inline T*   List_##T##_Get(List_##T* l, int i) { return l->data[i]; }                       \
static inline int  List_##T##_Count(List_##T* l)       { return l ? l->count : 0; }                \
static inline void List_##T##_Clear(List_##T* l)       { if (l) l->count = 0; }                    \
static inline void List_##T##_Free(List_##T* l)        { if (l) { free(l->data); free(l); } }      \
static inline void List_##T##_Remove(List_##T* l, int idx) {                                       \
    if (!l || idx < 0 || idx >= l->count) return;                                                  \
    for (int _i = idx; _i < l->count - 1; _i++) l->data[_i] = l->data[_i + 1];                   \
    l->count--; }                                                                                   \
static inline void List_##T##_RemoveValue(List_##T* l, T* val) {                                   \
    for (int _i = 0; _i < l->count; _i++) {                                                        \
        if (l->data[_i] == val) { List_##T##_Remove(l, _i); return; } } }

CS2SX_LIST_DEFINE(int)
CS2SX_LIST_DEFINE(float)
CS2SX_LIST_DEFINE(double)
CS2SX_LIST_DEFINE(u8)
CS2SX_LIST_DEFINE(u16)
CS2SX_LIST_DEFINE(u32)
CS2SX_LIST_DEFINE(u64)
CS2SX_LIST_DEFINE(s32)
CS2SX_LIST_DEFINE(s64)

// List<string>
typedef struct { const char** data; int count; int capacity; } List_str;
static inline List_str* List_str_New(void) {
    List_str* l = (List_str*)malloc(sizeof(List_str));
    if (!l) return NULL;
    l->data = (const char**)malloc(CS2SX_LIST_INITIAL_CAP * sizeof(const char*));
    l->count = 0; l->capacity = CS2SX_LIST_INITIAL_CAP; return l;
}
static inline void        List_str_Add(List_str* l, const char* val) {
    if (!l) return;
    if (l->count >= l->capacity) {
        l->capacity *= 2;
        l->data = (const char**)realloc(l->data, l->capacity * sizeof(const char*));
    }
    l->data[l->count++] = val;
}
static inline const char* List_str_Get(List_str* l, int i) { return l->data[i]; }
static inline int         List_str_Count(List_str* l) { return l ? l->count : 0; }
static inline void        List_str_Clear(List_str* l) { if (l) l->count = 0; }
static inline void        List_str_Free(List_str* l) { if (l) { free(l->data); free(l); } }
static inline void        List_str_Remove(List_str* l, int idx) {
    if (!l || idx < 0 || idx >= l->count) return;
    for (int _i = idx; _i < l->count - 1; _i++) l->data[_i] = l->data[_i + 1];
    l->count--;
}
static inline void List_str_RemoveValue(List_str* l, const char* val) {
    for (int _i = 0; _i < l->count; _i++) {
        if (strcmp(l->data[_i], val) == 0) { List_str_Remove(l, _i); return; }
    }
}
static inline int List_str_Contains(List_str* l, const char* val) {
    for (int _i = 0; _i < l->count; _i++) {
        if (strcmp(l->data[_i], val) == 0) return 1;
    }
    return 0;
}

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
static inline void Dict_##K##_##V##_Add(Dict_##K##_##V* d, K key, V val) {                             \
    if (!d) return;                                                                                     \
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
    Dict_##K##_##V##_TryGetValue(d, key, &_v); return _v; }                                            \
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
static inline void Dict_str_##V##_Add(Dict_str_##V* d, const char* key, V val) {                        \
    if (!d) return;                                                                                     \
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
    Dict_str_##V##_TryGetValue(d, key, &_v); return _v; }                                               \
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

// Dict<string, string>
typedef struct { const char** keys; const char** vals; int count; int capacity; } Dict_str_str;
static inline Dict_str_str* Dict_str_str_New(void) {
    Dict_str_str* d = (Dict_str_str*)malloc(sizeof(Dict_str_str));
    if (!d) return NULL;
    d->keys = (const char**)malloc(CS2SX_DICT_INITIAL_CAP * sizeof(const char*));
    d->vals = (const char**)malloc(CS2SX_DICT_INITIAL_CAP * sizeof(const char*));
    d->count = 0; d->capacity = CS2SX_DICT_INITIAL_CAP; return d;
}
static inline void Dict_str_str_Add(Dict_str_str* d, const char* key, const char* val) {
    if (!d) return;
    if (d->count >= d->capacity) {
        d->capacity *= 2;
        d->keys = (const char**)realloc(d->keys, d->capacity * sizeof(const char*));
        d->vals = (const char**)realloc(d->vals, d->capacity * sizeof(const char*));
    }
    d->keys[d->count] = key; d->vals[d->count] = val; d->count++;
}
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
    Dict_str_str_TryGetValue(d, key, &v);
    return v;
}
static inline void Dict_str_str_Set(Dict_str_str* d, const char* key, const char* val) {
    for (int _i = 0; _i < d->count; _i++) {
        if (strcmp(d->keys[_i], key) == 0) { d->vals[_i] = val; return; }
    }
    Dict_str_str_Add(d, key, val);
}
static inline void Dict_str_str_Remove(Dict_str_str* d, const char* key) {
    for (int _i = 0; _i < d->count; _i++) {
        if (strcmp(d->keys[_i], key) == 0) {
            for (int _j = _i; _j < d->count - 1; _j++) {
                d->keys[_j] = d->keys[_j + 1]; d->vals[_j] = d->vals[_j + 1];
            }
            d->count--; return;
        }
    }
}
static inline void Dict_str_str_Clear(Dict_str_str* d) { if (d) d->count = 0; }
static inline void Dict_str_str_Free(Dict_str_str* d) { if (d) { free(d->keys); free(d->vals); free(d); } }

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
    form->controls[form->count++] = ctrl;
}

static inline void Form_InitFocus(Form* form)
{
    if (!form) return;
    for (int i = 0; i < form->count; i++)
    {
        if (!form->controls[i]->focusable) continue;
        ((Button*)form->controls[i])->focused = 1;
        form->focusedIndex = i;
        return;
    }
}

static inline void Form_MoveFocus(Form* form, int direction)
{
    if (!form || form->count == 0) return;
    int current = -1;
    for (int i = 0; i < form->count; i++)
    {
        if (!form->controls[i]->focusable) continue;
        Button* b = (Button*)form->controls[i];
        if (b->focused) { b->focused = 0; current = i; break; }
    }
    if (current == -1) { Form_InitFocus(form); return; }
    int next = current;
    for (int t = 0; t < form->count; t++)
    {
        next += direction;
        if (next < 0)            next = form->count - 1;
        if (next >= form->count) next = 0;
        if (form->controls[next]->focusable)
        {
            ((Button*)form->controls[next])->focused = 1;
            form->focusedIndex = next;
            return;
        }
    }
    if (current >= 0 && form->controls[current]->focusable)
        ((Button*)form->controls[current])->focused = 1;
}

static inline void Form_DrawAll(Form* form)
{
    if (!form) return;
    for (int i = 0; i < form->count; i++)
    {
        Control* c = form->controls[i];
        if (c && c->visible && c->Draw) c->Draw(c);
    }
}

static inline void Form_UpdateAll(Form* form, u64 kDown, u64 kHeld)
{
    if (!form) return;
    if (kDown & HidNpadButton_Down) Form_MoveFocus(form, 1);
    if (kDown & HidNpadButton_Up)   Form_MoveFocus(form, -1);
    for (int i = 0; i < form->count; i++)
    {
        Control* c = form->controls[i];
        if (c && c->visible && c->Update) c->Update(c, kDown, kHeld);
    }
}

static inline void Form_Free(Form* form)
{
    if (!form) return;
    for (int i = 0; i < form->count; i++) { free(form->controls[i]); form->controls[i] = NULL; }
    form->count = 0;
    form->focusedIndex = -1;
}

// ============================================================================
// SwitchApp
// ============================================================================

//typedef struct SwitchApp SwitchApp;
//struct SwitchApp
//{
//    void (*OnInit)  (SwitchApp* self);
//    void (*OnUpdate)(SwitchApp* self, u64 kDown, u64 kHeld);
//    void (*OnDraw)  (SwitchApp* self);
//    void (*OnExit)  (SwitchApp* self);
//};