#include <stdlib.h>
#include "Program.h"

void UnicodeStringsApp_Init(UnicodeStringsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))UnicodeStringsApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))UnicodeStringsApp_OnFrame;
}

void UnicodeStringsApp_OnInit(UnicodeStringsApp* self)
{
    CS2SX_TRACE("UnicodeStringsApp.OnInit", "Program.cs");
    CS2SX_LINE(11);
    const char* s = "Größe";
    CS2SX_LINE(12);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "len %d %d %d", CS2SX_Str16Len(s), CS2SX_Str16Len("a😀b"), CS2SX_Str16Len("hello"));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(13);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "index %s %d %d %s", CS2SX_BoolStr((CS2SX_Str16At(s, 2) == ((u16)0x00F6))), ((int)CS2SX_Str16At(s, 3)), ((int)CS2SX_Str16At("a😀b", 1)), CS2SX_CharStr(CS2SX_Str16At(s, 4)));
    const char* _ps_1 = _sb1;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(14);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "sub %s %s %s", CS2SX_BoolStr((CS2SX_strcmp_safe(String_Substring(s, 0, 3), "Grö") == 0)), CS2SX_BoolStr((CS2SX_strcmp_safe(String_SubstringFrom(s, 3), "ße") == 0)), String_Substring("hello", 1, 3));
    const char* _ps_2 = _sb2;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(15);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "find %d %d %d %d", String_IndexOf(s, "ß"), String_IndexOfChar(s, 'e'), String_LastIndexOfChar(s, ((u16)0x00F6)), String_IndexOf("ääx", "x"));
    const char* _ps_3 = _sb3;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(16);
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "case %s %s %s %s %s", CS2SX_BoolStr((CS2SX_strcmp_safe(String_ToUpper(s), "GRÖßE") == 0)), CS2SX_BoolStr((CS2SX_strcmp_safe(String_ToLower("ÄÖÜ"), "äöü") == 0)), String_ToUpper("Hello"), CS2SX_BoolStr((CS2SX_strcmp_safe(String_ToLower("ΑΒΓ"), "αβγ") == 0)), CS2SX_BoolStr((CS2SX_strcmp_safe(String_ToUpper("привет"), "ПРИВЕТ") == 0)));
    const char* _ps_4 = _sb4;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(18);
    char* _sb5 = _cs2sx_next_buf();
    _sb5 = CS2SX_Format(_sb5, "char %s %s %s %s %s %s %s", CS2SX_BoolStr(CS2SX_CharIsLetter(((u16)0x00F6))), CS2SX_BoolStr(CS2SX_CharIsUpper(((u16)0x00C4))), CS2SX_BoolStr(CS2SX_CharIsLower(((u16)0x00DF))), CS2SX_BoolStr((CS2SX_CharUpper(((u16)0x00FC)) == ((u16)0x00DC))), CS2SX_BoolStr(CS2SX_CharIsWhiteSpace(((u16)0x00A0))), CS2SX_BoolStr(CS2SX_CharIsLetter('1')), CS2SX_BoolStr(CS2SX_CharIsDigit('7')));
    const char* _ps_5 = _sb5;
    printf("%s\n", _ps_5 ? _ps_5 : "");
    CS2SX_LINE(20);
    int count = 0;
    int umlauts = 0;
    CS2SX_LINE(21);
    CS2SX_Str16It _chars_6 = { s, 0 };
    for (u16 _ch_7; CS2SX_Str16Next(&_chars_6, &_ch_7); )
    {
        u16 c = _ch_7;
        CS2SX_LINE(21);
        count++;
        CS2SX_LINE(21);
        if (c > 127)
        {
            CS2SX_LINE(21);
            umlauts++;
        }
    }
    CS2SX_LINE(22);
    char* _sb6 = _cs2sx_next_buf();
    _sb6 = CS2SX_Format(_sb6, "each %d %d", count, umlauts);
    const char* _ps_8 = _sb6;
    printf("%s\n", _ps_8 ? _ps_8 : "");
    CS2SX_LINE(23);
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "pad %d %s [%s]", CS2SX_Str16Len(CS2SX_NN(String_PadRight("ä", 3, ' '))), CS2SX_BoolStr((CS2SX_strcmp_safe(String_PadLeft("ab", 4, ((u16)0x00B7)), "··ab") == 0)), String_PadLeft("ok", 4, ' '));
    const char* _ps_9 = _sb7;
    printf("%s\n", _ps_9 ? _ps_9 : "");
    CS2SX_LINE(24);
    char* _sb8 = _cs2sx_next_buf();
    _sb8 = CS2SX_Format(_sb8, "edit %s %s %s", CS2SX_BoolStr((CS2SX_strcmp_safe(String_Insert("Grße", 2, "ö"), s) == 0)), CS2SX_BoolStr((CS2SX_strcmp_safe(String_Remove(s, 2, 1), "Grße") == 0)), CS2SX_BoolStr((CS2SX_strcmp_safe(String_RemoveFrom(s, 3), "Grö") == 0)));
    const char* _ps_10 = _sb8;
    printf("%s\n", _ps_10 ? _ps_10 : "");
    CS2SX_LINE(25);
    u16* arr = CS2SX_Str16ToArray(s);
    CS2SX_LINE(26);
    char* _sb9 = _cs2sx_next_buf();
    _sb9 = CS2SX_Format(_sb9, "array %d %s %s %s %s", CS2SX_ArrLen(CS2SX_NN(arr)), CS2SX_BoolStr((CS2SX_AT(arr, 2) == ((u16)0x00F6))), CS2SX_BoolStr((CS2SX_strcmp_safe(({ const u16* _cs2sx_ca = (arr); CS2SX_CharsStr(_cs2sx_ca, 0, CS2SX_ArrLen(_cs2sx_ca)); }), s) == 0)), CS2SX_BoolStr((CS2SX_strcmp_safe(CS2SX_CharRepeat(((u16)0x00E9), 3), "ééé") == 0)), CS2SX_CharRepeat('-', 4));
    const char* _ps_11 = _sb9;
    printf("%s\n", _ps_11 ? _ps_11 : "");
    CS2SX_LINE(27);
    StringBuilder* sb = StringBuilder_New(256);
    CS2SX_LINE(28);
    StringBuilder_AppendChar(StringBuilder_AppendStr(StringBuilder_AppendChar(sb, ((u16)0x00F6)), "x"), ((u16)0x00DF));
    CS2SX_LINE(29);
    char* _sb10 = _cs2sx_next_buf();
    _sb10 = CS2SX_Format(_sb10, "x%s", CS2SX_CharStr(((u16)0x00FC)));
    char* _sb11 = _cs2sx_next_buf();
    _sb11 = CS2SX_Format(_sb11, "%s", CS2SX_CharStr(((u16)0x00E4)));
    char* _sb12 = _cs2sx_next_buf();
    _sb12 = CS2SX_Format(_sb12, "build %s %s %s", CS2SX_BoolStr((CS2SX_strcmp_safe(StringBuilder_ToString(sb), "öxß") == 0)), CS2SX_BoolStr((CS2SX_strcmp_safe(_sb10, "xü") == 0)), CS2SX_BoolStr((CS2SX_strcmp_safe(_sb11, "ä") == 0)));
    const char* _ps_12 = _sb12;
    printf("%s\n", _ps_12 ? _ps_12 : "");
    CS2SX_LINE(30);
    {
        CS2SX_ExFrame _exf_13;
        CS2SX_ExPush(&_exf_13);
        if (setjmp(_exf_13.buf) == 0)
        {
            CS2SX_LINE(30);
            const char* _ps_14 = CS2SX_CharStr(CS2SX_Str16At(s, 9));
            printf("%s\n", _ps_14 ? _ps_14 : "");
            CS2SX_ExPop(&_exf_13);
        }
        else
        {
            Exception* _exf_13_ex = _exf_13.ex;
            if (CS2SX_Exc_Is(_exf_13_ex, &CS2SX_TI_IndexOutOfRangeException))
            {
                CS2SX_LINE(31);
                printf("range ok\n");
                Exception_Free(_exf_13_ex);
            }
            else CS2SX_Throw(_exf_13_ex);
        }
    }
    CS2SX_LINE(32);
    printf("done\n");
}

void UnicodeStringsApp_OnFrame(UnicodeStringsApp* self)
{
    CS2SX_TRACE("UnicodeStringsApp.OnFrame", "Program.cs");
}

void UnicodeStringsApp_Free(UnicodeStringsApp* self)
{
    if (!self) return;
    free(self);
}
