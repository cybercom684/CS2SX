// Runtime module: C# strings and chars over UTF-8.
//
// A C# string is UTF-8 in C (const char*); a C# char is a UTF-16 code unit
// (uint16_t, like .NET). Length, s[i], Substring, IndexOf, PadLeft, … count those
// units, so "Größe".Length is 5 and s[2] == 'ö', exactly as in .NET. Characters
// beyond U+FFFF count twice (a surrogate pair), like .NET too. Bytes that are no
// valid UTF-8 count as one character each (U+FFFD when read as a char).
#include "cs2sx_prelude.h"

// ── decoding ────────────────────────────────────────────────────────────────

/// The code point at p and its length in bytes (invalid bytes: U+FFFD, 1 byte).
static unsigned int u8_decode(const unsigned char* p, int* len)
{
    unsigned int c = p[0];
    if (c < 0x80) { *len = 1; return c; }
    if (c >= 0xC2 && c < 0xE0 && (p[1] & 0xC0) == 0x80) { *len = 2; return ((c & 0x1F) << 6) | (p[1] & 0x3F); }
    if (c >= 0xE0 && c < 0xF0 && (p[1] & 0xC0) == 0x80 && (p[2] & 0xC0) == 0x80)
    {
        unsigned int cp = ((c & 0x0F) << 12) | ((p[1] & 0x3F) << 6) | (p[2] & 0x3F);
        if (cp >= 0x800) { *len = 3; return cp; }
    }
    if (c >= 0xF0 && c < 0xF5 && (p[1] & 0xC0) == 0x80 && (p[2] & 0xC0) == 0x80 && (p[3] & 0xC0) == 0x80)
    {
        unsigned int cp = ((c & 0x07) << 18) | ((p[1] & 0x3F) << 12) | ((p[2] & 0x3F) << 6) | (p[3] & 0x3F);
        if (cp >= 0x10000 && cp <= 0x10FFFF) { *len = 4; return cp; }
    }
    *len = 1;
    return 0xFFFD;
}

/// Writes code point cp as UTF-8 into out (room for 4 bytes); returns the byte count.
static int u8_encode(unsigned int cp, char* out)
{
    if (cp < 0x80) { out[0] = (char)cp; return 1; }
    if (cp < 0x800) { out[0] = (char)(0xC0 | (cp >> 6)); out[1] = (char)(0x80 | (cp & 0x3F)); return 2; }
    if (cp < 0x10000)
    {
        out[0] = (char)(0xE0 | (cp >> 12)); out[1] = (char)(0x80 | ((cp >> 6) & 0x3F)); out[2] = (char)(0x80 | (cp & 0x3F));
        return 3;
    }
    out[0] = (char)(0xF0 | (cp >> 18)); out[1] = (char)(0x80 | ((cp >> 12) & 0x3F));
    out[2] = (char)(0x80 | ((cp >> 6) & 0x3F)); out[3] = (char)(0x80 | (cp & 0x3F));
    return 4;
}

static int is_ascii(const char* s, int* bytes)
{
    const char* p = s;
    while (*p && !((unsigned char)*p & 0x80)) p++;
    if (*p) return 0;
    *bytes = (int)(p - s);
    return 1;
}

/// Bytes from s that are ASCII (no 0 byte, no byte >= 0x80), at most limit; 8 at a time.
static int ascii_run(const char* s, int limit)
{
    int i = 0;
    // byte by byte up to an 8-byte boundary: aligned reads never cross a page
    // boundary behind the terminating zero
    while (i < limit && ((uintptr_t)(s + i) & 7))
    {
        if (!s[i] || ((unsigned char)s[i] & 0x80)) return i;
        i++;
    }
    while (i + 8 <= limit)
    {
        uint64_t w = *(const uint64_t*)(const void*)(s + i);
        // a byte >= 0x80, or a zero byte, ends the run
        if ((w & 0x8080808080808080ULL) || ((w - 0x0101010101010101ULL) & ~w & 0x8080808080808080ULL)) break;
        i += 8;
    }
    while (i < limit && s[i] && !((unsigned char)s[i] & 0x80)) i++;
    return i;
}

// ── the UTF-16 view ─────────────────────────────────────────────────────────

int CS2SX_Str16Len(const char* s)
{
    if (!s) CS2SX_ThrowNullRef();
    int ascii = ascii_run(s, 0x7FFFFFFF);
    if (!s[ascii]) return ascii;
    int n = ascii, len;
    for (const unsigned char* p = (const unsigned char*)s + ascii; *p; p += len)
    {
        unsigned int cp = u8_decode(p, &len);
        n += cp >= 0x10000 ? 2 : 1;
    }
    return n;
}

/// Byte offset of UTF-16 index `units` (the end of the string for units == Length);
/// -1 when the string is shorter. An index inside a surrogate pair gives the pair's start.
int CS2SX_Str16Offset(const char* s, int units)
{
    if (units < 0) return -1;
    int n = 0, len;
    const unsigned char* p = (const unsigned char*)s;
    while (*p && n < units)
    {
        unsigned int cp = u8_decode(p, &len);
        int w = cp >= 0x10000 ? 2 : 1;
        if (n + w > units) break;
        n += w;
        p += len;
    }
    if (n < units && !*p) return -1;
    return (int)(p - (const unsigned char*)s);
}

/// UTF-16 units before byte offset `bytes`.
int CS2SX_Str16Index(const char* s, int bytes)
{
    int n = 0, len;
    for (const unsigned char* p = (const unsigned char*)s; *p && p < (const unsigned char*)s + bytes; p += len)
    {
        unsigned int cp = u8_decode(p, &len);
        n += cp >= 0x10000 ? 2 : 1;
    }
    return n;
}

uint16_t CS2SX_Str16At(const char* s, int index)
{
    if (!s) CS2SX_ThrowNullRef();
    // ASCII up to index: the byte is the character (the common case, fast).
    if (index >= 0 && ascii_run(s, index + 1) == index + 1) return (uint16_t)(unsigned char)s[index];
    if (index >= 0)
    {
        int n = 0, len;
        for (const unsigned char* p = (const unsigned char*)s; *p; p += len)
        {
            unsigned int cp = u8_decode(p, &len);
            if (cp < 0x10000) { if (n == index) return (uint16_t)cp; n++; }
            else
            {
                unsigned int v = cp - 0x10000;
                if (n == index) return (uint16_t)(0xD800 + (v >> 10));
                if (n + 1 == index) return (uint16_t)(0xDC00 + (v & 0x3FF));
                n += 2;
            }
        }
    }
    CS2SX_ThrowNew(&CS2SX_TI_IndexOutOfRangeException, NULL);
    return 0;
}

int CS2SX_Str16Next(CS2SX_Str16It* it, uint16_t* out)
{
    if (it->low) { *out = it->low; it->low = 0; return 1; }
    if (!it->p || !*it->p) return 0;
    int len;
    unsigned int cp = u8_decode((const unsigned char*)it->p, &len);
    it->p += len;
    if (cp < 0x10000) { *out = (uint16_t)cp; return 1; }
    unsigned int v = cp - 0x10000;
    *out = (uint16_t)(0xD800 + (v >> 10));
    it->low = (uint16_t)(0xDC00 + (v & 0x3FF));
    return 1;
}

const char* CS2SX_CharStr(uint16_t c)
{
    char* buf = CS2SX_StrAlloc(4);
    int n = u8_encode(c >= 0xD800 && c < 0xE000 ? 0xFFFD : c, buf);
    buf[n] = 0;
    return buf;
}

const char* CS2SX_CharsStr(const uint16_t* chars, int start, int count)
{
    if (!chars) CS2SX_ThrowNullRef();
    char* buf = CS2SX_StrAlloc((size_t)count * 3);
    int out = 0;
    for (int i = start; i < start + count; i++)
    {
        unsigned int c = chars[i];
        if (c >= 0xD800 && c < 0xDC00 && i + 1 < start + count && chars[i + 1] >= 0xDC00 && chars[i + 1] < 0xE000)
        {
            c = 0x10000 + ((c - 0xD800) << 10) + (chars[i + 1] - 0xDC00);
            i++;
        }
        else if (c >= 0xD800 && c < 0xE000) c = 0xFFFD;
        out += u8_encode(c, buf + out);
    }
    buf[out] = 0;
    return buf;
}

const char* CS2SX_CharRepeat(uint16_t c, int count)
{
    if (count < 0) CS2SX_ThrowArgOutOfRange("count");
    char one[4];
    int n = u8_encode(c >= 0xD800 && c < 0xE000 ? 0xFFFD : c, one);
    char* buf = CS2SX_StrAlloc((size_t)count * n);
    for (int i = 0; i < count; i++) memcpy(buf + i * n, one, n);
    buf[count * n] = 0;
    return buf;
}

uint16_t* CS2SX_Str16ToArray(const char* s)
{
    if (!s) CS2SX_ThrowNullRef();
    int n = CS2SX_Str16Len(s);
    uint16_t* arr = (uint16_t*)CS2SX_ArrNew(n, sizeof(uint16_t));
    CS2SX_Str16It it = { s, 0 };
    uint16_t c;
    for (int i = 0; i < n && CS2SX_Str16Next(&it, &c); i++) arr[i] = c;
    return arr;
}

// ── char ────────────────────────────────────────────────────────────────────

uint16_t CS2SX_CharUpper(uint16_t c)
{
    if (c < 0x80) return (c >= 'a' && c <= 'z') ? (uint16_t)(c - 32) : c;
    if (c >= 0xE0 && c <= 0xFE && c != 0xF7) return (uint16_t)(c - 0x20);
    if (c == 0xFF) return 0x178;
    if (c == 0xB5) return 0x39C;
    if (c >= 0x100 && c <= 0x17F)
    {
        if ((c >= 0x139 && c <= 0x148) || (c >= 0x179 && c <= 0x17E)) return (c & 1) ? c : (uint16_t)(c - 1);
        if (c == 0x131) return 'I';
        if (c == 0x17F) return 'S';
        if (c != 0x138 && c != 0x149) return (c & 1) ? (uint16_t)(c - 1) : c;
        return c;
    }
    if (c >= 0x3B1 && c <= 0x3C9 && c != 0x3C2) return (uint16_t)(c - 0x20);
    if (c == 0x3C2) return 0x3A3;
    if (c >= 0x430 && c <= 0x44F) return (uint16_t)(c - 0x20);
    if (c >= 0x450 && c <= 0x45F) return (uint16_t)(c - 0x50);
    return c;
}

uint16_t CS2SX_CharLower(uint16_t c)
{
    if (c < 0x80) return (c >= 'A' && c <= 'Z') ? (uint16_t)(c + 32) : c;
    if (c >= 0xC0 && c <= 0xDE && c != 0xD7) return (uint16_t)(c + 0x20);
    if (c == 0x178) return 0xFF;
    if (c >= 0x100 && c <= 0x17F)
    {
        if ((c >= 0x139 && c <= 0x148) || (c >= 0x179 && c <= 0x17E)) return (c & 1) ? (uint16_t)(c + 1) : c;
        if (c == 0x130) return 'i';
        if (c != 0x138 && c != 0x149 && c != 0x17F) return (c & 1) ? c : (uint16_t)(c + 1);
        return c;
    }
    if (c >= 0x391 && c <= 0x3A9 && c != 0x3A2) return (uint16_t)(c + 0x20);
    if (c >= 0x410 && c <= 0x42F) return (uint16_t)(c + 0x20);
    if (c >= 0x400 && c <= 0x40F) return (uint16_t)(c + 0x50);
    return c;
}

int CS2SX_CharIsLetter(uint16_t c)
{
    if (c < 0x80) return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    if (c == 0xAA || c == 0xB5 || c == 0xBA) return 1;
    if (c >= 0xC0 && c <= 0x24F) return c != 0xD7 && c != 0xF7;
    if (c >= 0x370 && c <= 0x3FF) return c != 0x37E && c != 0x387 && c != 0x375 && !(c >= 0x384 && c <= 0x385);
    if (c >= 0x400 && c <= 0x52F) return !(c >= 0x482 && c <= 0x489);
    if (c >= 0x5D0 && c <= 0x5EA) return 1;                       // Hebrew
    if (c >= 0x620 && c <= 0x64A) return 1;                       // Arabic
    if (c >= 0x3041 && c <= 0x30FF) return c != 0x30FB;           // Hiragana, Katakana
    if (c >= 0x3400 && c <= 0x9FFF) return 1;                     // CJK
    if (c >= 0xAC00 && c <= 0xD7A3) return 1;                     // Hangul
    return 0;
}

int CS2SX_CharIsUpper(uint16_t c) { return CS2SX_CharIsLetter(c) && CS2SX_CharLower(c) != c; }
int CS2SX_CharIsLower(uint16_t c)
{
    return CS2SX_CharIsLetter(c) && (CS2SX_CharUpper(c) != c || c == 0xDF || c == 0x138 || c == 0x149);
}
int CS2SX_CharIsDigit(uint16_t c) { return c >= '0' && c <= '9'; }
int CS2SX_CharIsWhiteSpace(uint16_t c)
{
    return c == ' ' || (c >= 0x09 && c <= 0x0D) || c == 0x85 || c == 0xA0 || c == 0x1680
        || (c >= 0x2000 && c <= 0x200A) || c == 0x2028 || c == 0x2029 || c == 0x202F || c == 0x205F || c == 0x3000;
}

// ── string functions that count characters ─────────────────────────────────

static const char* map_case(const char* s, int upper)
{
    if (!s) return "";
    int ascii;
    size_t bytes = strlen(s);
    char* buf = CS2SX_StrAlloc(bytes);
    if (is_ascii(s, &ascii))
    {
        for (int i = 0; i < ascii; i++) buf[i] = (char)(upper ? toupper((unsigned char)s[i]) : tolower((unsigned char)s[i]));
        buf[ascii] = 0;
        return buf;
    }
    int out = 0, len;
    for (const unsigned char* p = (const unsigned char*)s; *p; p += len)
    {
        unsigned int cp = u8_decode(p, &len);
        if (cp == 0xFFFD && len == 1) { buf[out++] = (char)*p; continue; }   // keep bytes that are no UTF-8
        if (cp < 0x10000) cp = upper ? CS2SX_CharUpper((uint16_t)cp) : CS2SX_CharLower((uint16_t)cp);
        char enc[4];
        int n = u8_encode(cp, enc);
        if ((size_t)(out + n) > bytes)   // a mapping can need one byte more (ÿ → Ÿ); grow
        {
            char* bigger = CS2SX_StrAlloc(bytes * 2 + 4);
            memcpy(bigger, buf, out);
            buf = bigger;
            bytes = bytes * 2 + 4;
        }
        memcpy(buf + out, enc, n);
        out += n;
    }
    buf[out] = 0;
    return buf;
}

const char* String_ToUpper(const char* s) { return map_case(s, 1); }
const char* String_ToLower(const char* s) { return map_case(s, 0); }

const char* String_Substring(const char* s, int start, int length)
{
    if (!s) CS2SX_ThrowNullRef();
    int from = CS2SX_Str16Offset(s, start);
    if (start < 0 || from < 0) CS2SX_ThrowArgOutOfRange("startIndex");
    if (length < 0) CS2SX_ThrowArgOutOfRange("length");
    int to = CS2SX_Str16Offset(s + from, length);
    if (to < 0) CS2SX_ThrowArgOutOfRange("length");
    char* buf = CS2SX_StrAlloc((size_t)to);
    memcpy(buf, s + from, to);
    buf[to] = '\0';
    return buf;
}

const char* String_SubstringFrom(const char* s, int start)
{
    if (!s) CS2SX_ThrowNullRef();
    int from = CS2SX_Str16Offset(s, start);
    if (start < 0 || from < 0) CS2SX_ThrowArgOutOfRange("startIndex");
    size_t rest = strlen(s + from);
    if (rest == 0) return "";
    char* buf = CS2SX_StrAlloc(rest);
    memcpy(buf, s + from, rest + 1);
    return buf;
}

int String_IndexOf(const char* s, const char* sub)
{
    if (!s || !sub) return -1;
    const char* p = strstr(s, sub);
    return p ? CS2SX_Str16Index(s, (int)(p - s)) : -1;
}

int String_IndexOfChar(const char* s, uint16_t c)
{
    if (!s) return -1;
    CS2SX_Str16It it = { s, 0 };
    uint16_t x;
    for (int i = 0; CS2SX_Str16Next(&it, &x); i++) if (x == c) return i;
    return -1;
}

int String_LastIndexOf(const char* s, const char* sub)
{
    if (!s || !sub) return -1;
    int sublen = (int)strlen(sub);
    int slen = (int)strlen(s);
    for (int i = slen - sublen; i >= 0; i--)
        if (strncmp(s + i, sub, sublen) == 0) return CS2SX_Str16Index(s, i);
    return -1;
}

int String_LastIndexOfChar(const char* s, uint16_t c)
{
    if (!s) return -1;
    CS2SX_Str16It it = { s, 0 };
    uint16_t x;
    int last = -1;
    for (int i = 0; CS2SX_Str16Next(&it, &x); i++) if (x == c) last = i;
    return last;
}

static const char* pad(const char* s, int totalWidth, uint16_t padChar, int left)
{
    if (!s) s = "";
    if (totalWidth < 0) CS2SX_ThrowArgOutOfRange("totalWidth");
    int len = CS2SX_Str16Len(s);
    int n = totalWidth - len;
    if (n <= 0) return s;
    char one[4];
    int w = u8_encode(padChar, one);
    size_t bytes = strlen(s);
    char* buf = CS2SX_StrAlloc((size_t)n * w + bytes);
    char* p = buf;
    if (!left) { memcpy(p, s, bytes); p += bytes; }
    for (int i = 0; i < n; i++) { memcpy(p, one, w); p += w; }
    if (left) { memcpy(p, s, bytes); p += bytes; }
    *p = 0;
    return buf;
}

const char* String_PadLeft(const char* s, int totalWidth, uint16_t padChar) { return pad(s, totalWidth, padChar, 1); }
const char* String_PadRight(const char* s, int totalWidth, uint16_t padChar) { return pad(s, totalWidth, padChar, 0); }

const char* String_Insert(const char* s, int index, const char* value)
{
    if (!s || !value) CS2SX_ThrowNullRef();
    int at = CS2SX_Str16Offset(s, index);
    if (at < 0) CS2SX_ThrowArgOutOfRange("startIndex");
    size_t a = strlen(s), b = strlen(value);
    char* buf = CS2SX_StrAlloc(a + b);
    memcpy(buf, s, at);
    memcpy(buf + at, value, b);
    memcpy(buf + at + b, s + at, a - at + 1);
    return buf;
}

const char* String_Remove(const char* s, int start, int count)
{
    if (!s) CS2SX_ThrowNullRef();
    int from = CS2SX_Str16Offset(s, start);
    if (start < 0 || from < 0) CS2SX_ThrowArgOutOfRange("startIndex");
    if (count < 0) CS2SX_ThrowArgOutOfRange("count");
    int len = CS2SX_Str16Offset(s + from, count);
    if (len < 0) CS2SX_ThrowArgOutOfRange("count");
    size_t total = strlen(s);
    char* buf = CS2SX_StrAlloc(total - len);
    memcpy(buf, s, from);
    memcpy(buf + from, s + from + len, total - from - len + 1);
    return buf;
}

const char* String_RemoveFrom(const char* s, int start)
{
    if (!s) CS2SX_ThrowNullRef();
    int from = CS2SX_Str16Offset(s, start);
    if (start < 0 || from < 0) CS2SX_ThrowArgOutOfRange("startIndex");
    char* buf = CS2SX_StrAlloc((size_t)from);
    memcpy(buf, s, from);
    buf[from] = 0;
    return buf;
}
