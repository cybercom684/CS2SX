// ============================================================================
// CS2SX — Runtime/src/rt_datetime.c
//
// DateTime as .NET stores it: ticks (100 ns) since 0001-01-01 00:00, local time,
// proleptic Gregorian calendar. TimeSpan is ticks too (switchforms.h), so
// a - b, a + span and comparisons are plain arithmetic. Formatting follows .NET's
// invariant culture (custom formats "yyyy-MM-dd HH:mm" and the standard ones).
// ============================================================================

#include "cs2sx_prelude.h"
#include <time.h>

#define DT_MAX_TICKS 3155378975999999999LL   // DateTime.MaxValue (9999-12-31 23:59:59.9999999)

// Days since 1970-01-01 of a civil date (H. Hinnant's algorithm).
static long long days_from_civil(long long y, int m, int d)
{
    y -= m <= 2;
    long long era = (y >= 0 ? y : y - 399) / 400;
    long long yoe = y - era * 400;
    long long doy = (153 * (m + (m > 2 ? -3 : 9)) + 2) / 5 + d - 1;
    long long doe = yoe * 365 + yoe / 4 - yoe / 100 + doy;
    return era * 146097 + doe - 719468;
}

static void civil_from_days(long long z, int* y, int* m, int* d)
{
    z += 719468;
    long long era = (z >= 0 ? z : z - 146096) / 146097;
    long long doe = z - era * 146097;
    long long yoe = (doe - doe / 1460 + doe / 36524 - doe / 146096) / 365;
    long long doy = doe - (365 * yoe + yoe / 4 - yoe / 100);
    long long mp = (5 * doy + 2) / 153;
    *d = (int)(doy - (153 * mp + 2) / 5 + 1);
    *m = (int)(mp < 10 ? mp + 3 : mp - 9);
    *y = (int)(yoe + era * 400 + (*m <= 2));
}

#define DT_EPOCH_DAYS 719162LL   // 0001-01-01 → 1970-01-01

int CS2SX_DT_IsLeapYear(int y)
{
    if (y < 1 || y > 9999) CS2SX_ThrowArgOutOfRange("year");
    return (y % 4 == 0 && y % 100 != 0) || y % 400 == 0;
}

int CS2SX_DT_DaysInMonth(int y, int m)
{
    if (m < 1 || m > 12) CS2SX_ThrowArgOutOfRange("month");
    static const int days[] = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
    return m == 2 && CS2SX_DT_IsLeapYear(y) ? 29 : days[m - 1];
}

CS2SX_DateTime CS2SX_DT_New(int y, int mo, int d, int h, int mi, int s, int ms)
{
    if (y < 1 || y > 9999 || mo < 1 || mo > 12 || d < 1 || d > CS2SX_DT_DaysInMonth(y, mo)
        || h < 0 || h > 23 || mi < 0 || mi > 59 || s < 0 || s > 59 || ms < 0 || ms > 999)
        CS2SX_ThrowNew(&CS2SX_TI_ArgumentOutOfRangeException,
                       "Year, Month, and Day parameters describe an un-representable DateTime.");
    long long days = days_from_civil(y, mo, d) + DT_EPOCH_DAYS;
    return days * CS2SX_TICKS_PER_DAY + h * CS2SX_TICKS_PER_HOUR + mi * CS2SX_TICKS_PER_MIN
         + s * CS2SX_TICKS_PER_SEC + ms * CS2SX_TICKS_PER_MS;
}

static CS2SX_DateTime from_tm(const struct tm* t)
{
    return t ? CS2SX_DT_New(t->tm_year + 1900, t->tm_mon + 1, t->tm_mday, t->tm_hour, t->tm_min,
                            t->tm_sec > 59 ? 59 : t->tm_sec, 0) : 0;
}

CS2SX_DateTime CS2SX_DT_Now(void)    { time_t t = time(NULL); return from_tm(localtime(&t)); }
CS2SX_DateTime CS2SX_DT_UtcNow(void) { time_t t = time(NULL); return from_tm(gmtime(&t)); }
CS2SX_DateTime CS2SX_DT_Today(void)  { CS2SX_DateTime n = CS2SX_DT_Now(); return n - n % CS2SX_TICKS_PER_DAY; }

int CS2SX_DT_Part(CS2SX_DateTime t, int part)
{
    long long days = t / CS2SX_TICKS_PER_DAY;
    long long tod = t % CS2SX_TICKS_PER_DAY;
    int y, m, d;
    civil_from_days(days - DT_EPOCH_DAYS, &y, &m, &d);
    switch (part)
    {
        case CS2SX_DT_YEAR:   return y;
        case CS2SX_DT_MONTH:  return m;
        case CS2SX_DT_DAY:    return d;
        case CS2SX_DT_HOUR:   return (int)(tod / CS2SX_TICKS_PER_HOUR);
        case CS2SX_DT_MINUTE: return (int)(tod / CS2SX_TICKS_PER_MIN % 60);
        case CS2SX_DT_SECOND: return (int)(tod / CS2SX_TICKS_PER_SEC % 60);
        case CS2SX_DT_MS:     return (int)(tod / CS2SX_TICKS_PER_MS % 1000);
        case CS2SX_DT_DOW:    return (int)((days + 1) % 7);           // 0001-01-01 was a Monday
        case CS2SX_DT_DOY:    return (int)(days - (days_from_civil(y, 1, 1) + DT_EPOCH_DAYS)) + 1;
    }
    return 0;
}

static CS2SX_DateTime checked_ticks(long long t)
{
    if (t < 0 || t > DT_MAX_TICKS)
        CS2SX_ThrowNew(&CS2SX_TI_ArgumentOutOfRangeException,
                       "The added or subtracted value results in an un-representable DateTime.");
    return t;
}

CS2SX_DateTime CS2SX_DT_AddTicks(CS2SX_DateTime t, long long ticks) { return checked_ticks(t + ticks); }

CS2SX_DateTime CS2SX_DT_Add(CS2SX_DateTime t, double value, long long unit)
{
    // .NET rounds the added amount to whole milliseconds (AddDays(0.5) = 12 h).
    double ms = value * ((double)unit / CS2SX_TICKS_PER_MS);
    long long whole = (long long)(ms + (ms >= 0 ? 0.5 : -0.5));
    return checked_ticks(t + whole * CS2SX_TICKS_PER_MS);
}

CS2SX_DateTime CS2SX_DT_AddMonths(CS2SX_DateTime t, int months)
{
    if (months < -120000 || months > 120000) CS2SX_ThrowArgOutOfRange("months");
    int y = CS2SX_DT_Part(t, CS2SX_DT_YEAR), m = CS2SX_DT_Part(t, CS2SX_DT_MONTH), d = CS2SX_DT_Part(t, CS2SX_DT_DAY);
    long long tod = t % CS2SX_TICKS_PER_DAY;
    long long total = (long long)y * 12 + (m - 1) + months;
    long long ny = total / 12, nm = total % 12 + 1;
    if (ny < 1 || ny > 9999)
        CS2SX_ThrowNew(&CS2SX_TI_ArgumentOutOfRangeException,
                       "The added or subtracted value results in an un-representable DateTime.");
    int dim = CS2SX_DT_DaysInMonth((int)ny, (int)nm);
    if (d > dim) d = dim;
    return CS2SX_DT_New((int)ny, (int)nm, d, 0, 0, 0, 0) + tod;
}

// ── Formatting ───────────────────────────────────────────────────────────────

static const char* const s_months[] = { "January", "February", "March", "April", "May", "June", "July",
                                        "August", "September", "October", "November", "December" };
static const char* const s_days[] = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

const char* CS2SX_DayOfWeekName(int dow) { return dow >= 0 && dow < 7 ? s_days[dow] : Int_ToString(dow); }

static void put(char* out, int* n, int cap, const char* s)
{
    while (*s && *n < cap - 1) out[(*n)++] = *s++;
    out[*n] = '\0';
}

static void putnum(char* out, int* n, int cap, long long v, int width)
{
    char tmp[32];
    snprintf(tmp, sizeof tmp, "%0*lld", width, v);
    put(out, n, cap, tmp);
}

static const char* standard_format(char c)
{
    switch (c)
    {
        case 'd': return "MM/dd/yyyy";
        case 'D': return "dddd, dd MMMM yyyy";
        case 'f': return "dddd, dd MMMM yyyy HH:mm";
        case 'F': return "dddd, dd MMMM yyyy HH:mm:ss";
        case 'g': return "MM/dd/yyyy HH:mm";
        case 'G': return "MM/dd/yyyy HH:mm:ss";
        case 'm': case 'M': return "MMMM dd";
        case 'o': case 'O': return "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff";
        case 's': return "yyyy'-'MM'-'dd'T'HH':'mm':'ss";
        case 't': return "HH:mm";
        case 'T': return "HH:mm:ss";
        case 'u': return "yyyy'-'MM'-'dd HH':'mm':'ss'Z'";
        case 'y': case 'Y': return "yyyy MMMM";
    }
    return NULL;
}

const char* CS2SX_DT_ToString(CS2SX_DateTime t, const char* fmt)
{
    if (!fmt || !*fmt) fmt = "G";
    if (fmt[1] == '\0')
    {
        const char* std = standard_format(fmt[0]);
        if (!std) CS2SX_ThrowNew(&CS2SX_TI_FormatException, "Input string was not in a correct format.");
        fmt = std;
    }
    int y = CS2SX_DT_Part(t, CS2SX_DT_YEAR), mo = CS2SX_DT_Part(t, CS2SX_DT_MONTH), d = CS2SX_DT_Part(t, CS2SX_DT_DAY);
    int h = CS2SX_DT_Part(t, CS2SX_DT_HOUR), mi = CS2SX_DT_Part(t, CS2SX_DT_MINUTE), s = CS2SX_DT_Part(t, CS2SX_DT_SECOND);
    long long frac = t % CS2SX_TICKS_PER_SEC;   // 7 digits
    char* out = _cs2sx_next_buf();
    int n = 0, cap = CS2SX_STRBUF_SIZE;
    out[0] = '\0';
    for (const char* p = fmt; *p; )
    {
        char c = *p;
        if (c == '\'' || c == '"')
        {
            for (p++; *p && *p != c; p++) { char one[2] = { *p, 0 }; put(out, &n, cap, one); }
            if (*p) p++;
            continue;
        }
        if (c == '\\' && p[1]) { char one[2] = { p[1], 0 }; put(out, &n, cap, one); p += 2; continue; }
        if (c == '%' && p[1]) { p++; continue; }
        int run = 1;
        while (p[run] == c) run++;
        switch (c)
        {
            case 'y':
                if (run <= 2) putnum(out, &n, cap, y % 100, run);
                else putnum(out, &n, cap, y, run);
                break;
            case 'M':
                if (run <= 2) putnum(out, &n, cap, mo, run);
                else if (run == 3) { char a[4] = { 0 }; memcpy(a, s_months[mo - 1], 3); put(out, &n, cap, a); }
                else put(out, &n, cap, s_months[mo - 1]);
                break;
            case 'd':
                if (run <= 2) putnum(out, &n, cap, d, run);
                else
                {
                    const char* dn = s_days[CS2SX_DT_Part(t, CS2SX_DT_DOW)];
                    if (run == 3) { char a[4] = { 0 }; memcpy(a, dn, 3); put(out, &n, cap, a); }
                    else put(out, &n, cap, dn);
                }
                break;
            case 'H': putnum(out, &n, cap, h, run > 2 ? 2 : run); break;
            case 'h': putnum(out, &n, cap, h % 12 == 0 ? 12 : h % 12, run > 2 ? 2 : run); break;
            case 'm': putnum(out, &n, cap, mi, run > 2 ? 2 : run); break;
            case 's': putnum(out, &n, cap, s, run > 2 ? 2 : run); break;
            case 'f': case 'F':
            {
                int digits = run > 7 ? 7 : run;
                long long v = frac;
                for (int k = 7; k > digits; k--) v /= 10;
                char tmp[16];
                snprintf(tmp, sizeof tmp, "%0*lld", digits, v);
                if (c == 'F') { int len = (int)strlen(tmp); while (len > 0 && tmp[len - 1] == '0') tmp[--len] = '\0'; }
                put(out, &n, cap, tmp);
                break;
            }
            case 't':
                put(out, &n, cap, run == 1 ? (h < 12 ? "A" : "P") : (h < 12 ? "AM" : "PM"));
                break;
            default:
            {
                for (int k = 0; k < run; k++) { char one[2] = { c, 0 }; put(out, &n, cap, one); }
            }
        }
        p += run;
    }
    return out;
}

const char* CS2SX_TS_ToString(CS2SX_TimeSpan ts)
{
    long long t = ts.ticks;
    char* out = _cs2sx_next_buf();
    const char* sign = "";
    unsigned long long u = (unsigned long long)t;
    if (t < 0) { sign = "-"; u = 0ULL - u; }
    unsigned long long days = u / CS2SX_TICKS_PER_DAY;
    unsigned long long rest = u % CS2SX_TICKS_PER_DAY;
    int h = (int)(rest / CS2SX_TICKS_PER_HOUR), m = (int)(rest / CS2SX_TICKS_PER_MIN % 60), s = (int)(rest / CS2SX_TICKS_PER_SEC % 60);
    unsigned long long frac = rest % CS2SX_TICKS_PER_SEC;
    int n = days ? snprintf(out, CS2SX_STRBUF_SIZE, "%s%llu.%02d:%02d:%02d", sign, days, h, m, s)
                 : snprintf(out, CS2SX_STRBUF_SIZE, "%s%02d:%02d:%02d", sign, h, m, s);
    if (frac && n > 0 && n < CS2SX_STRBUF_SIZE) snprintf(out + n, CS2SX_STRBUF_SIZE - n, ".%07llu", frac);
    return out;
}
