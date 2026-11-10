#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

int TreeNode_Alive;
void TreeNode_Ctor(TreeNode* self, const char* name)
{
    CS2SX_TRACE("TreeNode..ctor", "Program.cs");
    self->f_Children = List_TreeNode_Retain(List_TreeNode_New());
    CS2SX_LINE(14);
    CS2SX_STR_STORE(self->f_Name, name);
    CS2SX_LINE(14);
    TreeNode_Alive++;
}

TreeNode* TreeNode_New(const char* name)
{
    TreeNode* self = (TreeNode*)calloc(1, sizeof(TreeNode));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))TreeNode_Dispose;
    self->_gc = TreeNode_Gc;
    TreeNode_Ctor(self, name);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)TreeNode_Free);
    return self;
}

TreeNode* TreeNode_Add(TreeNode* self, const char* name)
{
    CS2SX_TRACE("TreeNode.Add", "Program.cs");
    CS2SX_LINE(19);
    TreeNode* c = TreeNode_New(name);
    CS2SX_LINE(20);
    CS2SX_ARC_STORE(TreeNode, c->f_Parent, CS2SX_RETAIN(TreeNode, self));
    CS2SX_LINE(21);
    List_TreeNode_Add(self->f_Children, c);
    CS2SX_LINE(22);
    return c;
}

void TreeNode_Gc(void* _self, CS2SX_GcVisit visit)
{
    TreeNode* self = (TreeNode*)_self; (void)self;
    if (visit) visit(self->f_Parent, &TreeNode_GcKind);
    else if (self->f_Parent) { void* _o = self->f_Parent; self->f_Parent = NULL; ((void (*)(void*))TreeNode_Free)(_o); }
    if (visit) visit(self->f_Children, &List_TreeNode_GcKind);
    else if (self->f_Children) { void* _o = self->f_Children; self->f_Children = NULL; ((void (*)(void*))List_TreeNode_Free)(_o); }
}

static void TreeNode_GcDyn(void* self, CS2SX_GcVisit visit) { ((TreeNode*)self)->_gc(self, visit); }
const CS2SX_GcKind TreeNode_GcKind = { offsetof(TreeNode, _rc), TreeNode_GcDyn, (CS2SX_FreeFn)TreeNode_Free };

void TreeNode_Dispose(TreeNode* self)
{
    CS2SX_LINE(15);
    TreeNode_Alive--;
    CS2SX_StrDrop(self->f_Name); self->f_Name = NULL;
    if (self->f_Parent) { TreeNode_Free(self->f_Parent); self->f_Parent = NULL; }
}

void TreeNode_Free(TreeNode* self)
{
    if (!self) return;
    TreeNode* _o = (TreeNode*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &TreeNode_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

TreeNode* TreeNode_Retain(TreeNode* self)
{
    if (self) ((TreeNode*)self)->_rc++;
    return self;
}

int Link_Alive;
void Link_Ctor(Link* self)
{
    CS2SX_TRACE("Link..ctor", "Program.cs");
    CS2SX_LINE(30);
    Link_Alive++;
}

Link* Link_New(void)
{
    Link* self = (Link*)calloc(1, sizeof(Link));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Link_Dispose;
    self->_gc = Link_Gc;
    Link_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Link_Free);
    return self;
}

void Link_Gc(void* _self, CS2SX_GcVisit visit)
{
    Link* self = (Link*)_self; (void)self;
    if (visit) visit(self->f_Next, &Link_GcKind);
    else if (self->f_Next) { void* _o = self->f_Next; self->f_Next = NULL; ((void (*)(void*))Link_Free)(_o); }
}

static void Link_GcDyn(void* self, CS2SX_GcVisit visit) { ((Link*)self)->_gc(self, visit); }
const CS2SX_GcKind Link_GcKind = { offsetof(Link, _rc), Link_GcDyn, (CS2SX_FreeFn)Link_Free };

void Link_Dispose(Link* self)
{
    CS2SX_LINE(31);
    Link_Alive--;
    if (self->f_Next) { Link_Free((Link*)self->f_Next); self->f_Next = NULL; }
}

void Link_Free(Link* self)
{
    if (!self) return;
    Link* _o = (Link*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Link_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Link* Link_Retain(Link* self)
{
    if (self) ((Link*)self)->_rc++;
    return self;
}

void Boss_Ctor(Boss* self)
{
    CS2SX_TRACE("Boss..ctor", "Program.cs");
    Link_Ctor((Link*)self);
}

Boss* Boss_New(void)
{
    Boss* self = (Boss*)calloc(1, sizeof(Boss));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Boss_Dispose;
    self->base._gc = Boss_Gc;
    Boss_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Boss_Free);
    return self;
}

void Boss_Gc(void* _self, CS2SX_GcVisit visit)
{
    Boss* self = (Boss*)_self; (void)self;
    if (visit) visit(self->f_Minion, &Link_GcKind);
    else if (self->f_Minion) { void* _o = self->f_Minion; self->f_Minion = NULL; ((void (*)(void*))Link_Free)(_o); }
    Link_Gc(_self, visit);
}

void Boss_Dispose(Boss* self)
{
    if (self->f_Minion) { Link_Free(self->f_Minion); self->f_Minion = NULL; }
    Link_Dispose((Link*)self);
}

void Boss_Free(Boss* self)
{
    if (!self) return;
    Link* _o = (Link*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Link_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Boss* Boss_Retain(Boss* self)
{
    if (self) ((Link*)self)->_rc++;
    return self;
}

void CyclesApp_Init(CyclesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))CyclesApp_OnFrame;
}

TreeNode* CyclesApp_BuildTree()
{
    CS2SX_TRACE("CyclesApp.BuildTree", "Program.cs");
    CS2SX_LINE(48);
    TreeNode* root = TreeNode_New("root");
    CS2SX_LINE(49);
    TreeNode* a = TreeNode_Add(root, "a");
    CS2SX_LINE(50);
    TreeNode_Add(CS2SX_NN(a), "a1");
    CS2SX_LINE(51);
    TreeNode_Add(CS2SX_NN(a), "a2");
    CS2SX_LINE(52);
    TreeNode_Add(root, "b");
    CS2SX_LINE(53);
    return root;
}

Link* CyclesApp_BuildRing(int n)
{
    CS2SX_TRACE("CyclesApp.BuildRing", "Program.cs");
    CS2SX_LINE(58);
    Link* first = Link_New();
    CS2SX_LINE(59);
    Link* cur = first;
    CS2SX_LINE(60);
    for (int i = 1; i < n; i++)
    {
        CS2SX_LINE(62);
        Link* next = Link_New();
        CS2SX_LINE(63);
        CS2SX_ARC_STORE(Link, CS2SX_NN(cur)->f_Next, CS2SX_RETAIN(Link, next));
        CS2SX_LINE(64);
        cur = next;
    }
    CS2SX_LINE(66);
    CS2SX_ARC_STORE(Link, CS2SX_NN(cur)->f_Next, CS2SX_RETAIN(Link, first));
    CS2SX_LINE(67);
    return first;
}

void CyclesApp_OnFrame(CyclesApp* self)
{
    CS2SX_TRACE("CyclesApp.OnFrame", "Program.cs");
    CS2SX_LINE(72);
    self->f_frame++;
    CS2SX_LINE(75);
    if (self->f_frame > 40)
    {
        CS2SX_LINE(77);
        CyclesApp_BuildTree();
        CS2SX_LINE(78);
        CyclesApp_BuildRing(3);
    }
    CS2SX_LINE(81);
    switch (self->f_frame)
    {
        case 1:
            CS2SX_LINE(84);
            CS2SX_ARC_STORE(TreeNode, self->f_tree, CS2SX_RETAIN(TreeNode, CyclesApp_BuildTree()));
            CS2SX_LINE(85);
            CS2SX_ARC_STORE(Link, self->f_ring, CS2SX_RETAIN(Link, CyclesApp_BuildRing(4)));
            CS2SX_LINE(86);
            Link* loop = Link_New();
            CS2SX_LINE(87);
            CS2SX_ARC_STORE(Link, loop->f_Next, CS2SX_RETAIN(Link, loop));
            CS2SX_LINE(88);
            Boss* boss = Boss_New();
            CS2SX_LINE(89);
            CS2SX_ARC_STORE(Link, boss->f_Minion, CS2SX_RETAIN(Link, Link_New()));
            CS2SX_LINE(90);
            CS2SX_ARC_STORE(Link, CS2SX_NN(boss->f_Minion)->f_Next, CS2SX_RETAIN(Link, boss));
            CS2SX_LINE(91);
            char* _sb0 = _cs2sx_next_buf();
            snprintf(_sb0, CS2SX_STRBUF_SIZE, "built %d %d", TreeNode_Alive, Link_Alive);
            const char* _ps_0 = _sb0;
            printf("%s\n", _ps_0 ? _ps_0 : "");
            CS2SX_LINE(92);
            break;
        case 20:
            CS2SX_LINE(95);
            char* _sb1 = _cs2sx_next_buf();
            snprintf(_sb1, CS2SX_STRBUF_SIZE, "kept %d %d %s", TreeNode_Alive, Link_Alive, CS2SX_NN(CS2SX_NN(CS2SX_NN(List_TreeNode_Get(CS2SX_NN(List_TreeNode_Get(CS2SX_NN(self->f_tree)->f_Children, 0))->f_Children, 1))->f_Parent)->f_Parent)->f_Name);
            const char* _ps_1 = _sb1;
            printf("%s\n", _ps_1 ? _ps_1 : "");
            CS2SX_LINE(97);
            CS2SX_ARC_STORE(TreeNode, self->f_tree, NULL);
            CS2SX_LINE(98);
            Link* keep = CS2SX_NN(self->f_ring)->f_Next;
            CS2SX_LINE(99);
            CS2SX_ARC_STORE(Link, self->f_ring, CS2SX_RETAIN(Link, keep));
            CS2SX_LINE(100);
            break;
        case 40:
            CS2SX_LINE(102);
            char* _sb2 = _cs2sx_next_buf();
            snprintf(_sb2, CS2SX_STRBUF_SIZE, "dropped %d %d", TreeNode_Alive, Link_Alive);
            const char* _ps_2 = _sb2;
            printf("%s\n", _ps_2 ? _ps_2 : "");
            CS2SX_LINE(103);
            CS2SX_ARC_STORE(Link, self->f_ring, NULL);
            CS2SX_LINE(104);
            ((void)CS2SX_GcCollect());
            CS2SX_LINE(105);
            char* _sb3 = _cs2sx_next_buf();
            snprintf(_sb3, CS2SX_STRBUF_SIZE, "collect %d", Link_Alive);
            const char* _ps_3 = _sb3;
            printf("%s\n", _ps_3 ? _ps_3 : "");
            CS2SX_LINE(106);
            break;
        case 60:
            CS2SX_LINE(108);
            self->f_base = CS2SX_HeapUsed();
            CS2SX_LINE(109);
            break;
        case 300:
            CS2SX_LINE(111);
            long long grown = CS2SX_HeapUsed() - self->f_base;
            CS2SX_LINE(112);
            char* _sb4 = _cs2sx_next_buf();
            snprintf(_sb4, CS2SX_STRBUF_SIZE, "stable %s %s %s", CS2SX_BoolStr((grown < 4096)), CS2SX_BoolStr((TreeNode_Alive < 200)), CS2SX_BoolStr((Link_Alive < 200)));
            const char* _ps_4 = _sb4;
            printf("%s\n", _ps_4 ? _ps_4 : "");
            CS2SX_LINE(113);
            break;
        case 302:
            CS2SX_LINE(115);
            printf("done\n");
            CS2SX_LINE(116);
            break;
    }
}

void CyclesApp_Free(CyclesApp* self)
{
    if (!self) return;
    if (self->f_tree) { TreeNode_Free(self->f_tree); self->f_tree = NULL; }
    if (self->f_ring) { Link_Free(self->f_ring); self->f_ring = NULL; }
    free(self);
}
