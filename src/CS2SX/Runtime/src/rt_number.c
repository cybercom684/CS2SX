// Runtime module: numbers <-> text with C# semantics — int.Parse & co., and the
// default ToString() of double/float (shortest round-trip, like .NET Core 3.0+).
#include "cs2sx_prelude.h"
#include <ctype.h>
#include <strings.h>

enum { PARSE_OK = 0, PARSE_FORMAT = 1, PARSE_OVERFLOW = 2, PARSE_NULL = 3 };

static int is_ws(char c) { return isspace((unsigned char)c) != 0; }
static int is_digit(char c) { return c >= '0' && c <= '9'; }

// NumberStyles.Integer: [ws][sign]digits[ws]. maxNeg is the largest magnitude a
// negative value may have (0 for unsigned types — "-0" is still valid).
static int parse_integer(const char* s, unsigned long long maxPos, unsigned long long maxNeg,
                         int* neg, unsigned long long* mag)
{
    if (!s) return PARSE_NULL;
    const char* p = s;
    while (is_ws(*p)) p++;
    int n = 0;
    if (*p == '-') { n = 1; p++; }
    else if (*p == '+') p++;
    if (!is_digit(*p)) return PARSE_FORMAT;
    unsigned long long v = 0;
    int overflow = 0;
    for (; is_digit(*p); p++)
    {
        unsigned d = (unsigned)(*p - '0');
        if (v > (ULLONG_MAX - d) / 10) overflow = 1;
        else v = v * 10 + d;
    }
    while (is_ws(*p)) p++;
    if (*p) return PARSE_FORMAT;   // trailing characters win over overflow, like .NET
    if (overflow || (n ? v > maxNeg : v > maxPos)) return PARSE_OVERFLOW;
    *neg = n;
    *mag = v;
    return PARSE_OK;
}

#define CS2SX_INT_PARSE(Name, T, NetName, MaxPos, MaxNeg)                               \
    static int parse_##Name(const char* s, T* out)                                      \
    {                                                                                   \
        int neg; unsigned long long mag;                                                \
        int r = parse_integer(s, (MaxPos), (MaxNeg), &neg, &mag);                       \
        if (r == PARSE_OK) *out = (T)(neg ? 0ULL - mag : mag);                          \
        return r;                                                                       \
    }                                                                                   \
    T CS2SX_##Name##_Parse(const char* s)                                               \
    {                                                                                   \
        T v = 0;                                                                        \
        switch (parse_##Name(s, &v))                                                    \
        {                                                                               \
            case PARSE_FORMAT:   CS2SX_ThrowFormat(s);                                  \
            case PARSE_OVERFLOW: CS2SX_ThrowOverflow(NetName);                          \
            case PARSE_NULL:     CS2SX_ThrowArgNull("s");                               \
        }                                                                               \
        return v;                                                                       \
    }                                                                                   \
    int CS2SX_##Name##_TryParse(const char* s, T* out_val)                              \
    {                                                                                   \
        T v = 0;                                                                        \
        int ok = parse_##Name(s, &v) == PARSE_OK;                                       \
        if (out_val) *out_val = ok ? v : (T)0;                                          \
        return ok;                                                                      \
    }

CS2SX_INT_PARSE(Int,    int,                "Int32",  INT_MAX,    (unsigned long long)INT_MAX + 1)
CS2SX_INT_PARSE(UInt,   unsigned int,       "UInt32", UINT_MAX,   0)
CS2SX_INT_PARSE(Long,   long long,          "Int64",  LLONG_MAX,  (unsigned long long)LLONG_MAX + 1)
CS2SX_INT_PARSE(ULong,  unsigned long long, "UInt64", ULLONG_MAX, 0)
CS2SX_INT_PARSE(Short,  short,              "Int16",  SHRT_MAX,   (unsigned long long)SHRT_MAX + 1)
CS2SX_INT_PARSE(UShort, unsigned short,     "UInt16", USHRT_MAX,  0)
CS2SX_INT_PARSE(Byte,   unsigned char,      "Byte",   UCHAR_MAX,  0)
CS2SX_INT_PARSE(SByte,  signed char,        "SByte",  SCHAR_MAX,  (unsigned long long)SCHAR_MAX + 1)

// NumberStyles.Float | AllowThousands (invariant culture): [ws][sign]digits[,digits][.digits]
// [e[sign]digits][ws], or NaN / Infinity. Writes a strtod-ready copy (no ',') to buf.
static int normalize_real(const char* s, char* buf, size_t cap, int* special, int* negative)
{
    if (!s) return PARSE_NULL;
    const char* p = s;
    while (is_ws(*p)) p++;
    const char* end = p + strlen(p);
    while (end > p && is_ws(end[-1])) end--;
    if (end == p) return PARSE_FORMAT;

    const char* q = p;
    *negative = 0;
    if (*q == '-' || *q == '+') { *negative = *q == '-'; q++; }
    size_t rest = (size_t)(end - q);
    *special = 0;
    if (rest == 3 && strncasecmp(q, "NaN", 3) == 0) { *special = 1; return PARSE_OK; }
    if (rest == 8 && strncasecmp(q, "Infinity", 8) == 0) { *special = 2; return PARSE_OK; }

    size_t n = 0;
    int digits = 0;
    if ((size_t)(end - p) >= cap) return PARSE_FORMAT;
    if (*p == '-' || *p == '+') buf[n++] = *p;
    for (; q < end && (is_digit(*q) || (*q == ',' && digits > 0)); q++)
        if (*q != ',') { buf[n++] = *q; digits++; }
    if (q < end && *q == '.')
    {
        buf[n++] = *q++;
        for (; q < end && is_digit(*q); q++) { buf[n++] = *q; digits++; }
    }
    if (!digits) return PARSE_FORMAT;
    if (q < end && (*q == 'e' || *q == 'E'))
    {
        buf[n++] = *q++;
        if (q < end && (*q == '+' || *q == '-')) buf[n++] = *q++;
        if (q >= end || !is_digit(*q)) return PARSE_FORMAT;
        for (; q < end && is_digit(*q); q++) buf[n++] = *q;
    }
    if (q != end) return PARSE_FORMAT;
    buf[n] = '\0';
    return PARSE_OK;
}

#define CS2SX_REAL_PARSE(Name, T, STRTO, NANV, INFV)                                    \
    static int parse_##Name(const char* s, T* out)                                      \
    {                                                                                   \
        char buf[128]; int special, neg;                                                \
        int r = normalize_real(s, buf, sizeof buf, &special, &neg);                     \
        if (r != PARSE_OK) return r;                                                    \
        if (special == 1) *out = NANV;                                                  \
        else if (special == 2) *out = neg ? -INFV : INFV;                               \
        else *out = STRTO(buf, NULL);   /* out of range → ±HUGE_VAL = ±Infinity */      \
        return PARSE_OK;                                                                \
    }                                                                                   \
    T CS2SX_##Name##_Parse(const char* s)                                               \
    {                                                                                   \
        T v = 0;                                                                        \
        switch (parse_##Name(s, &v))                                                    \
        {                                                                               \
            case PARSE_FORMAT: CS2SX_ThrowFormat(s);                                    \
            case PARSE_NULL:   CS2SX_ThrowArgNull("s");                                 \
        }                                                                               \
        return v;                                                                       \
    }                                                                                   \
    int CS2SX_##Name##_TryParse(const char* s, T* out_val)                              \
    {                                                                                   \
        T v = 0;                                                                        \
        int ok = parse_##Name(s, &v) == PARSE_OK;                                       \
        if (out_val) *out_val = ok ? v : (T)0;                                          \
        return ok;                                                                      \
    }

CS2SX_REAL_PARSE(Float,  float,  strtof, NAN, INFINITY)
CS2SX_REAL_PARSE(Double, double, strtod, (double)NAN, (double)INFINITY)

// ── double/float → text ──────────────────────────────────────────────────────
// .NET Core 3.0+ prints the shortest digits that round-trip, in fixed notation for
// decimal exponents -4 … 14 (0.0001, 123456789012345) and as d.dddE±XX otherwise
// (1E-05, 1E+15), invariant culture.
static const char* real_to_string(double v, int maxDigits, int isFloat)
{
    if (isnan(v)) return "NaN";
    if (isinf(v)) return v > 0 ? "Infinity" : "-Infinity";
    if (v == 0) return signbit(v) ? "-0" : "0";

    char e[48];
    int p = 1;
    for (; p <= maxDigits; p++)
    {
        snprintf(e, sizeof e, "%.*e", p - 1, v);
        if (isFloat ? strtof(e, NULL) == (float)v : strtod(e, NULL) == v) break;
    }
    if (p > maxDigits) snprintf(e, sizeof e, "%.*e", maxDigits - 1, v);

    // e = [-]d[.ddd]e±XX → sign, digit string, decimal exponent
    const char* q = e;
    int neg = 0;
    if (*q == '-') { neg = 1; q++; }
    char digits[24];
    int nd = 0;
    for (; *q && *q != 'e'; q++)
        if (*q != '.' && nd < 23) digits[nd++] = *q;
    while (nd > 1 && digits[nd - 1] == '0') nd--;   // trailing zeros carry no information
    digits[nd] = '\0';
    int exp = atoi(q + 1);

    char* buf = _cs2sx_next_buf();
    char* o = buf;
    if (neg) *o++ = '-';
    if (exp >= -4 && exp < 15)
    {
        if (exp < 0)
        {
            *o++ = '0'; *o++ = '.';
            for (int i = -1; i > exp; i--) *o++ = '0';
            for (int i = 0; i < nd; i++) *o++ = digits[i];
        }
        else
        {
            for (int i = 0; i <= exp; i++) *o++ = i < nd ? digits[i] : '0';
            if (nd > exp + 1)
            {
                *o++ = '.';
                for (int i = exp + 1; i < nd; i++) *o++ = digits[i];
            }
        }
        *o = '\0';
    }
    else
    {
        *o++ = digits[0];
        if (nd > 1) { *o++ = '.'; for (int i = 1; i < nd; i++) *o++ = digits[i]; }
        snprintf(o, 16, "E%c%02d", exp < 0 ? '-' : '+', exp < 0 ? -exp : exp);
    }
    return buf;
}

const char* Double_ToString(double val) { return real_to_string(val, 17, 0); }
const char* Float_ToString(float val)   { return real_to_string(val, 9, 1); }

// bool.Parse: "True"/"False" in any case, surrounding whitespace allowed — not "1"/"0".
static int parse_Bool(const char* s, int* out)
{
    if (!s) return PARSE_NULL;
    const char* p = s;
    while (is_ws(*p)) p++;
    size_t n = strlen(p);
    while (n > 0 && is_ws(p[n - 1])) n--;
    if (n == 4 && strncasecmp(p, "true", 4) == 0)  { *out = 1; return PARSE_OK; }
    if (n == 5 && strncasecmp(p, "false", 5) == 0) { *out = 0; return PARSE_OK; }
    return PARSE_FORMAT;
}

int CS2SX_Bool_Parse(const char* s)
{
    int v = 0;
    switch (parse_Bool(s, &v))
    {
        case PARSE_FORMAT: CS2SX_ThrowNew(&CS2SX_TI_FormatException,
                               "String was not recognized as a valid Boolean.");
        case PARSE_NULL:   CS2SX_ThrowArgNull("value");
    }
    return v;
}

int CS2SX_Bool_TryParse(const char* s, int* out_val)
{
    int v = 0;
    int ok = parse_Bool(s, &v) == PARSE_OK;
    if (out_val) *out_val = ok ? v : 0;
    return ok;
}

// ── number.ToString(format) / $"{x:F2}" / string.Format("{0:N0}") ─────────────
// .NET's standard numeric formats with the invariant culture ('.' decimal point,
// ',' group separator): F, N, D, X/x, E/e, P, C, R, G, and simple custom formats
// such as "0.00", "0.##", "#,##0", "000". The result lives until the next frame.

static char* fmt_out(const char* s)
{
    size_t n = strlen(s);
    char* buf = CS2SX_StrAlloc(n);
    memcpy(buf, s, n + 1);
    return buf;
}

/// digits (no sign) with ',' every three places
static void group_digits(const char* digits, char* out)
{
    int n = (int)strlen(digits), o = 0;
    for (int i = 0; i < n; i++)
    {
        out[o++] = digits[i];
        int left = n - 1 - i;
        if (left > 0 && left % 3 == 0) out[o++] = ',';
    }
    out[o] = 0;
}

/// v with `decimals` places, optionally grouped; minInt: at least that many integer digits
static void fixed_text(double v, int decimals, int grouped, int minInt, char* out)
{
    char tmp[400];
    int neg = v < 0;
    snprintf(tmp, sizeof tmp, "%.*f", decimals, neg ? -v : v);
    char* dot = strchr(tmp, '.');
    char fp[120] = "";
    if (dot) { *dot = 0; snprintf(fp, sizeof fp, "%s", dot + 1); }
    char padded[400];
    int len = (int)strlen(tmp);
    if (len < minInt) { int z = minInt - len; memset(padded, '0', z); memcpy(padded + z, tmp, len + 1); }
    else memcpy(padded, tmp, len + 1);
    char g[540];
    if (grouped) group_digits(padded, g); else snprintf(g, sizeof g, "%s", padded);
    int isZero = 1;
    for (const char* p = padded; *p; p++) if (*p != '0') isZero = 0;
    for (const char* p = fp; *p; p++) if (*p != '0') isZero = 0;
    sprintf(out, "%s%s%s%s", neg && !isZero ? "-" : "", g, decimals > 0 ? "." : "", decimals > 0 ? fp : "");
}

static int parse_precision(const char* fmt, int def)
{
    if (!fmt[1]) return def;
    int p = atoi(fmt + 1);
    return p < 0 ? def : p > 99 ? 99 : p;
}

/// Custom format "0.00", "#,##0.##", "000": integer zeros, grouping, decimals.
static const char* custom_format(double v, const char* fmt)
{
    const char* dot = strchr(fmt, '.');
    int minInt = 0, grouped = 0, minDec = 0, maxDec = 0;
    for (const char* p = fmt; *p && p != dot; p++)
    {
        if (*p == '0') minInt++;
        if (*p == ',') grouped = 1;
    }
    if (dot)
        for (const char* p = dot + 1; *p; p++)
        {
            if (*p == '0') { minDec++; maxDec++; }
            else if (*p == '#') maxDec++;
        }
    char out[640];
    fixed_text(v, maxDec, grouped, minInt, out);
    char* d = strchr(out, '.');   // drop optional (#) trailing zeros
    if (d)
    {
        int have = (int)strlen(d + 1);
        while (have > minDec && d[have] == '0') d[have--] = 0;
        if (have == 0) *d = 0;
    }
    return fmt_out(out);
}

/// E format: d.dddE+ddd (at least three exponent digits, like .NET)
static const char* exp_format(double v, int prec, char e)
{
    char tmp[64];
    snprintf(tmp, sizeof tmp, "%.*e", prec, v);
    char* ep = strchr(tmp, 'e');
    if (!ep) return fmt_out(tmp);
    char sign = ep[1];
    int ex = atoi(ep + 2);
    *ep = 0;
    char out[96];
    snprintf(out, sizeof out, "%s%c%c%03d", tmp, e, sign == '-' ? '-' : '+', ex);
    return fmt_out(out);
}

const char* CS2SX_FmtD(double v, const char* fmt)
{
    if (!fmt || !*fmt) return Double_ToString(v);
    char kind = fmt[0];
    int standard = isalpha((unsigned char)kind) && (!fmt[1] || isdigit((unsigned char)fmt[1]));
    if (!standard) return custom_format(v, fmt);
    if (isnan(v)) return "NaN";
    if (isinf(v)) return v > 0 ? "Infinity" : "-Infinity";
    char out[640];
    switch (kind)
    {
        case 'F': case 'f': fixed_text(v, parse_precision(fmt, 2), 0, 1, out); return fmt_out(out);
        case 'N': case 'n': fixed_text(v, parse_precision(fmt, 2), 1, 1, out); return fmt_out(out);
        case 'P': case 'p': fixed_text(v * 100.0, parse_precision(fmt, 2), 1, 1, out); strcat(out, " %"); return fmt_out(out);
        case 'C': case 'c':
        {
            char num[640];
            fixed_text(v < 0 ? -v : v, parse_precision(fmt, 2), 1, 1, num);
            snprintf(out, sizeof out, "%s\xC2\xA4%s", v < 0 ? "-" : "", num);   /* the invariant currency sign */
            return fmt_out(out);
        }
        case 'E': case 'e': return exp_format(v, parse_precision(fmt, 6), kind);
        case 'D': case 'd': case 'X': case 'x': return CS2SX_FmtI((long long)v, 64, fmt);
        case 'G': case 'g':
            if (!fmt[1]) return Double_ToString(v);
            snprintf(out, sizeof out, "%.*G", parse_precision(fmt, 15), v);
            return fmt_out(out);
        default: return Double_ToString(v);
    }
}

const char* CS2SX_FmtI(long long v, int bits, const char* fmt)
{
    if (!fmt || !*fmt) return Long_ToString(v);
    char kind = fmt[0];
    char out[160];
    switch (kind)
    {
        case 'D': case 'd':
        {
            int prec = parse_precision(fmt, 0);
            unsigned long long mag = v < 0 ? 0ULL - (unsigned long long)v : (unsigned long long)v;
            snprintf(out, sizeof out, "%s%0*llu", v < 0 ? "-" : "", prec, mag);
            return fmt_out(out);
        }
        case 'X': case 'x':
        {
            unsigned long long u = (unsigned long long)v;
            if (bits < 64) u &= (1ULL << bits) - 1;   // negative values: two's complement of the type
            snprintf(out, sizeof out, kind == 'X' ? "%0*llX" : "%0*llx", parse_precision(fmt, 0), u);
            return fmt_out(out);
        }
        case 'G': case 'g': case 'R': case 'r':
            if (!fmt[1]) return Long_ToString(v);
            break;
    }
    return CS2SX_FmtD((double)v, fmt);
}

const char* CS2SX_FmtU(unsigned long long v, int bits, const char* fmt)
{
    if (!fmt || !*fmt) return ULong_ToString(v);
    char kind = fmt[0];
    char out[160];
    if (kind == 'D' || kind == 'd') { snprintf(out, sizeof out, "%0*llu", parse_precision(fmt, 0), v); return fmt_out(out); }
    if (kind == 'X' || kind == 'x')
    {
        if (bits < 64) v &= (1ULL << bits) - 1;
        snprintf(out, sizeof out, kind == 'X' ? "%0*llX" : "%0*llx", parse_precision(fmt, 0), v);
        return fmt_out(out);
    }
    if ((kind == 'G' || kind == 'g' || kind == 'R' || kind == 'r') && !fmt[1]) return ULong_ToString(v);
    return CS2SX_FmtD((double)v, fmt);
}

const char* CS2SX_ToBase(long long v, int bits, int toBase)
{
    if (toBase == 10) return Long_ToString(v);
    if (toBase != 2 && toBase != 8 && toBase != 16)
        CS2SX_ThrowNew(&CS2SX_TI_ArgumentException, "Invalid Base.");
    unsigned long long u = (unsigned long long)v;
    if (bits < 64) u &= (1ULL << bits) - 1;
    char out[72];
    int i = (int)sizeof out - 1;
    out[i] = 0;
    do { out[--i] = "0123456789abcdef"[u % (unsigned)toBase]; u /= (unsigned)toBase; } while (u);
    return fmt_out(out + i);
}
