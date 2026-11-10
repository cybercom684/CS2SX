#include <stdlib.h>
#include "_generics.h"
#include "_interfaces.h"
#include "Program.h"

void Plain_Ctor(Plain* self)
{
    CS2SX_TRACE("Plain..ctor", "Program.cs");
}

Plain* Plain_New(void)
{
    Plain* self = (Plain*)calloc(1, sizeof(Plain));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Plain_Dispose;
    self->_gc = Plain_Gc;
    Plain_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Plain_Free);
    return self;
}

const char* Plain_Format_int(Plain* self, int value)
{
    CS2SX_TRACE("Plain.Format_int", "Program.cs");
    CS2SX_LINE(22);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d", value);
    return _sb0;
}

const char* Plain_Format_str(Plain* self, const char* value)
{
    CS2SX_TRACE("Plain.Format_str", "Program.cs");
    CS2SX_LINE(25);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%s", value);
    return _sb1;
}

void Plain_Gc(void* _self, CS2SX_GcVisit visit)
{
    Plain* self = (Plain*)_self; (void)self;
}

static void Plain_GcDyn(void* self, CS2SX_GcVisit visit) { ((Plain*)self)->_gc(self, visit); }
const CS2SX_GcKind Plain_GcKind = { offsetof(Plain, _rc), Plain_GcDyn, (CS2SX_FreeFn)Plain_Free };

void Plain_Dispose(Plain* self)
{
}

void Plain_Free(Plain* self)
{
    if (!self) return;
    Plain* _o = (Plain*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Plain_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Plain* Plain_Retain(Plain* self)
{
    if (self) ((Plain*)self)->_rc++;
    return self;
}

IFormatter_vtable Plain_IFormatter_vtable_instance =
{
    .Format_int = (const char* (*)(void*, int))Plain_Format_int,
    .Format_str = (const char* (*)(void*, const char*))Plain_Format_str,
};


void Bracket_Ctor(Bracket* self)
{
    CS2SX_TRACE("Bracket..ctor", "Program.cs");
}

Bracket* Bracket_New(void)
{
    Bracket* self = (Bracket*)calloc(1, sizeof(Bracket));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Bracket_Dispose;
    self->_gc = Bracket_Gc;
    Bracket_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Bracket_Free);
    return self;
}

const char* Bracket_Format_int(Bracket* self, int value)
{
    CS2SX_TRACE("Bracket.Format_int", "Program.cs");
    CS2SX_LINE(33);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "[%d]", value);
    return _sb0;
}

const char* Bracket_Format_str(Bracket* self, const char* value)
{
    CS2SX_TRACE("Bracket.Format_str", "Program.cs");
    CS2SX_LINE(36);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "[%s]", value);
    return _sb1;
}

void Bracket_Gc(void* _self, CS2SX_GcVisit visit)
{
    Bracket* self = (Bracket*)_self; (void)self;
}

static void Bracket_GcDyn(void* self, CS2SX_GcVisit visit) { ((Bracket*)self)->_gc(self, visit); }
const CS2SX_GcKind Bracket_GcKind = { offsetof(Bracket, _rc), Bracket_GcDyn, (CS2SX_FreeFn)Bracket_Free };

void Bracket_Dispose(Bracket* self)
{
}

void Bracket_Free(Bracket* self)
{
    if (!self) return;
    Bracket* _o = (Bracket*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Bracket_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Bracket* Bracket_Retain(Bracket* self)
{
    if (self) ((Bracket*)self)->_rc++;
    return self;
}

IFormatter_vtable Bracket_IFormatter_vtable_instance =
{
    .Format_int = (const char* (*)(void*, int))Bracket_Format_int,
    .Format_str = (const char* (*)(void*, const char*))Bracket_Format_str,
};


static Picker_vtable Picker_vtable_instance =
{
    .Echo_int = (int(*)(void*, int))Picker_Echo_int,
};

void Picker_Ctor(Picker* self)
{
    CS2SX_TRACE("Picker..ctor", "Program.cs");
}

Picker* Picker_New(void)
{
    Picker* self = (Picker*)calloc(1, sizeof(Picker));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Picker_Dispose;
    self->_gc = Picker_Gc;
    self->vtable = &Picker_vtable_instance;
    Picker_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Picker_Free);
    return self;
}

int Picker_Echo_int(Picker* self, int x)
{
    CS2SX_TRACE("Picker.Echo_int", "Program.cs");
    CS2SX_LINE(51);
    return x;
}

void Picker_Gc(void* _self, CS2SX_GcVisit visit)
{
    Picker* self = (Picker*)_self; (void)self;
}

static void Picker_GcDyn(void* self, CS2SX_GcVisit visit) { ((Picker*)self)->_gc(self, visit); }
const CS2SX_GcKind Picker_GcKind = { offsetof(Picker, _rc), Picker_GcDyn, (CS2SX_FreeFn)Picker_Free };

void Picker_Dispose(Picker* self)
{
}

void Picker_Free(Picker* self)
{
    if (!self) return;
    Picker* _o = (Picker*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Picker_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Picker* Picker_Retain(Picker* self)
{
    if (self) ((Picker*)self)->_rc++;
    return self;
}

static Picker_vtable FirstPicker_vtable_instance;

void FirstPicker_Ctor(FirstPicker* self)
{
    CS2SX_TRACE("FirstPicker..ctor", "Program.cs");
    Picker_Ctor((Picker*)self);
}

FirstPicker* FirstPicker_New(void)
{
    FirstPicker* self = (FirstPicker*)calloc(1, sizeof(FirstPicker));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))FirstPicker_Dispose;
    self->base._gc = FirstPicker_Gc;
    self->base.vtable = &FirstPicker_vtable_instance;
    FirstPicker_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)FirstPicker_Free);
    return self;
}

int FirstPicker_Pick_int(FirstPicker* self, int a, int b)
{
    CS2SX_TRACE("FirstPicker.Pick_int", "Program.cs");
    CS2SX_LINE(59);
    return a;
}

const char* FirstPicker_Pick_str(FirstPicker* self, const char* a, const char* b)
{
    CS2SX_TRACE("FirstPicker.Pick_str", "Program.cs");
    CS2SX_LINE(62);
    return a;
}

void FirstPicker_Gc(void* _self, CS2SX_GcVisit visit)
{
    FirstPicker* self = (FirstPicker*)_self; (void)self;
    Picker_Gc(_self, visit);
}

void FirstPicker_Dispose(FirstPicker* self)
{
    Picker_Dispose((Picker*)self);
}

void FirstPicker_Free(FirstPicker* self)
{
    if (!self) return;
    Picker* _o = (Picker*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Picker_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

FirstPicker* FirstPicker_Retain(FirstPicker* self)
{
    if (self) ((Picker*)self)->_rc++;
    return self;
}

static Picker_vtable FirstPicker_vtable_instance =
{
    .Pick_int = (int(*)(void*, int, int))FirstPicker_Pick_int,
    .Pick_str = (const char*(*)(void*, const char*, const char*))FirstPicker_Pick_str,
    .Echo_int = (int(*)(void*, int))Picker_Echo_int,
};

static Picker_vtable SecondPicker_vtable_instance;

void SecondPicker_Ctor(SecondPicker* self)
{
    CS2SX_TRACE("SecondPicker..ctor", "Program.cs");
    Picker_Ctor((Picker*)self);
}

SecondPicker* SecondPicker_New(void)
{
    SecondPicker* self = (SecondPicker*)calloc(1, sizeof(SecondPicker));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))SecondPicker_Dispose;
    self->base._gc = SecondPicker_Gc;
    self->base.vtable = &SecondPicker_vtable_instance;
    SecondPicker_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)SecondPicker_Free);
    return self;
}

int SecondPicker_Pick_int(SecondPicker* self, int a, int b)
{
    CS2SX_TRACE("SecondPicker.Pick_int", "Program.cs");
    CS2SX_LINE(71);
    return b;
}

const char* SecondPicker_Pick_str(SecondPicker* self, const char* a, const char* b)
{
    CS2SX_TRACE("SecondPicker.Pick_str", "Program.cs");
    CS2SX_LINE(74);
    return b;
}

int SecondPicker_Echo_int(SecondPicker* self, int x)
{
    CS2SX_TRACE("SecondPicker.Echo_int", "Program.cs");
    CS2SX_LINE(77);
    return self->base.vtable->Pick_int(self, x, x);
}

void SecondPicker_Gc(void* _self, CS2SX_GcVisit visit)
{
    SecondPicker* self = (SecondPicker*)_self; (void)self;
    Picker_Gc(_self, visit);
}

void SecondPicker_Dispose(SecondPicker* self)
{
    Picker_Dispose((Picker*)self);
}

void SecondPicker_Free(SecondPicker* self)
{
    if (!self) return;
    Picker* _o = (Picker*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Picker_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

SecondPicker* SecondPicker_Retain(SecondPicker* self)
{
    if (self) ((Picker*)self)->_rc++;
    return self;
}

static Picker_vtable SecondPicker_vtable_instance =
{
    .Pick_int = (int(*)(void*, int, int))SecondPicker_Pick_int,
    .Pick_str = (const char*(*)(void*, const char*, const char*))SecondPicker_Pick_str,
    .Echo_int = (int(*)(void*, int))SecondPicker_Echo_int,
};

// ── Struct: Pair_int ────────────────────────────────────────
Pair_int Pair_int_New(int a, int b)
{
    Pair_int _self_value = {0};
    Pair_int* self = &_self_value;
    CS2SX_LINE(92);
    self->A = a;
    CS2SX_LINE(92);
    self->B = b;
    return _self_value;
}

Pair_int Pair_int_Swapped(Pair_int* self)
{
    return Pair_int_New(self->B, self->A);
}

int Pair_int_Equals(Pair_int a, Pair_int b)
{
    return memcmp(&a, &b, sizeof(Pair_int)) == 0;
}

// ── Struct: Pair_str ────────────────────────────────────────
Pair_str Pair_str_New(const char* a, const char* b)
{
    Pair_str _self_value = {0};
    Pair_str* self = &_self_value;
    CS2SX_LINE(101);
    self->A = _cs2sx_heap_strdup(a);
    CS2SX_LINE(101);
    self->B = _cs2sx_heap_strdup(b);
    return _self_value;
}

Pair_str Pair_str_Swapped(Pair_str* self)
{
    return Pair_str_New(self->B, self->A);
}

int Pair_str_Equals(Pair_str a, Pair_str b)
{
    return memcmp(&a, &b, sizeof(Pair_str)) == 0;
}

// ── Struct: Slot_str_int ────────────────────────────────────────
int Slot_str_int_Equals(Slot_str_int a, Slot_str_int b)
{
    return memcmp(&a, &b, sizeof(Slot_str_int)) == 0;
}

void GenericTypesApp_Init(GenericTypesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))GenericTypesApp_OnInit;
}

void GenericTypesApp_OnInit(GenericTypesApp* self)
{
    CS2SX_TRACE("GenericTypesApp.OnInit", "Program.cs");
    CS2SX_LINE(123);
    List_IFormatter* _list_0 = List_IFormatter_New();
    List_IFormatter_Add(_list_0, Plain_as_IFormatter(CS2SX_RETAIN(Plain, Plain_New())));
    List_IFormatter_Add(_list_0, Bracket_as_IFormatter(CS2SX_RETAIN(Bracket, Bracket_New())));
    List_IFormatter* formatters = _list_0;
    CS2SX_LINE(124);
    for (int _i_f = 0; _i_f < (int)(formatters->count); _i_f++)
    {
        IFormatter* f = List_IFormatter_Get(formatters, _i_f);
        char* _sb0 = _cs2sx_next_buf();
        snprintf(_sb0, CS2SX_STRBUF_SIZE, "fmt %s %s", ({ __auto_type _cs2sx_r = CS2SX_NN(f); _cs2sx_r->vtable->Format_int(_cs2sx_r->obj, 7); }), ({ __auto_type _cs2sx_r = CS2SX_NN(f); _cs2sx_r->vtable->Format_str(_cs2sx_r->obj, "x"); }));
        const char* _ps_1 = _sb0;
        printf("%s\n", _ps_1 ? _ps_1 : "");
    }
    CS2SX_LINE(127);
    Picker* p1 = (Picker*)(FirstPicker_New());
    CS2SX_LINE(128);
    Picker* p2 = (Picker*)(SecondPicker_New());
    CS2SX_LINE(129);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "pick %d %d %s", p1->vtable->Pick_int(p1, 1, 2), p2->vtable->Pick_int(p2, 1, 2), p2->vtable->Pick_str(p2, "a", "b"));
    const char* _ps_2 = _sb1;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(130);
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "echo %d %d", p1->vtable->Echo_int(p1, 5), p2->vtable->Echo_int(p2, 6));
    const char* _ps_3 = _sb2;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(132);
    Pair_int pi = Pair_int_New(3, 4);
    CS2SX_LINE(133);
    Pair_str ps = ({ Pair_str _sv_4 = Pair_str_New("l", "r"); Pair_str_Swapped(&_sv_4); });
    CS2SX_LINE(134);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "pair %d%d %s%s %d", pi.A, pi.B, ps.A, ps.B, Pair_int_Swapped(&pi).A);
    const char* _ps_5 = _sb3;
    printf("%s\n", _ps_5 ? _ps_5 : "");
    CS2SX_LINE(136);
    Slot_str_int slot = (Slot_str_int){ .Key = "hp", .Value = 30 };
    CS2SX_LINE(137);
    char* _sb4 = _cs2sx_next_buf();
    snprintf(_sb4, CS2SX_STRBUF_SIZE, "slot %s=%d", slot.Key, slot.Value);
    const char* _ps_6 = _sb4;
    printf("%s\n", _ps_6 ? _ps_6 : "");
}

void GenericTypesApp_Free(GenericTypesApp* self)
{
    if (!self) return;
    free(self);
}
