#include <stdlib.h>
#include "Program.h"

void ExceptionsApp_Init(ExceptionsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))ExceptionsApp_OnInit;
}

int ExceptionsApp_Divide(int a, int b)
{
    if (b == 0)
    {
        CS2SX_Throw((Exception*)(CS2SX_Exc_New(&CS2SX_TI_Exception, "division by zero", NULL, NULL)));
    }
    return CS2SX_IDIV(a, b);
}

void ExceptionsApp_OnInit(ExceptionsApp* self)
{
    {
        Exception* _exf_0_pending = NULL;
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            printf("%d\n", ExceptionsApp_Divide(10, 2));
            printf("%d\n", ExceptionsApp_Divide(1, 0));
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            CS2SX_ExFrame _exf_0_c;
            CS2SX_ExPush(&_exf_0_c);
            if (setjmp(_exf_0_c.buf) == 0)
            {
                if (1)
                {
                    Exception* ex = (Exception*)_exf_0_ex;
                    char* _sb0 = _cs2sx_next_buf();
                    snprintf(_sb0, CS2SX_STRBUF_SIZE, "caught: %s", ((Exception*)(ex))->f_Message);
                    const char* _ps_1 = _sb0;
                    printf("%s\n", _ps_1 ? _ps_1 : "");
                }
                CS2SX_ExPop(&_exf_0_c);
            }
            else _exf_0_pending = _exf_0_c.ex;
        }
        {
            printf("finally\n");
        }
        if (_exf_0_pending) CS2SX_Throw(_exf_0_pending);
    }
}

void ExceptionsApp_Free(ExceptionsApp* self)
{
    if (!self) return;
    free(self);
}
