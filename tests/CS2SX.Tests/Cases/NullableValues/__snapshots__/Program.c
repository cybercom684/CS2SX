#include <stdlib.h>
#include "Program.h"

// ── Struct: Size ────────────────────────────────────────
int Size_Equals(Size a, Size b)
{
    return memcmp(&a, &b, sizeof(Size)) == 0;
}

void Profile_Ctor(Profile* self)
{
    CS2SX_TRACE("Profile..ctor", "Program.cs");
}

Profile* Profile_New(void)
{
    Profile* self = (Profile*)calloc(1, sizeof(Profile));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Profile_Dispose;
    self->_gc = Profile_Gc;
    Profile_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Profile_Free);
    return self;
}

Nullable_int Profile_Find(Profile* self, int key)
{
    CS2SX_TRACE("Profile.Find", "Program.cs");
    CS2SX_LINE(15);
    return (key > 0 ? ((Nullable_int){ key * 10, 1 }) : ((Nullable_int){0}));
}

int Profile_Add(Profile* self, Nullable_int bonus)
{
    CS2SX_TRACE("Profile.Add", "Program.cs");
    CS2SX_LINE(17);
    return 1 + (((bonus).has ? (bonus).value : (0)));
}

void Profile_Gc(void* _self, CS2SX_GcVisit visit)
{
    Profile* self = (Profile*)_self; (void)self;
}

static void Profile_GcDyn(void* self, CS2SX_GcVisit visit) { ((Profile*)self)->_gc(self, visit); }
const CS2SX_GcKind Profile_GcKind = { offsetof(Profile, _rc), Profile_GcDyn, (CS2SX_FreeFn)Profile_Free };

void Profile_Dispose(Profile* self)
{
    CS2SX_StrDrop(self->f_Nickname); self->f_Nickname = NULL;
}

void Profile_Free(Profile* self)
{
    if (!self) return;
    Profile* _o = (Profile*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Profile_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Profile* Profile_Retain(Profile* self)
{
    if (self) ((Profile*)self)->_rc++;
    return self;
}

void NullableValuesApp_Init(NullableValuesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))NullableValuesApp_OnInit;
}

void NullableValuesApp_OnInit(NullableValuesApp* self)
{
    CS2SX_TRACE("NullableValuesApp.OnInit", "Program.cs");
    CS2SX_LINE(24);
    Profile* p = Profile_New();
    CS2SX_LINE(25);
    p->f_BestScore = ((Nullable_int){ 120, 1 });
    CS2SX_LINE(26);
    p->f_LastMode = ((Nullable_Mode){ Mode_Run, 1 });
    CS2SX_LINE(27);
    CS2SX_STR_STORE(p->f_Nickname, NULL);
    CS2SX_LINE(29);
    Nullable_int found = Profile_Find(p, 3);
    CS2SX_LINE(30);
    Nullable_int missing = Profile_Find(p, -1);
    CS2SX_LINE(31);
    Nullable_int fallback = ((missing).has ? (missing) : (found));
    CS2SX_LINE(33);
    int better = ({ __auto_type _na_0 = found; __auto_type _nb_1 = ((Nullable_int){ 20, 1 }); _na_0.has && _nb_1.has && _na_0.value > _nb_1.value; });
    CS2SX_LINE(34);
    Nullable_int sum = ({ __auto_type _na_2 = found; __auto_type _nb_3 = p->f_BestScore; (_na_2.has && _nb_3.has) ? ((Nullable_int){ _na_2.value + _nb_3.value, 1 }) : ((Nullable_int){0}); });
    CS2SX_LINE(35);
    int same = ({ __auto_type _na_4 = found; __auto_type _nb_5 = missing; _na_4.has == _nb_5.has && (!_na_4.has || _na_4.value == _nb_5.value); });
    CS2SX_LINE(37);
    Nullable_Size box = ((Nullable_Size){0});
    CS2SX_LINE(38);
    if (!(box).has)
    {
        CS2SX_LINE(38);
        box = ((Nullable_Size){ (Size){ .W = 4, .H = 2 }, 1 });
    }
    CS2SX_LINE(40);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d %d", Profile_Add(p, ((Nullable_int){ 5, 1 })), Profile_Add(p, ((Nullable_int){0})));
    const char* _ps_6 = _sb0;
    printf("%s\n", _ps_6 ? _ps_6 : "");
    CS2SX_LINE(41);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d %d", (fallback).value, ((missing).has ? (missing).value : (-7)));
    const char* _ps_7 = _sb1;
    printf("%s\n", _ps_7 ? _ps_7 : "");
    CS2SX_LINE(42);
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "%s %s %d", CS2SX_BoolStr(better), CS2SX_BoolStr(same), (((sum).has ? (sum).value : 0)));
    const char* _ps_8 = _sb2;
    printf("%s\n", _ps_8 ? _ps_8 : "");
    CS2SX_LINE(43);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "%s %d", CS2SX_BoolStr((({ __auto_type _na_9 = p->f_LastMode; __auto_type _nb_10 = ((Nullable_Mode){ Mode_Run, 1 }); _na_9.has == _nb_10.has && (!_na_9.has || _na_9.value == _nb_10.value); }))), (box).value.W);
    const char* _ps_11 = _sb3;
    printf("%s\n", _ps_11 ? _ps_11 : "");
    CS2SX_LINE(44);
    const char* _ps_12 = (p->f_Nickname ?: "anon");
    printf("%s\n", _ps_12 ? _ps_12 : "");
}

void NullableValuesApp_Free(NullableValuesApp* self)
{
    if (!self) return;
    free(self);
}
