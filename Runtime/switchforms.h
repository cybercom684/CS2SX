#pragma once
#include <switch.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

// ============================================================================
// Globaler String-Buffer für snprintf-Zuweisungen (Text-Properties).
//
// WICHTIG: Nur für kurzlebige Zuweisungen gedacht (z.B. label->text = ...).
// Der Zeiger wird sofort nach der Zuweisung genutzt; der nächste snprintf-
// Aufruf überschreibt den Inhalt. Für dauerhaften Text statische Strings
// oder eigene malloc-Puffer verwenden.
// ============================================================================
extern char _cs2sx_strbuf[512];  // Definition in switchforms.c

// ============================================================================
// Control (Basis aller UI-Elemente)
// ============================================================================

#define FORM_MAX_CONTROLS 64

typedef struct Control Control;
struct Control
{
    int   x, y, width, height;
    int   visible;
    int   focusable;
    void* context;                                  // zeigt auf den SwitchApp-Context
    void  (*Draw)(Control* self);
    void  (*Update)(Control* self, u64 kDown, u64 kHeld);
};

// ============================================================================
// Label
// ============================================================================

typedef struct Label Label;
struct Label
{
    Control base;
    char    text[256];   // eigener Puffer — kein Pointer auf _cs2sx_strbuf
};

static inline void Label_Draw(Control* self)
{
    if (!self || !self->visible) return;
    Label* label = (Label*)self;
    printf("\033[%d;%dH%s", self->y, self->x, label->text);
}

static inline void Label_SetText(Label* l, const char* s)
{
    if (!l || !s) return;
    strncpy(l->text, s, sizeof(l->text) - 1);
    l->text[sizeof(l->text) - 1] = '\0';
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
    Button* btn = (Button*)self;
    if (btn->focused)
        printf("\033[%d;%dH> %s <", self->y, self->x, btn->text ? btn->text : "");
    else
        printf("\033[%d;%dH  %s  ", self->y, self->x, btn->text ? btn->text : "");
}

static inline void Button_Update(Control* self, u64 kDown, u64 kHeld)
{
    (void)kHeld;
    if (!self) return;
    Button* btn = (Button*)self;
    if (btn->focused && (kDown & HidNpadButton_A))
        if (btn->OnClick) btn->OnClick(self->context);
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
    int     value;      // 0–100
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
// StringBuilder — C#-kompatibler String-Builder
//
// Verwendung in C#:
//   var sb = new StringBuilder(512);
//   sb.Append("Hello");
//   sb.Append(42);
//   sb.AppendLine("World");
//   string result = sb.ToString();
//   sb.Clear();
//   int len = sb.Length;
// ============================================================================

typedef struct
{
    char* buf;
    int    length;
    int    capacity;
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
    int newCap = sb->capacity * 2;
    while (newCap < sb->length + needed + 1) newCap *= 2;
    sb->buf = (char*)realloc(sb->buf, newCap);
    sb->capacity = newCap;
}

static inline void StringBuilder_AppendStr(StringBuilder* sb, const char* s)
{
    if (!sb || !s) return;
    int len = (int)strlen(s);
    StringBuilder__grow(sb, len);
    memcpy(sb->buf + sb->length, s, len + 1);
    sb->length += len;
}

static inline void StringBuilder_AppendInt(StringBuilder* sb, int val)
{
    if (!sb) return;
    char tmp[32];
    snprintf(tmp, sizeof(tmp), "%d", val);
    StringBuilder_AppendStr(sb, tmp);
}

static inline void StringBuilder_AppendUInt(StringBuilder* sb, unsigned int val)
{
    if (!sb) return;
    char tmp[32];
    snprintf(tmp, sizeof(tmp), "%u", val);
    StringBuilder_AppendStr(sb, tmp);
}

static inline void StringBuilder_AppendFloat(StringBuilder* sb, float val)
{
    if (!sb) return;
    char tmp[32];
    snprintf(tmp, sizeof(tmp), "%f", val);
    StringBuilder_AppendStr(sb, tmp);
}

static inline void StringBuilder_AppendChar(StringBuilder* sb, char c)
{
    if (!sb) return;
    StringBuilder__grow(sb, 1);
    sb->buf[sb->length++] = c;
    sb->buf[sb->length] = '\0';
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

// ToString() gibt den internen Puffer zurueck (kein malloc — Zeiger bleibt gueltig bis Clear/Free)
static inline const char* StringBuilder_ToString(StringBuilder* sb)
{
    if (!sb) return "";
    return sb->buf;
}

// int.ToString() / uint.ToString() Hilfsfunktionen
// Schreiben in _cs2sx_strbuf — sofort verwenden!
static inline const char* Int_ToString(int val)
{
    extern char _cs2sx_strbuf[512];
    snprintf(_cs2sx_strbuf, 512, "%d", val);
    return _cs2sx_strbuf;
}

static inline const char* UInt_ToString(unsigned int val)
{
    extern char _cs2sx_strbuf[512];
    snprintf(_cs2sx_strbuf, 512, "%u", val);
    return _cs2sx_strbuf;
}

static inline const char* Float_ToString(float val)
{
    extern char _cs2sx_strbuf[512];
    snprintf(_cs2sx_strbuf, 512, "%f", val);
    return _cs2sx_strbuf;
}

// string.IsNullOrEmpty
static inline int String_IsNullOrEmpty(const char* s)
{
    return s == NULL || s[0] == '\0';
}

// string.Contains
static inline int String_Contains(const char* haystack, const char* needle)
{
    if (!haystack || !needle) return 0;
    return strstr(haystack, needle) != NULL;
}

// string.StartsWith
static inline int String_StartsWith(const char* s, const char* prefix)
{
    if (!s || !prefix) return 0;
    return strncmp(s, prefix, strlen(prefix)) == 0;
}

// string.EndsWith
static inline int String_EndsWith(const char* s, const char* suffix)
{
    if (!s || !suffix) return 0;
    int sl = (int)strlen(s);
    int fl = (int)strlen(suffix);
    if (fl > sl) return 0;
    return strcmp(s + sl - fl, suffix) == 0;
}

// string.Length → strlen (bereits via TypeMapper, aber zur Vollstaendigkeit)
#define String_Length(s) ((int)strlen(s))

// ============================================================================
// List<T> — dynamische Liste fuer primitive Typen und Pointer
//
// Verwendung:
//   CS2SX_LIST_DEFINE(int)       → erzeugt List_int mit Add/Get/Count/Clear/Free
//   CS2SX_LIST_DEFINE_PTR(Label) → erzeugt List_Label fuer Pointer-Typen
//
// In C# verwendbar als:
//   List<int> nums = new List<int>();
//   List<string> names = new List<string>();
//   List<Label> labels = new List<Label>();
// ============================================================================

#define CS2SX_LIST_INITIAL_CAP 8

#define CS2SX_LIST_DEFINE(T) \
typedef struct { T* data; int count; int capacity; } List_##T; \
static inline List_##T* List_##T##_New(void) { \
    List_##T* l = (List_##T*)malloc(sizeof(List_##T)); \
    if (!l) return NULL; \
    l->data = (T*)malloc(CS2SX_LIST_INITIAL_CAP * sizeof(T)); \
    l->count = 0; l->capacity = CS2SX_LIST_INITIAL_CAP; return l; } \
static inline void List_##T##_Add(List_##T* l, T val) { \
    if (!l) return; \
    if (l->count >= l->capacity) { \
        l->capacity *= 2; \
        l->data = (T*)realloc(l->data, l->capacity * sizeof(T)); } \
    l->data[l->count++] = val; } \
static inline T List_##T##_Get(List_##T* l, int i) { return l->data[i]; } \
static inline void List_##T##_Clear(List_##T* l) { if (l) l->count = 0; } \
static inline void List_##T##_Free(List_##T* l) { if (l) { free(l->data); free(l); } } \
static inline int List_##T##_Contains(List_##T* l, T val) { \
    for (int _i = 0; _i < l->count; _i++) { if (l->data[_i] == val) return 1; } return 0; } \
static inline void List_##T##_Remove(List_##T* l, int idx) { \
    if (!l || idx < 0 || idx >= l->count) return; \
    for (int _i = idx; _i < l->count - 1; _i++) l->data[_i] = l->data[_i+1]; \
    l->count--; }

// Pointer-Variante (fuer Label*, Button* etc.)
#define CS2SX_LIST_DEFINE_PTR(T) \
typedef struct { T** data; int count; int capacity; } List_##T; \
static inline List_##T* List_##T##_New(void) { \
    List_##T* l = (List_##T*)malloc(sizeof(List_##T)); \
    if (!l) return NULL; \
    l->data = (T**)malloc(CS2SX_LIST_INITIAL_CAP * sizeof(T*)); \
    l->count = 0; l->capacity = CS2SX_LIST_INITIAL_CAP; return l; } \
static inline void List_##T##_Add(List_##T* l, T* val) { \
    if (!l) return; \
    if (l->count >= l->capacity) { \
        l->capacity *= 2; \
        l->data = (T**)realloc(l->data, l->capacity * sizeof(T*)); } \
    l->data[l->count++] = val; } \
static inline T* List_##T##_Get(List_##T* l, int i) { return l->data[i]; } \
static inline void List_##T##_Clear(List_##T* l) { if (l) l->count = 0; } \
static inline void List_##T##_Free(List_##T* l) { if (l) { free(l->data); free(l); } } \
static inline void List_##T##_Remove(List_##T* l, int idx) { \
    if (!l || idx < 0 || idx >= l->count) return; \
    for (int _i = idx; _i < l->count - 1; _i++) l->data[_i] = l->data[_i+1]; \
    l->count--; }

// Vordefinierte Typen
CS2SX_LIST_DEFINE(int)
CS2SX_LIST_DEFINE(float)
CS2SX_LIST_DEFINE(double)
CS2SX_LIST_DEFINE(u8)
CS2SX_LIST_DEFINE(u16)
CS2SX_LIST_DEFINE(u32)
CS2SX_LIST_DEFINE(u64)
CS2SX_LIST_DEFINE(s32)
CS2SX_LIST_DEFINE(s64)
// string = const char* Pointer-Liste
// String-Liste: speichert const char* (String-Literale sind const)
typedef struct { const char** data; int count; int capacity; } List_char;
static inline List_char* List_char_New(void) {
    List_char* l = (List_char*)malloc(sizeof(List_char));
    if (!l) return NULL;
    l->data = (const char**)malloc(CS2SX_LIST_INITIAL_CAP * sizeof(const char*));
    l->count = 0; l->capacity = CS2SX_LIST_INITIAL_CAP; return l;
}
static inline void List_char_Add(List_char* l, const char* val) {
    if (!l) return;
    if (l->count >= l->capacity) {
        l->capacity *= 2;
        l->data = (const char**)realloc(l->data, l->capacity * sizeof(const char*));
    }
    l->data[l->count++] = val;
}
static inline const char* List_char_Get(List_char* l, int i) { return l->data[i]; }
static inline void List_char_Clear(List_char* l) { if (l) l->count = 0; }
static inline void List_char_Free(List_char* l) { if (l) { free(l->data); free(l); } }
static inline void List_char_Remove(List_char* l, int idx) {
    if (!l || idx < 0 || idx >= l->count) return;
    for (int _i = idx; _i < l->count - 1; _i++) l->data[_i] = l->data[_i + 1];
    l->count--;
}

// ============================================================================
// Form
// ============================================================================

typedef struct Form Form;
struct Form
{
    Control* controls[FORM_MAX_CONTROLS];
    int      count;
    int      focusedIndex;  // Index des aktuell fokussierten Controls (-1 = keiner)
};

static inline void Form_Add(Form* form, Control* control)
{
    if (!form || !control) return;
    if (form->count >= FORM_MAX_CONTROLS) return;
    form->controls[form->count++] = control;
}

// Gibt den Index des aktuell fokussierten, fokussierbaren Controls zurück (-1 wenn keiner).
static inline int Form_GetFocusedIndex(const Form* form)
{
    if (!form) return -1;
    for (int i = 0; i < form->count; i++)
    {
        if (!form->controls[i]->focusable) continue;
        Button* btn = (Button*)form->controls[i];
        if (btn->focused) return i;
    }
    return -1;
}

// Verschiebt den Fokus um `direction` (+1 = vorwärts, -1 = rückwärts).
static inline void Form_MoveFocus(Form* form, int direction)
{
    if (!form || form->count == 0) return;

    // Aktuellen Fokus finden und entfernen
    int current = -1;
    for (int i = 0; i < form->count; i++)
    {
        if (!form->controls[i]->focusable) continue;
        Button* btn = (Button*)form->controls[i];
        if (btn->focused)
        {
            btn->focused = 0;
            current = i;
            break;
        }
    }

    // Ersten fokussierbaren Control finden, wenn noch keiner gewählt war
    if (current == -1)
    {
        for (int i = 0; i < form->count; i++)
        {
            if (form->controls[i]->focusable)
            {
                ((Button*)form->controls[i])->focused = 1;
                form->focusedIndex = i;
                return;
            }
        }
        return;  // kein fokussierbares Control vorhanden
    }

    // Nächsten fokussierbaren Control in `direction` suchen
    int next = current;
    for (int tries = 0; tries < form->count; tries++)
    {
        next += direction;
        if (next < 0)           next = form->count - 1;
        if (next >= form->count) next = 0;
        if (form->controls[next]->focusable)
        {
            ((Button*)form->controls[next])->focused = 1;
            form->focusedIndex = next;
            return;
        }
    }
    // Fallback: ursprünglichen Fokus wiederherstellen
    if (current >= 0 && form->controls[current]->focusable)
        ((Button*)form->controls[current])->focused = 1;
}

// Setzt initialen Fokus auf den ersten fokussierbaren Control.
static inline void Form_InitFocus(Form* form)
{
    if (!form) return;
    for (int i = 0; i < form->count; i++)
    {
        if (form->controls[i]->focusable)
        {
            ((Button*)form->controls[i])->focused = 1;
            form->focusedIndex = i;
            return;
        }
    }
}

static inline void Form_DrawAll(Form* form)
{
    if (!form) return;
    for (int i = 0; i < form->count; i++)
    {
        Control* ctrl = form->controls[i];
        if (ctrl && ctrl->visible && ctrl->Draw)
            ctrl->Draw(ctrl);
    }
}

static inline void Form_UpdateAll(Form* form, u64 kDown, u64 kHeld)
{
    if (!form) return;

    // Navigation
    if (kDown & HidNpadButton_Down) Form_MoveFocus(form, 1);
    if (kDown & HidNpadButton_Up)   Form_MoveFocus(form, -1);

    // Controls aktualisieren
    for (int i = 0; i < form->count; i++)
    {
        Control* ctrl = form->controls[i];
        if (ctrl && ctrl->visible && ctrl->Update)
            ctrl->Update(ctrl, kDown, kHeld);
    }
}

// Gibt alle Controls frei.
static inline void Form_Free(Form* form)
{
    if (!form) return;
    for (int i = 0; i < form->count; i++)
    {
        free(form->controls[i]);
        form->controls[i] = NULL;
    }
    form->count = 0;
    form->focusedIndex = -1;
}