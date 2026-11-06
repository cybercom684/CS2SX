#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

Item* Item_New(const char* name, int count)
{
    Item* self = (Item*)malloc(sizeof(Item));
    if (!self) return NULL;
    memset(self, 0, sizeof(Item));
    self->_rc = 1;
    self->f_Name = _cs2sx_heap_strdup(name);
    self->f_Count = count;
    return self;
}

void Item_Free(Item* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Item* Item_Retain(Item* self)
{
    if (self) self->_rc++;
    return self;
}

void GenericsCollectionsApp_Init(GenericsCollectionsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))GenericsCollectionsApp_OnInit;
}

void GenericsCollectionsApp_OnInit(GenericsCollectionsApp* self)
{
    List_int* numbers = List_int_New();
    for (int i = 0; i < 10; i++)
    {
        List_int_Add(numbers, i * i);
    }
    List_int_RemoveValue(numbers, 4);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "count: %d first: %d", numbers->count, List_int_Get(numbers, 0));
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "count: %d first: %d", numbers->count, List_int_Get(numbers, 0));
    const char* _ps_0 = _sb1;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    List_str* _list_1 = List_str_New();
    List_str_Add(_list_1, "b");
    List_str_Add(_list_1, "a");
    List_str_Add(_list_1, "c");
    List_str* names = _list_1;
    List_str_Sort(names);
    const char* _ps_2 = String_Join(",", names);
    printf("%s\n", _ps_2 ? _ps_2 : "");
    Dict_str_int* stock = Dict_str_int_New();
    Dict_str_int_Set(stock, "apple", 3);
    Dict_str_int_Set(stock, "pear", 5);
    if (Dict_str_int_ContainsKey(stock, "apple"))
    {
        Dict_str_int_Set(stock, "apple", 1);
    }
    for (int _i_kv = 0; _i_kv < (int)(stock->count); _i_kv++)
    {
        const char* kv_Key = stock->keys[_i_kv];
        int kv_Value = stock->vals[_i_kv];
        char* _sb2 = _cs2sx_next_buf();
        snprintf(_sb2, CS2SX_STRBUF_SIZE, "%s=%d", kv_Key, kv_Value);
        char* _sb3 = _cs2sx_next_buf();
        snprintf(_sb3, CS2SX_STRBUF_SIZE, "%s=%d", kv_Key, kv_Value);
        const char* _ps_3 = _sb3;
        printf("%s\n", _ps_3 ? _ps_3 : "");
    }
    List_Item* items = List_Item_New();
    List_Item_Add(items, Item_New("sword", 1));
    List_Item_Add(items, Item_New("potion", 4));
    const char* _ps_4 = List_Item_Get(items, 1)->f_Name;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    Box<int>* box = Box_int_New(42);
    char* _sb4 = _cs2sx_next_buf();
    snprintf(_sb4, CS2SX_STRBUF_SIZE, "box: %d", Box_Get((Box*)box));
    char* _sb5 = _cs2sx_next_buf();
    snprintf(_sb5, CS2SX_STRBUF_SIZE, "box: %d", Box_Get((Box*)box));
    const char* _ps_5 = _sb5;
    printf("%s\n", _ps_5 ? _ps_5 : "");
}

void GenericsCollectionsApp_Free(GenericsCollectionsApp* self)
{
    if (!self) return;
    free(self);
}
