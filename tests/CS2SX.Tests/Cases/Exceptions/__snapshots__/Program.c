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
        fprintf(stderr, "Unhandled exception: %s\n", "division by zero");
        abort();
    }
    return a / b;
}

void ExceptionsApp_OnInit(ExceptionsApp* self)
{
    char _ex_msg[512] = "unknown error";
    jmp_buf _ex_buf__tmp_0;
    int _ex_val = setjmp(_ex_buf__tmp_0);
    if (_ex_val == 0)
    {
        printf("%d\n", ExceptionsApp_Divide(10, 2));
        printf("%d\n", ExceptionsApp_Divide(1, 0));
    }
    else
    {
        /* catch (Exception) */
        {
            char* _sb0 = _cs2sx_next_buf();
            snprintf(_sb0, CS2SX_STRBUF_SIZE, "caught: %s", _ex_msg);
            const char* _ps_1 = _sb0;
            printf("%s\n", _ps_1 ? _ps_1 : "");
        }
    }
    /* finally — always runs after try/catch */
    {
        printf("finally\n");
    }
}

void ExceptionsApp_Free(ExceptionsApp* self)
{
    if (!self) return;
    free(self);
}
