// Runtime module for switchforms.h — generated split: state and non-trivial functions live
// here exactly once (a header `static` would give every translation unit its own copy).
#include "cs2sx_prelude.h"
#include <sys/iosupport.h>
#include <malloc.h>

/// GC.GetTotalMemory: bytes currently allocated on the heap.
long long CS2SX_HeapUsed(void)
{
    struct mallinfo mi = mallinfo();
    return (long long)mi.uordblks;
}

// ── Test output (runtime tests) ─────────────────────────────────────────────
// With g_cs2sx_test_output set (build with CS2SX_TEST_OUTPUT=1) every stdout line
// is also sent to svcOutputDebugString; emulators log it as guest output, where the
// test harness reads it. stdout keeps going to the console when one is active.
extern int g_cs2sx_test_output;
static const devoptab_t* s_cs2sx_console_stdout;
static devoptab_t s_cs2sx_test_stdout;
static char s_cs2sx_test_line[1024];
static size_t s_cs2sx_test_len;

static ssize_t cs2sx_test_write(struct _reent* r, void* fd, const char* ptr, size_t len)
{
    for (size_t i = 0; i < len; i++)
    {
        if (ptr[i] == '\n')
        {
            svcOutputDebugString(s_cs2sx_test_line, s_cs2sx_test_len);
            s_cs2sx_test_len = 0;
        }
        else if (ptr[i] != '\r' && s_cs2sx_test_len < sizeof(s_cs2sx_test_line))
            s_cs2sx_test_line[s_cs2sx_test_len++] = ptr[i];
    }
    if (s_cs2sx_console_stdout && s_cs2sx_console_stdout->write_r)
        return s_cs2sx_console_stdout->write_r(r, fd, ptr, len);
    return (ssize_t)len;
}

void CS2SX_InstallTestOutput(int console_active)
{
    if (!g_cs2sx_test_output) return;
    const devoptab_t* cur = devoptab_list[STD_OUT];
    if (cur != &s_cs2sx_test_stdout)
        s_cs2sx_console_stdout = cur;
    if (!console_active)
        s_cs2sx_console_stdout = NULL;
    s_cs2sx_test_stdout = s_cs2sx_console_stdout ? *s_cs2sx_console_stdout : (devoptab_t){ 0 };
    s_cs2sx_test_stdout.name = "cs2sx_test";
    s_cs2sx_test_stdout.write_r = cs2sx_test_write;
    devoptab_list[STD_OUT] = &s_cs2sx_test_stdout;
    setvbuf(stdout, NULL, _IONBF, 0);
}

void* CS2SX_ArrNew(int count, size_t elemSize)
{
    if (count < 0) count = 0;
    CS2SX_ArrHdr* h = (CS2SX_ArrHdr*)calloc(1, sizeof(CS2SX_ArrHdr) + (size_t)count * elemSize);
    if (!h) return NULL;
    h->magic = CS2SX_ARR_MAGIC;
    h->length = count;
    return h + 1;
}

void CS2SX_ArrFree(void* a)
{
    if (!a) return;
    CS2SX_ArrHdr* h = (CS2SX_ArrHdr*)a - 1;
    if (h->magic != CS2SX_ARR_MAGIC) return;   // not ours (or a stack array) — never free
    h->magic = 0;
    free(h);
}

void* CS2SX_ArrResize(void* a, int newCount, size_t elemSize)
{
    void* r = CS2SX_ArrNew(newCount, elemSize);
    if (!r) return a;
    int keep = CS2SX_ArrLen(a);
    if (keep > newCount) keep = newCount;
    if (a && keep > 0) memcpy(r, a, (size_t)keep * elemSize);
    CS2SX_ArrFree(a);
    return r;
}

// ── Deferred releases (see CS2SX_ReleaseLater in switchforms.h) ─────────────
typedef struct { void* obj; CS2SX_FreeFn fn; } CS2SX_PendingRelease;
static CS2SX_PendingRelease* s_cs2sx_pending;
static int s_cs2sx_pending_count, s_cs2sx_pending_cap;

void CS2SX_ReleaseLater(void* obj, CS2SX_FreeFn free_fn)
{
    if (!obj || !free_fn) return;
    if (s_cs2sx_pending_count == s_cs2sx_pending_cap)
    {
        int cap = s_cs2sx_pending_cap ? s_cs2sx_pending_cap * 2 : 64;
        CS2SX_PendingRelease* p = (CS2SX_PendingRelease*)realloc(s_cs2sx_pending, cap * sizeof(*p));
        if (!p) { free_fn(obj); return; }   // out of memory: release now rather than leak
        s_cs2sx_pending = p;
        s_cs2sx_pending_cap = cap;
    }
    s_cs2sx_pending[s_cs2sx_pending_count].obj = obj;
    s_cs2sx_pending[s_cs2sx_pending_count].fn = free_fn;
    s_cs2sx_pending_count++;
}

void CS2SX_DrainReleases(void)
{
    // A release can destroy an object whose cleanup queues further releases
    // (a finalizer clearing a list): keep going until the queue is empty.
    while (s_cs2sx_pending_count > 0)
    {
        CS2SX_PendingRelease r = s_cs2sx_pending[--s_cs2sx_pending_count];
        r.fn(r.obj);
    }
}

char* _cs2sx_next_buf(void)
{
    size_t pos = (_cs2sx_arena_pos + 7u) & ~(size_t)7u;
    if (pos + CS2SX_STRBUF_SIZE > CS2SX_ARENA_SIZE) pos = 0;
    _cs2sx_arena_pos = pos + CS2SX_STRBUF_SIZE;
    return _cs2sx_arena + pos;
}

// ── Owned strings of fields ─────────────────────────────────────────────────
// A string field holds its own heap copy, marked by a header in front of the
// characters, so overwriting the field can free the old value — and leave alone
// what it does not own (literals, borrowed pointers).
#define CS2SX_STR_MAGIC 0x52545343u   /* "CSTR" */
typedef struct { unsigned int magic; unsigned int reserved; } CS2SX_StrHdr;

const char* CS2SX_StrOwn(const char* s)
{
    if (!s) return NULL;
    size_t len = strlen(s);
    CS2SX_StrHdr* h = (CS2SX_StrHdr*)malloc(sizeof(CS2SX_StrHdr) + len + 1);
    if (!h) return NULL;
    h->magic = CS2SX_STR_MAGIC;
    h->reserved = 0;
    char* chars = (char*)(h + 1);
    memcpy(chars, s, len + 1);
    return chars;
}

void CS2SX_StrDrop(const char* s)
{
    if (!s) return;
    CS2SX_StrHdr* h = (CS2SX_StrHdr*)s - 1;
    if (h->magic != CS2SX_STR_MAGIC) return;   // not an owned copy — never free
    h->magic = 0;                               // dropped once only
    CS2SX_ReleaseLater(h, free);                // readers keep it until the next frame
}

char* _cs2sx_heap_strdup(const char* src)
{
    if (!src) return NULL;
    int len = (int)strlen(src);
    char* buf = (char*)malloc(len + 1);
    if (!buf) return NULL;
    memcpy(buf, src, len + 1);
    return buf;
}

Label* Label_New(const char* text)
{
    Label* l = (Label*)malloc(sizeof(Label));
    if (!l) return NULL;
    memset(l, 0, sizeof(Label));
    l->base.visible = 1;
    l->base.focusable = 0;
    l->base.Draw = Label_Draw;
    if (text && *text) Label_SetText(l, text);
    return l;
}

void Button_Draw(Control* self)
{
    if (!self || !self->visible) return;
    Button* b = (Button*)self;
    const char* t = b->text ? b->text : "";
    if (self->focused)
        printf("\033[%d;%dH> %s <", self->y, self->x, t);
    else
        printf("\033[%d;%dH  %s  ", self->y, self->x, t);
}

void Button_Update(Control* self, u64 kDown, u64 kHeld)
{
    (void)kHeld;
    if (!self) return;
    Button* b = (Button*)self;
    if (self->focused && (kDown & HidNpadButton_A) && b->OnClick)
        Action_Invoke(b->OnClick);
}

Button* Button_New(const char* text)
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

void ProgressBar_Draw(Control* self)
{
    if (!self || !self->visible) return;
    ProgressBar* pb = (ProgressBar*)self;
    int          fill = (pb->value * pb->width_chars) / 100;
    printf("\033[%d;%dH[", self->y, self->x);
    for (int i = 0; i < pb->width_chars; i++)
        printf(i < fill ? "#" : "-");
    printf("] %3d%%", pb->value);
}

ProgressBar* ProgressBar_New(int width_chars)
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

StringBuilder* StringBuilder_New(int capacity)
{
    if (capacity <= 0) capacity = 256;
    StringBuilder* sb = (StringBuilder*)malloc(sizeof(StringBuilder));
    if (!sb) return NULL;
    sb->buf = (char*)malloc(capacity);
    if (!sb->buf) { free(sb); return NULL; }
    sb->buf[0] = '\0';
    sb->length = 0;
    sb->capacity = capacity;
    sb->_rc = 1;
    CS2SX_ReleaseLater(sb, (CS2SX_FreeFn)StringBuilder_Free);   // the frame's reference
    return sb;
}

void StringBuilder__grow(StringBuilder* sb, int needed)
{
    if (sb->length + needed + 1 <= sb->capacity) return;
    int cap = sb->capacity * 2;
    while (cap < sb->length + needed + 1) cap *= 2;
    char* nb = (char*)realloc(sb->buf, cap);
    if (!nb) return;   // OOM: keep the existing buffer rather than leaking/NULL-derefing
    sb->buf = nb;
    sb->capacity = cap;
}

StringBuilder* StringBuilder_AppendStr(StringBuilder* sb, const char* s)
{
    if (!sb || !s) return sb;
    int len = (int)strlen(s);
    StringBuilder__grow(sb, len);
    memcpy(sb->buf + sb->length, s, len + 1);
    sb->length += len;
    return sb;
}

StringBuilder* StringBuilder_AppendChar(StringBuilder* sb, char c)
{
    if (!sb) return sb;
    StringBuilder__grow(sb, 1);
    sb->buf[sb->length++] = c;
    sb->buf[sb->length] = '\0';
    return sb;
}

StringBuilder* StringBuilder_AppendInt(StringBuilder* sb, int val)
{
    char tmp[32];
    snprintf(tmp, sizeof(tmp), "%d", val);
    StringBuilder_AppendStr(sb, tmp);
    return sb;
}

StringBuilder* StringBuilder_AppendUInt(StringBuilder* sb, unsigned int val)
{
    char tmp[32];
    snprintf(tmp, sizeof(tmp), "%u", val);
    StringBuilder_AppendStr(sb, tmp);
    return sb;
}

StringBuilder* StringBuilder_AppendFloat(StringBuilder* sb, float val)
{
    char tmp[32];
    snprintf(tmp, sizeof(tmp), "%f", val);
    StringBuilder_AppendStr(sb, tmp);
    return sb;
}

StringBuilder* StringBuilder_AppendLong(StringBuilder* sb, long long val)
{
    char tmp[32];
    snprintf(tmp, sizeof(tmp), "%lld", val);
    StringBuilder_AppendStr(sb, tmp);
    return sb;
}

StringBuilder* StringBuilder_AppendULong(StringBuilder* sb, unsigned long long val)
{
    char tmp[32];
    snprintf(tmp, sizeof(tmp), "%llu", val);
    StringBuilder_AppendStr(sb, tmp);
    return sb;
}

StringBuilder* StringBuilder_AppendDouble(StringBuilder* sb, double val)
{
    char tmp[32];
    snprintf(tmp, sizeof(tmp), "%g", val);
    StringBuilder_AppendStr(sb, tmp);
    return sb;
}

void StringBuilder_Insert(StringBuilder* sb, int index, const char* s)
{
    if (!sb || !s || index < 0 || index > sb->length) return;
    int len = (int)strlen(s);
    StringBuilder__grow(sb, len);
    memmove(sb->buf + index + len, sb->buf + index, sb->length - index + 1);
    memcpy(sb->buf + index, s, len);
    sb->length += len;
}

void StringBuilder_Replace(StringBuilder* sb, const char* from, const char* to)
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

const char* String_Trim(const char* s)
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

const char* String_TrimStart(const char* s)
{
    if (!s) return "";
    while (*s == ' ' || *s == '\t') s++;
    // Copy into a pooled buffer instead of returning an interior pointer into
    // the source (which would alias/dangle if the source is freed or reused).
    char* buf = _cs2sx_next_buf();
    int i = 0;
    for (; s[i] && i < CS2SX_STRBUF_SIZE - 1; i++) buf[i] = s[i];
    buf[i] = '\0';
    return buf;
}

const char* String_TrimEnd(const char* s)
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

const char* String_ToUpper(const char* s)
{
    if (!s) return "";
    char* buf = _cs2sx_next_buf();
    int i = 0;
    for (; s[i] && i < CS2SX_STRBUF_SIZE - 1; i++)
        buf[i] = (char)toupper((unsigned char)s[i]);
    buf[i] = '\0';
    return buf;
}

const char* String_ToLower(const char* s)
{
    if (!s) return "";
    char* buf = _cs2sx_next_buf();
    int i = 0;
    for (; s[i] && i < CS2SX_STRBUF_SIZE - 1; i++)
        buf[i] = (char)tolower((unsigned char)s[i]);
    buf[i] = '\0';
    return buf;
}

const char* String_Substring(const char* s, int start, int length)
{
    if (!s) CS2SX_ThrowNullRef();
    int slen = (int)strlen(s);
    // C#: startIndex in [0, Length], length >= 0, startIndex + length <= Length
    if (start < 0 || start > slen) CS2SX_ThrowArgOutOfRange("startIndex");
    if (length < 0 || length > slen - start) CS2SX_ThrowArgOutOfRange("length");
    char* buf = _cs2sx_next_buf();
    if (length >= CS2SX_STRBUF_SIZE) length = CS2SX_STRBUF_SIZE - 1;
    memcpy(buf, s + start, length);
    buf[length] = '\0';
    return buf;
}

const char* String_SubstringFrom(const char* s, int start)
{
    if (!s) CS2SX_ThrowNullRef();
    int slen = (int)strlen(s);
    if (start < 0 || start > slen) CS2SX_ThrowArgOutOfRange("startIndex");
    if (start == slen) return "";
    // Copy into a pooled buffer rather than returning an interior pointer.
    char* buf = _cs2sx_next_buf();
    int i = 0;
    for (; s[start + i] && i < CS2SX_STRBUF_SIZE - 1; i++) buf[i] = s[start + i];
    buf[i] = '\0';
    return buf;
}

int String_LastIndexOf(const char* s, const char* sub)
{
    if (!s || !sub) return -1;
    int sublen = (int)strlen(sub);
    int slen = (int)strlen(s);
    for (int i = slen - sublen; i >= 0; i--)
        if (strncmp(s + i, sub, sublen) == 0) return i;
    return -1;
}

const char* String_Replace(const char* s, const char* from, const char* to)
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

const char* String_PadLeft(const char* s, int totalWidth, char padChar)
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

const char* String_PadRight(const char* s, int totalWidth, char padChar)
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

int String_EqualsIgnoreCase(const char* a, const char* b)
{
    if (!a || !b) return 0;
    while (*a && *b)
    {
        if (tolower((unsigned char)*a) != tolower((unsigned char)*b)) return 0;
        a++; b++;
    }
    return *a == '\0' && *b == '\0';
}

// int.Parse & co. live in rt_parse.c.

List_str* List_str_New(void)
{
    List_str* l = (List_str*)malloc(sizeof(List_str));
    if (!l) return NULL;
    l->data = (char**)malloc(CS2SX_LIST_INITIAL_CAP * sizeof(char*));
    l->count = 0; l->capacity = CS2SX_LIST_INITIAL_CAP;
    l->_rc = 1;
    CS2SX_ReleaseLater(l, (CS2SX_FreeFn)List_str_Free);   // the frame's reference
    return l;
}

void List_str_Add(List_str* l, const char* val)
{
    if (!l) CS2SX_ThrowNullRef();
    if (l->count >= l->capacity)
    {
        l->capacity *= 2;
        l->data = (char**)realloc(l->data, l->capacity * sizeof(char*));
    }
    // Heap-Kopie: sicher gegenüber Pool-Rotation
    l->data[l->count++] = val ? _cs2sx_heap_strdup(val) : NULL;
}

void List_str_Free(List_str* l)
{
    if (!l || --l->_rc > 0) return;
    for (int i = 0; i < l->count; i++) free(l->data[i]);
    free(l->data);
    free(l);
}

const char** CS2SX_StrArr_FromList(List_str* l)
{
    int n = l ? l->count : 0;
    const char** a = (const char**)CS2SX_ArrNew(n, sizeof(const char*));
    if (a && n > 0) memcpy((void*)a, l->data, (size_t)n * sizeof(char*));
    if (l) l->count = 0;   // the strings now belong to the array; the list itself to the frame
    return a;
}

void List_str_Remove(List_str* l, int idx)
{
    CS2SX_ListCheck(l ? l->count : -1, idx);
    free(l->data[idx]);
    for (int _i = idx; _i < l->count - 1; _i++) l->data[_i] = l->data[_i + 1];
    l->count--;
}

void List_str_Insert(List_str* l, int idx, const char* val)
{
    CS2SX_ListCheck(l ? l->count + 1 : -1, idx);
    List_str_Add(l, val);   // grows and copies val; then move it into place
    char* added = l->data[l->count - 1];
    for (int _i = l->count - 1; _i > idx; _i--) l->data[_i] = l->data[_i - 1];
    l->data[idx] = added;
}

void List_str_Sort(List_str* l)
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

const char* String_Join(const char* sep, List_str* list)
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

List_str* String_Split(const char* s, const char* sep)
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

List_str* String_Split_RemoveEmpty(const char* s, const char* sep)
{
    List_str* raw = String_Split(s, sep);
    if (!raw) return List_str_New();
    List_str* result = List_str_New();
    for (int _i = 0; _i < raw->count; _i++)
        if (raw->data[_i] && raw->data[_i][0] != '\0')
            List_str_Add(result, raw->data[_i]);
    // raw belongs to the frame (released at the next frame)
    return result;
}

List_str* String_Split_Whitespace(const char* s)
{
    List_str* result = List_str_New();
    if (!s || !result) return result;

    int srclen = (int)strlen(s);
    char* src = (char*)malloc(srclen + 1);
    if (!src) return result;
    memcpy(src, s, srclen + 1);

    char* cur = src;
    while (*cur)
    {
        while (*cur == ' ' || *cur == '\t' || *cur == '\n' || *cur == '\r') cur++;
        if (!*cur) break;
        char* start = cur;
        while (*cur && *cur != ' ' && *cur != '\t' && *cur != '\n' && *cur != '\r') cur++;
        if (*cur) { *cur = '\0'; cur++; }
        List_str_Add(result, start);
    }
    free(src);
    return result;
}

Dict_str_str* Dict_str_str_New(void)
{
    Dict_str_str* d = (Dict_str_str*)malloc(sizeof(Dict_str_str));
    if (!d) return NULL;
    d->keys = (const char**)malloc(CS2SX_DICT_INITIAL_CAP * sizeof(const char*));
    d->vals = (const char**)malloc(CS2SX_DICT_INITIAL_CAP * sizeof(const char*));
    d->count = 0; d->capacity = CS2SX_DICT_INITIAL_CAP;
    d->_rc = 1;
    CS2SX_ReleaseLater(d, (CS2SX_FreeFn)Dict_str_str_Free);   // the frame's reference
    return d;
}

void Dict_str_str_Add(Dict_str_str* d, const char* key, const char* val)
{
    if (!d) CS2SX_ThrowNullRef();
    if (!key) CS2SX_ThrowArgNull("key");
    if (Dict_str_str_ContainsKey(d, key)) CS2SX_ThrowDuplicateKey();
    if (d->count >= d->capacity) {
        d->capacity *= 2;
        d->keys = (const char**)realloc(d->keys, d->capacity * sizeof(const char*));
        d->vals = (const char**)realloc(d->vals, d->capacity * sizeof(const char*));
    }
    d->keys[d->count] = key; d->vals[d->count] = val; d->count++;
}

void Dict_str_str_Set(Dict_str_str* d, const char* key, const char* val)
{
    for (int _i = 0; _i < d->count; _i++) {
        if (strcmp(d->keys[_i], key) == 0) { d->vals[_i] = val; return; }
    }
    Dict_str_str_Add(d, key, val);
}

void Dict_str_str_Remove(Dict_str_str* d, const char* key)
{
    for (int _i = 0; _i < d->count; _i++) {
        if (strcmp(d->keys[_i], key) == 0) {
            for (int _j = _i; _j < d->count - 1; _j++) {
                d->keys[_j] = d->keys[_j + 1]; d->vals[_j] = d->vals[_j + 1];
            }
            d->count--; return;
        }
    }
}

List_str* cs2sx_dop_entries(const char* path, int mode)
{
    List_str* result = List_str_New();
    if (!result) return result;
    DIR* dir = opendir(path);
    if (!dir) return result;
    struct dirent* e;
    char pathbuf[1024];
    while ((e = readdir(dir)) != NULL)
    {
        const char* nm = e->d_name;
        if (nm[0] == '.' && (nm[1] == '\0' || (nm[1] == '.' && nm[2] == '\0'))) continue;
        cs2sx_join(pathbuf, sizeof(pathbuf), path, nm);
        if (mode != 0)
        {
            int isdir = cs2sx_dop_isdir(pathbuf);
            if (mode == 1 && isdir) continue;
            if (mode == 2 && !isdir) continue;
        }
        List_str_Add(result, pathbuf);
    }
    closedir(dir);
    return result;
}

int cs2sx_dop_rmtree(const char* path)
{
    if (!cs2sx_dop_isdir(path)) return remove(path) == 0;
    DIR* dir = opendir(path);
    if (dir)
    {
        struct dirent* e;
        char child[1024];
        while ((e = readdir(dir)) != NULL)
        {
            const char* nm = e->d_name;
            if (nm[0] == '.' && (nm[1] == '\0' || (nm[1] == '.' && nm[2] == '\0'))) continue;
            cs2sx_join(child, sizeof(child), path, nm);
            cs2sx_dop_rmtree(child);
        }
        closedir(dir);
    }
    return rmdir(path) == 0;
}

int cs2sx_dop_copy(const char* src, const char* dst)
{
    FILE* in = fopen(src, "rb");
    if (!in) return 0;
    FILE* out = fopen(dst, "wb");
    if (!out) { fclose(in); return 0; }
    static char cbuf[65536];
    size_t r; int ok = 1;
    while ((r = fread(cbuf, 1, sizeof(cbuf), in)) > 0)
        if (fwrite(cbuf, 1, r, out) != r) { ok = 0; break; }
    fclose(out); fclose(in);
    return ok;
}

int CS2SX_File_Exists(const char* path)
{
    if (cs2sx_devoptab(path)) return cs2sx_dop_isfile(path);
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    FsFile f;
    int exists = R_SUCCEEDED(fsFsOpenFile(&fs, path, FsOpenMode_Read, &f));
    if (exists) fsFileClose(&f);
    fsFsClose(&fs);
    return exists;
}

int CS2SX_Dir_Exists(const char* path)
{
    if (cs2sx_devoptab(path)) return cs2sx_dop_isdir(path);
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    FsDir d;
    int exists = R_SUCCEEDED(fsFsOpenDirectory(&fs, path, FsDirOpenMode_ReadDirs, &d));
    if (exists) fsDirClose(&d);
    fsFsClose(&fs);
    return exists;
}

int CS2SX_Dir_Create(const char* path)
{
    if (cs2sx_devoptab(path)) return mkdir(path, 0777) == 0;
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    Result rc = fsFsCreateDirectory(&fs, path);
    fsFsClose(&fs);
    return R_SUCCEEDED(rc);
}

// File.ReadAllText: the whole file as a NEW string per call (C# strings are values;
// a shared buffer would change earlier results). A UTF-8 BOM is dropped like in .NET.
// Missing file → FileNotFoundException, other failures → IOException.
static const char* file_text_finish(char* buf, size_t len)
{
    buf[len] = '\0';
    if (len >= 3 && (unsigned char)buf[0] == 0xEF && (unsigned char)buf[1] == 0xBB
        && (unsigned char)buf[2] == 0xBF)
        memmove(buf, buf + 3, len - 2);
    return buf;
}

const char* CS2SX_File_ReadAllText(const char* path)
{
    if (!path) CS2SX_ThrowArgNull("path");

    // devoptab paths (romfs:/, ums0:/ …) use stdio — fsFs only sees the SD card
    if (cs2sx_devoptab(path))
    {
        FILE* fp = fopen(path, "rb");
        if (!fp) CS2SX_ThrowFileNotFound(path);
        fseek(fp, 0, SEEK_END);
        long sz = ftell(fp);
        fseek(fp, 0, SEEK_SET);
        if (sz < 0) { fclose(fp); CS2SX_ThrowIO("Could not read the file."); }
        char* buf = (char*)malloc((size_t)sz + 1);
        if (!buf) { fclose(fp); CS2SX_ThrowNew(&CS2SX_TI_OutOfMemoryException, NULL); }
        size_t got = fread(buf, 1, (size_t)sz, fp);
        fclose(fp);
        return file_text_finish(buf, got);
    }

    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) CS2SX_ThrowIO("The SD card is not available.");
    FsFile f;
    if (R_FAILED(fsFsOpenFile(&fs, path, FsOpenMode_Read, &f)))
    {
        fsFsClose(&fs);
        CS2SX_ThrowFileNotFound(path);
    }
    s64 size = 0;
    fsFileGetSize(&f, &size);
    char* buf = size >= 0 ? (char*)malloc((size_t)size + 1) : NULL;
    if (!buf)
    {
        fsFileClose(&f);
        fsFsClose(&fs);
        CS2SX_ThrowNew(&CS2SX_TI_OutOfMemoryException, NULL);
    }
    u64 bytesRead = 0;
    Result rc = fsFileRead(&f, 0, buf, (u64)size, FsReadOption_None, &bytesRead);
    fsFileClose(&f);
    fsFsClose(&fs);
    if (R_FAILED(rc)) { free(buf); CS2SX_ThrowIO("Could not read the file."); }
    return file_text_finish(buf, (size_t)bytesRead);
}

int  g_nsp_ok = 0;
char g_nsp_tid[17];
int  g_nsp_type = 0;
int  g_nsp_nca = 0;
long g_nsp_total = 0;
int  g_nsp_ticket = 0;
int  g_nsp_nfiles = 0;
char g_nsp_name[CS2SX_NSP_MAXFILES][64];
long g_nsp_size[CS2SX_NSP_MAXFILES];
int cs2sx_hexv(char c)
{
    if (c >= '0' && c <= '9') return c - '0';
    if (c >= 'a' && c <= 'f') return c - 'a' + 10;
    if (c >= 'A' && c <= 'F') return c - 'A' + 10;
    return -1;
}

int CS2SX_Nsp_Open(const char* path)
{
    g_nsp_ok = 0; g_nsp_type = 0; g_nsp_nca = 0; g_nsp_total = 0;
    g_nsp_ticket = 0; g_nsp_nfiles = 0; g_nsp_tid[0] = '\0';

    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    FsFile f;
    if (R_FAILED(fsFsOpenFile(&fs, path, FsOpenMode_Read, &f))) { fsFsClose(&fs); return 0; }

    unsigned char hdr[16];
    u64 got = 0;
    if (R_FAILED(fsFileRead(&f, 0, hdr, 16, FsReadOption_None, &got)) || got < 16
        || hdr[0] != 'P' || hdr[1] != 'F' || hdr[2] != 'S' || hdr[3] != '0')
    { fsFileClose(&f); fsFsClose(&fs); return 0; }

    unsigned int nfile = hdr[4] | (hdr[5] << 8) | (hdr[6] << 16) | ((unsigned)hdr[7] << 24);
    unsigned int strsz = hdr[8] | (hdr[9] << 8) | (hdr[10] << 16) | ((unsigned)hdr[11] << 24);
    if (nfile == 0 || nfile > 4096 || strsz > (1u << 20))
    { fsFileClose(&f); fsFsClose(&fs); return 0; }

    unsigned int entsz = nfile * 24;
    unsigned char* ent = (unsigned char*)malloc(entsz);
    char* strtab = (char*)malloc(strsz + 1);
    if (!ent || !strtab) { if (ent) free(ent); if (strtab) free(strtab); fsFileClose(&f); fsFsClose(&fs); return 0; }

    u64 g2 = 0, g3 = 0;
    fsFileRead(&f, 16, ent, entsz, FsReadOption_None, &g2);
    fsFileRead(&f, 16 + entsz, strtab, strsz, FsReadOption_None, &g3);
    strtab[strsz] = '\0';
    if (g2 < entsz) { free(ent); free(strtab); fsFileClose(&f); fsFsClose(&fs); return 0; }

    int stored = 0;
    for (unsigned int i = 0; i < nfile; i++)
    {
        unsigned char* e = ent + (size_t)i * 24;
        u64 fsize = (u64)e[8] | ((u64)e[9] << 8) | ((u64)e[10] << 16) | ((u64)e[11] << 24)
                  | ((u64)e[12] << 32) | ((u64)e[13] << 40) | ((u64)e[14] << 48) | ((u64)e[15] << 56);
        unsigned int noff = e[16] | (e[17] << 8) | (e[18] << 16) | ((unsigned)e[19] << 24);
        const char* name = (noff < strsz) ? (strtab + noff) : "";

        g_nsp_total += (long)fsize;
        if (cs2sx_ends(name, ".nca")) g_nsp_nca++;
        if (cs2sx_ends(name, ".tik"))
        {
            g_nsp_ticket = 1;
            if (g_nsp_tid[0] == '\0')   // title id = first 16 hex of the rights-id filename
            {
                int ok = 1;
                for (int k = 0; k < 16; k++) if (cs2sx_hexv(name[k]) < 0) { ok = 0; break; }
                if (ok) { for (int k = 0; k < 16; k++) g_nsp_tid[k] = name[k]; g_nsp_tid[16] = '\0'; }
            }
        }
        if (stored < CS2SX_NSP_MAXFILES)
        {
            int j = 0;
            while (name[j] && j < 63) { g_nsp_name[stored][j] = name[j]; j++; }
            g_nsp_name[stored][j] = '\0';
            g_nsp_size[stored] = (long)fsize;
            stored++;
        }
    }
    g_nsp_nfiles = stored;

    // type guess from the title-id's low 12 bits: 000 = app, 800 = update, else dlc
    if (g_nsp_tid[0])
    {
        int v = cs2sx_hexv(g_nsp_tid[13]) * 256 + cs2sx_hexv(g_nsp_tid[14]) * 16 + cs2sx_hexv(g_nsp_tid[15]);
        if (v == 0) g_nsp_type = 1;
        else if (v == 0x800) g_nsp_type = 2;
        else g_nsp_type = 3;
    }

    free(ent); free(strtab); fsFileClose(&f); fsFsClose(&fs);
    g_nsp_ok = 1;
    return 1;
}

List_str* CS2SX_File_ReadAllLines(const char* path)
{
    List_str* result = List_str_New();
    if (!result) return result;

    char* src = (char*)CS2SX_File_ReadAllText(path);   // own heap copy; throws if missing
    char* cur = src;
    while (*cur)
    {
        char* end = cur;
        while (*end && *end != '\n') end++;
        int linelen = (int)(end - cur);
        if (linelen > 0 && cur[linelen - 1] == '\r') linelen--;

        char saved = cur[linelen];
        cur[linelen] = '\0';
        List_str_Add(result, cur);  // List_str_Add macht Heap-Kopie
        cur[linelen] = saved;

        cur = (*end == '\n') ? end + 1 : end;
    }

    free(src);
    return result;
}

int CS2SX_File_WriteAllText(const char* path, const char* content)
{
    if (!content) return 0;
    if (cs2sx_devoptab(path))
    {
        FILE* fp = fopen(path, "wb");
        if (!fp) return 0;
        size_t L = strlen(content);
        int ok = fwrite(content, 1, L, fp) == L;
        fclose(fp);
        return ok ? 1 : 0;
    }
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

int CS2SX_File_AppendAllText(const char* path, const char* content)
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

int CS2SX_File_Delete(const char* path)
{
    if (cs2sx_devoptab(path)) return remove(path) == 0;
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    Result rc = fsFsDeleteFile(&fs, path);
    fsFsClose(&fs);
    return R_SUCCEEDED(rc);
}

// stdio path for any file: bare paths ("/switch/x") are on the SD card.
static void cs2sx_stdio_path(const char* path, char* out, size_t cap)
{
    snprintf(out, cap, cs2sx_devoptab(path) ? "%s" : "sdmc:%s", path);
}

// File.Copy: byte-exact (binary files too). Missing source → FileNotFoundException.
int CS2SX_File_Copy(const char* src, const char* dst)
{
    if (!src) CS2SX_ThrowArgNull("sourceFileName");
    if (!dst) CS2SX_ThrowArgNull("destFileName");
    char ps[1024], pd[1024];
    cs2sx_stdio_path(src, ps, sizeof ps);
    cs2sx_stdio_path(dst, pd, sizeof pd);
    FILE* in = fopen(ps, "rb");
    if (!in) CS2SX_ThrowFileNotFound(src);
    FILE* out = fopen(pd, "wb");
    if (!out) { fclose(in); CS2SX_ThrowIO("Could not create the destination file."); }
    char buf[16 * 1024];
    size_t n;
    int ok = 1;
    while ((n = fread(buf, 1, sizeof buf, in)) > 0)
        if (fwrite(buf, 1, n, out) != n) { ok = 0; break; }
    fclose(in);
    if (fclose(out) != 0) ok = 0;
    if (!ok) CS2SX_ThrowIO("Could not write the destination file.");
    return 1;
}

int CS2SX_Dir_Delete(const char* path)
{
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    Result rc = fsFsDeleteDirectory(&fs, path);
    fsFsClose(&fs);
    return R_SUCCEEDED(rc);
}

int CS2SX_File_CopyBinary(const char* src, const char* dst)
{
    // If either side is a devoptab mount (USB), copy through stdio — it reaches
    // both the SD card and "ums0:/…", so cross-device copies work transparently.
    if (cs2sx_devoptab(src) || cs2sx_devoptab(dst)) return cs2sx_dop_copy(src, dst);

    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;

    FsFile in;
    if (R_FAILED(fsFsOpenFile(&fs, src, FsOpenMode_Read, &in)))
    { fsFsClose(&fs); return 0; }

    s64 size = 0;
    fsFileGetSize(&in, &size);

    fsFsDeleteFile(&fs, dst);
    if (R_FAILED(fsFsCreateFile(&fs, dst, size, 0)))
    { fsFileClose(&in); fsFsClose(&fs); return 0; }

    FsFile out;
    if (R_FAILED(fsFsOpenFile(&fs, dst, FsOpenMode_Write, &out)))
    { fsFileClose(&in); fsFsClose(&fs); return 0; }

    const int CHUNK = 256 * 1024;
    char* buf = (char*)malloc((size_t)CHUNK);
    int ok = (buf != NULL);
    s64 off = 0;
    while (ok && off < size)
    {
        u64 toRead = (u64)((size - off) < CHUNK ? (size - off) : CHUNK);
        u64 got = 0;
        if (R_FAILED(fsFileRead(&in, (u64)off, buf, toRead, FsReadOption_None, &got)) || got == 0)
        { ok = 0; break; }
        if (R_FAILED(fsFileWrite(&out, (u64)off, buf, got, FsWriteOption_None)))
        { ok = 0; break; }
        off += (s64)got;
    }
    if (ok) fsFileFlush(&out);
    if (buf) free(buf);
    fsFileClose(&out);
    fsFileClose(&in);
    fsFsClose(&fs);
    return ok;
}

const char* CS2SX_File_ReadHexDump(const char* path, int maxBytes)
{
    static char* buf = NULL;
    static int   cap = 0;

    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return "";
    FsFile f;
    if (R_FAILED(fsFsOpenFile(&fs, path, FsOpenMode_Read, &f))) { fsFsClose(&fs); return ""; }

    s64 size = 0;
    fsFileGetSize(&f, &size);
    if (maxBytes <= 0) maxBytes = 4096;
    s64 toRead = (size < maxBytes) ? size : (s64)maxBytes;

    unsigned char* raw = (unsigned char*)malloc((size_t)(toRead > 0 ? toRead : 1));
    u64 got = 0;
    if (raw && toRead > 0)
        fsFileRead(&f, 0, raw, (u64)toRead, FsReadOption_None, &got);
    fsFileClose(&f);
    fsFsClose(&fs);
    if (!raw) return "";

    int rows = (int)((got + 15) / 16);
    int needed = rows * 80 + 16;
    if (cap < needed)
    {
        char* nb = (char*)realloc(buf, needed);
        if (!nb) { free(raw); return ""; }
        buf = nb; cap = needed;
    }

    int p = 0;
    for (u64 r = 0; r < got; r += 16)
    {
        p += snprintf(buf + p, cap - p, "%08x  ", (unsigned)r);
        for (int c = 0; c < 16; c++)
        {
            if (r + (u64)c < got) p += snprintf(buf + p, cap - p, "%02x ", raw[r + c]);
            else                  p += snprintf(buf + p, cap - p, "   ");
        }
        p += snprintf(buf + p, cap - p, " |");
        for (int c = 0; c < 16 && r + (u64)c < got; c++)
        {
            unsigned char ch = raw[r + c];
            buf[p++] = (ch >= 32 && ch < 127) ? (char)ch : '.';
        }
        buf[p++] = '|';
        buf[p++] = '\n';
    }
    buf[p] = '\0';
    free(raw);
    return buf;
}

CS2SX_CopyCtx g_cs2sx_copy = {0};
void CS2SX_File_CopyEnd(void)
{
    if (!g_cs2sx_copy.active) return;
    fsFileClose(&g_cs2sx_copy.out);
    fsFileClose(&g_cs2sx_copy.in);
    fsFsClose(&g_cs2sx_copy.fs);
    if (g_cs2sx_copy.buf) { free(g_cs2sx_copy.buf); g_cs2sx_copy.buf = NULL; }
    g_cs2sx_copy.active = 0;
}

long long CS2SX_File_CopyBegin(const char* src, const char* dst)
{
    CS2SX_File_CopyEnd();
    if (R_FAILED(fsOpenSdCardFileSystem(&g_cs2sx_copy.fs))) return -1;
    if (R_FAILED(fsFsOpenFile(&g_cs2sx_copy.fs, src, FsOpenMode_Read, &g_cs2sx_copy.in)))
    { fsFsClose(&g_cs2sx_copy.fs); return -1; }

    s64 size = 0;
    fsFileGetSize(&g_cs2sx_copy.in, &size);

    fsFsDeleteFile(&g_cs2sx_copy.fs, dst);
    if (R_FAILED(fsFsCreateFile(&g_cs2sx_copy.fs, dst, size, 0)))
    { fsFileClose(&g_cs2sx_copy.in); fsFsClose(&g_cs2sx_copy.fs); return -1; }
    if (R_FAILED(fsFsOpenFile(&g_cs2sx_copy.fs, dst, FsOpenMode_Write, &g_cs2sx_copy.out)))
    { fsFileClose(&g_cs2sx_copy.in); fsFsClose(&g_cs2sx_copy.fs); return -1; }

    g_cs2sx_copy.buf = (char*)malloc(CS2SX_COPY_BUF);
    if (!g_cs2sx_copy.buf)
    { fsFileClose(&g_cs2sx_copy.out); fsFileClose(&g_cs2sx_copy.in); fsFsClose(&g_cs2sx_copy.fs); return -1; }

    g_cs2sx_copy.size = size;
    g_cs2sx_copy.off  = 0;
    g_cs2sx_copy.active = 1;
    return (long long)size;
}

long long CS2SX_File_CopyStep(int chunkBytes)
{
    if (!g_cs2sx_copy.active || !g_cs2sx_copy.buf) return -1;
    if (chunkBytes <= 0 || chunkBytes > CS2SX_COPY_BUF) chunkBytes = CS2SX_COPY_BUF;

    s64 remaining = g_cs2sx_copy.size - g_cs2sx_copy.off;
    if (remaining <= 0) return (long long)g_cs2sx_copy.off;   // already complete

    u64 toRead = (u64)(remaining < chunkBytes ? remaining : chunkBytes);
    u64 got = 0;
    if (R_FAILED(fsFileRead(&g_cs2sx_copy.in, (u64)g_cs2sx_copy.off, g_cs2sx_copy.buf,
                            toRead, FsReadOption_None, &got)) || got == 0)
        return -1;
    if (R_FAILED(fsFileWrite(&g_cs2sx_copy.out, (u64)g_cs2sx_copy.off, g_cs2sx_copy.buf,
                             got, FsWriteOption_None)))
        return -1;

    g_cs2sx_copy.off += (s64)got;
    if (g_cs2sx_copy.off >= g_cs2sx_copy.size) fsFileFlush(&g_cs2sx_copy.out);
    return (long long)g_cs2sx_copy.off;
}

long long CS2SX_File_GetSize(const char* path)
{
    if (cs2sx_devoptab(path))
    {
        struct stat st;
        return stat(path, &st) == 0 ? (long long)st.st_size : -1;
    }
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return -1;
    FsFile f;
    if (R_FAILED(fsFsOpenFile(&fs, path, FsOpenMode_Read, &f))) { fsFsClose(&fs); return -1; }
    s64 size = 0;
    fsFileGetSize(&f, &size);
    fsFileClose(&f);
    fsFsClose(&fs);
    return (long long)size;
}

long long CS2SX_File_GetMTime(const char* path)
{
    if (cs2sx_devoptab(path))
    {
        struct stat st;
        return stat(path, &st) == 0 ? (long long)st.st_mtime : 0;
    }
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    FsTimeStampRaw ts;
    Result rc = fsFsGetFileTimeStampRaw(&fs, path, &ts);
    fsFsClose(&fs);
    if (R_FAILED(rc) || !ts.is_valid) return 0;
    return (long long)ts.modified;
}

int CS2SX_File_Rename(const char* src, const char* dst)
{
    if (cs2sx_devoptab(src) || cs2sx_devoptab(dst)) return rename(src, dst) == 0;
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    Result rc = fsFsRenameFile(&fs, src, dst);
    fsFsClose(&fs);
    return R_SUCCEEDED(rc);
}

int CS2SX_Dir_Rename(const char* src, const char* dst)
{
    if (cs2sx_devoptab(src) || cs2sx_devoptab(dst)) return rename(src, dst) == 0;
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    Result rc = fsFsRenameDirectory(&fs, src, dst);
    fsFsClose(&fs);
    return R_SUCCEEDED(rc);
}

int CS2SX_Dir_DeleteRecursive(const char* path)
{
    if (cs2sx_devoptab(path)) return cs2sx_dop_rmtree(path);
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return 0;
    Result rc = fsFsDeleteDirectoryRecursively(&fs, path);
    fsFsClose(&fs);
    return R_SUCCEEDED(rc);
}

long long CS2SX_Fs_GetFreeSpace(void)
{
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return -1;
    s64 out = 0;
    Result rc = fsFsGetFreeSpace(&fs, "/", &out);
    fsFsClose(&fs);
    return R_SUCCEEDED(rc) ? (long long)out : -1;
}

long long CS2SX_Fs_GetTotalSpace(void)
{
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return -1;
    s64 out = 0;
    Result rc = fsFsGetTotalSpace(&fs, "/", &out);
    fsFsClose(&fs);
    return R_SUCCEEDED(rc) ? (long long)out : -1;
}

long long _cs2sx_dirsize_rec(FsFileSystem* fs, const char* path)
{
    FsDir d;
    if (R_FAILED(fsFsOpenDirectory(fs, path, FsDirOpenMode_ReadDirs | FsDirOpenMode_ReadFiles, &d)))
        return 0;

    long long total = 0;
    FsDirectoryEntry ent;
    s64 read = 0;
    while (R_SUCCEEDED(fsDirRead(&d, &read, 1, &ent)) && read == 1)
    {
        if (ent.name[0] == '.' &&
            (ent.name[1] == '\0' || (ent.name[1] == '.' && ent.name[2] == '\0')))
            continue;

        char child[769];   // FS_MAX_PATH
        if (path[0] == '/' && path[1] == '\0')
            snprintf(child, sizeof(child), "/%s", ent.name);
        else
            snprintf(child, sizeof(child), "%s/%s", path, ent.name);

        if (ent.type == FsDirEntryType_Dir) total += _cs2sx_dirsize_rec(fs, child);
        else                                total += (long long)ent.file_size;
    }
    fsDirClose(&d);
    return total;
}

long long CS2SX_Fs_DirSize(const char* path)
{
    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return -1;
    long long t = _cs2sx_dirsize_rec(&fs, path);
    fsFsClose(&fs);
    return t;
}

List_str* CS2SX_Dir_GetFiles(const char* path, const char* pattern)
{
    (void)pattern;
    if (cs2sx_devoptab(path)) return cs2sx_dop_entries(path, 1);
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

    // FIX: Stack statt static — kein Aliasing bei verschachtelten Aufrufen
    FsDirectoryEntry entries[64];
    char pathbuf[512];
    s64 count = 0;
    fsDirRead(&d, &count, 64, entries);
    (void)pattern;
    for (int i = 0; i < (int)count && i < 64; i++)
    {
        snprintf(pathbuf, sizeof(pathbuf), "%s/%s", path, entries[i].name);
        List_str_Add(result, pathbuf);
    }
    fsDirClose(&d);
    fsFsClose(&fs);
    return result;
}

List_str* CS2SX_Dir_GetDirectories(const char* path)
{
    if (cs2sx_devoptab(path)) return cs2sx_dop_entries(path, 2);
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

    FsDirectoryEntry entries[64];
    char pathbuf[512];
    s64 count = 0;
    fsDirRead(&d, &count, 64, entries);
    for (int i = 0; i < (int)count && i < 64; i++)
    {
        snprintf(pathbuf, sizeof(pathbuf), "%s/%s", path, entries[i].name);
        List_str_Add(result, pathbuf);
    }
    fsDirClose(&d);
    fsFsClose(&fs);
    return result;
}

List_str* CS2SX_Dir_GetEntries(const char* path)
{
    if (cs2sx_devoptab(path)) return cs2sx_dop_entries(path, 0);
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

    FsDirectoryEntry entries[128];
    char pathbuf[512];
    s64 count = 0;
    fsDirRead(&d, &count, 128, entries);
    for (int i = 0; i < (int)count && i < 128; i++)
    {
        snprintf(pathbuf, sizeof(pathbuf), "%s/%s", path, entries[i].name);
        List_str_Add(result, pathbuf);
    }
    fsDirClose(&d);
    fsFsClose(&fs);
    return result;
}

const char* CS2SX_Path_GetFileName(const char* path)
{
    if (!path) return "";
    const char* last = path;
    for (const char* p = path; *p; p++)
        if (*p == '/') last = p + 1;
    return last;
}

const char* CS2SX_Path_GetExtension(const char* path)
{
    const char* name = CS2SX_Path_GetFileName(path);
    const char* dot = NULL;
    for (const char* p = name; *p; p++)
        if (*p == '.') dot = p;
    return dot ? dot : "";
}

const char* CS2SX_Path_GetDirectoryName(const char* path)
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

const char* CS2SX_Path_GetFileNameWithoutExt(const char* path)
{
    char* buf = _cs2sx_next_buf();
    const char* fname = CS2SX_Path_GetFileName(path);
    strncpy(buf, fname, CS2SX_STRBUF_SIZE - 1);
    buf[CS2SX_STRBUF_SIZE - 1] = '\0';
    char* dot = NULL;
    for (char* p = buf; *p; p++)
        if (*p == '.') dot = p;
    if (dot) *dot = '\0';
    return buf;
}

const char* CS2SX_Path_ChangeExtension(const char* path, const char* ext)
{
    char* buf = _cs2sx_next_buf();
    if (!path) { buf[0] = '\0'; return buf; }
    char base[CS2SX_STRBUF_SIZE];
    strncpy(base, path, CS2SX_STRBUF_SIZE - 1);
    base[CS2SX_STRBUF_SIZE - 1] = '\0';
    char* dot = NULL;
    for (char* p = base; *p; p++)
        if (*p == '.') dot = p;
    if (dot) *dot = '\0';
    if (ext && ext[0]) {
        if (ext[0] != '.')
            snprintf(buf, CS2SX_STRBUF_SIZE, "%s.%s", base, ext);
        else
            snprintf(buf, CS2SX_STRBUF_SIZE, "%s%s", base, ext);
    } else {
        strncpy(buf, base, CS2SX_STRBUF_SIZE - 1);
        buf[CS2SX_STRBUF_SIZE - 1] = '\0';
    }
    return buf;
}

int String_LastIndexOfChar(const char* s, char c)
{
    if (!s) return -1;
    const char* last = NULL;
    for (const char* p = s; *p; p++)
        if (*p == c) last = p;
    return last ? (int)(last - s) : -1;
}

void Form_InitFocus(Form* form)
{
    if (!form) return;
    for (int i = 0; i < form->count; i++)
    {
        Control* c = form->controls[i];
        if (!c || !c->focusable) continue;
        c->focused = 1;
        form->focusedIndex = i;
        return;
    }
}

void Form_MoveFocus(Form* form, int direction)
{
    if (!form || form->count == 0) return;
    int current = -1;
    for (int i = 0; i < form->count; i++)
    {
        Control* c = form->controls[i];
        if (!c || !c->focusable) continue;
        if (c->focused) { c->focused = 0; current = i; break; }
    }
    if (current == -1) { Form_InitFocus(form); return; }
    int next = current;
    for (int t = 0; t < form->count; t++)
    {
        next += direction;
        if (next < 0)            next = form->count - 1;
        if (next >= form->count) next = 0;
        Control* nc = form->controls[next];
        if (nc && nc->focusable)
        {
            nc->focused = 1;
            form->focusedIndex = next;
            return;
        }
    }
    // No other focusable found — restore focus to current
    if (current >= 0 && form->controls[current] && form->controls[current]->focusable)
        form->controls[current]->focused = 1;
}

void Form_DrawAll(Form* form)
{
    if (!form) return;
    for (int i = 0; i < form->count; i++)
    {
        Control* c = form->controls[i];
        if (c && c->visible && c->Draw) c->Draw(c);
    }
}

void Form_UpdateAll(Form* form, u64 kDown, u64 kHeld)
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

void Form_Free(Form* form)
{
    if (!form) return;
    for (int i = 0; i < form->count; i++)
    {
        Control* c = form->controls[i];
        if (!c) continue;
        // Call the virtual Free function if present, otherwise plain free.
        // This ensures subclass resources (textures, strings, etc.) are released.
        if (c->Free)
            c->Free(c);
        else
            free(c);
        form->controls[i] = NULL;
    }
    form->count = 0;
    form->focusedIndex = -1;
}

const char* CS2SX_File_GetModifiedString(const char* path)
{
    static char buf[32];
    long long t = CS2SX_File_GetMTime(path);
    if (t <= 0) { buf[0] = '\0'; return buf; }
    time_t tt = (time_t)t;
    struct tm* lt = localtime(&tt);
    if (!lt) { buf[0] = '\0'; return buf; }
    snprintf(buf, sizeof(buf), "%04d-%02d-%02d %02d:%02d",
        lt->tm_year + 1900, lt->tm_mon + 1, lt->tm_mday, lt->tm_hour, lt->tm_min);
    return buf;
}

CS2SX_Stopwatch* CS2SX_Stopwatch_New(void)
{
    CS2SX_Stopwatch* sw = (CS2SX_Stopwatch*)malloc(sizeof(CS2SX_Stopwatch));
    if (!sw) return NULL;
    memset(sw, 0, sizeof(CS2SX_Stopwatch));
    return sw;
}

CS2SX_Stopwatch* CS2SX_Stopwatch_StartNew(void)
{
    CS2SX_Stopwatch* sw = CS2SX_Stopwatch_New();
    if (!sw) return NULL;
    sw->start   = armGetSystemTick();
    sw->running = true;
    return sw;
}

void CS2SX_Stopwatch_Reset(CS2SX_Stopwatch* sw)
{
    if (!sw) return;
    sw->accumulated = 0;
    sw->start       = 0;
    sw->running     = false;
}

void CS2SX_Stopwatch_Restart(CS2SX_Stopwatch* sw)
{
    if (!sw) return;
    sw->accumulated = 0;
    sw->start       = armGetSystemTick();
    sw->running     = true;
}

CS2SX_Regex* CS2SX_Regex_New(const char* pattern)
{
    CS2SX_Regex* r = (CS2SX_Regex*)malloc(sizeof(CS2SX_Regex));
    if (!r) return NULL;
    strncpy(r->pattern, pattern ? pattern : "", sizeof(r->pattern)-1);
    r->pattern[sizeof(r->pattern)-1] = '\0';
    return r;
}

int CS2SX_Regex_IsMatch(const char* input, const char* pattern)
{
    if (!input || !pattern) return 0;
    regex_t rx;
    if (regcomp(&rx, pattern, REG_EXTENDED | REG_NOSUB) != 0) return 0;
    int r = regexec(&rx, input, 0, NULL, 0) == 0 ? 1 : 0;
    regfree(&rx);
    return r;
}

const char* CS2SX_Regex_Escape(const char* s)
{
    char* out = _cs2sx_next_buf();
    if (!s) { out[0] = '\0'; return out; }
    int j = 0;
    for (int i = 0; s[i] && j < CS2SX_STRBUF_SIZE - 2; i++)
    {
        char c = s[i];
        if (c=='.'||c=='^'||c=='$'||c=='*'||c=='+'||c=='?'||c=='('||c==')'||
            c=='['||c==']'||c=='{'||c=='}'||c=='|'||c=='\\')
            out[j++] = '\\';
        out[j++] = c;
    }
    out[j] = '\0';
    return out;
}

void CS2SX_Regex_Match(const char* input, const char* pattern,
                                      char* out_buf, int out_size)
{
    if (!input || !pattern || !out_buf || out_size <= 0) return;
    out_buf[0] = '\0';
    regex_t rx;
    if (regcomp(&rx, pattern, REG_EXTENDED) != 0) return;
    regmatch_t m;
    if (regexec(&rx, input, 1, &m, 0) == 0 && m.rm_so >= 0)
    {
        int len = m.rm_eo - m.rm_so;
        if (len >= out_size) len = out_size - 1;
        memcpy(out_buf, input + m.rm_so, len);
        out_buf[len] = '\0';
    }
    regfree(&rx);
}

void CS2SX_Regex_Replace(const char* input, const char* pattern,
                                         const char* replacement,
                                         char* out_buf, int out_size)
{
    if (!input || !pattern || !replacement || !out_buf || out_size <= 0) return;
    out_buf[0] = '\0';
    regex_t rx;
    if (regcomp(&rx, pattern, REG_EXTENDED) != 0) { strncpy(out_buf, input, out_size-1); return; }
    const char* src = input;
    int pos = 0;
    regmatch_t m;
    while (pos < out_size - 1 && regexec(&rx, src, 1, &m, 0) == 0 && m.rm_so >= 0)
    {
        int before = m.rm_so;
        if (pos + before >= out_size - 1) break;
        memcpy(out_buf + pos, src, before); pos += before;
        int replen = (int)strlen(replacement);
        if (pos + replen >= out_size - 1) replen = out_size - 1 - pos;
        memcpy(out_buf + pos, replacement, replen); pos += replen;
        src += m.rm_eo;
        if (m.rm_eo == m.rm_so) { if (*src) out_buf[pos++] = *src++; else break; }
    }
    int rest = (int)strlen(src);
    if (pos + rest >= out_size) rest = out_size - 1 - pos;
    memcpy(out_buf + pos, src, rest); pos += rest;
    out_buf[pos] = '\0';
    regfree(&rx);
}

List_str* CS2SX_Regex_Split(const char* input, const char* pattern)
{
    List_str* result = List_str_New();
    if (!input || !pattern) return result;
    regex_t rx;
    if (regcomp(&rx, pattern, REG_EXTENDED) != 0) { List_str_Add(result, input); return result; }
    const char* src = input;
    regmatch_t m;
    while (regexec(&rx, src, 1, &m, 0) == 0 && m.rm_so >= 0)
    {
        char tmp[CS2SX_STRBUF_SIZE];
        int len = (int)m.rm_so;
        if (len >= CS2SX_STRBUF_SIZE - 1) len = CS2SX_STRBUF_SIZE - 1;
        memcpy(tmp, src, (size_t)len); tmp[len] = '\0';
        List_str_Add(result, tmp);
        src += m.rm_eo;
        if (m.rm_eo == m.rm_so && *src) src++;
    }
    if (*src) List_str_Add(result, src);
    regfree(&rx);
    return result;
}

