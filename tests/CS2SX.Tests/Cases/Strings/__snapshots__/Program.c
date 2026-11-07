#include <stdlib.h>
#include "Program.h"

void StringsApp_Init(StringsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))StringsApp_OnInit;
}

void StringsApp_OnInit(StringsApp* self)
{
    const char* name = "Switch";
    int score = 1234;
    float ratio = 0.75f;
    printf("Hello %s, score=%d, ratio=%s\n", name, score, Float_ToString(ratio));
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%s %s %d", String_ToUpper(name), String_ToLower(name), strlen(name));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    const char* _ps_1 = String_Substring(name, 1, 3);
    printf("%s\n", _ps_1 ? _ps_1 : "");
    const char* _ps_2 = (String_Contains(name, "wit") ? "contains" : "missing");
    printf("%s\n", _ps_2 ? _ps_2 : "");
    const char* _ps_3 = String_Replace(name, "S", "Tw");
    printf("%s\n", _ps_3 ? _ps_3 : "");
    const char* _ps_4 = String_Trim("  padded  ");
    printf("%s\n", _ps_4 ? _ps_4 : "");
    printf("%s\n", CS2SX_BoolStr(String_StartsWith(name, "Sw") && String_EndsWith(name, "ch")));
    printf("%d\n", String_IndexOfChar(name, 't'));
    const char** parts = CS2SX_StrArr_FromList(String_Split("a,b,c", ","));
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d parts, last=%s", CS2SX_ArrLen(parts), CS2SX_AT(parts, 2));
    const char* _ps_5 = _sb1;
    printf("%s\n", _ps_5 ? _ps_5 : "");
    const char* _ps_6 = String_JoinArr("-", parts);
    printf("%s\n", _ps_6 ? _ps_6 : "");
    const char* _ps_7 = (String_IsNullOrEmpty("") ? "empty" : "not empty");
    printf("%s\n", _ps_7 ? _ps_7 : "");
    int parsed = CS2SX_Int_Parse("77");
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "%s %s", Int_ToString((int)parsed), Int_ToString((int)score));
    const char* _ps_8 = _sb2;
    printf("%s\n", _ps_8 ? _ps_8 : "");
    StringBuilder* sb = StringBuilder_New(256);
    for (int i = 0; i < 3; i++)
    {
        StringBuilder_AppendChar(StringBuilder_AppendInt(sb, i), ';');
    }
    const char* _ps_9 = StringBuilder_ToString(sb);
    printf("%s\n", _ps_9 ? _ps_9 : "");
}

void StringsApp_Free(StringsApp* self)
{
    if (!self) return;
    free(self);
}
