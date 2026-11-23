#include <stdlib.h>
#include "_generics.h"
#include "Program.h"
#include "CS2SX_Collections.h"

void Task1_Ctor(Task1* self, const char* name, int weight)
{
    CS2SX_TRACE("Task1..ctor", "Program.cs");
    CS2SX_LINE(10);
    CS2SX_STR_STORE(self->f_Name, name);
    CS2SX_LINE(10);
    self->f_Weight = weight;
}

Task1* Task1_New(const char* name, int weight)
{
    Task1* self = (Task1*)calloc(1, sizeof(Task1));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Task1__Destroy;
    self->_gc = Task1_Gc;
    self->_class = &Task1_ClassInfo;
    Task1_Ctor(self, name, weight);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Task1_Free);
    return self;
}

int Task1_CompareTo(Task1* self, Task1* other)
{
    CS2SX_TRACE("Task1.CompareTo", "Program.cs");
    CS2SX_LINE(11);
    return self->f_Weight - CS2SX_NN(other)->f_Weight;
}

const CS2SX_ClassInfo Task1_ClassInfo = { "Task1", NULL };

void Task1_Gc(void* _self, CS2SX_GcVisit visit)
{
    Task1* self = (Task1*)_self; (void)self;
}

static void Task1_GcDyn(void* self, CS2SX_GcVisit visit) { ((Task1*)self)->_gc(self, visit); }
const CS2SX_GcKind Task1_GcKind = { offsetof(Task1, _rc), Task1_GcDyn, (CS2SX_FreeFn)Task1_Free };

void Task1__Destroy(Task1* self)
{
    CS2SX_StrDrop(self->f_Name); self->f_Name = NULL;
}

void Task1_Free(Task1* self)
{
    if (!self) return;
    Task1* _o = (Task1*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Task1_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Task1* Task1_Retain(Task1* self)
{
    if (self) ((Task1*)self)->_rc++;
    return self;
}

void SortedCollectionsApp_Init(SortedCollectionsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))SortedCollectionsApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))SortedCollectionsApp_OnFrame;
    self->f_scores = CS2SX_RETAIN(CS2SX_SortedDictionary_str_int, CS2SX_SortedDictionary_str_int_New());
}

static int _lambda_0(CS2SX_KV_str_int p)
{
    return p.Value;
}

static int _lambda_1(int x)
{
    return x > 2;
}

void SortedCollectionsApp_OnInit(SortedCollectionsApp* self)
{
    CS2SX_TRACE("SortedCollectionsApp.OnInit", "Program.cs");
    CS2SX_LINE(21);
    CS2SX_SortedDictionary_str_int_set(((CS2SX_SortedDictionary_str_int*)self->f_scores), "mia", 30);
    CS2SX_LINE(21);
    CS2SX_SortedDictionary_str_int_set(((CS2SX_SortedDictionary_str_int*)self->f_scores), "anna", 50);
    CS2SX_LINE(21);
    CS2SX_SortedDictionary_str_int_set(((CS2SX_SortedDictionary_str_int*)self->f_scores), "zoe", 10);
    CS2SX_LINE(22);
    CS2SX_SortedDictionary_str_int_Add(CS2SX_NN(self->f_scores), "ben", 20);
    CS2SX_LINE(23);
    ({ __auto_type _cs2sx_c = ((CS2SX_SortedDictionary_str_int*)self->f_scores); __auto_type _cs2sx_k = "mia"; CS2SX_SortedDictionary_str_int_set(_cs2sx_c, _cs2sx_k, CS2SX_SortedDictionary_str_int_get(_cs2sx_c, _cs2sx_k) + (5)); });
    CS2SX_LINE(24);
    const char* keys = "";
    {
        CS2SX_LINE(25);
        CS2SX_SortedDictionary_str_int* cs2sx_fc1 = self->f_scores;
        CS2SX_LINE(25);
        for (int cs2sx_fi1 = 0; cs2sx_fi1 < CS2SX_SortedDictionary_str_int_get_Count(CS2SX_NN(cs2sx_fc1)); cs2sx_fi1++)
        {
            CS2SX_LINE(25);
            CS2SX_KV_str_int kv = CS2SX_SortedDictionary_str_int_EntryAt(CS2SX_NN(cs2sx_fc1), cs2sx_fi1);
            CS2SX_LINE(25);
            char* _sb0 = _cs2sx_next_buf();
            _sb0 = CS2SX_Format(_sb0, "%s=%d ", kv.Key, kv.Value);
            char* _sb1 = _cs2sx_next_buf();
            _sb1 = CS2SX_Format(_sb1, "%s%s", keys ? keys : "", _sb0 ? _sb0 : "");
            keys = _sb1;
        }
    }
    CS2SX_LINE(26);
    const char* _ps_0 = keys;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(27);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "%s %s %s %d", CS2SX_BoolStr(CS2SX_SortedDictionary_str_int_ContainsKey(CS2SX_NN(self->f_scores), "ben")), CS2SX_BoolStr(CS2SX_SortedDictionary_str_int_Remove(CS2SX_NN(self->f_scores), "ben")), CS2SX_BoolStr(CS2SX_SortedDictionary_str_int_ContainsKey(CS2SX_NN(self->f_scores), "ben")), CS2SX_SortedDictionary_str_int_get_Count(CS2SX_NN(self->f_scores)));
    const char* _ps_1 = _sb2;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(28);
    int z = {0};
    if (CS2SX_SortedDictionary_str_int_TryGetValue(CS2SX_NN(self->f_scores), "zoe", &z))
    {
        CS2SX_LINE(28);
        char* _sb3 = _cs2sx_next_buf();
        _sb3 = CS2SX_Format(_sb3, "zoe %d", z);
        const char* _ps_2 = _sb3;
        printf("%s\n", _ps_2 ? _ps_2 : "");
    }
    CS2SX_LINE(29);
    int nope = {0};
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "missing %s %d", CS2SX_BoolStr(CS2SX_SortedDictionary_str_int_TryGetValue(CS2SX_NN(self->f_scores), "x", &nope)), nope);
    const char* _ps_3 = _sb4;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    {
        CS2SX_LINE(30);
        CS2SX_SortedDictionary_str_int* cs2sx_fc2 = self->f_scores;
        CS2SX_LINE(30);
        for (int cs2sx_fi2 = 0; cs2sx_fi2 < CS2SX_SortedDictionary_str_int_get_Count(CS2SX_NN(cs2sx_fc2)); cs2sx_fi2++)
        {
            CS2SX_LINE(30);
            const char* name = CS2SX_SortedDictionary_str_int_KeyAt(CS2SX_NN(cs2sx_fc2), cs2sx_fi2);
            CS2SX_LINE(30);
            int points = CS2SX_SortedDictionary_str_int_ValueAt(CS2SX_NN(cs2sx_fc2), cs2sx_fi2);
            CS2SX_LINE(30);
            if (points > 40)
            {
                CS2SX_LINE(30);
                char* _sb5 = _cs2sx_next_buf();
                _sb5 = CS2SX_Format(_sb5, "top %s", name);
                const char* _ps_4 = _sb5;
                printf("%s\n", _ps_4 ? _ps_4 : "");
            }
        }
    }
    CS2SX_LINE(31);
    __auto_type _src_5 = CS2SX_SortedDictionary_str_int_ToEntries(CS2SX_NN(self->f_scores));
    List_CS2SX_KV_str_int* _ord_6 = List_CS2SX_KV_str_int_New();
    for (int _i_7 = 0; _i_7 < _src_5->count; _i_7++)
      List_CS2SX_KV_str_int_Add(_ord_6, List_CS2SX_KV_str_int_Get(_src_5, _i_7));
    for (int _i_7 = 1; _i_7 < _ord_6->count; _i_7++)
    {
        CS2SX_KV_str_int _otmp_9 = _ord_6->data[_i_7];
        int _j_8 = _i_7 - 1;
        while (_j_8 >= 0 && (_lambda_0(_ord_6->data[_j_8]) != _lambda_0(_otmp_9) ? (_lambda_0(_ord_6->data[_j_8]) < _lambda_0(_otmp_9)) : 0))
          { _ord_6->data[_j_8+1] = _ord_6->data[_j_8]; _j_8--; }
        _ord_6->data[_j_8+1] = _otmp_9;
    }
    CS2SX_KV_str_int _first_10 = {0};
    if (_ord_6->count > 0) { _first_10 = List_CS2SX_KV_str_int_Get(_ord_6, 0); }
    char* _sb6 = _cs2sx_next_buf();
    _sb6 = CS2SX_Format(_sb6, "keys %s best %s", String_Join(",", CS2SX_SortedDictionary_str_int_get_Keys(CS2SX_NN(self->f_scores))), (_ord_6->count > 0 ? _first_10 : (CS2SX_ThrowNoElements(), _first_10)).Key);
    const char* _ps_11 = _sb6;
    printf("%s\n", _ps_11 ? _ps_11 : "");
    CS2SX_LINE(32);
    CS2SX_SortedDictionary_int_str* lit = CS2SX_SortedDictionary_int_str_Of((int*)CS2SX_ArrFromInit((int[]){ 3, 1, 2 }, 3, sizeof(int)), (const char**)CS2SX_ArrFromInit((const char*[]){ "c", "a", "b" }, 3, sizeof(const char*)));
    CS2SX_LINE(33);
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "lit %s", String_Join("", CS2SX_SortedDictionary_int_str_get_Values(CS2SX_NN(lit))));
    const char* _ps_12 = _sb7;
    printf("%s\n", _ps_12 ? _ps_12 : "");
    CS2SX_LINE(34);
    {
        CS2SX_ExFrame _exf_13;
        CS2SX_ExPush(&_exf_13);
        if (setjmp(_exf_13.buf) == 0)
        {
            CS2SX_LINE(34);
            CS2SX_SortedDictionary_str_int_Add(CS2SX_NN(self->f_scores), "mia", 1);
            CS2SX_ExPop(&_exf_13);
        }
        else
        {
            Exception* _exf_13_ex = _exf_13.ex;
            if (CS2SX_Exc_Is(_exf_13_ex, &CS2SX_TI_ArgumentException))
            {
                CS2SX_LINE(34);
                printf("dup\n");
                Exception_Free(_exf_13_ex);
            }
            else CS2SX_Throw(_exf_13_ex);
        }
    }
    CS2SX_LINE(35);
    {
        CS2SX_ExFrame _exf_14;
        CS2SX_ExPush(&_exf_14);
        if (setjmp(_exf_14.buf) == 0)
        {
            CS2SX_LINE(35);
            printf("%d\n", CS2SX_SortedDictionary_str_int_get((CS2SX_SortedDictionary_str_int*)self->f_scores, "nobody"));
            CS2SX_ExPop(&_exf_14);
        }
        else
        {
            Exception* _exf_14_ex = _exf_14.ex;
            if (CS2SX_Exc_Is(_exf_14_ex, &CS2SX_TI_KeyNotFoundException))
            {
                CS2SX_LINE(35);
                printf("nokey\n");
                Exception_Free(_exf_14_ex);
            }
            else CS2SX_Throw(_exf_14_ex);
        }
    }
    CS2SX_LINE(38);
    CS2SX_SortedSet_int* set = CS2SX_SortedSet_int_Of((int*)CS2SX_ArrFromInit((int[]){ 5, 1, 9, 1, 3 }, 5, sizeof(int)));
    CS2SX_LINE(39);
    char* _sb8 = _cs2sx_next_buf();
    _sb8 = CS2SX_Format(_sb8, "set %d %d %d %s", CS2SX_SortedSet_int_get_Count(CS2SX_NN(set)), CS2SX_SortedSet_int_get_Min(CS2SX_NN(set)), CS2SX_SortedSet_int_get_Max(CS2SX_NN(set)), ({ __auto_type _jl = (CS2SX_SortedSet_int_ToList(CS2SX_NN(set))); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%d", (int)(_jl->data[_ji])); }));
    const char* _ps_15 = _sb8;
    printf("%s\n", _ps_15 ? _ps_15 : "");
    CS2SX_LINE(40);
    char* _sb9 = _cs2sx_next_buf();
    _sb9 = CS2SX_Format(_sb9, "%s %s %s %s", CS2SX_BoolStr(CS2SX_SortedSet_int_Add(CS2SX_NN(set), 4)), CS2SX_BoolStr(CS2SX_SortedSet_int_Add(CS2SX_NN(set), 4)), CS2SX_BoolStr(CS2SX_SortedSet_int_Contains(CS2SX_NN(set), 9)), CS2SX_BoolStr(CS2SX_SortedSet_int_Remove(CS2SX_NN(set), 9)));
    const char* _ps_16 = _sb9;
    printf("%s\n", _ps_16 ? _ps_16 : "");
    CS2SX_LINE(41);
    __auto_type _src_17 = CS2SX_SortedSet_int_ToList(CS2SX_NN(set));
    List_int* _where_18 = List_int_New();
    for (int _i_19 = 0; _i_19 < _src_17->count; _i_19++)
    {
        int _e__where_18 = List_int_Get(_src_17, _i_19);
        if (_lambda_1(_e__where_18)) List_int_Add(_where_18, _e__where_18);
    }
    char* _sb10 = _cs2sx_next_buf();
    _sb10 = CS2SX_Format(_sb10, "where %s", ({ __auto_type _jl = (_where_18); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%d", (int)(_jl->data[_ji])); }));
    const char* _ps_20 = _sb10;
    printf("%s\n", _ps_20 ? _ps_20 : "");
    CS2SX_LINE(42);
    List_str* _list_21 = List_str_New();
    List_str_Add(_list_21, "pear");
    List_str_Add(_list_21, "apple");
    List_str_Add(_list_21, "fig");
    CS2SX_SortedSet_str* words = CS2SX_SortedSet_str_New_1_0(_list_21);
    {
        CS2SX_LINE(43);
        CS2SX_SortedSet_str* cs2sx_fc3 = words;
        CS2SX_LINE(43);
        for (int cs2sx_fi3 = 0; cs2sx_fi3 < CS2SX_SortedSet_str_get_Count(CS2SX_NN(cs2sx_fc3)); cs2sx_fi3++)
        {
            CS2SX_LINE(43);
            const char* w = CS2SX_SortedSet_str_ItemAt(CS2SX_NN(cs2sx_fc3), cs2sx_fi3);
            CS2SX_LINE(43);
            char* _sb11 = _cs2sx_next_buf();
            _sb11 = CS2SX_Format(_sb11, "%s ", w);
            const char* _ps_22 = _sb11;
            printf("%s", _ps_22 ? _ps_22 : "");
        }
    }
    CS2SX_LINE(44);
    printf("\n");
    CS2SX_LINE(47);
    CS2SX_PriorityQueue_str_int* pq = CS2SX_PriorityQueue_str_int_New();
    CS2SX_LINE(48);
    CS2SX_PriorityQueue_str_int_Enqueue(pq, "low", 5);
    CS2SX_LINE(48);
    CS2SX_PriorityQueue_str_int_Enqueue(pq, "urgent", 1);
    CS2SX_LINE(48);
    CS2SX_PriorityQueue_str_int_Enqueue(pq, "mid", 3);
    CS2SX_LINE(48);
    CS2SX_PriorityQueue_str_int_Enqueue(pq, "later", 9);
    CS2SX_LINE(49);
    const char* order = "";
    CS2SX_LINE(50);
    const char* item = {0};
    int prio = {0};
    while (CS2SX_PriorityQueue_str_int_TryDequeue(pq, &item, &prio))
    {
        CS2SX_LINE(50);
        char* _sb12 = _cs2sx_next_buf();
        _sb12 = CS2SX_Format(_sb12, "%s%d ", item, prio);
        char* _sb13 = _cs2sx_next_buf();
        _sb13 = CS2SX_Format(_sb13, "%s%s", order ? order : "", _sb12 ? _sb12 : "");
        order = _sb13;
    }
    CS2SX_LINE(51);
    const char* _ps_23 = order;
    printf("%s\n", _ps_23 ? _ps_23 : "");
    CS2SX_LINE(52);
    {
        CS2SX_ExFrame _exf_24;
        CS2SX_ExPush(&_exf_24);
        if (setjmp(_exf_24.buf) == 0)
        {
            CS2SX_LINE(52);
            CS2SX_PriorityQueue_str_int_Dequeue(pq);
            CS2SX_ExPop(&_exf_24);
        }
        else
        {
            Exception* _exf_24_ex = _exf_24.ex;
            if (CS2SX_Exc_Is(_exf_24_ex, &CS2SX_TI_InvalidOperationException))
            {
                CS2SX_LINE(52);
                printf("empty\n");
                Exception_Free(_exf_24_ex);
            }
            else CS2SX_Throw(_exf_24_ex);
        }
    }
    CS2SX_LINE(53);
    CS2SX_PriorityQueue_str_Task1* jobs = CS2SX_PriorityQueue_str_Task1_New();
    CS2SX_LINE(54);
    CS2SX_PriorityQueue_str_Task1_Enqueue(jobs, "b", Task1_New("b", 7));
    CS2SX_LINE(54);
    CS2SX_PriorityQueue_str_Task1_Enqueue(jobs, "a", Task1_New("a", 2));
    CS2SX_LINE(55);
    char* _sb14 = _cs2sx_next_buf();
    _sb14 = CS2SX_Format(_sb14, "job %s %d", CS2SX_PriorityQueue_str_Task1_Peek(jobs), CS2SX_PriorityQueue_str_Task1_get_Count(jobs));
    const char* _ps_25 = _sb14;
    printf("%s\n", _ps_25 ? _ps_25 : "");
    CS2SX_LINE(58);
    CS2SX_LinkedList_int* list = CS2SX_LinkedList_int_New_0();
    CS2SX_LINE(59);
    CS2SX_LinkedList_int_AddLast_1_int(list, 2);
    CS2SX_LINE(59);
    CS2SX_LinkedList_int_AddLast_1_int(list, 3);
    CS2SX_LINE(59);
    CS2SX_LinkedList_int_AddFirst_1_int(list, 1);
    CS2SX_LINE(60);
    CS2SX_LinkedListNode_int* mid = CS2SX_LinkedList_int_Find(list, 2);
    CS2SX_LINE(61);
    CS2SX_LinkedList_int_AddAfter(list, mid, 25);
    CS2SX_LINE(62);
    CS2SX_LinkedList_int_AddBefore(list, mid, 15);
    CS2SX_LINE(63);
    CS2SX_LinkedList_int_Remove_1_int(list, 3);
    CS2SX_LINE(64);
    const char* walk = "";
    {
        CS2SX_LINE(65);
        CS2SX_LinkedList_int* cs2sx_fc4 = list;
        CS2SX_LINE(65);
        for (CS2SX_LinkedListNode_int* cs2sx_fi4 = CS2SX_LinkedList_int_get_First(CS2SX_NN(cs2sx_fc4)); cs2sx_fi4 != NULL; cs2sx_fi4 = CS2SX_LinkedListNode_int_get_Next(CS2SX_NN(cs2sx_fi4)))
        {
            CS2SX_LINE(65);
            int x = CS2SX_NN(cs2sx_fi4)->f_Value;
            CS2SX_LINE(65);
            char* _sb15 = _cs2sx_next_buf();
            _sb15 = CS2SX_Format(_sb15, "%d ", x);
            char* _sb16 = _cs2sx_next_buf();
            _sb16 = CS2SX_Format(_sb16, "%s%s", walk ? walk : "", _sb15 ? _sb15 : "");
            walk = _sb16;
        }
    }
    CS2SX_LINE(66);
    char* _sb17 = _cs2sx_next_buf();
    _sb17 = CS2SX_Format(_sb17, "%s%d %d %d", walk, CS2SX_LinkedList_int_get_Count(list), CS2SX_NN(CS2SX_LinkedList_int_get_First(list))->f_Value, CS2SX_NN(CS2SX_LinkedList_int_get_Last(list))->f_Value);
    const char* _ps_26 = _sb17;
    printf("%s\n", _ps_26 ? _ps_26 : "");
    CS2SX_LINE(67);
    const char* back = "";
    CS2SX_LINE(68);
    for (CS2SX_LinkedListNode_int* n = CS2SX_LinkedList_int_get_Last(list); n != NULL; n = CS2SX_LinkedListNode_int_get_Previous(CS2SX_NN(n)))
    {
        CS2SX_LINE(68);
        char* _sb18 = _cs2sx_next_buf();
        _sb18 = CS2SX_Format(_sb18, "%s%d", back ? back : "", CS2SX_NN(n)->f_Value);
        back = _sb18;
    }
    CS2SX_LINE(69);
    __auto_type _src_27 = CS2SX_LinkedList_int_ToList(list);
    int _sum_29 = 0;
    for (int _i_28 = 0; _i_28 < _src_27->count; _i_28++)
      _sum_29 += List_int_Get(_src_27, _i_28);
    char* _sb19 = _cs2sx_next_buf();
    _sb19 = CS2SX_Format(_sb19, "back %s %s %d", back, CS2SX_BoolStr(CS2SX_LinkedList_int_Contains(list, 15)), _sum_29);
    const char* _ps_30 = _sb19;
    printf("%s\n", _ps_30 ? _ps_30 : "");
    CS2SX_LINE(70);
    CS2SX_LinkedList_int_RemoveFirst(list);
    CS2SX_LINE(70);
    CS2SX_LinkedList_int_RemoveLast(list);
    CS2SX_LINE(71);
    char* _sb20 = _cs2sx_next_buf();
    _sb20 = CS2SX_Format(_sb20, "left %s", ({ __auto_type _jl = (CS2SX_LinkedList_int_ToList(list)); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%d", (int)(_jl->data[_ji])); }));
    const char* _ps_31 = _sb20;
    printf("%s\n", _ps_31 ? _ps_31 : "");
    CS2SX_LINE(72);
    printf("done\n");
}

void SortedCollectionsApp_OnFrame(SortedCollectionsApp* self)
{
    CS2SX_TRACE("SortedCollectionsApp.OnFrame", "Program.cs");
}

void SortedCollectionsApp_Free(SortedCollectionsApp* self)
{
    if (!self) return;
    if (self->f_scores) { CS2SX_SortedDictionary_str_int_Free(self->f_scores); self->f_scores = NULL; }
    free(self);
}
