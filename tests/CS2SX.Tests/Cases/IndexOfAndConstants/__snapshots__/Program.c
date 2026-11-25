#include <stdlib.h>
#include "Program.h"

void IndexOfAndConstantsApp_Init(IndexOfAndConstantsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))IndexOfAndConstantsApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))IndexOfAndConstantsApp_OnFrame;
}

void IndexOfAndConstantsApp_OnInit(IndexOfAndConstantsApp* self)
{
    CS2SX_TRACE("IndexOfAndConstantsApp.OnInit", "Program.cs");
    CS2SX_LINE(23);
    const char* s = "a.b.c.d";
    CS2SX_LINE(24);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "idx %d %d %d %d %d", String_IndexOfCharAt(s, '.', 2, -1), String_IndexOfAt(s, ".", 4, -1), String_IndexOfCharAt(s, 'x', 1, -1), String_IndexOfCharAt(s, '.', 2, 2), String_IndexOfCharAt(s, '.', 4, 1));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(26);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "last %d %d %d %d", String_LastIndexOfCharAt(s, '.', 4, -1), String_LastIndexOfAt(s, ".", 2, -1), String_LastIndexOfCharAt(s, '.', 6, 2), String_LastIndexOfChar(s, '.'));
    const char* _ps_1 = _sb1;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(28);
    u16 dot = '.';
    CS2SX_LINE(29);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "var %d %d", String_IndexOfChar(s, dot), String_IndexOfCharAt(s, dot, 4, -1));
    const char* _ps_2 = _sb2;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(30);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "case %d %d %d", String_IndexOf(String_ToLower("Hello World"), String_ToLower("WORLD")), String_IndexOfAt("Hello World", "o", 5, -1), String_IndexOf("Hello World", "WORLD"));
    const char* _ps_3 = _sb3;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(32);
    const char* u = "äb.c.";
    CS2SX_LINE(33);
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "utf %d %d %d", String_IndexOfCharAt(u, '.', 3, -1), String_IndexOfAt(u, ".", 3, -1), String_LastIndexOfCharAt(u, '.', 3, -1));
    const char* _ps_4 = _sb4;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(34);
    {
        CS2SX_ExFrame _exf_5;
        CS2SX_ExPush(&_exf_5);
        if (setjmp(_exf_5.buf) == 0)
        {
            CS2SX_LINE(34);
            char* _sb5 = _cs2sx_next_buf();
            _sb5 = CS2SX_Format(_sb5, "no %d", String_IndexOfCharAt(s, '.', 99, -1));
            const char* _ps_6 = _sb5;
            printf("%s\n", _ps_6 ? _ps_6 : "");
            CS2SX_ExPop(&_exf_5);
        }
        else
        {
            Exception* _exf_5_ex = _exf_5.ex;
            if (CS2SX_Exc_Is(_exf_5_ex, &CS2SX_TI_ArgumentOutOfRangeException))
            {
                CS2SX_LINE(35);
                printf("range\n");
                Exception_Free(_exf_5_ex);
            }
            else CS2SX_Throw(_exf_5_ex);
        }
    }
    CS2SX_LINE(37);
    char* _sb6 = _cs2sx_next_buf();
    _sb6 = CS2SX_Format(_sb6, "const %s %s %s %s %s %s %s %s", Float_ToString(IndexOfAndConstantsApp_R * 2), Double_ToString(IndexOfAndConstantsApp_D / 2), Float_ToString(IndexOfAndConstantsApp_F2), Float_ToString(IndexOfAndConstantsApp_S), Float_ToString(6.0f / 4), Double_ToString(3.0 / 2), Double_ToString(1E+30), Double_ToString(((double)NAN)));
    const char* _ps_7 = _sb6;
    printf("%s\n", _ps_7 ? _ps_7 : "");
    CS2SX_LINE(39);
    printf("done\n");
}

void IndexOfAndConstantsApp_OnFrame(IndexOfAndConstantsApp* self)
{
    CS2SX_TRACE("IndexOfAndConstantsApp.OnFrame", "Program.cs");
}

void IndexOfAndConstantsApp_Free(IndexOfAndConstantsApp* self)
{
    if (!self) return;
    free(self);
}
