// Runtime module for cs2sx_exc.h — exception objects, type info, throw.
#include "cs2sx_prelude.h"

_Thread_local CS2SX_ExFrame* g_cs2sx_ex_top = NULL;

// Bottom of the trace stack: CS2SX_LINE always has a frame to write to.
static CS2SX_TraceFrame s_cs2sx_trace_root = { NULL, NULL, NULL, 0 };   // shared: only a sentinel
_Thread_local CS2SX_TraceFrame* g_cs2sx_trace = &s_cs2sx_trace_root;

/// "   at Game.Update (Game.cs:42)\n…" for the current trace (innermost first).
static char* trace_capture(void)
{
    char buf[1536];
    size_t n = 0;
    int frames = 0;
    for (CS2SX_TraceFrame* f = g_cs2sx_trace; f && f->method && n < sizeof buf - 1; f = f->prev)
    {
        if (++frames > 24) { n += snprintf(buf + n, sizeof buf - n, "   ...\n"); break; }
        n += snprintf(buf + n, sizeof buf - n, "   at %s (%s:%d)\n", f->method, f->file ? f->file : "?", f->line);
        if (n >= sizeof buf) n = sizeof buf - 1;
    }
    buf[n] = 0;
    return n > 0 ? strdup(buf) : NULL;
}

const CS2SX_TypeInfo CS2SX_TI_Exception =
    { "Exception", NULL, "Exception of type 'System.Exception' was thrown." };

#define CS2SX_TI(N, B, M) const CS2SX_TypeInfo CS2SX_TI_##N = { #N, &CS2SX_TI_##B, M };
CS2SX_TI(SystemException,             Exception,           "System error.")
CS2SX_TI(ApplicationException,        Exception,           "Error in the application.")
CS2SX_TI(ArgumentException,           SystemException,     "Value does not fall within the expected range.")
CS2SX_TI(ArgumentNullException,       ArgumentException,   "Value cannot be null.")
CS2SX_TI(ArgumentOutOfRangeException, ArgumentException,   "Specified argument was out of the range of valid values.")
CS2SX_TI(ArithmeticException,         SystemException,     "Overflow or underflow in the arithmetic operation.")
CS2SX_TI(DivideByZeroException,       ArithmeticException, "Attempted to divide by zero.")
CS2SX_TI(OverflowException,           ArithmeticException, "Arithmetic operation resulted in an overflow.")
CS2SX_TI(FormatException,             SystemException,     "One of the identified items was in an invalid format.")
CS2SX_TI(IndexOutOfRangeException,    SystemException,     "Index was outside the bounds of the array.")
CS2SX_TI(InvalidCastException,        SystemException,     "Specified cast is not valid.")
CS2SX_TI(InvalidOperationException,   SystemException,     "Operation is not valid due to the current state of the object.")
CS2SX_TI(ObjectDisposedException,     InvalidOperationException, "Cannot access a disposed object.")
CS2SX_TI(NotSupportedException,       SystemException,     "Specified method is not supported.")
CS2SX_TI(NotImplementedException,     SystemException,     "The method or operation is not implemented.")
CS2SX_TI(NullReferenceException,      SystemException,     "Object reference not set to an instance of an object.")
CS2SX_TI(OutOfMemoryException,        SystemException,     "Insufficient memory to continue the execution of the program.")
CS2SX_TI(TimeoutException,            SystemException,     "The operation has timed out.")
CS2SX_TI(KeyNotFoundException,        SystemException,     "The given key was not present in the dictionary.")
CS2SX_TI(IOException,                 SystemException,     "I/O error occurred.")
CS2SX_TI(FileNotFoundException,       IOException,         "Unable to find the specified file.")
CS2SX_TI(DirectoryNotFoundException,  IOException,         "Attempted to access a path that is not on the disk.")
CS2SX_TI(EndOfStreamException,        IOException,         "Attempted to read past the end of the stream.")
#undef CS2SX_TI

static char* exc_strdup(const char* s)
{
    if (!s) return NULL;
    size_t n = strlen(s) + 1;
    char* d = (char*)malloc(n);
    if (d) memcpy(d, s, n);
    return d;
}

void CS2SX_Exc_Init(Exception* self, const char* message, const char* paramName, Exception* inner)
{
    if (!self) return;
    // Message strings often live in rotating format buffers — always copy.
    if (!message)
        message = self->_type && self->_type->defaultMessage
            ? self->_type->defaultMessage : CS2SX_TI_Exception.defaultMessage;
    if (paramName && *paramName)
    {
        char buf[512];
        snprintf(buf, sizeof buf, "%s (Parameter '%s')", message, paramName);
        self->f_Message = exc_strdup(buf);
    }
    else
        self->f_Message = exc_strdup(message);
    self->f_ParamName = exc_strdup(paramName);
    self->f_InnerException = Exception_Retain(inner);
}

Exception* CS2SX_Exc_New(const CS2SX_TypeInfo* type, const char* message,
                         const char* paramName, Exception* inner)
{
    Exception* self = (Exception*)calloc(1, sizeof(Exception));
    if (!self) return NULL;
    self->_rc = 1;
    self->_type = type ? type : &CS2SX_TI_Exception;
    CS2SX_Exc_Init(self, message, paramName, inner);
    return self;
}

void Exception_Free(Exception* self)
{
    if (!self || --self->_rc > 0) return;
    free((void*)self->f_Message);
    free((void*)self->f_ParamName);
    free((void*)self->f_StackTrace);
    Exception_Free(self->f_InnerException);
    free(self);
}

Exception* Exception_Retain(Exception* self)
{
    if (self) self->_rc++;
    return self;
}

int CS2SX_Exc_Is(const Exception* ex, const CS2SX_TypeInfo* type)
{
    if (!ex) return 0;
    for (const CS2SX_TypeInfo* t = ex->_type; t; t = t->base)
        if (t == type) return 1;
    return 0;
}

const char* CS2SX_Exc_TypeName(const Exception* ex)
{
    return ex && ex->_type ? ex->_type->name : "Exception";
}

const char* CS2SX_Exc_ToString(const Exception* ex)
{
    static _Thread_local char buf[640];
    if (!ex) return "";
    // Built-in types live in System; user exception classes print their bare name.
    const CS2SX_TypeInfo* t = ex->_type;
    int builtin = 0;
    if (t == &CS2SX_TI_Exception) builtin = 1;
#define CS2SX_IS_BUILTIN(N) if (t == &CS2SX_TI_##N) builtin = 1;
    CS2SX_EXCEPTION_TYPES(CS2SX_IS_BUILTIN)
#undef CS2SX_IS_BUILTIN
    snprintf(buf, sizeof buf, "%s%s: %s", builtin ? "System." : "",
             CS2SX_Exc_TypeName(ex), ex->f_Message ? ex->f_Message : "");
    return buf;
}

// Uncaught: tell the developer (stderr → nxlink, debug log → emulators) and the
// user (system error dialog), then quit like an unhandled .NET exception would.
__attribute__((noreturn)) static void exc_unhandled(Exception* ex)
{
    char msg[2048];
    snprintf(msg, sizeof msg, "Unhandled exception. %s%s%s", CS2SX_Exc_ToString(ex),
             ex->f_StackTrace ? "\n" : "", ex->f_StackTrace ? ex->f_StackTrace : "");
    fprintf(stderr, "%s\n", msg);
    // One debug-log entry per line (message, then "   at …" frames).
    for (const char* line = msg; *line; )
    {
        const char* end = strchr(line, '\n');
        size_t len = end ? (size_t)(end - line) : strlen(line);
        svcOutputDebugString(line, len);
        if (!end) break;
        line = end + 1;
    }
    if (appletGetAppletType() != AppletType_None)
    {
        ErrorApplicationConfig cfg;
        if (R_SUCCEEDED(errorApplicationCreate(&cfg, "The app stopped because of an error.", msg)))
            errorApplicationShow(&cfg);
    }
    exit(1);
}

void CS2SX_Throw(Exception* ex)
{
    if (!ex) ex = CS2SX_Exc_New(&CS2SX_TI_NullReferenceException, NULL, NULL, NULL);
    if (!ex->f_StackTrace) ex->f_StackTrace = trace_capture();   // a rethrow keeps the original
    CS2SX_ExFrame* f = g_cs2sx_ex_top;
    if (!f) exc_unhandled(ex);
    g_cs2sx_ex_top = f->prev;
    g_cs2sx_trace = f->trace;   // the frames between throw and catch are gone
    f->ex = ex;
    longjmp(f->buf, 1);
}

void CS2SX_ThrowNew(const CS2SX_TypeInfo* type, const char* message)
{
    CS2SX_Throw(CS2SX_Exc_New(type, message, NULL, NULL));
}

void* CS2SX_ThrowExpr(Exception* ex)
{
    CS2SX_Throw(ex);
}

// ── Exceptions of the runtime library ───────────────────────────────────────

void CS2SX_ThrowNullRef(void)
{
    CS2SX_ThrowNew(&CS2SX_TI_NullReferenceException, NULL);
}

void CS2SX_ThrowArgNull(const char* paramName)
{
    CS2SX_Throw(CS2SX_Exc_New(&CS2SX_TI_ArgumentNullException, NULL, paramName, NULL));
}

void CS2SX_ThrowArgOutOfRange(const char* paramName)
{
    CS2SX_Throw(CS2SX_Exc_New(&CS2SX_TI_ArgumentOutOfRangeException,
        "Index was out of range. Must be non-negative and less than the size of the collection.",
        paramName, NULL));
}

void CS2SX_ThrowIndexOutOfRange(void)
{
    CS2SX_ThrowNew(&CS2SX_TI_IndexOutOfRangeException, NULL);
}

void CS2SX_ThrowKeyNotFound(void)
{
    CS2SX_ThrowNew(&CS2SX_TI_KeyNotFoundException, NULL);
}

void CS2SX_ThrowDuplicateKey(void)
{
    CS2SX_ThrowNew(&CS2SX_TI_ArgumentException, "An item with the same key has already been added.");
}

void CS2SX_ThrowInvalidOp(const char* message)
{
    CS2SX_ThrowNew(&CS2SX_TI_InvalidOperationException, message);
}

void CS2SX_ThrowNoElements(void)
{
    CS2SX_ThrowNew(&CS2SX_TI_InvalidOperationException, "Sequence contains no elements");
}

void CS2SX_ThrowDivideByZero(void)
{
    CS2SX_ThrowNew(&CS2SX_TI_DivideByZeroException, NULL);
}

void CS2SX_ThrowFormat(const char* input)
{
    char msg[256];
    snprintf(msg, sizeof msg, "The input string '%.180s' was not in a correct format.", input ? input : "");
    CS2SX_ThrowNew(&CS2SX_TI_FormatException, msg);
}

void CS2SX_ThrowOverflow(const char* typeName)
{
    char msg[128];
    snprintf(msg, sizeof msg, "Value was either too large or too small for %s %s.",
             (typeName[0] == 'I' || typeName[0] == 'U') ? "an" : "a", typeName);
    CS2SX_ThrowNew(&CS2SX_TI_OverflowException, msg);
}

void CS2SX_ThrowFileNotFound(const char* path)
{
    char msg[600];
    snprintf(msg, sizeof msg, "Could not find file '%.500s'.", path ? path : "");
    CS2SX_ThrowNew(&CS2SX_TI_FileNotFoundException, msg);
}

void CS2SX_ThrowIO(const char* message)
{
    CS2SX_ThrowNew(&CS2SX_TI_IOException, message);
}
