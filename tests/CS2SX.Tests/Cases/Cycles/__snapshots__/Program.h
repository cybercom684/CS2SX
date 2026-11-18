#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct TreeNode
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    const char* f_Name;
    TreeNode* f_Parent;
    List_TreeNode* f_Children;
};

extern int TreeNode_Alive;
TreeNode* TreeNode_New(const char* name);
void TreeNode_Ctor(TreeNode* self, const char* name);
TreeNode* TreeNode_Add(TreeNode* self, const char* name);

void TreeNode__Destroy(TreeNode* self);
extern const CS2SX_ClassInfo TreeNode_ClassInfo;
void TreeNode_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind TreeNode_GcKind;
void TreeNode_Free(TreeNode* self);
TreeNode* TreeNode_Retain(TreeNode* self);
struct Link
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Link* f_Next;
};

extern int Link_Alive;
Link* Link_New(void);
void Link_Ctor(Link* self);

void Link__Destroy(Link* self);
extern const CS2SX_ClassInfo Link_ClassInfo;
void Link_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Link_GcKind;
void Link_Free(Link* self);
Link* Link_Retain(Link* self);
struct Boss
{
    Link base;
    Link* f_Minion;
};

Boss* Boss_New(void);
void Boss_Ctor(Boss* self);

void Boss__Destroy(Boss* self);
extern const CS2SX_ClassInfo Boss_ClassInfo;
void Boss_Gc(void* self, CS2SX_GcVisit visit);
void Boss_Free(Boss* self);
Boss* Boss_Retain(Boss* self);
struct CyclesApp
{
    SwitchApp base;
    TreeNode* f_tree;
    Link* f_ring;
    long long f_base;
    int f_frame;
};

void CyclesApp_Init(CyclesApp* self);
TreeNode* CyclesApp_BuildTree();
Link* CyclesApp_BuildRing(int n);
void CyclesApp_OnFrame(CyclesApp* self);

void CyclesApp_Free(CyclesApp* self);


#endif
