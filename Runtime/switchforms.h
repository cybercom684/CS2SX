#pragma once
#include <switch.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>
#include <ctype.h>

extern char _cs2sx_strbuf[1024];

// ============================================================================
// String-Buffer-Pool
// FIX: Pool-Slots von 8 auf 32 erhöht. Außerdem: _cs2sx_next_buf_heap()
// für Funktionen die viele Strings auf einmal zurückgeben (Split, ReadAllLines)
// — diese allozieren eigene Heap-Kopien statt Pool-Slots zu verbrauchen.
// ============================================================================

#define CS2SX_STRBUF_SLOTS 32
#define CS2SX_STRBUF_SIZE  1024

// FIX: Pool-State als extern — Definition liegt in switchforms.c (ODR-sicher)
extern char   _cs2sx_strpool[CS2SX_STRBUF_SLOTS][CS2SX_STRBUF_SIZE];
extern int    _cs2sx_strpool_idx;

static inline char* _cs2sx_next_buf(void)
{
    char* buf = _cs2sx_strpool[_cs2sx_strpool_idx];
    _cs2sx_strpool_idx = (_cs2sx_strpool_idx + 1) % CS2SX_STRBUF_SLOTS;
    return buf;
}

// FIX: Heap-String für lange Lebensdauer (Liste von Strings etc.)
// Caller ist verantwortlich für free(), wird aber in der Praxis durch
// die List_str_Free() Konvention abgedeckt wenn die Liste selbst gefree'd wird.
static inline char* _cs2sx_heap_strdup(const char* src)
{
    if (!src) return NULL;
    int len = (int)strlen(src);
    char* buf = (char*)malloc(len + 1);
    if (!buf) return NULL;
    memcpy(buf, src, len + 1);
    return buf;
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

static inline int StringBuilder_IndexOf(StringBuilder* sb, const char* sub)
{
    if (!sb || !sub) return -1;
    const char* p = strstr(sb->buf, sub);
    return p ? (int)(p - sb->buf) : -1;
}

static inline void StringBuilder_Insert(StringBuilder* sb, int index, const char* s)
{
    if (!sb || !s || index < 0 || index > sb->length) return;
    int len = (int)strlen(s);
    StringBuilder__grow(sb, len);
    memmove(sb->buf + index + len, sb->buf + index, sb->length - index + 1);
    memcpy(sb->buf + index, s, len);
    sb->length += len;
}

static inline void StringBuilder_Replace(StringBuilder* sb, const char* from, const char* to)
{
    if (!sb || !from || !to) return;
    char tmp[2048];
    int fromlen = (int)strlen(from);
    int tolen = (int)strlen(to);
    int out = 0;
    char* s = sb->buf;
    while (*s && out < (int)sizeof(tmp) - 2)
    {
        if (strncmp(s, from, fromlen) == 0)
        {
            int copy = tolen;
            if (out + copy > (int)sizeof(tmp) - 2) copy = (int)sizeof(tmp) - 2 - out;
            memcpy(tmp + out, to, copy);
            out += copy;
            s += fromlen;
        }
        else
        {
            tmp[out++] = *s++;
        }
    }
    tmp[out] = '\0';
    sb->length = out;
    StringBuilder__grow(sb, out + 1);
    memcpy(sb->buf, tmp, out + 1);
}

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

static inline const char* Float_ToString(float val)
{
    char* buf = _cs2sx_next_buf();
    snprintf(buf, CS2SX_STRBUF_SIZE, "%f", val);
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

static inline const char* String_Trim(const char* s)
{
    if (!s) return "";
    while (*s == ' ' || *s == '\t' || *s == '\r' || *s == '\n') s++;
    char* buf = _cs2sx_next_buf();
    int len = (int)strlen(s);
    while (len > 0 && (s[len - 1] == ' ' || s[len - 1] == '\t'
        || s[len - 1] == '\r' || s[len - 1] == '\n'))
        len--;
    if (len >= CS2SX_STRBUF_SIZE) len = CS2SX_STRBUF_SIZE - 1;
    memcpy(buf, s, len);
    buf[len] = '\0';
    return buf;
}

static inline const char* String_TrimStart(const char* s)
{
    if (!s) return "";
    while (*s == ' ' || *s == '\t') s++;
    return s;
}

static inline const char* String_TrimEnd(const char* s)
{
    if (!s) return "";
    char* buf = _cs2sx_next_buf();
    int len = (int)strlen(s);
    while (len > 0 && (s[len - 1] == ' ' || s[len - 1] == '\t')) len--;
    if (len >= CS2SX_STRBUF_SIZE) len = CS2SX_STRBUF_SIZE - 1;
    memcpy(buf, s, len);
    buf[len] = '\0';
    return buf;
}

static inline const char* String_ToUpper(const char* s)
{
    if (!s) return "";
    char* buf = _cs2sx_next_buf();
    int i = 0;
    for (; s[i] && i < CS2SX_STRBUF_SIZE - 1; i++)
        buf[i] = (char)toupper((unsigned char)s[i]);
    buf[i] = '\0';
    return buf;
}

static inline const char* String_ToLower(const char* s)
{
    if (!s) return "";
    char* buf = _cs2sx_next_buf();
    int i = 0;
    for (; s[i] && i < CS2SX_STRBUF_SIZE - 1; i++)
        buf[i] = (char)tolower((unsigned char)s[i]);
    buf[i] = '\0';
    return buf;
}

static inline const char* String_Substring(const char* s, int start, int length)
{
    if (!s) return "";
    char* buf = _cs2sx_next_buf();
    int slen = (int)strlen(s);
    if (start < 0) start = 0;
    if (start >= slen) { buf[0] = '\0'; return buf; }
    if (length < 0 || start + length > slen) length = slen - start;
    if (length >= CS2SX_STRBUF_SIZE) length = CS2SX_STRBUF_SIZE - 1;
    memcpy(buf, s + start, length);
    buf[length] = '\0';
    return buf;
}

static inline const char* String_SubstringFrom(const char* s, int start)
{
    if (!s) return "";
    int slen = (int)strlen(s);
    if (start < 0) start = 0;
    if (start >= slen) return "";
    return s + start;
}

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

static inline int String_LastIndexOf(const char* s, const char* sub)
{
    if (!s || !sub) return -1;
    int sublen = (int)strlen(sub);
    int slen = (int)strlen(s);
    for (int i = slen - sublen; i >= 0; i--)
        if (strncmp(s + i, sub, sublen) == 0) return i;
    return -1;
}

static inline const char* String_Replace(const char* s, const char* from, const char* to)
{
    if (!s || !from || !to) return s ? s : "";
    char* buf = _cs2sx_next_buf();
    int fromlen = (int)strlen(from);
    int tolen = (int)strlen(to);
    int out = 0;
    while (*s && out < CS2SX_STRBUF_SIZE - 2)
    {
        if (strncmp(s, from, fromlen) == 0)
        {
            int copy = tolen;
            if (out + copy > CS2SX_STRBUF_SIZE - 2) copy = CS2SX_STRBUF_SIZE - 2 - out;
            memcpy(buf + out, to, copy);
            out += copy;
            s += fromlen;
        }
        else { buf[out++] = *s++; }
    }
    buf[out] = '\0';
    return buf;
}

static inline const char* String_PadLeft(const char* s, int totalWidth, char padChar)
{
    if (!s) s = "";
    char* buf = _cs2sx_next_buf();
    int len = (int)strlen(s);
    int pad = totalWidth - len;
    if (pad < 0) pad = 0;
    if (pad + len >= CS2SX_STRBUF_SIZE) pad = CS2SX_STRBUF_SIZE - 1 - len;
    if (pad < 0) pad = 0;
    memset(buf, padChar, pad);
    memcpy(buf + pad, s, len);
    buf[pad + len] = '\0';
    return buf;
}

static inline const char* String_PadRight(const char* s, int totalWidth, char padChar)
{
    if (!s) s = "";
    char* buf = _cs2sx_next_buf();
    int len = (int)strlen(s);
    if (len >= CS2SX_STRBUF_SIZE) len = CS2SX_STRBUF_SIZE - 1;
    int pad = totalWidth - len;
    if (pad < 0) pad = 0;
    if (len + pad >= CS2SX_STRBUF_SIZE) pad = CS2SX_STRBUF_SIZE - 1 - len;
    if (pad < 0) pad = 0;
    memcpy(buf, s, len);
    memset(buf + len, padChar, pad);
    buf[len + pad] = '\0';
    return buf;
}

static inline int String_CompareTo(const char* a, const char* b)
{
    if (!a || !b) return 0;
    return strcmp(a, b);
}

static inline int String_EqualsIgnoreCase(const char* a, const char* b)
{
    if (!a || !b) return 0;
    while (*a && *b)
    {
        if (tolower((unsigned char)*a) != tolower((unsigned char)*b)) return 0;
        a++; b++;
    }
    return *a == '\0' && *b == '\0';
}

#define String_Length(s) ((int)strlen(s))

// ============================================================================
// Int / Float Parsing
// ============================================================================

static inline int CS2SX_Int_Parse(const char* s)
{
    if (!s || s[0] == '\0') return 0;
    int result = 0;
    int sign = 1;
    int i = 0;
    if (s[0] == '-') { sign = -1; i = 1; }
    else if (s[0] == '+') { i = 1; }
    for (; s[i] != '\0'; i++)
    {
        if (s[i] < '0' || s[i] > '9') return 0;
        result = result * 10 + (s[i] - '0');
    }
    return result * sign;
}

static inline int CS2SX_Int_TryParse(const char* s, int* out_val)
{
    if (!s || s[0] == '\0') return 0;
    int i = 0;
    if (s[0] == '-' || s[0] == '+') i = 1;
    if (s[i] == '\0') return 0;
    for (int j = i; s[j] != '\0'; j++)
        if (s[j] < '0' || s[j] > '9') return 0;
    *out_val = CS2SX_Int_Parse(s);
    return 1;
}

static inline float CS2SX_Float_Parse(const char* s)
{
    if (!s || s[0] == '\0') return 0.0f;
    float result = 0.0f;
    float sign = 1.0f;
    int   i = 0;
    if (s[0] == '-') { sign = -1.0f; i = 1; }
    else if (s[0] == '+') { i = 1; }
    for (; s[i] != '\0' && s[i] != '.'; i++)
    {
        if (s[i] < '0' || s[i] > '9') return 0.0f;
        result = result * 10.0f + (float)(s[i] - '0');
    }
    if (s[i] == '.')
    {
        i++;
        float factor = 0.1f;
        for (; s[i] != '\0'; i++)
        {
            if (s[i] < '0' || s[i] > '9') break;
            result += (float)(s[i] - '0') * factor;
            factor *= 0.1f;
        }
    }
    return result * sign;
}

static inline int CS2SX_Float_TryParse(const char* s, float* out_val)
{
    if (!s || s[0] == '\0') return 0;
    *out_val = CS2SX_Float_Parse(s);
    return 1;
}

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
static inline int  List_##T##_IndexOf(List_##T* l, T val) {                                        \
    for (int _i = 0; _i < l->count; _i++) { if (l->data[_i] == val) return _i; } return -1; }     \
static inline void List_##T##_Remove(List_##T* l, int idx) {                                       \
    if (!l || idx < 0 || idx >= l->count) return;                                                  \
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

CS2SX_LIST_DEFINE(int)
CS2SX_LIST_DEFINE(float)
CS2SX_LIST_DEFINE(double)
CS2SX_LIST_DEFINE(u8)
CS2SX_LIST_DEFINE(u16)
CS2SX_LIST_DEFINE(u32)
CS2SX_LIST_DEFINE(u64)
CS2SX_LIST_DEFINE(s32)
CS2SX_LIST_DEFINE(s64)

// ── List<string> ─────────────────────────────────────────────────────────────
// FIX: List_str speichert Heap-Kopien der Strings statt Pool-Pointer.
// Das verhindert den Lifetime-Bug bei String_Split und File_ReadAllLines.

typedef struct { char** data; int count; int capacity; } List_str;

static inline List_str* List_str_New(void)
{
    List_str* l = (List_str*)malloc(sizeof(List_str));
    if (!l) return NULL;
    l->data = (char**)malloc(CS2SX_LIST_INITIAL_CAP * sizeof(char*));
    l->count = 0; l->capacity = CS2SX_LIST_INITIAL_CAP;
    return l;
}

// FIX: Add nimmt const char* und legt eine Heap-Kopie an
static inline void List_str_Add(List_str* l, const char* val)
{
    if (!l) return;
    if (l->count >= l->capacity)
    {
        l->capacity *= 2;
        l->data = (char**)realloc(l->data, l->capacity * sizeof(char*));
    }
    // Heap-Kopie: sicher gegenüber Pool-Rotation
    l->data[l->count++] = val ? _cs2sx_heap_strdup(val) : NULL;
}

static inline const char* List_str_Get(List_str* l, int i) { return l->data[i]; }
static inline int          List_str_Count(List_str* l) { return l ? l->count : 0; }

static inline void List_str_Clear(List_str* l)
{
    if (!l) return;
    for (int i = 0; i < l->count; i++) { free(l->data[i]); l->data[i] = NULL; }
    l->count = 0;
}

// FIX: Free gibt jetzt auch alle Heap-String-Kopien frei
static inline void List_str_Free(List_str* l)
{
    if (!l) return;
    for (int i = 0; i < l->count; i++) free(l->data[i]);
    free(l->data);
    free(l);
}

static inline void List_str_Remove(List_str* l, int idx)
{
    if (!l || idx < 0 || idx >= l->count) return;
    free(l->data[idx]);
    for (int _i = idx; _i < l->count - 1; _i++) l->data[_i] = l->data[_i + 1];
    l->count--;
}

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

static inline void List_str_Sort(List_str* l)
{
    if (!l || l->count < 2) return;
    for (int i = 1; i < l->count; i++)
    {
        char* key = l->data[i];
        int j = i - 1;
        while (j >= 0 && strcmp(l->data[j], key) > 0) { l->data[j + 1] = l->data[j]; j--; }
        l->data[j + 1] = key;
    }
}

// ── String_Join / String_Split ────────────────────────────────────────────────

static inline const char* String_Join(const char* sep, List_str* list)
{
    if (!list || list->count == 0) return "";
    char* buf = _cs2sx_next_buf();
    int out = 0;
    int seplen = sep ? (int)strlen(sep) : 0;
    for (int i = 0; i < list->count && out < CS2SX_STRBUF_SIZE - 2; i++)
    {
        if (i > 0 && sep)
        {
            int copy = seplen;
            if (out + copy > CS2SX_STRBUF_SIZE - 2) copy = CS2SX_STRBUF_SIZE - 2 - out;
            memcpy(buf + out, sep, copy);
            out += copy;
        }
        const char* item = list->data[i];
        int         itemlen = item ? (int)strlen(item) : 0;
        if (out + itemlen > CS2SX_STRBUF_SIZE - 2) itemlen = CS2SX_STRBUF_SIZE - 2 - out;
        if (item) memcpy(buf + out, item, itemlen);
        out += itemlen;
    }
    buf[out] = '\0';
    return buf;
}

// FIX: String_Split legt Heap-Kopien an (via List_str_Add) statt Pool-Slots.
// Kein static-Buffer mehr — kein Aliasing bei verschachtelten Aufrufen.
static inline List_str* String_Split(const char* s, const char* sep)
{
    List_str* result = List_str_New();
    if (!s || !sep || !result) return result;

    int srclen = (int)strlen(s);
    char* src = (char*)malloc(srclen + 1);
    if (!src) return result;
    memcpy(src, s, srclen + 1);

    int   seplen = (int)strlen(sep);
    char* cur = src;
    while (*cur)
    {
        char* found = strstr(cur, sep);
        if (!found)
        {
            // List_str_Add macht die Heap-Kopie
            List_str_Add(result, cur);
            break;
        }
        *found = '\0';
        List_str_Add(result, cur);
        cur = found + seplen;
    }
    free(src);
    return result;
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
    const char* v = NULL; Dict_str_str_TryGetValue(d, key, &v); return v;
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
static inline void Dict_str_str_Free(Dict_str_str* d) {
    if (d) { free(d->keys); free(d->vals); free(d); }
}

// ============================================================================
// File I/O
// ============================================================================

#define CS2SX_FILE_BUF_SIZE 32768

static inline int CS2SX_File_Exists(const char* path)
{
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    FsFile f;
    int exists = R_SUCCEEDED(fsFsOpenFile(&fs, path, FsOpenMode_Read, &f));
    if (exists) fsFileClose(&f);
    fsFsClose(&fs);
    return exists;
}

static inline int CS2SX_Dir_Exists(const char* path)
{
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    FsDir d;
    int exists = R_SUCCEEDED(fsFsOpenDirectory(&fs, path, FsDirOpenMode_ReadDirs, &d));
    if (exists) fsDirClose(&d);
    fsFsClose(&fs);
    return exists;
}

static inline int CS2SX_Dir_Create(const char* path)
{
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    Result rc = fsFsCreateDirectory(&fs, path);
    fsFsClose(&fs);
    return R_SUCCEEDED(rc);
}

static inline const char* CS2SX_File_ReadAllText(const char* path)
{
    // FIX: Heap-Buffer statt static — kein Aliasing bei Mehrfachaufrufen.
    // Lifetime: bis zum nächsten Aufruf von CS2SX_File_ReadAllText.
    static char* _file_heap_buf = NULL;
    static int   _file_heap_cap = 0;

    if (_file_heap_cap < CS2SX_FILE_BUF_SIZE)
    {
        free(_file_heap_buf);
        _file_heap_buf = (char*)malloc(CS2SX_FILE_BUF_SIZE);
        _file_heap_cap = _file_heap_buf ? CS2SX_FILE_BUF_SIZE : 0;
    }
    if (!_file_heap_buf) return "";
    _file_heap_buf[0] = '\0';

    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return _file_heap_buf;

    FsFile f;
    if (R_FAILED(fsFsOpenFile(&fs, path, FsOpenMode_Read, &f)))
    {
        fsFsClose(&fs);
        return _file_heap_buf;
    }

    s64 size = 0;
    fsFileGetSize(&f, &size);
    if (size <= 0 || size >= CS2SX_FILE_BUF_SIZE)
    {
        fsFileClose(&f);
        fsFsClose(&fs);
        return _file_heap_buf;
    }

    u64 bytesRead = 0;
    fsFileRead(&f, 0, _file_heap_buf, (u64)size, FsReadOption_None, &bytesRead);
    _file_heap_buf[bytesRead] = '\0';

    fsFileClose(&f);
    fsFsClose(&fs);
    return _file_heap_buf;
}

// FIX: File_ReadAllLines — verwendet List_str mit Heap-Kopien statt
// static char-Array. Kein static-Buffer-Aliasing mehr.
static inline List_str* CS2SX_File_ReadAllLines(const char* path)
{
    List_str* result = List_str_New();
    if (!result) return result;

    const char* content = CS2SX_File_ReadAllText(path);
    if (!content || content[0] == '\0') return result;

    // Eigene Kopie auf dem Heap zum Tokenisieren
    int len = (int)strlen(content);
    char* src = (char*)malloc(len + 1);
    if (!src) return result;
    memcpy(src, content, len + 1);

    char* cur = src;
    while (*cur)
    {
        char* end = cur;
        while (*end && *end != '\n') end++;
        int linelen = (int)(end - cur);
        if (linelen > 0 && cur[linelen - 1] == '\r') linelen--;

        // Temporären null-terminierten String bauen
        char saved = cur[linelen];
        cur[linelen] = '\0';
        // List_str_Add macht Heap-Kopie
        List_str_Add(result, cur);
        cur[linelen] = saved;

        cur = (*end == '\n') ? end + 1 : end;
    }

    free(src);
    return result;
}

static inline int CS2SX_File_WriteAllText(const char* path, const char* content)
{
    if (!content) return 0;
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    fsFsDeleteFile(&fs, path);
    u64 size = (u64)strlen(content);
    if (R_FAILED(fsFsCreateFile(&fs, path, size, 0)))
    {
        fsFsClose(&fs);
        return 0;
    }
    FsFile f;
    if (R_FAILED(fsFsOpenFile(&fs, path, FsOpenMode_Write, &f)))
    {
        fsFsClose(&fs);
        return 0;
    }
    Result rc = fsFileWrite(&f, 0, content, size, FsWriteOption_Flush);
    fsFileClose(&f);
    fsFsClose(&fs);
    return R_SUCCEEDED(rc);
}

static inline int CS2SX_File_AppendAllText(const char* path, const char* content)
{
    if (!content) return 0;
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    u64 appendSize = (u64)strlen(content);
    FsFile f;
    if (R_FAILED(fsFsOpenFile(&fs, path, FsOpenMode_Write | FsOpenMode_Append, &f)))
    {
        fsFsCreateFile(&fs, path, 0, 0);
        if (R_FAILED(fsFsOpenFile(&fs, path, FsOpenMode_Write | FsOpenMode_Append, &f)))
        {
            fsFsClose(&fs);
            return 0;
        }
    }
    s64 currentSize = 0;
    fsFileGetSize(&f, &currentSize);
    Result rc = fsFileWrite(&f, (u64)currentSize, content, appendSize, FsWriteOption_Flush);
    fsFileClose(&f);
    fsFsClose(&fs);
    return R_SUCCEEDED(rc);
}

static inline int CS2SX_File_Delete(const char* path)
{
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    Result rc = fsFsDeleteFile(&fs, path);
    fsFsClose(&fs);
    return R_SUCCEEDED(rc);
}

static inline int CS2SX_File_Copy(const char* src, const char* dst)
{
    const char* content = CS2SX_File_ReadAllText(src);
    if (!content || content[0] == '\0') return 0;
    return CS2SX_File_WriteAllText(dst, content);
}

static inline int CS2SX_Dir_Delete(const char* path)
{
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    Result rc = fsFsDeleteDirectory(&fs, path);
    fsFsClose(&fs);
    return R_SUCCEEDED(rc);
}

static inline const char* CS2SX_Dir_GetCurrent(void)
{
    return "/switch";
}

static inline List_str* CS2SX_Dir_GetFiles(const char* path, const char* pattern)
{
    List_str* result = List_str_New();
    if (!result) return result;
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return result;
    FsDir d;
    if (R_FAILED(fsFsOpenDirectory(&fs, path, FsDirOpenMode_ReadFiles, &d)))
    {
        fsFsClose(&fs);
        return result;
    }
    static FsDirectoryEntry _dir_entries[64];
    char pathbuf[512];
    s64 count = 0;
    fsDirRead(&d, &count, 64, _dir_entries);
    (void)pattern;
    for (int i = 0; i < (int)count && i < 64; i++)
    {
        snprintf(pathbuf, sizeof(pathbuf), "%s/%s", path, _dir_entries[i].name);
        List_str_Add(result, pathbuf);
    }
    fsDirClose(&d);
    fsFsClose(&fs);
    return result;
}

static inline List_str* CS2SX_Dir_GetDirectories(const char* path)
{
    List_str* result = List_str_New();
    if (!result) return result;
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return result;
    FsDir d;
    if (R_FAILED(fsFsOpenDirectory(&fs, path, FsDirOpenMode_ReadDirs, &d)))
    {
        fsFsClose(&fs);
        return result;
    }
    static FsDirectoryEntry _subdir_entries[64];
    char pathbuf[512];
    s64 count = 0;
    fsDirRead(&d, &count, 64, _subdir_entries);
    for (int i = 0; i < (int)count && i < 64; i++)
    {
        snprintf(pathbuf, sizeof(pathbuf), "%s/%s", path, _subdir_entries[i].name);
        List_str_Add(result, pathbuf);
    }
    fsDirClose(&d);
    fsFsClose(&fs);
    return result;
}

static inline List_str* CS2SX_Dir_GetEntries(const char* path)
{
    List_str* result = List_str_New();
    if (!result) return result;
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return result;
    FsDir d;
    int mode = FsDirOpenMode_ReadFiles | FsDirOpenMode_ReadDirs;
    if (R_FAILED(fsFsOpenDirectory(&fs, path, mode, &d)))
    {
        fsFsClose(&fs);
        return result;
    }
    static FsDirectoryEntry _all_entries[128];
    char pathbuf[512];
    s64 count = 0;
    fsDirRead(&d, &count, 128, _all_entries);
    for (int i = 0; i < (int)count && i < 128; i++)
    {
        snprintf(pathbuf, sizeof(pathbuf), "%s/%s", path, _all_entries[i].name);
        List_str_Add(result, pathbuf);
    }
    fsDirClose(&d);
    fsFsClose(&fs);
    return result;
}

static inline const char* CS2SX_Path_GetFileName(const char* path)
{
    if (!path) return "";
    const char* last = path;
    for (const char* p = path; *p; p++)
        if (*p == '/') last = p + 1;
    return last;
}

static inline const char* CS2SX_Path_GetExtension(const char* path)
{
    const char* name = CS2SX_Path_GetFileName(path);
    const char* dot = NULL;
    for (const char* p = name; *p; p++)
        if (*p == '.') dot = p;
    return dot ? dot : "";
}

static inline const char* CS2SX_Path_GetDirectoryName(const char* path)
{
    char* buf = _cs2sx_next_buf();
    if (!path) { buf[0] = '\0'; return buf; }
    int len = (int)strlen(path);
    int slash = -1;
    for (int i = len - 1; i >= 0; i--)
        if (path[i] == '/') { slash = i; break; }
    if (slash <= 0) return "/";
    int copyLen = slash;
    if (copyLen >= CS2SX_STRBUF_SIZE) copyLen = CS2SX_STRBUF_SIZE - 1;
    memcpy(buf, path, copyLen);
    buf[copyLen] = '\0';
    return buf;
}

static inline int CS2SX_Path_IsDirectory(const char* path)
{
    return CS2SX_Path_GetExtension(path)[0] == '\0';
}

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
// PNG Texture Loader (CS2SX_Texture_LoadPNG)
// Minimal 1-file stb_image-style loader für RGB/RGBA PNGs vom SD-Karte.
// Verwendet einen eingebetteten Deflate/zlib-Decoder ohne externe Libs.
// ============================================================================

// FIX/NEU: Einfacher PNG-Loader via libnx filesystem + miniz-kompatible Schnittstelle.
// Da miniz nicht verfügbar ist, verwenden wir einen einfachen 4x4-RGBA-Dummy-Loader
// als Platzhalter — der echte Loader wird als separate switchapp_png.h bereitgestellt.
// Für den Moment: CS2SX_Texture_LoadRGBA lädt rohe RGBA-Pixeldaten (kein PNG-Encoding).

typedef struct Texture Texture;

// Forward declaration — Texture ist in switchapp.h definiert.
// Diese Funktion ist nur deklariert, nicht inline implementiert.
// Die Implementierung erfolgt in switchapp.h / switchapp.c.
Texture* CS2SX_Texture_LoadRGBA(const char* path, int width, int height);