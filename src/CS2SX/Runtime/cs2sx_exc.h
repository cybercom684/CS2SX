#pragma once
// ============================================================================
// cs2sx_exc.h — C# exceptions: objects with runtime type info, try/catch frames.
//
//   throw new ArgumentException("x");   CS2SX_Throw(CS2SX_Exc_New(&CS2SX_TI_ArgumentException, "x", NULL, NULL));
//   try { A(); }                        { CS2SX_ExFrame f; CS2SX_ExPush(&f);
//   catch (ArgumentException e) { B; }    if (setjmp(f.buf) == 0) { A(); CS2SX_ExPop(&f); }
//                                         else if (CS2SX_Exc_Is(f.ex, &CS2SX_TI_ArgumentException)) { … B; }
//                                         else CS2SX_Throw(f.ex); }
//
// A throw unwinds to the innermost frame (a thread-local stack of jmp_bufs), so it
// crosses function calls like in C#. CS2SX_Throw pops that frame before jumping.
// Uncaught exceptions print the message and show it in the system error dialog.
//
// User exception classes embed `Exception base;` and get their own CS2SX_TypeInfo,
// whose `base` points to the parent class — CS2SX_Exc_Is walks that chain.
// ============================================================================

#include <setjmp.h>

typedef struct CS2SX_TypeInfo CS2SX_TypeInfo;
struct CS2SX_TypeInfo
{
    const char*           name;      // "ArgumentException"
    const CS2SX_TypeInfo* base;      // NULL for Exception
    const char*           defaultMessage;
};

typedef struct Exception Exception;
struct Exception
{
    int                   _rc;
    const CS2SX_TypeInfo* _type;
    const char*           f_Message;          // owned copy
    const char*           f_ParamName;        // owned copy or NULL
    Exception*            f_InnerException;   // retained or NULL
};

// The System exception hierarchy. Keep in sync with TypeRegistry.BuiltinExceptions.
#define CS2SX_EXCEPTION_TYPES(X)                                    \
    X(SystemException)             X(ApplicationException)          \
    X(ArgumentException)           X(ArgumentNullException)         \
    X(ArgumentOutOfRangeException) X(ArithmeticException)           \
    X(DivideByZeroException)       X(OverflowException)             \
    X(FormatException)             X(IndexOutOfRangeException)      \
    X(InvalidCastException)        X(InvalidOperationException)     \
    X(ObjectDisposedException)     X(NotSupportedException)         \
    X(NotImplementedException)     X(NullReferenceException)        \
    X(OutOfMemoryException)        X(TimeoutException)              \
    X(KeyNotFoundException)        X(IOException)                   \
    X(FileNotFoundException)       X(DirectoryNotFoundException)    \
    X(EndOfStreamException)

extern const CS2SX_TypeInfo CS2SX_TI_Exception;
#define CS2SX_EXC_DECLARE(N) typedef Exception N; extern const CS2SX_TypeInfo CS2SX_TI_##N;
CS2SX_EXCEPTION_TYPES(CS2SX_EXC_DECLARE)
#undef CS2SX_EXC_DECLARE

/// Allocates an exception of `type`. NULL message → the type's default message.
Exception* CS2SX_Exc_New(const CS2SX_TypeInfo* type, const char* message,
                         const char* paramName, Exception* inner);
/// Constructor body of Exception, for `: base(message)` in user exception classes.
/// Leaves `_type` alone (the allocating _New of the most derived class sets it).
void CS2SX_Exc_Init(Exception* self, const char* message, const char* paramName, Exception* inner);
void Exception_Free(Exception* self);
Exception* Exception_Retain(Exception* self);

int CS2SX_Exc_Is(const Exception* ex, const CS2SX_TypeInfo* type);
const char* CS2SX_Exc_TypeName(const Exception* ex);   // GetType().Name
const char* CS2SX_Exc_ToString(const Exception* ex);   // "System.FormatException: msg"

// ── try / catch ─────────────────────────────────────────────────────────────

typedef struct CS2SX_ExFrame CS2SX_ExFrame;
struct CS2SX_ExFrame
{
    jmp_buf        buf;
    CS2SX_ExFrame* prev;
    Exception*     ex;      // the caught exception once setjmp returns non-zero
};

extern _Thread_local CS2SX_ExFrame* g_cs2sx_ex_top;

static inline void CS2SX_ExPush(CS2SX_ExFrame* f)
{
    f->prev = g_cs2sx_ex_top;
    f->ex = NULL;
    g_cs2sx_ex_top = f;
}

/// Leaves a try block normally (also before return/break/continue out of it).
static inline void CS2SX_ExPop(CS2SX_ExFrame* f)
{
    g_cs2sx_ex_top = f->prev;
}

__attribute__((noreturn)) void CS2SX_Throw(Exception* ex);
__attribute__((noreturn)) void CS2SX_ThrowNew(const CS2SX_TypeInfo* type, const char* message);
/// The exceptions the runtime library throws itself (messages as in .NET).
__attribute__((noreturn)) void CS2SX_ThrowNullRef(void);
__attribute__((noreturn)) void CS2SX_ThrowArgNull(const char* paramName);
__attribute__((noreturn)) void CS2SX_ThrowArgOutOfRange(const char* paramName);
__attribute__((noreturn)) void CS2SX_ThrowIndexOutOfRange(void);
__attribute__((noreturn)) void CS2SX_ThrowKeyNotFound(void);
__attribute__((noreturn)) void CS2SX_ThrowDuplicateKey(void);
__attribute__((noreturn)) void CS2SX_ThrowInvalidOp(const char* message);
__attribute__((noreturn)) void CS2SX_ThrowNoElements(void);
__attribute__((noreturn)) void CS2SX_ThrowDivideByZero(void);
__attribute__((noreturn)) void CS2SX_ThrowFormat(const char* input);
__attribute__((noreturn)) void CS2SX_ThrowOverflow(const char* typeName);
__attribute__((noreturn)) void CS2SX_ThrowFileNotFound(const char* path);
__attribute__((noreturn)) void CS2SX_ThrowIO(const char* message);

/// Integer `a / b` and `a % b` as C# evaluates them: operands left to right, a zero
/// divisor throws DivideByZeroException (AArch64 would silently yield 0).
#define CS2SX_IDIV(a, b) ({ __typeof__(a) _cs2sx_x = (a); __typeof__(b) _cs2sx_y = (b); \
    if (__builtin_expect(_cs2sx_y == 0, 0)) CS2SX_ThrowDivideByZero(); _cs2sx_x / _cs2sx_y; })
#define CS2SX_IMOD(a, b) ({ __typeof__(a) _cs2sx_x = (a); __typeof__(b) _cs2sx_y = (b); \
    if (__builtin_expect(_cs2sx_y == 0, 0)) CS2SX_ThrowDivideByZero(); _cs2sx_x % _cs2sx_y; })

/// `x ?? throw …` / `c ? v : throw …`: usable as an lvalue of any type,
/// `*(T*)CS2SX_ThrowExpr(ex)`; never returns.
__attribute__((noreturn)) void* CS2SX_ThrowExpr(Exception* ex);
