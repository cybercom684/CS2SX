#include <stdlib.h>
#include "Program.h"

static const char* Rank__boxstr(void* b) { return Rank_ToString(((CS2SX_Box*)b)->v.i); }
const CS2SX_ClassInfo Rank_BoxInfo = { "Rank", NULL, Rank__boxstr };
static const char* Code__boxstr(void* b) { return Code_ToString(((CS2SX_Box*)b)->v.i); }
const CS2SX_ClassInfo Code_BoxInfo = { "Code", NULL, Code__boxstr };
static const char* Access__boxstr(void* b) { return Access_ToString(((CS2SX_Box*)b)->v.i); }
const CS2SX_ClassInfo Access_BoxInfo = { "Access", NULL, Access__boxstr };
void Player_Ctor(Player* self)
{
    CS2SX_TRACE("Player..ctor", "Program.cs");
    self->f_Rank = Rank_Silver;
}

Player* Player_New(void)
{
    Player* self = (Player*)calloc(1, sizeof(Player));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Player__Destroy;
    self->_gc = Player_Gc;
    self->_class = &Player_ClassInfo;
    Player_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Player_Free);
    return self;
}

const CS2SX_ClassInfo Player_ClassInfo = { "Player", NULL, NULL };

void Player_Gc(void* _self, CS2SX_GcVisit visit)
{
    Player* self = (Player*)_self; (void)self;
}

static void Player_GcDyn(void* self, CS2SX_GcVisit visit) { ((Player*)self)->_gc(self, visit); }
const CS2SX_GcKind Player_GcKind = { offsetof(Player, _rc), Player_GcDyn, (CS2SX_FreeFn)Player_Free };

void Player__Destroy(Player* self)
{
}

void Player_Free(Player* self)
{
    if (!self) return;
    Player* _o = (Player*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Player_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Player* Player_Retain(Player* self)
{
    if (self) ((Player*)self)->_rc++;
    return self;
}

void EnumNamesApp_Init(EnumNamesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))EnumNamesApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))EnumNamesApp_OnFrame;
}

static const char* _lambda_0(Rank x)
{
    return Rank_ToString(x);
}

void EnumNamesApp_OnInit(EnumNamesApp* self)
{
    CS2SX_TRACE("EnumNamesApp.OnInit", "Program.cs");
    CS2SX_LINE(21);
    Rank r = Rank_Gold;
    CS2SX_LINE(22);
    const char* _ps_0 = Rank_ToString(r);
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(23);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "rank %s %s", Rank_ToString(r), Rank_ToString(r));
    const char* _ps_1 = _sb0;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(24);
    printf("interp %s %d %s\n", Rank_ToString(r), ((int)r), Code_ToString(Code_NotFound));
    CS2SX_LINE(25);
    Player* p = Player_New();
    CS2SX_LINE(26);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "prop %s", Rank_ToString(p->f_Rank));
    const char* _ps_2 = _sb1;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(27);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "odd %s %s", Rank_ToString(((Rank)7)), Code_ToString(((Code)201)));
    const char* _ps_3 = _sb2;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(28);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "flags %s / %s / %s / %s", Access_ToString((Access_Read | Access_Exec)), Access_ToString(Access_All), Access_ToString(Access_None), Access_ToString(((Access)8)));
    const char* _ps_4 = _sb3;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(30);
    const char* s = "x";
    CS2SX_LINE(31);
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "%s%s", s ? s : "", Rank_ToString(Rank_Bronze));
    s = _sb4;
    CS2SX_LINE(32);
    const char* _ps_5 = s;
    printf("%s\n", _ps_5 ? _ps_5 : "");
    CS2SX_LINE(33);
    List_Rank* _list_6 = List_Rank_New();
    List_Rank_Add(_list_6, Rank_Gold);
    List_Rank_Add(_list_6, Rank_Bronze);
    List_Rank* list = _list_6;
    CS2SX_LINE(34);
    List_str* _sel_7 = List_str_New();
    for (int _i_8 = 0; _i_8 < list->count; _i_8++)
    {
        Rank _e__sel_7 = List_Rank_Get(list, _i_8);
        List_str_Add(_sel_7, (const char*)_lambda_0(_e__sel_7));
    }
    char* _sb5 = _cs2sx_next_buf();
    _sb5 = CS2SX_Format(_sb5, "list %s", String_Join(",", _sel_7));
    const char* _ps_9 = _sb5;
    printf("%s\n", _ps_9 ? _ps_9 : "");
    CS2SX_LINE(35);
    printf("done\n");
}

void EnumNamesApp_OnFrame(EnumNamesApp* self)
{
    CS2SX_TRACE("EnumNamesApp.OnFrame", "Program.cs");
}

void EnumNamesApp_Free(EnumNamesApp* self)
{
    if (!self) return;
    free(self);
}
