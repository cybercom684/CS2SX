#include <stdlib.h>
#include "_generics.h"
#include "Program.h"
#include "CS2SX_Json.h"

static const char* Rank__boxstr(void* b) { return Rank_ToString(((CS2SX_Box*)b)->v.i); }
const CS2SX_ClassInfo Rank_BoxInfo = { "Rank", NULL, Rank__boxstr };
void Item_Ctor(Item* self)
{
    CS2SX_TRACE("Item..ctor", "Program.cs");
    self->f_Name = CS2SX_StrOwn("");
}

Item* Item_New(void)
{
    Item* self = (Item*)calloc(1, sizeof(Item));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Item__Destroy;
    self->_gc = Item_Gc;
    self->_class = &Item_ClassInfo;
    Item_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Item_Free);
    return self;
}

const CS2SX_ClassInfo Item_ClassInfo = { "Item", NULL };

void Item_Gc(void* _self, CS2SX_GcVisit visit)
{
    Item* self = (Item*)_self; (void)self;
}

static void Item_GcDyn(void* self, CS2SX_GcVisit visit) { ((Item*)self)->_gc(self, visit); }
const CS2SX_GcKind Item_GcKind = { offsetof(Item, _rc), Item_GcDyn, (CS2SX_FreeFn)Item_Free };

void Item__Destroy(Item* self)
{
    CS2SX_StrDrop(self->f_Name); self->f_Name = NULL;
}

void Item_Free(Item* self)
{
    if (!self) return;
    Item* _o = (Item*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Item_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Item* Item_Retain(Item* self)
{
    if (self) ((Item*)self)->_rc++;
    return self;
}

void SaveGame_Ctor(SaveGame* self)
{
    CS2SX_TRACE("SaveGame..ctor", "Program.cs");
    self->f_Lives = 3;
    self->f_Player = CS2SX_StrOwn("");
    self->f_Items = List_Item_Retain(List_Item_New());
    self->f_Scores = (int*)CS2SX_ArrNew(0, sizeof(int));
    self->f_Flags = Dict_str_int_Retain(Dict_str_int_New());
    self->f_Health = 100;
    self->f_Secret = CS2SX_StrOwn("keep");
    self->f_Note = CS2SX_StrOwn("default");
}

SaveGame* SaveGame_New(void)
{
    SaveGame* self = (SaveGame*)calloc(1, sizeof(SaveGame));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))SaveGame__Destroy;
    self->_gc = SaveGame_Gc;
    self->_class = &SaveGame_ClassInfo;
    SaveGame_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)SaveGame_Free);
    return self;
}

const CS2SX_ClassInfo SaveGame_ClassInfo = { "SaveGame", NULL };

void SaveGame_Gc(void* _self, CS2SX_GcVisit visit)
{
    SaveGame* self = (SaveGame*)_self; (void)self;
    if (visit) visit((void*)self->f_Items, &List_Item_GcKind);
    else if (self->f_Items) { void* _o = (void*)self->f_Items; self->f_Items = NULL; ((void (*)(void*))List_Item_Free)(_o); }
}

static void SaveGame_GcDyn(void* self, CS2SX_GcVisit visit) { ((SaveGame*)self)->_gc(self, visit); }
const CS2SX_GcKind SaveGame_GcKind = { offsetof(SaveGame, _rc), SaveGame_GcDyn, (CS2SX_FreeFn)SaveGame_Free };

void SaveGame__Destroy(SaveGame* self)
{
    CS2SX_StrDrop(self->f_Player); self->f_Player = NULL;
    if (self->f_Items) { List_Item_Free(self->f_Items); self->f_Items = NULL; }
    if (self->f_Flags) { Dict_str_int_Free(self->f_Flags); self->f_Flags = NULL; }
    CS2SX_StrDrop(self->f_Secret); self->f_Secret = NULL;
    CS2SX_StrDrop(self->f_Note); self->f_Note = NULL;
}

void SaveGame_Free(SaveGame* self)
{
    if (!self) return;
    SaveGame* _o = (SaveGame*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &SaveGame_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

SaveGame* SaveGame_Retain(SaveGame* self)
{
    if (self) ((SaveGame*)self)->_rc++;
    return self;
}

Point* Point_New(int X, int Y)
{
    Point* self = (Point*)calloc(1, sizeof(Point));
    if (!self) return NULL;
    self->_rc = 1;
    self->f_X = X;
    self->f_Y = Y;
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Point_Free);
    return self;
}

void Point_Free(Point* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Point* Point_Retain(Point* self)
{
    if (self) self->_rc++;
    return self;
}

const CS2SX_GcKind Point_GcKind = { offsetof(Point, _rc), CS2SX_GcLeaf, (CS2SX_FreeFn)Point_Free };
const CS2SX_ClassInfo Point_ClassInfo = { "Point", NULL };

// ── Struct: Size ────────────────────────────────────────
int Size_Equals(Size a, Size b)
{
    return memcmp(&a, &b, sizeof(Size)) == 0;
}

void JsonApp_Init(JsonApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))JsonApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))JsonApp_OnFrame;
}

void JsonApp_OnInit(JsonApp* self)
{
    CS2SX_TRACE("JsonApp.OnInit", "Program.cs");
    CS2SX_LINE(45);
    SaveGame* _savegame_0 = SaveGame_New();
    CS2SX_STR_STORE(_savegame_0->f_Player, "Mia \"M\"");
    _savegame_0->f_Level = 7;
    _savegame_0->f_Time = 12.5;
    _savegame_0->f_Hard = 1;
    _savegame_0->f_Rank = Rank_Gold;
    SaveGame* g = _savegame_0;
    CS2SX_LINE(46);
    Item* _item_1 = Item_New();
    CS2SX_STR_STORE(_item_1->f_Name, "Schwert");
    _item_1->f_Count = 1;
    List_Item_Add(g->f_Items, _item_1);
    CS2SX_LINE(47);
    Item* _item_2 = Item_New();
    CS2SX_STR_STORE(_item_2->f_Name, "Trank");
    _item_2->f_Count = 3;
    List_Item_Add(g->f_Items, _item_2);
    CS2SX_LINE(48);
    g->f_Scores = (int*)CS2SX_ArrFromInit((int[]){ 10, 20 }, 2, sizeof(int));
    CS2SX_LINE(49);
    Dict_str_int_Set(g->f_Flags, "door", 1);
    CS2SX_LINE(50);
    const char* text = CS2SX_Json_S_SaveGame(g, NULL);
    CS2SX_LINE(51);
    const char* _ps_3 = text;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(54);
    SaveGame* back = CS2SX_Json_D_SaveGame(text, NULL);
    CS2SX_LINE(55);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "%s %d %s %s %s %d %s %d %d %s %s %s", CS2SX_NN(back)->f_Player, CS2SX_NN(back)->f_Level, Double_ToString(CS2SX_NN(back)->f_Time), CS2SX_BoolStr(CS2SX_NN(back)->f_Hard), Rank_ToString(CS2SX_NN(back)->f_Rank), CS2SX_NN(CS2SX_NN(back)->f_Items)->count, CS2SX_NN(List_Item_Get(CS2SX_NN(back)->f_Items, 1))->f_Name, CS2SX_AT(CS2SX_NN(back)->f_Scores, 1), Dict_str_int_Get(CS2SX_NN(back)->f_Flags, "door"), Float_ToString(CS2SX_NN(back)->f_Health), CS2SX_NN(back)->f_Secret, CS2SX_BoolStr((!(CS2SX_NN(back)->f_Best).has)));
    const char* _ps_4 = _sb0;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(60);
    SaveGame* part = CS2SX_Json_D_SaveGame("{ \"Level\": 3, \"Unknown\": [1, {\"a\": null}], \"Best\": 9 }", NULL);
    CS2SX_LINE(61);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "part %d %s %d %d", CS2SX_NN(part)->f_Level, CS2SX_NN(part)->f_Note, CS2SX_NN(part)->f_Best, CS2SX_NN(part)->f_Lives);
    const char* _ps_5 = _sb1;
    printf("%s\n", _ps_5 ? _ps_5 : "");
    CS2SX_LINE(64);
    CS2SX_JsonOptions* _cs2sx_jsonoptions_6 = CS2SX_JsonOptions_New();
    _cs2sx_jsonoptions_6->f_WriteIndented = 1;
    _cs2sx_jsonoptions_6->f_PropertyNamingPolicy = 1;
    _cs2sx_jsonoptions_6->f_IncludeFields = 1;
    CS2SX_JsonOptions* opts = _cs2sx_jsonoptions_6;
    CS2SX_LINE(65);
    Item* _item_7 = Item_New();
    CS2SX_STR_STORE(_item_7->f_Name, "Ring");
    _item_7->f_Count = 2;
    Item* item = _item_7;
    CS2SX_LINE(66);
    const char* _ps_8 = CS2SX_Json_S_Item(item, opts);
    printf("%s\n", _ps_8 ? _ps_8 : "");
    CS2SX_LINE(71);
    SaveGame* _savegame_9 = SaveGame_New();
    _savegame_9->f_Lives = 5;
    CS2SX_JsonOptions* _cs2sx_jsonoptions_10 = CS2SX_JsonOptions_New();
    _cs2sx_jsonoptions_10->f_IncludeFields = 1;
    const char* withFields = CS2SX_Json_S_SaveGame(_savegame_9, _cs2sx_jsonoptions_10);
    CS2SX_LINE(72);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "fields %s", CS2SX_BoolStr(String_Contains(CS2SX_NN(withFields), "\"Lives\":5")));
    const char* _ps_11 = _sb2;
    printf("%s\n", _ps_11 ? _ps_11 : "");
    CS2SX_LINE(73);
    CS2SX_JsonOptions* _cs2sx_jsonoptions_12 = CS2SX_JsonOptions_New();
    _cs2sx_jsonoptions_12->f_PropertyNameCaseInsensitive = 1;
    Item* ci = CS2SX_Json_D_Item("{\"NAME\":\"Axt\",\"count\":4}", _cs2sx_jsonoptions_12);
    CS2SX_LINE(74);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "ci %s %d", CS2SX_NN(ci)->f_Name, CS2SX_NN(ci)->f_Count);
    const char* _ps_13 = _sb3;
    printf("%s\n", _ps_13 ? _ps_13 : "");
    CS2SX_LINE(77);
    Point* p = CS2SX_Json_D_Point("{\"X\":4,\"Y\":-2}", NULL);
    CS2SX_LINE(78);
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "point %s %d", CS2SX_Json_S_Point(p, NULL), CS2SX_NN(p)->f_Y);
    const char* _ps_14 = _sb4;
    printf("%s\n", _ps_14 ? _ps_14 : "");
    CS2SX_LINE(79);
    Size s = CS2SX_Json_D_Size("{\"W\":3,\"H\":5}", NULL);
    CS2SX_LINE(80);
    char* _sb5 = _cs2sx_next_buf();
    _sb5 = CS2SX_Format(_sb5, "size %d", (s.W * s.H));
    const char* _ps_15 = _sb5;
    printf("%s\n", _ps_15 ? _ps_15 : "");
    CS2SX_LINE(81);
    List_str* list = CS2SX_Json_D_List_str("[\"a\\nb\", \"\\u00e4\\u20ac\", null]", NULL);
    CS2SX_LINE(82);
    char* _sb6 = _cs2sx_next_buf();
    _sb6 = CS2SX_Format(_sb6, "list %d %d %s %s", CS2SX_NN(list)->count, strlen(CS2SX_NN(List_str_Get(list, 0))), CS2SX_BoolStr((CS2SX_strcmp_safe(List_str_Get(list, 1), "ä€") == 0)), CS2SX_BoolStr((List_str_Get(list, 2) == NULL)));
    const char* _ps_16 = _sb6;
    printf("%s\n", _ps_16 ? _ps_16 : "");
    CS2SX_LINE(83);
    List_double* _list_17 = List_double_New();
    List_double_Add(_list_17, 1.5);
    List_double_Add(_list_17, -2);
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "plain %s %s %s %d", CS2SX_Json_S_int(42, NULL), CS2SX_Json_S_str("x", NULL), CS2SX_Json_S_List_double(_list_17, NULL), CS2SX_Json_D_int(" 17 ", NULL));
    const char* _ps_18 = _sb7;
    printf("%s\n", _ps_18 ? _ps_18 : "");
    CS2SX_LINE(87);
    {
        CS2SX_ExFrame _exf_19;
        CS2SX_ExPush(&_exf_19);
        if (setjmp(_exf_19.buf) == 0)
        {
            CS2SX_LINE(87);
            CS2SX_Json_D_Item("{\"Count\":\"x\"}", NULL);
            CS2SX_ExPop(&_exf_19);
        }
        else
        {
            Exception* _exf_19_ex = _exf_19.ex;
            if (CS2SX_Exc_Is(_exf_19_ex, &CS2SX_TI_JsonException))
            {
                CS2SX_LINE(88);
                printf("error type\n");
                Exception_Free(_exf_19_ex);
            }
            else CS2SX_Throw(_exf_19_ex);
        }
    }
    CS2SX_LINE(89);
    {
        CS2SX_ExFrame _exf_20;
        CS2SX_ExPush(&_exf_20);
        if (setjmp(_exf_20.buf) == 0)
        {
            CS2SX_LINE(89);
            CS2SX_Json_D_Item("{\"Count\":1", NULL);
            CS2SX_ExPop(&_exf_20);
        }
        else
        {
            Exception* _exf_20_ex = _exf_20.ex;
            if (CS2SX_Exc_Is(_exf_20_ex, &CS2SX_TI_JsonException))
            {
                CS2SX_LINE(90);
                printf("error end\n");
                Exception_Free(_exf_20_ex);
            }
            else CS2SX_Throw(_exf_20_ex);
        }
    }
    CS2SX_LINE(91);
    {
        CS2SX_ExFrame _exf_21;
        CS2SX_ExPush(&_exf_21);
        if (setjmp(_exf_21.buf) == 0)
        {
            CS2SX_LINE(91);
            CS2SX_Json_D_byte("300", NULL);
            CS2SX_ExPop(&_exf_21);
        }
        else
        {
            Exception* _exf_21_ex = _exf_21.ex;
            if (CS2SX_Exc_Is(_exf_21_ex, &CS2SX_TI_JsonException))
            {
                CS2SX_LINE(92);
                printf("error range\n");
                Exception_Free(_exf_21_ex);
            }
            else CS2SX_Throw(_exf_21_ex);
        }
    }
    CS2SX_LINE(93);
    printf("done\n");
}

void JsonApp_OnFrame(JsonApp* self)
{
    CS2SX_TRACE("JsonApp.OnFrame", "Program.cs");
}

void JsonApp_Free(JsonApp* self)
{
    if (!self) return;
    free(self);
}
