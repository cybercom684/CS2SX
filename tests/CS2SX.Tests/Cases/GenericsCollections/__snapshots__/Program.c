#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

void Item_Ctor(Item* self, const char* name, int count)
{
    CS2SX_TRACE("Item..ctor", "Program.cs");
    CS2SX_LINE(14);
    CS2SX_STR_STORE(self->f_Name, name);
    CS2SX_LINE(14);
    self->f_Count = count;
}

Item* Item_New(const char* name, int count)
{
    Item* self = (Item*)calloc(1, sizeof(Item));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Item_Dispose;
    self->_gc = Item_Gc;
    Item_Ctor(self, name, count);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Item_Free);
    return self;
}

void Item_Gc(void* _self, CS2SX_GcVisit visit)
{
    Item* self = (Item*)_self; (void)self;
}

static void Item_GcDyn(void* self, CS2SX_GcVisit visit) { ((Item*)self)->_gc(self, visit); }
const CS2SX_GcKind Item_GcKind = { offsetof(Item, _rc), Item_GcDyn, (CS2SX_FreeFn)Item_Free };

void Item_Dispose(Item* self)
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

void GenericsCollectionsApp_Init(GenericsCollectionsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))GenericsCollectionsApp_OnInit;
}

void GenericsCollectionsApp_OnInit(GenericsCollectionsApp* self)
{
    CS2SX_TRACE("GenericsCollectionsApp.OnInit", "Program.cs");
    CS2SX_LINE(21);
    List_int* numbers = List_int_New();
    CS2SX_LINE(22);
    for (int i = 0; i < 10; i++)
    {
        CS2SX_LINE(22);
        List_int_Add(numbers, i * i);
    }
    CS2SX_LINE(23);
    List_int_RemoveValue(numbers, 4);
    CS2SX_LINE(24);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "count: %d first: %d", numbers->count, List_int_Get(numbers, 0));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(26);
    List_str* _list_1 = List_str_New();
    List_str_Add(_list_1, "b");
    List_str_Add(_list_1, "a");
    List_str_Add(_list_1, "c");
    List_str* names = _list_1;
    CS2SX_LINE(27);
    List_str_Sort(names);
    CS2SX_LINE(28);
    const char* _ps_2 = String_Join(",", names);
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(30);
    Dict_str_int* stock = Dict_str_int_New();
    CS2SX_LINE(31);
    Dict_str_int_Set(stock, "apple", 3);
    CS2SX_LINE(32);
    Dict_str_int_Set(stock, "pear", 5);
    CS2SX_LINE(33);
    if (Dict_str_int_ContainsKey(stock, "apple"))
    {
        CS2SX_LINE(33);
        ({ __auto_type _cs2sx_c = stock; __auto_type _cs2sx_k = "apple"; Dict_str_int_Set(_cs2sx_c, _cs2sx_k, Dict_str_int_Get(_cs2sx_c, _cs2sx_k) + (1)); });
    }
    CS2SX_LINE(34);
    for (int _i_kv = 0; _i_kv < (int)(stock->count); _i_kv++)
    {
        const char* kv_Key = stock->keys[_i_kv];
        int kv_Value = stock->vals[_i_kv];
        char* _sb1 = _cs2sx_next_buf();
        snprintf(_sb1, CS2SX_STRBUF_SIZE, "%s=%d", kv_Key, kv_Value);
        const char* _ps_3 = _sb1;
        printf("%s\n", _ps_3 ? _ps_3 : "");
    }
    CS2SX_LINE(37);
    List_Item* items = List_Item_New();
    CS2SX_LINE(38);
    List_Item_Add(items, Item_New("sword", 1));
    CS2SX_LINE(39);
    List_Item_Add(items, Item_New("potion", 4));
    CS2SX_LINE(40);
    const char* _ps_4 = CS2SX_NN(List_Item_Get(items, 1))->f_Name;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(42);
    Box_int* box = Box_int_New(42);
    CS2SX_LINE(43);
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "box: %d", Box_int_Get(box));
    const char* _ps_5 = _sb2;
    printf("%s\n", _ps_5 ? _ps_5 : "");
}

void GenericsCollectionsApp_Free(GenericsCollectionsApp* self)
{
    if (!self) return;
    free(self);
}
