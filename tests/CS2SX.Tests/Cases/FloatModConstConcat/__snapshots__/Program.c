#include <stdlib.h>
#include "Program.h"

const char* const P_Root = "sdmc:/switch";
const char* const P_Sub = "sdmc:/switch/app";
const char* const P_Deep = "sdmc:/switch/app/cache/x\"q";
const char* const FloatModConstConcatApp_Local = "sdmc:/switch/local";
void FloatModConstConcatApp_Init(FloatModConstConcatApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))FloatModConstConcatApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))FloatModConstConcatApp_OnFrame;
}

void FloatModConstConcatApp_OnInit(FloatModConstConcatApp* self)
{
    CS2SX_TRACE("FloatModConstConcatApp.OnInit", "Program.cs");
    CS2SX_LINE(20);
    float f = 7.5f;
    CS2SX_LINE(21);
    double d = -7.5;
    CS2SX_LINE(22);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "mod %s %s %s %s", Float_ToString((fmodf(f, 2.0f))), Double_ToString((fmod(d, 2.0))), Double_ToString((fmod(5.25, 1.0))), Float_ToString((fmodf(-1.0f, 3.0f))));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(23);
    f = fmodf(f, 4.0f);
    CS2SX_LINE(24);
    d = fmod(d, 4.0);
    CS2SX_LINE(25);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "modeq %s %s", Float_ToString(f), Double_ToString(d));
    const char* _ps_1 = _sb1;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(26);
    int i = 7;
    CS2SX_LINE(27);
    i %= 3;
    CS2SX_LINE(28);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "int %d %d", (7 % 3), i);
    const char* _ps_2 = _sb2;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(30);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "const %s %s %s", "sdmc:/switch/app", FloatModConstConcatApp_Local, "sdmc:/switch/app/cache/x\"q");
    const char* _ps_3 = _sb3;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(32);
    for (int n = 0; n < 4; n++)
    {
        CS2SX_LINE(34);
        char* _sb4 = _cs2sx_next_buf();
        _sb4 = CS2SX_Format(_sb4, "x%d", n);
        const char* a = _sb4;
        CS2SX_LINE(35);
        const char* r = 0;
        CS2SX_LINE(36);
        if (n == 0)
        {
            CS2SX_LINE(36);
            r = "zero";
        }
        else
        {
            char* _sb5 = _cs2sx_next_buf();
            _sb5 = CS2SX_Format(_sb5, "x%d", 1);
            if (CS2SX_strcmp_safe(a, _sb5) == 0)
            {
                CS2SX_LINE(37);
                r = "one";
            }
            else
            {
                char* _sb6 = _cs2sx_next_buf();
                _sb6 = CS2SX_Format(_sb6, "2%d", n);
                int v = {0};
                if (CS2SX_Int_TryParse(_sb6, &v) && v == 22)
                {
                    CS2SX_LINE(38);
                    r = "two";
                }
                else
                {
                    CS2SX_LINE(39);
                    r = "other";
                }
            }
        }
        CS2SX_LINE(40);
        char* _sb7 = _cs2sx_next_buf();
        _sb7 = CS2SX_Format(_sb7, "chain %d %s", n, r);
        const char* _ps_4 = _sb7;
        printf("%s\n", _ps_4 ? _ps_4 : "");
    }
    CS2SX_LINE(42);
    printf("done\n");
}

void FloatModConstConcatApp_OnFrame(FloatModConstConcatApp* self)
{
    CS2SX_TRACE("FloatModConstConcatApp.OnFrame", "Program.cs");
}

void FloatModConstConcatApp_Free(FloatModConstConcatApp* self)
{
    if (!self) return;
    free(self);
}
