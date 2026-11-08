#include <stdlib.h>
#include "Program.h"

void RuntimeExceptionsApp_Init(RuntimeExceptionsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))RuntimeExceptionsApp_OnInit;
}

const char* RuntimeExceptionsApp_Try(Action_t action)
{
    CS2SX_TRACE("RuntimeExceptionsApp.Try", "Program.cs");
    CS2SX_LINE(11);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(11);
            Action_Invoke(action);
            CS2SX_LINE(11);
            {
                __auto_type _ret_1 = "none";
                CS2SX_ExPop(&_exf_0);
                return _ret_1;
            }
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* e = (Exception*)_exf_0_ex;
                CS2SX_LINE(12);
                return CS2SX_Exc_TypeName((Exception*)(e));
            }
        }
    }
}

static void _lambda_0(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    (void)_env;
    CS2SX_Int_Parse("abc");
}

static CS2SX_Delegate _dg_0 = CS2SX_DELEGATE_STATIC(_lambda_0);

static void _lambda_1(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    (void)_env;
    CS2SX_Int_Parse("12x");
}

static CS2SX_Delegate _dg_1 = CS2SX_DELEGATE_STATIC(_lambda_1);

static void _lambda_2(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    (void)_env;
    CS2SX_Int_Parse("99999999999");
}

static CS2SX_Delegate _dg_2 = CS2SX_DELEGATE_STATIC(_lambda_2);

static void _lambda_3(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    (void)_env;
    CS2SX_Byte_Parse("300");
}

static CS2SX_Delegate _dg_3 = CS2SX_DELEGATE_STATIC(_lambda_3);

static void _lambda_4(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    (void)_env;
    CS2SX_Int_Parse(NULL);
}

static CS2SX_Delegate _dg_4 = CS2SX_DELEGATE_STATIC(_lambda_4);

static void _lambda_5(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    (void)_env;
    CS2SX_Bool_Parse("1");
}

static CS2SX_Delegate _dg_5 = CS2SX_DELEGATE_STATIC(_lambda_5);

struct _cap_6
{
    CS2SX_Env _hdr;
    List_int* list;
};

static void _lambda_6(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    struct _cap_6* _c = (struct _cap_6*)_env;
    List_int* list = _c->list;
    CS2SX_LINE(31);
    int x = List_int_Get(list, 5);
}

static void _envdtor_6(void* _e)
{
    struct _cap_6* _c = (struct _cap_6*)_e;
    CS2SX_ReleaseLater(_c->list, (CS2SX_FreeFn)List_int_Free);
}

struct _cap_7
{
    CS2SX_Env _hdr;
    List_int* list;
};

static void _lambda_7(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    struct _cap_7* _c = (struct _cap_7*)_env;
    List_int* list = _c->list;
    List_int_Remove(list, -1);
}

static void _envdtor_7(void* _e)
{
    struct _cap_7* _c = (struct _cap_7*)_e;
    CS2SX_ReleaseLater(_c->list, (CS2SX_FreeFn)List_int_Free);
}

struct _cap_8
{
    CS2SX_Env _hdr;
    List_int* list;
};

static void _lambda_8(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    struct _cap_8* _c = (struct _cap_8*)_env;
    List_int* list = _c->list;
    List_int_Insert(list, 9, 0);
}

static void _envdtor_8(void* _e)
{
    struct _cap_8* _c = (struct _cap_8*)_e;
    CS2SX_ReleaseLater(_c->list, (CS2SX_FreeFn)List_int_Free);
}

struct _cap_9
{
    CS2SX_Env _hdr;
    Dict_str_int* dict;
};

static void _lambda_9(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    struct _cap_9* _c = (struct _cap_9*)_env;
    Dict_str_int* dict = _c->dict;
    CS2SX_LINE(38);
    int y = Dict_str_int_Get(dict, "zzz");
}

static void _envdtor_9(void* _e)
{
    struct _cap_9* _c = (struct _cap_9*)_e;
    CS2SX_ReleaseLater(_c->dict, (CS2SX_FreeFn)Dict_str_int_Free);
}

struct _cap_10
{
    CS2SX_Env _hdr;
    Dict_str_int* dict;
};

static void _lambda_10(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    struct _cap_10* _c = (struct _cap_10*)_env;
    Dict_str_int* dict = _c->dict;
    Dict_str_int_Add(dict, "a", 5);
}

static void _envdtor_10(void* _e)
{
    struct _cap_10* _c = (struct _cap_10*)_e;
    CS2SX_ReleaseLater(_c->dict, (CS2SX_FreeFn)Dict_str_int_Free);
}

struct _cap_11
{
    CS2SX_Env _hdr;
    Stack_int* stack;
};

static void _lambda_11(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    struct _cap_11* _c = (struct _cap_11*)_env;
    Stack_int* stack = _c->stack;
    Stack_int_Pop(stack);
}

static void _envdtor_11(void* _e)
{
    struct _cap_11* _c = (struct _cap_11*)_e;
    CS2SX_ReleaseLater(_c->stack, (CS2SX_FreeFn)Stack_int_Free);
}

struct _cap_12
{
    CS2SX_Env _hdr;
    Queue_int* queue;
};

static void _lambda_12(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    struct _cap_12* _c = (struct _cap_12*)_env;
    Queue_int* queue = _c->queue;
    Queue_int_Dequeue(queue);
}

static void _envdtor_12(void* _e)
{
    struct _cap_12* _c = (struct _cap_12*)_e;
    CS2SX_ReleaseLater(_c->queue, (CS2SX_FreeFn)Queue_int_Free);
}

struct _cap_13
{
    CS2SX_Env _hdr;
    int* arr;
};

static void _lambda_13(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    struct _cap_13* _c = (struct _cap_13*)_env;
    int* arr = _c->arr;
    CS2SX_AT(arr, 3) = 1;
}

struct _cap_14
{
    CS2SX_Env _hdr;
    int* none;
};

static void _lambda_14(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    struct _cap_14* _c = (struct _cap_14*)_env;
    int* none = _c->none;
    CS2SX_AT(none, 0) = 1;
}

struct _cap_15
{
    CS2SX_Env _hdr;
    List_int* empty;
};

static void _lambda_15(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    struct _cap_15* _c = (struct _cap_15*)_env;
    List_int* empty = _c->empty;
    CS2SX_LINE(53);
    int _first_21 = 0;
    if (empty->count > 0) { _first_21 = List_int_Get(empty, 0); }
    int f = (empty->count > 0 ? _first_21 : (CS2SX_ThrowNoElements(), _first_21));
}

static void _envdtor_15(void* _e)
{
    struct _cap_15* _c = (struct _cap_15*)_e;
    CS2SX_ReleaseLater(_c->empty, (CS2SX_FreeFn)List_int_Free);
}

struct _cap_16
{
    CS2SX_Env _hdr;
    List_int* empty;
};

static void _lambda_16(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    struct _cap_16* _c = (struct _cap_16*)_env;
    List_int* empty = _c->empty;
    CS2SX_LINE(54);
    int _maxv_24 = (empty->count > 0) ? List_int_Get(empty, 0) : 0;
    for (int _i_23 = 1; _i_23 < empty->count; _i_23++)
      if (List_int_Get(empty, _i_23) > _maxv_24) _maxv_24 = List_int_Get(empty, _i_23);
    int m = (empty->count > 0 ? _maxv_24 : (CS2SX_ThrowNoElements(), _maxv_24));
}

static void _envdtor_16(void* _e)
{
    struct _cap_16* _c = (struct _cap_16*)_e;
    CS2SX_ReleaseLater(_c->empty, (CS2SX_FreeFn)List_int_Free);
}

struct _cap_17
{
    CS2SX_Env _hdr;
    int zero;
};

static void _lambda_17(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    struct _cap_17* _c = (struct _cap_17*)_env;
    int zero = _c->zero;
    CS2SX_LINE(58);
    int d = CS2SX_IDIV(10, zero);
}

static void _lambda_18(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    (void)_env;
    String_SubstringFrom("abc", 5);
}

static CS2SX_Delegate _dg_18 = CS2SX_DELEGATE_STATIC(_lambda_18);

static void _lambda_19(void* _env)
{
    CS2SX_TRACE("RuntimeExceptionsApp.<lambda>", "Program.cs");
    (void)_env;
    CS2SX_File_ReadAllText("sdmc:/cs2sx/does-not-exist.txt");
}

static CS2SX_Delegate _dg_19 = CS2SX_DELEGATE_STATIC(_lambda_19);

void RuntimeExceptionsApp_OnInit(RuntimeExceptionsApp* self)
{
    CS2SX_TRACE("RuntimeExceptionsApp.OnInit", "Program.cs");
    CS2SX_LINE(18);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "parse %d %s %s", CS2SX_Int_Parse(" 42 "), Double_ToString(CS2SX_Double_Parse("2.5")), CS2SX_BoolStr(CS2SX_Bool_Parse("True")));
    const char* _ps_2 = _sb0;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(19);
    const char* _ps_3 = RuntimeExceptionsApp_Try((&_dg_0));
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(20);
    const char* _ps_4 = RuntimeExceptionsApp_Try((&_dg_1));
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(21);
    const char* _ps_5 = RuntimeExceptionsApp_Try((&_dg_2));
    printf("%s\n", _ps_5 ? _ps_5 : "");
    CS2SX_LINE(22);
    const char* _ps_6 = RuntimeExceptionsApp_Try((&_dg_3));
    printf("%s\n", _ps_6 ? _ps_6 : "");
    CS2SX_LINE(23);
    const char* _ps_7 = RuntimeExceptionsApp_Try((&_dg_4));
    printf("%s\n", _ps_7 ? _ps_7 : "");
    CS2SX_LINE(24);
    const char* _ps_8 = RuntimeExceptionsApp_Try((&_dg_5));
    printf("%s\n", _ps_8 ? _ps_8 : "");
    CS2SX_LINE(25);
    int v = 0;
    CS2SX_LINE(26);
    int ok = CS2SX_Int_TryParse("12x", &v);
    CS2SX_LINE(27);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "try %s %d", CS2SX_BoolStr(ok), v);
    const char* _ps_9 = _sb1;
    printf("%s\n", _ps_9 ? _ps_9 : "");
    CS2SX_LINE(30);
    List_int* _list_10 = List_int_New();
    List_int_Add(_list_10, 1);
    List_int_Add(_list_10, 2);
    List_int_Add(_list_10, 3);
    List_int* list = _list_10;
    CS2SX_LINE(31);
    struct _cap_6* _env_6 = CS2SX_ENV_NEW(struct _cap_6);
    _env_6->list = List_int_Retain(list);
    _env_6->_hdr._dtor = _envdtor_6;
    const char* _ps_11 = RuntimeExceptionsApp_Try(CS2SX_Delegate_New((void*)_lambda_6, _env_6, 1));
    printf("%s\n", _ps_11 ? _ps_11 : "");
    CS2SX_LINE(32);
    struct _cap_7* _env_7 = CS2SX_ENV_NEW(struct _cap_7);
    _env_7->list = List_int_Retain(list);
    _env_7->_hdr._dtor = _envdtor_7;
    const char* _ps_12 = RuntimeExceptionsApp_Try(CS2SX_Delegate_New((void*)_lambda_7, _env_7, 1));
    printf("%s\n", _ps_12 ? _ps_12 : "");
    CS2SX_LINE(33);
    struct _cap_8* _env_8 = CS2SX_ENV_NEW(struct _cap_8);
    _env_8->list = List_int_Retain(list);
    _env_8->_hdr._dtor = _envdtor_8;
    const char* _ps_13 = RuntimeExceptionsApp_Try(CS2SX_Delegate_New((void*)_lambda_8, _env_8, 1));
    printf("%s\n", _ps_13 ? _ps_13 : "");
    CS2SX_LINE(34);
    Dict_str_int* dict = Dict_str_int_New();
    CS2SX_LINE(35);
    Dict_str_int_Set(dict, "a", 1);
    CS2SX_LINE(36);
    ({ __auto_type _cs2sx_c = dict; __auto_type _cs2sx_k = "a"; Dict_str_int_Set(_cs2sx_c, _cs2sx_k, Dict_str_int_Get(_cs2sx_c, _cs2sx_k) + (2)); });
    CS2SX_LINE(37);
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "dict %d", Dict_str_int_Get(dict, "a"));
    const char* _ps_14 = _sb2;
    printf("%s\n", _ps_14 ? _ps_14 : "");
    CS2SX_LINE(38);
    struct _cap_9* _env_9 = CS2SX_ENV_NEW(struct _cap_9);
    _env_9->dict = Dict_str_int_Retain(dict);
    _env_9->_hdr._dtor = _envdtor_9;
    const char* _ps_15 = RuntimeExceptionsApp_Try(CS2SX_Delegate_New((void*)_lambda_9, _env_9, 1));
    printf("%s\n", _ps_15 ? _ps_15 : "");
    CS2SX_LINE(39);
    struct _cap_10* _env_10 = CS2SX_ENV_NEW(struct _cap_10);
    _env_10->dict = Dict_str_int_Retain(dict);
    _env_10->_hdr._dtor = _envdtor_10;
    const char* _ps_16 = RuntimeExceptionsApp_Try(CS2SX_Delegate_New((void*)_lambda_10, _env_10, 1));
    printf("%s\n", _ps_16 ? _ps_16 : "");
    CS2SX_LINE(40);
    Stack_int* stack = Stack_int_New();
    CS2SX_LINE(41);
    struct _cap_11* _env_11 = CS2SX_ENV_NEW(struct _cap_11);
    _env_11->stack = Stack_int_Retain(stack);
    _env_11->_hdr._dtor = _envdtor_11;
    const char* _ps_17 = RuntimeExceptionsApp_Try(CS2SX_Delegate_New((void*)_lambda_11, _env_11, 1));
    printf("%s\n", _ps_17 ? _ps_17 : "");
    CS2SX_LINE(42);
    Queue_int* queue = Queue_int_New();
    CS2SX_LINE(43);
    struct _cap_12* _env_12 = CS2SX_ENV_NEW(struct _cap_12);
    _env_12->queue = Queue_int_Retain(queue);
    _env_12->_hdr._dtor = _envdtor_12;
    const char* _ps_18 = RuntimeExceptionsApp_Try(CS2SX_Delegate_New((void*)_lambda_12, _env_12, 1));
    printf("%s\n", _ps_18 ? _ps_18 : "");
    CS2SX_LINE(46);
    int* arr = (int*)CS2SX_ArrNew(3, sizeof(int));
    CS2SX_LINE(47);
    struct _cap_13* _env_13 = CS2SX_ENV_NEW(struct _cap_13);
    _env_13->arr = arr;
    const char* _ps_19 = RuntimeExceptionsApp_Try(CS2SX_Delegate_New((void*)_lambda_13, _env_13, 1));
    printf("%s\n", _ps_19 ? _ps_19 : "");
    CS2SX_LINE(48);
    int* none = NULL;
    CS2SX_LINE(49);
    struct _cap_14* _env_14 = CS2SX_ENV_NEW(struct _cap_14);
    _env_14->none = none;
    const char* _ps_20 = RuntimeExceptionsApp_Try(CS2SX_Delegate_New((void*)_lambda_14, _env_14, 1));
    printf("%s\n", _ps_20 ? _ps_20 : "");
    CS2SX_LINE(52);
    List_int* empty = List_int_New();
    CS2SX_LINE(53);
    struct _cap_15* _env_15 = CS2SX_ENV_NEW(struct _cap_15);
    _env_15->empty = List_int_Retain(empty);
    _env_15->_hdr._dtor = _envdtor_15;
    const char* _ps_22 = RuntimeExceptionsApp_Try(CS2SX_Delegate_New((void*)_lambda_15, _env_15, 1));
    printf("%s\n", _ps_22 ? _ps_22 : "");
    CS2SX_LINE(54);
    struct _cap_16* _env_16 = CS2SX_ENV_NEW(struct _cap_16);
    _env_16->empty = List_int_Retain(empty);
    _env_16->_hdr._dtor = _envdtor_16;
    const char* _ps_25 = RuntimeExceptionsApp_Try(CS2SX_Delegate_New((void*)_lambda_16, _env_16, 1));
    printf("%s\n", _ps_25 ? _ps_25 : "");
    CS2SX_LINE(57);
    int zero = 0;
    CS2SX_LINE(58);
    struct _cap_17* _env_17 = CS2SX_ENV_NEW(struct _cap_17);
    _env_17->zero = zero;
    const char* _ps_26 = RuntimeExceptionsApp_Try(CS2SX_Delegate_New((void*)_lambda_17, _env_17, 1));
    printf("%s\n", _ps_26 ? _ps_26 : "");
    CS2SX_LINE(59);
    const char* _ps_27 = RuntimeExceptionsApp_Try((&_dg_18));
    printf("%s\n", _ps_27 ? _ps_27 : "");
    CS2SX_LINE(60);
    const char* _ps_28 = RuntimeExceptionsApp_Try((&_dg_19));
    printf("%s\n", _ps_28 ? _ps_28 : "");
    CS2SX_LINE(62);
    printf("fmt %s %s %s %s %s %s\n", Double_ToString(0.1), Double_ToString(1e21), Float_ToString(1.5f), CS2SX_BoolStr(1), Double_ToString(1000000.0), Double_ToString(0.00001));
    CS2SX_LINE(63);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "%s-%s-%s", "b", "a", "b");
    char* _sb4 = _cs2sx_next_buf();
    snprintf(_sb4, CS2SX_STRBUF_SIZE, "%s %s", _sb3, Double_ToString(2.25));
    const char* _ps_29 = _sb4;
    printf("%s\n", _ps_29 ? _ps_29 : "");
    CS2SX_LINE(64);
    printf("%.2f %s\n", 3.14159, CS2SX_BoolStr(0));
    CS2SX_LINE(65);
    printf("done\n");
}

void RuntimeExceptionsApp_Free(RuntimeExceptionsApp* self)
{
    if (!self) return;
    free(self);
}
