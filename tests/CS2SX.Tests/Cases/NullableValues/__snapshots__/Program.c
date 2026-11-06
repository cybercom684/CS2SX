#include <stdlib.h>
#include "Program.h"

// ── Struct: Size ────────────────────────────────────────
int Size_Equals(Size a, Size b)
{
    return memcmp(&a, &b, sizeof(Size)) == 0;
}

Profile* Profile_New(void)
{
    Profile* self = (Profile*)malloc(sizeof(Profile));
    if (!self) return NULL;
    memset(self, 0, sizeof(Profile));
    self->_rc = 1;
    return self;
}

Nullable_int Profile_Find(Profile* self, int key)
{
    return (key > 0 ? ((Nullable_int){ key * 10, 1 }) : ((Nullable_int){0}));
}

int Profile_Add(Profile* self, Nullable_int bonus)
{
    return 1 + (((bonus).has ? (bonus).value : (0)));
}

void Profile_Free(Profile* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Profile* Profile_Retain(Profile* self)
{
    if (self) self->_rc++;
    return self;
}

void NullableValuesApp_Init(NullableValuesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))NullableValuesApp_OnInit;
}

void NullableValuesApp_OnInit(NullableValuesApp* self)
{
    Profile* p = Profile_New();
    p->f_BestScore = ((Nullable_int){ 120, 1 });
    p->f_LastMode = ((Nullable_Mode){ Run, 1 });
    p->f_Nickname = NULL;
    Nullable_int found = Profile_Find(p, 3);
    Nullable_int missing = Profile_Find(p, -1);
    Nullable_int fallback = ((missing).has ? (missing) : (found));
    int better = ({ __auto_type _na_0 = found; __auto_type _nb_1 = ((Nullable_int){ 20, 1 }); _na_0.has && _nb_1.has && _na_0.value > _nb_1.value; });
    Nullable_int sum = ({ __auto_type _na_2 = found; __auto_type _nb_3 = p->f_BestScore; (_na_2.has && _nb_3.has) ? ((Nullable_int){ _na_2.value + _nb_3.value, 1 }) : ((Nullable_int){0}); });
    int same = ({ __auto_type _na_4 = found; __auto_type _nb_5 = missing; _na_4.has == _nb_5.has && (!_na_4.has || _na_4.value == _nb_5.value); });
    Nullable_Size box = ((Nullable_Size){0});
    if (!(box).has)
    {
        box = ((Nullable_Size){ (Size){ .W = 4, .H = 2 }, 1 });
    }
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d %d", Profile_Add(p, ((Nullable_int){ 5, 1 })), Profile_Add(p, ((Nullable_int){0})));
    const char* _ps_6 = _sb0;
    printf("%s\n", _ps_6 ? _ps_6 : "");
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d %d", (fallback).value, ((missing).has ? (missing).value : (-7)));
    const char* _ps_7 = _sb1;
    printf("%s\n", _ps_7 ? _ps_7 : "");
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "%d %d %d", better, same, (((sum).has ? (sum).value : 0)));
    const char* _ps_8 = _sb2;
    printf("%s\n", _ps_8 ? _ps_8 : "");
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "%d %d", (({ __auto_type _na_9 = p->f_LastMode; __auto_type _nb_10 = ((Nullable_Mode){ Run, 1 }); _na_9.has == _nb_10.has && (!_na_9.has || _na_9.value == _nb_10.value); })), (box).value.W);
    const char* _ps_11 = _sb3;
    printf("%s\n", _ps_11 ? _ps_11 : "");
    const char* _ps_12 = (p->f_Nickname ?: "anon");
    printf("%s\n", _ps_12 ? _ps_12 : "");
}

void NullableValuesApp_Free(NullableValuesApp* self)
{
    if (!self) return;
    free(self);
}
