#include <stdlib.h>
#include "Program.h"

void Item_Ctor(Item* self, const char* n)
{
    CS2SX_TRACE("Item..ctor", "Program.cs");
    CS2SX_LINE(6);
    CS2SX_STR_STORE(self->f_Name, n);
}

Item* Item_New(const char* n)
{
    Item* self = (Item*)calloc(1, sizeof(Item));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Item__Destroy;
    self->_gc = Item_Gc;
    self->_class = &Item_ClassInfo;
    Item_Ctor(self, n);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Item_Free);
    return self;
}

const CS2SX_ClassInfo Item_ClassInfo = { "Item", NULL, NULL };

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

void Bag_Ctor(Bag* self)
{
    CS2SX_TRACE("Bag..ctor", "Program.cs");
    self->f_items = List_int_Retain(List_int_New());
}

Bag* Bag_New(void)
{
    Bag* self = (Bag*)calloc(1, sizeof(Bag));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Bag__Destroy;
    self->_gc = Bag_Gc;
    self->_class = &Bag_ClassInfo;
    Bag_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Bag_Free);
    return self;
}

void Bag_Add(Bag* self, int x)
{
    CS2SX_TRACE("Bag.Add", "Program.cs");
    CS2SX_LINE(12);
    List_int_Add(self->f_items, x);
}

int Bag_TryFirst(Bag* self, int* v)
{
    CS2SX_TRACE("Bag.TryFirst", "Program.cs");
    CS2SX_LINE(14);
    if (CS2SX_NN(self->f_items)->count == 0)
    {
        CS2SX_LINE(14);
        *v = 0;
        CS2SX_LINE(14);
        return 0;
    }
    CS2SX_LINE(14);
    *v = List_int_Get(self->f_items, 0);
    CS2SX_LINE(14);
    return 1;
}

void Bag_Swap(Bag* self, int* a, int* b)
{
    CS2SX_TRACE("Bag.Swap", "Program.cs");
    CS2SX_LINE(15);
    int t = (*a);
    CS2SX_LINE(15);
    *a = (*b);
    CS2SX_LINE(15);
    *b = t;
}

int Bag_Find(Bag* self, const char* name, Item** found)
{
    CS2SX_TRACE("Bag.Find", "Program.cs");
    CS2SX_LINE(16);
    *found = (CS2SX_strcmp_safe(name, "x") == 0 ? NULL : Item_New(name));
    CS2SX_LINE(16);
    return (*found) != NULL;
}

const CS2SX_ClassInfo Bag_ClassInfo = { "Bag", NULL, NULL };

void Bag_Gc(void* _self, CS2SX_GcVisit visit)
{
    Bag* self = (Bag*)_self; (void)self;
}

static void Bag_GcDyn(void* self, CS2SX_GcVisit visit) { ((Bag*)self)->_gc(self, visit); }
const CS2SX_GcKind Bag_GcKind = { offsetof(Bag, _rc), Bag_GcDyn, (CS2SX_FreeFn)Bag_Free };

void Bag__Destroy(Bag* self)
{
    if (self->f_items) { List_int_Free(self->f_items); self->f_items = NULL; }
}

void Bag_Free(Bag* self)
{
    if (!self) return;
    Bag* _o = (Bag*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Bag_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Bag* Bag_Retain(Bag* self)
{
    if (self) ((Bag*)self)->_rc++;
    return self;
}

int Bag_get_Count(Bag* self)
{
    CS2SX_TRACE("Bag.get_Count", "Program.cs");
    CS2SX_LINE(13);
    return CS2SX_NN(self->f_items)->count;
}

int Bag_get(Bag* self, int i)
{
    CS2SX_LINE(11);
    return List_int_Get(self->f_items, i);
}

void Bag_set(Bag* self, int i, int value)
{
    CS2SX_LINE(11);
    List_int_Set(self->f_items, i, value);
}

void Grid_Ctor(Grid* self)
{
    CS2SX_TRACE("Grid..ctor", "Program.cs");
    self->f_cells = Dict_str_str_Retain(Dict_str_str_New());
}

Grid* Grid_New(void)
{
    Grid* self = (Grid*)calloc(1, sizeof(Grid));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Grid__Destroy;
    self->_gc = Grid_Gc;
    self->_class = &Grid_ClassInfo;
    Grid_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Grid_Free);
    return self;
}

const CS2SX_ClassInfo Grid_ClassInfo = { "Grid", NULL, NULL };

void Grid_Gc(void* _self, CS2SX_GcVisit visit)
{
    Grid* self = (Grid*)_self; (void)self;
}

static void Grid_GcDyn(void* self, CS2SX_GcVisit visit) { ((Grid*)self)->_gc(self, visit); }
const CS2SX_GcKind Grid_GcKind = { offsetof(Grid, _rc), Grid_GcDyn, (CS2SX_FreeFn)Grid_Free };

void Grid__Destroy(Grid* self)
{
    if (self->f_cells) { Dict_str_str_Free(self->f_cells); self->f_cells = NULL; }
}

void Grid_Free(Grid* self)
{
    if (!self) return;
    Grid* _o = (Grid*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Grid_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Grid* Grid_Retain(Grid* self)
{
    if (self) ((Grid*)self)->_rc++;
    return self;
}

const char* Grid_get(Grid* self, const char* key)
{
    return (Dict_str_str_ContainsKey(self->f_cells, key) ? Dict_str_str_Get(self->f_cells, key) : "-");
}

void Grid_set(Grid* self, const char* key, const char* value)
{
    Dict_str_str_Set(self->f_cells, key, value);
}

void BigBag_Ctor(BigBag* self)
{
    CS2SX_TRACE("BigBag..ctor", "Program.cs");
    Bag_Ctor((Bag*)self);
}

BigBag* BigBag_New(void)
{
    BigBag* self = (BigBag*)calloc(1, sizeof(BigBag));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))BigBag__Destroy;
    self->base._gc = BigBag_Gc;
    self->base._class = &BigBag_ClassInfo;
    BigBag_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)BigBag_Free);
    return self;
}

const CS2SX_ClassInfo BigBag_ClassInfo = { "BigBag", &Bag_ClassInfo, NULL };

void BigBag_Gc(void* _self, CS2SX_GcVisit visit)
{
    BigBag* self = (BigBag*)_self; (void)self;
    Bag_Gc(_self, visit);
}

void BigBag__Destroy(BigBag* self)
{
    Bag__Destroy((Bag*)self);
}

void BigBag_Free(BigBag* self)
{
    if (!self) return;
    Bag* _o = (Bag*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Bag_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

BigBag* BigBag_Retain(BigBag* self)
{
    if (self) ((Bag*)self)->_rc++;
    return self;
}

void IndexersAndOutApp_Init(IndexersAndOutApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))IndexersAndOutApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))IndexersAndOutApp_OnFrame;
    self->f_field = CS2SX_RETAIN(Bag, Bag_New());
}

int IndexersAndOutApp_Mine(IndexersAndOutApp* self, const char** s)
{
    CS2SX_TRACE("IndexersAndOutApp.Mine", "Program.cs");
    CS2SX_LINE(28);
    *s = "mine";
    CS2SX_LINE(28);
    return 1;
}

void IndexersAndOutApp_OnInit(IndexersAndOutApp* self)
{
    CS2SX_TRACE("IndexersAndOutApp.OnInit", "Program.cs");
    CS2SX_LINE(32);
    Bag* b = Bag_New();
    CS2SX_LINE(33);
    Bag_Add(b, 4);
    CS2SX_LINE(33);
    Bag_Add(b, 5);
    CS2SX_LINE(34);
    Bag_set(((Bag*)b), 1, 9);
    CS2SX_LINE(35);
    ({ __auto_type _cs2sx_c = ((Bag*)b); __auto_type _cs2sx_k = 0; Bag_set(_cs2sx_c, _cs2sx_k, Bag_get(_cs2sx_c, _cs2sx_k) + (1)); });
    CS2SX_LINE(36);
    Bag_Add(CS2SX_NN(self->f_field), 3);
    CS2SX_LINE(37);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "index %d %d %d %d", Bag_get((Bag*)b, 0), Bag_get((Bag*)b, 1), Bag_get_Count(b), Bag_get((Bag*)self->f_field, 0));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(38);
    Grid* _grid_1 = Grid_New();
    Grid_set((Grid*)_grid_1, "a", "x");
    Grid* g = _grid_1;
    CS2SX_LINE(39);
    Grid_set(((Grid*)g), "b", "y");
    CS2SX_LINE(40);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "grid %s%s%s", Grid_get((Grid*)g, "a"), Grid_get((Grid*)g, "b"), Grid_get((Grid*)g, "c"));
    const char* _ps_2 = _sb1;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(41);
    BigBag* bb = BigBag_New();
    CS2SX_LINE(42);
    Bag_Add((Bag*)bb, 7);
    CS2SX_LINE(43);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "derived %d", Bag_get((Bag*)bb, 0));
    const char* _ps_3 = _sb2;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(45);
    int x = 0;
    CS2SX_LINE(46);
    Bag_TryFirst(b, &x);
    CS2SX_LINE(47);
    int first = {0};
    int ok = Bag_TryFirst(b, &first);
    CS2SX_LINE(48);
    int f = {0};
    if (Bag_TryFirst(b, &f))
    {
        CS2SX_LINE(48);
        char* _sb3 = _cs2sx_next_buf();
        _sb3 = CS2SX_Format(_sb3, "out %d %d %s %d", x, first, CS2SX_BoolStr(ok), f);
        const char* _ps_4 = _sb3;
        printf("%s\n", _ps_4 ? _ps_4 : "");
    }
    CS2SX_LINE(49);
    int p = 1;
    int q = 2;
    CS2SX_LINE(50);
    Bag_Swap(b, &p, &q);
    CS2SX_LINE(51);
    const char* s = {0};
    if (IndexersAndOutApp_Mine(self, &s))
    {
        CS2SX_LINE(51);
        char* _sb4 = _cs2sx_next_buf();
        _sb4 = CS2SX_Format(_sb4, "ref %d%d %s", p, q, s);
        const char* _ps_5 = _sb4;
        printf("%s\n", _ps_5 ? _ps_5 : "");
    }
    CS2SX_LINE(52);
    Item* it = NULL;
    if (Bag_Find(b, "ring", &it) && it != NULL)
    {
        CS2SX_LINE(52);
        char* _sb5 = _cs2sx_next_buf();
        _sb5 = CS2SX_Format(_sb5, "class out %s", CS2SX_NN(it)->f_Name);
        const char* _ps_6 = _sb5;
        printf("%s\n", _ps_6 ? _ps_6 : "");
    }
    CS2SX_LINE(53);
    Item* none = NULL;
    char* _sb6 = _cs2sx_next_buf();
    _sb6 = CS2SX_Format(_sb6, "missing %s %s", CS2SX_BoolStr(Bag_Find(b, "x", &none)), CS2SX_BoolStr((none == NULL)));
    const char* _ps_7 = _sb6;
    printf("%s\n", _ps_7 ? _ps_7 : "");
    CS2SX_LINE(54);
    printf("%s\n", CS2SX_BoolStr(CS2SX_strcmp_safe("a", "b") < 0));
    CS2SX_LINE(56);
    int sum = 0;
    CS2SX_LINE(57);
    for (int i = 0; i < Bag_get_Count(b); i++)
    {
        CS2SX_LINE(57);
        sum += Bag_get((Bag*)b, i);
    }
    CS2SX_LINE(58);
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "for %d", sum);
    const char* _ps_8 = _sb7;
    printf("%s\n", _ps_8 ? _ps_8 : "");
    CS2SX_LINE(59);
    printf("done\n");
}

void IndexersAndOutApp_OnFrame(IndexersAndOutApp* self)
{
    CS2SX_TRACE("IndexersAndOutApp.OnFrame", "Program.cs");
}

void IndexersAndOutApp_Free(IndexersAndOutApp* self)
{
    if (!self) return;
    if (self->f_field) { Bag_Free(self->f_field); self->f_field = NULL; }
    free(self);
}
