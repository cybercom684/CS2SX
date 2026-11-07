#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

void Point_Ctor_0(Point* self)
{
    Point_Ctor_2(self, 0, 0);
}

Point* Point_New_0(void)
{
    Point* self = (Point*)calloc(1, sizeof(Point));
    if (!self) return NULL;
    self->_rc = 1;
    Point_Ctor_0(self);
    return self;
}

void Point_Ctor_1(Point* self, int v)
{
    Point_Ctor_2(self, v, v);
}

Point* Point_New_1(int v)
{
    Point* self = (Point*)calloc(1, sizeof(Point));
    if (!self) return NULL;
    self->_rc = 1;
    Point_Ctor_1(self, v);
    return self;
}

void Point_Ctor_2(Point* self, int x, int y)
{
    Point_Ctor_3(self, x, y, "p");
}

Point* Point_New_2(int x, int y)
{
    Point* self = (Point*)calloc(1, sizeof(Point));
    if (!self) return NULL;
    self->_rc = 1;
    Point_Ctor_2(self, x, y);
    return self;
}

void Point_Ctor_3(Point* self, int x, int y, const char* tag)
{
    self->f_X = x;
    self->f_Y = y;
    self->f_Tag = _cs2sx_heap_strdup(tag);
}

Point* Point_New_3(int x, int y, const char* tag)
{
    Point* self = (Point*)calloc(1, sizeof(Point));
    if (!self) return NULL;
    self->_rc = 1;
    Point_Ctor_3(self, x, y, tag);
    return self;
}

void Point_Free(Point* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Point* Point_Retain(Point* self)
{
    if (self) self->_rc++;
    return self;
}

void Point3_Ctor_3(Point3* self, int x, int y, int z)
{
    Point_Ctor_2((Point*)self, x, y);
    self->f_Z = z;
}

Point3* Point3_New_3(int x, int y, int z)
{
    Point3* self = (Point3*)calloc(1, sizeof(Point3));
    if (!self) return NULL;
    self->base._rc = 1;
    Point3_Ctor_3(self, x, y, z);
    return self;
}

void Point3_Ctor_1(Point3* self, int v)
{
    Point3_Ctor_3(self, v, v, v);
}

Point3* Point3_New_1(int v)
{
    Point3* self = (Point3*)calloc(1, sizeof(Point3));
    if (!self) return NULL;
    self->base._rc = 1;
    Point3_Ctor_1(self, v);
    return self;
}

void Point3_Free(Point3* self)
{
    if (!self) return;
    Point_Free((Point*)self);
}

const CS2SX_TypeInfo CS2SX_TI_ParseError = { "ParseError", &CS2SX_TI_Exception, "Exception of type 'ParseError' was thrown." };

void ParseError_Ctor(ParseError* self, const char* message, int position)
{
    CS2SX_Exc_Init((Exception*)(self), message, NULL, NULL);
    self->f_Position = position;
}

ParseError* ParseError_New(const char* message, int position)
{
    ParseError* self = (ParseError*)calloc(1, sizeof(ParseError));
    if (!self) return NULL;
    self->base._rc = 1;
    ((Exception*)self)->_type = &CS2SX_TI_ParseError;
    ParseError_Ctor(self, message, position);
    return self;
}

void ParseError_Free(ParseError* self)
{
    if (!self) return;
    Exception_Free((Exception*)self);
}

void Counter_Ctor(Counter* self)
{
}

Counter* Counter_New(void)
{
    Counter* self = (Counter*)calloc(1, sizeof(Counter));
    if (!self) return NULL;
    self->_rc = 1;
    Counter_Ctor(self);
    return self;
}

void Counter_Add(Counter* self, int n)
{
    self->value += n;
    (self->f_Changed != NULL ? Action_int_Invoke(self->f_Changed, self->value) : (void)0);
}

void Counter_Free(Counter* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Counter* Counter_Retain(Counter* self)
{
    if (self) self->_rc++;
    return self;
}

void LanguagePhase4App_Init(LanguagePhase4App* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))LanguagePhase4App_OnInit;
}

int LanguagePhase4App_Parse(const char* s)
{
    if (strlen(CS2SX_NN(s)) == 0)
    {
        CS2SX_Throw((Exception*)(ParseError_New("empty", 0)));
    }
    if (CS2SX_strcmp_safe(s, "x") == 0)
    {
        CS2SX_Throw((Exception*)(CS2SX_Exc_New(&CS2SX_TI_ArgumentException, "bad arg", NULL, NULL)));
    }
    if (CS2SX_strcmp_safe(s, "!") == 0)
    {
        CS2SX_Throw((Exception*)(CS2SX_Exc_New(&CS2SX_TI_InvalidOperationException, "bad state", NULL, NULL)));
    }
    return strlen(CS2SX_NN(s));
}

const char* LanguagePhase4App_Classify(const char* s)
{
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            {
                char* _sb0 = _cs2sx_next_buf();
                snprintf(_sb0, CS2SX_STRBUF_SIZE, "ok %d", LanguagePhase4App_Parse(s));
                __auto_type _ret_1 = _sb0;
                CS2SX_ExPop(&_exf_0);
                return _ret_1;
            }
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (CS2SX_Exc_Is(_exf_0_ex, &CS2SX_TI_ParseError))
            {
                ParseError* pe = (ParseError*)_exf_0_ex;
                char* _sb1 = _cs2sx_next_buf();
                snprintf(_sb1, CS2SX_STRBUF_SIZE, "parse@%d %s", CS2SX_NN(pe)->f_Position, ((Exception*)(pe))->f_Message);
                return _sb1;
            }
            else if (CS2SX_Exc_Is(_exf_0_ex, &CS2SX_TI_ArgumentException))
            {
                ArgumentException* ae = (ArgumentException*)_exf_0_ex;
                char* _sb2 = _cs2sx_next_buf();
                snprintf(_sb2, CS2SX_STRBUF_SIZE, "arg %s", ((Exception*)(ae))->f_Message);
                return _sb2;
            }
            else if (1)
            {
                Exception* e = (Exception*)_exf_0_ex;
                char* _sb3 = _cs2sx_next_buf();
                snprintf(_sb3, CS2SX_STRBUF_SIZE, "other %s", ((Exception*)(e))->f_Message);
                return _sb3;
            }
        }
    }
}

const char* LanguagePhase4App_Rethrow()
{
    {
        CS2SX_ExFrame _exf_2;
        CS2SX_ExPush(&_exf_2);
        if (setjmp(_exf_2.buf) == 0)
        {
            {
                CS2SX_ExFrame _exf_3;
                CS2SX_ExPush(&_exf_3);
                if (setjmp(_exf_3.buf) == 0)
                {
                    LanguagePhase4App_Parse("x");
                    CS2SX_ExPop(&_exf_3);
                }
                else
                {
                    Exception* _exf_3_ex = _exf_3.ex;
                    if (CS2SX_Exc_Is(_exf_3_ex, &CS2SX_TI_ArgumentException))
                    {
                        CS2SX_Throw(_exf_3_ex);
                    }
                    else CS2SX_Throw(_exf_3_ex);
                }
            }
            CS2SX_ExPop(&_exf_2);
        }
        else
        {
            Exception* _exf_2_ex = _exf_2.ex;
            if (1)
            {
                Exception* e = (Exception*)_exf_2_ex;
                char* _sb4 = _cs2sx_next_buf();
                snprintf(_sb4, CS2SX_STRBUF_SIZE, "rethrown %s", ((Exception*)(e))->f_Message);
                return _sb4;
            }
        }
    }
    return "none";
}

void LanguagePhase4App_OnChangedA(LanguagePhase4App* self, int v)
{
    self->f_sum += v;
    char* _sb5 = _cs2sx_next_buf();
    snprintf(_sb5, CS2SX_STRBUF_SIZE, "A %d", v);
    const char* _ps_4 = _sb5;
    printf("%s\n", _ps_4 ? _ps_4 : "");
}

void LanguagePhase4App_OnChangedB(LanguagePhase4App* self, int v)
{
    self->f_sum += 100;
    char* _sb6 = _cs2sx_next_buf();
    snprintf(_sb6, CS2SX_STRBUF_SIZE, "B %d", v);
    const char* _ps_5 = _sb6;
    printf("%s\n", _ps_5 ? _ps_5 : "");
}

static void _lambda_0(void* _env, const char* s)
{
    (void)_env;
    char* _sb11 = _cs2sx_next_buf();
    snprintf(_sb11, CS2SX_STRBUF_SIZE, "1:%s", s);
    const char* _ps_15 = _sb11;
    printf("%s\n", _ps_15 ? _ps_15 : "");
}

static CS2SX_Delegate _dg_0 = CS2SX_DELEGATE_STATIC(_lambda_0);

static void _lambda_1(void* _env, const char* s)
{
    (void)_env;
    char* _sb12 = _cs2sx_next_buf();
    snprintf(_sb12, CS2SX_STRBUF_SIZE, "2:%s", s);
    const char* _ps_16 = _sb12;
    printf("%s\n", _ps_16 ? _ps_16 : "");
}

static CS2SX_Delegate _dg_1 = CS2SX_DELEGATE_STATIC(_lambda_1);

struct _cap_2
{
    CS2SX_Env _hdr;
    int k;
};

static int _lambda_2(void* _env)
{
    struct _cap_2* _c = (struct _cap_2*)_env;
    int k = _c->k;
    return k;
}

void LanguagePhase4App_OnInit(LanguagePhase4App* self)
{
    Point* p0 = Point_New_0();
    Point* p1 = Point_New_1(5);
    Point3* p3 = Point3_New_1(7);
    char* _sb7 = _cs2sx_next_buf();
    snprintf(_sb7, CS2SX_STRBUF_SIZE, "%d %d %s", p0->f_X, p0->f_Y, p0->f_Tag);
    const char* _ps_6 = _sb7;
    printf("%s\n", _ps_6 ? _ps_6 : "");
    char* _sb8 = _cs2sx_next_buf();
    snprintf(_sb8, CS2SX_STRBUF_SIZE, "%d %d %s", p1->f_X, p1->f_Y, p1->f_Tag);
    const char* _ps_7 = _sb8;
    printf("%s\n", _ps_7 ? _ps_7 : "");
    char* _sb9 = _cs2sx_next_buf();
    snprintf(_sb9, CS2SX_STRBUF_SIZE, "%d %d %d %s", p3->base.f_X, p3->base.f_Y, p3->f_Z, p3->base.f_Tag);
    const char* _ps_8 = _sb9;
    printf("%s\n", _ps_8 ? _ps_8 : "");
    const char* _ps_9 = LanguagePhase4App_Classify("abc");
    printf("%s\n", _ps_9 ? _ps_9 : "");
    const char* _ps_10 = LanguagePhase4App_Classify("");
    printf("%s\n", _ps_10 ? _ps_10 : "");
    const char* _ps_11 = LanguagePhase4App_Classify("x");
    printf("%s\n", _ps_11 ? _ps_11 : "");
    const char* _ps_12 = LanguagePhase4App_Classify("!");
    printf("%s\n", _ps_12 ? _ps_12 : "");
    const char* _ps_13 = LanguagePhase4App_Rethrow();
    printf("%s\n", _ps_13 ? _ps_13 : "");
    Counter* c = Counter_New();
    c->f_Changed = CS2SX_Delegate_Combine(c->f_Changed, CS2SX_Delegate_New((void*)LanguagePhase4App_OnChangedA, self, 0));
    c->f_Changed = CS2SX_Delegate_Combine(c->f_Changed, CS2SX_Delegate_New((void*)LanguagePhase4App_OnChangedB, self, 0));
    Counter_Add(c, 1);
    c->f_Changed = CS2SX_Delegate_Remove(c->f_Changed, CS2SX_Delegate_New((void*)LanguagePhase4App_OnChangedA, self, 0));
    Counter_Add(c, 2);
    char* _sb10 = _cs2sx_next_buf();
    snprintf(_sb10, CS2SX_STRBUF_SIZE, "sum %d", self->f_sum);
    const char* _ps_14 = _sb10;
    printf("%s\n", _ps_14 ? _ps_14 : "");
    Action_str_t log = (&_dg_0);
    log = CS2SX_Delegate_Combine(log, (&_dg_1));
    Action_str_Invoke(log, "hi");
    Mode m = Mode_None;
    Level l = Level_High;
    if (m == Mode_None && l != Level_None)
    {
        char* _sb13 = _cs2sx_next_buf();
        snprintf(_sb13, CS2SX_STRBUF_SIZE, "enums %d%d", ((int)Mode_Slow), ((int)Level_High));
        const char* _ps_17 = _sb13;
        printf("%s\n", _ps_17 ? _ps_17 : "");
    }
    List_Func_int_t* fs = List_Func_int_t_New();
    for (int i = 0; i < 3; i++)
    {
        int k = i * 10;
        struct _cap_2* _env_2 = CS2SX_ENV_NEW(struct _cap_2);
        _env_2->k = k;
        List_Func_int_t_Add(fs, CS2SX_Delegate_New((void*)_lambda_2, _env_2, 1));
    }
    const char* r = "";
    for (int _i_f = 0; _i_f < (int)(fs->count); _i_f++)
    {
        Func_int_t f = List_Func_int_t_Get(fs, _i_f);
        char* _sb14 = _cs2sx_next_buf();
        snprintf(_sb14, CS2SX_STRBUF_SIZE, "%d,", Func_int_Invoke(f));
        char* _sb15 = _cs2sx_next_buf();
        snprintf(_sb15, CS2SX_STRBUF_SIZE, "%s%s", r ? r : "", _sb14 ? _sb14 : "");
        r = _sb15;
    }
    char* _sb16 = _cs2sx_next_buf();
    snprintf(_sb16, CS2SX_STRBUF_SIZE, "closures %s", r);
    const char* _ps_18 = _sb16;
    printf("%s\n", _ps_18 ? _ps_18 : "");
}

void LanguagePhase4App_Free(LanguagePhase4App* self)
{
    if (!self) return;
    free(self);
}
