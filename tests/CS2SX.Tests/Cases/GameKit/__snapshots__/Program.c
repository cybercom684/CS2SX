#include <stdlib.h>
#include "Program.h"

void Score_Ctor(Score* self)
{
    CS2SX_TRACE("Score..ctor", "Program.cs");
}

Score* Score_New(void)
{
    Score* self = (Score*)calloc(1, sizeof(Score));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Score__Destroy;
    self->_gc = Score_Gc;
    self->_class = &Score_ClassInfo;
    Score_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Score_Free);
    return self;
}

const CS2SX_ClassInfo Score_ClassInfo = { "Score", NULL, NULL };

void Score_Gc(void* _self, CS2SX_GcVisit visit)
{
    Score* self = (Score*)_self; (void)self;
}

static void Score_GcDyn(void* self, CS2SX_GcVisit visit) { ((Score*)self)->_gc(self, visit); }
const CS2SX_GcKind Score_GcKind = { offsetof(Score, _rc), Score_GcDyn, (CS2SX_FreeFn)Score_Free };

void Score__Destroy(Score* self)
{
}

void Score_Free(Score* self)
{
    if (!self) return;
    Score* _o = (Score*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Score_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Score* Score_Retain(Score* self)
{
    if (self) ((Score*)self)->_rc++;
    return self;
}

int Score_get_Points(Score* self)
{
    return self->f_points;
}

void Score_set_Points(Score* self, int value)
{
    CS2SX_TRACE("Score.set_Points", "Program.cs");
    CS2SX_LINE(15);
    self->f_points = (value < 0 ? 0 : value);
    CS2SX_LINE(15);
    self->f_Changes++;
}

void GameKitApp_Init(GameKitApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))GameKitApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))GameKitApp_OnFrame;
}

void GameKitApp_OnInit(GameKitApp* self)
{
    CS2SX_TRACE("GameKitApp.OnInit", "Program.cs");
    CS2SX_LINE(27);
    const char* path = "/switch/cs2sx_gamekit.dat";
    CS2SX_LINE(28);
    {
        BinaryWriter* w = BinaryWriter_New(CS2SX_File_Create(path));
        {
            CS2SX_LINE(30);
            BinaryWriter_WriteInt32(CS2SX_NN(w), 42);
            CS2SX_LINE(30);
            BinaryWriter_WriteBoolean(CS2SX_NN(w), 1);
            CS2SX_LINE(30);
            BinaryWriter_WriteSingle(CS2SX_NN(w), 1.5f);
            CS2SX_LINE(30);
            BinaryWriter_WriteString(CS2SX_NN(w), "hallo");
            CS2SX_LINE(30);
            BinaryWriter_WriteByte(CS2SX_NN(w), ((unsigned char)7));
            CS2SX_LINE(30);
            BinaryWriter_WriteInt64(CS2SX_NN(w), 123456789012LL);
            CS2SX_LINE(31);
            BinaryWriter_WriteBytes(CS2SX_NN(w), (unsigned char*)CS2SX_ArrFromInit((unsigned char[]){ 1, 2, 3 }, 3, sizeof(unsigned char)));
        }
        if (w) BinaryWriter_Dispose(w);
    }
    CS2SX_LINE(33);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "size %d", CS2SX_ArrLen(CS2SX_NN(CS2SX_File_ReadAllBytes(path))));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(34);
    {
        BinaryReader* r = BinaryReader_New(CS2SX_File_OpenRead(path));
        {
            CS2SX_LINE(36);
            char* _sb1 = _cs2sx_next_buf();
            _sb1 = CS2SX_Format(_sb1, "%d %s %s %s %u %lld", BinaryReader_ReadInt32(CS2SX_NN(r)), CS2SX_BoolStr(BinaryReader_ReadBoolean(CS2SX_NN(r))), Float_ToString(BinaryReader_ReadSingle(CS2SX_NN(r))), BinaryReader_ReadString(CS2SX_NN(r)), BinaryReader_ReadByte(CS2SX_NN(r)), BinaryReader_ReadInt64(CS2SX_NN(r)));
            const char* _ps_1 = _sb1;
            printf("%s\n", _ps_1 ? _ps_1 : "");
            CS2SX_LINE(38);
            unsigned char* rest = BinaryReader_ReadBytes(CS2SX_NN(r), 10);
            CS2SX_LINE(39);
            char* _sb2 = _cs2sx_next_buf();
            _sb2 = CS2SX_Format(_sb2, "rest %d %u %lld", CS2SX_ArrLen(CS2SX_NN(rest)), CS2SX_AT(rest, 2), Stream_get_Position(CS2SX_NN(CS2SX_NN(r)->f_BaseStream)));
            const char* _ps_2 = _sb2;
            printf("%s\n", _ps_2 ? _ps_2 : "");
        }
        if (r) BinaryReader_Dispose(r);
    }
    CS2SX_LINE(41);
    MemoryStream* ms = MemoryStream_New_0();
    CS2SX_LINE(42);
    BinaryWriter* mw = BinaryWriter_New(ms);
    CS2SX_LINE(43);
    BinaryWriter_WriteInt16(mw, ((short)-2));
    CS2SX_LINE(43);
    BinaryWriter_WriteDouble(mw, 3.25);
    CS2SX_LINE(44);
    BinaryReader* mr = BinaryReader_New(MemoryStream_New_1(MemoryStream_ToArray(ms)));
    CS2SX_LINE(45);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "mem %d %s", BinaryReader_ReadInt16(mr), Double_ToString(BinaryReader_ReadDouble(mr)));
    const char* _ps_3 = _sb3;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(46);
    Stream_set_Position(CS2SX_NN(mr->f_BaseStream), 0);
    CS2SX_LINE(47);
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "again %d", BinaryReader_ReadInt16(mr));
    const char* _ps_4 = _sb4;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(48);
    CS2SX_File_WriteAllBytes(path, (unsigned char*)CS2SX_ArrFromInit((unsigned char[]){ 9, 8 }, 2, sizeof(unsigned char)));
    CS2SX_LINE(49);
    CS2SX_File_AppendAllBytes(path, (unsigned char*)CS2SX_ArrFromInit((unsigned char[]){ 7 }, 1, sizeof(unsigned char)));
    CS2SX_LINE(50);
    char* _sb5 = _cs2sx_next_buf();
    _sb5 = CS2SX_Format(_sb5, "bytes %d", CS2SX_ArrLen(CS2SX_NN(CS2SX_File_ReadAllBytes(path))));
    const char* _ps_5 = _sb5;
    printf("%s\n", _ps_5 ? _ps_5 : "");
    CS2SX_LINE(51);
    {
        CS2SX_ExFrame _exf_6;
        CS2SX_ExPush(&_exf_6);
        if (setjmp(_exf_6.buf) == 0)
        {
            CS2SX_LINE(51);
            CS2SX_File_ReadAllBytes("/switch/cs2sx_missing.bin");
            CS2SX_ExPop(&_exf_6);
        }
        else
        {
            Exception* _exf_6_ex = _exf_6.ex;
            if (CS2SX_Exc_Is(_exf_6_ex, &CS2SX_TI_FileNotFoundException))
            {
                CS2SX_LINE(52);
                printf("missing ok\n");
                Exception_Free(_exf_6_ex);
            }
            else CS2SX_Throw(_exf_6_ex);
        }
    }
    CS2SX_LINE(55);
    CS2SX_SaveData_Write("note", "zeile 1\nzeile 2");
    CS2SX_LINE(56);
    char* _sb6 = _cs2sx_next_buf();
    _sb6 = CS2SX_Format(_sb6, "save %d %s", CS2SX_Str16Len(CS2SX_NN(CS2SX_SaveData_Read("note"))), CS2SX_BoolStr(!CS2SX_File_Exists("/switch/cs2sx_data/note.txt")));
    const char* _ps_7 = _sb6;
    printf("%s\n", _ps_7 ? _ps_7 : "");
    CS2SX_LINE(59);
    Score* score = Score_New();
    CS2SX_LINE(60);
    Score_set_Points((Score*)score, 5);
    CS2SX_LINE(61);
    Score_set_Points((Score*)score, (Score_get_Points((Score*)score) + (3)));
    CS2SX_LINE(62);
    Score_set_Points((Score*)score, -4);
    CS2SX_LINE(63);
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "points %d %d", Score_get_Points(score), score->f_Changes);
    const char* _ps_8 = _sb7;
    printf("%s\n", _ps_8 ? _ps_8 : "");
    CS2SX_LINE(66);
    char* _sb8 = _cs2sx_next_buf();
    _sb8 = CS2SX_Format(_sb8, "players %s %s %d", CS2SX_BoolStr((CS2SX_Input_ConnectedCount() >= 0)), CS2SX_BoolStr(CS2SX_Input_DownOf(0, HidNpadButton_A)), CS2SX_Input_StickLeftOf(1).x);
    const char* _ps_9 = _sb8;
    printf("%s\n", _ps_9 ? _ps_9 : "");
    CS2SX_LINE(70);
    unsigned int* pixels = (unsigned int*)CS2SX_ArrNew(8 * 4, sizeof(unsigned int));
    CS2SX_LINE(71);
    for (int i = 0; i < 32; i++)
    {
        CS2SX_LINE(71);
        CS2SX_AT(pixels, i) = CS2SX_RGB(((unsigned char)(i * 8)), 100, 200);
    }
    CS2SX_LINE(72);
    self->f_sheet = Texture_New(8, 4, pixels);
    CS2SX_LINE(73);
    Graphics_Init(1280, 720);
}

void GameKitApp_OnFrame(GameKitApp* self)
{
    CS2SX_TRACE("GameKitApp.OnFrame", "Program.cs");
    CS2SX_LINE(78);
    self->f_frame++;
    CS2SX_LINE(79);
    Graphics_FillScreen(CS2SX_RGB(20, 20, 30));
    CS2SX_LINE(80);
    int f = self->f_frame / 10 % 4;
    CS2SX_LINE(81);
    Graphics_DrawTextureRegion(self->f_sheet, f * 2, 0, 2, 2, 100, 100, 64, 64, 0.0f, 4294967295, 0, 0);
    CS2SX_LINE(82);
    Graphics_DrawTextureRegion(self->f_sheet, f * 2, 2, 2, 2, 200, 100, 64, 64, self->f_frame * 3, CS2SX_RGBA(255, 255, 255, 160), 1, 0);
    CS2SX_LINE(83);
    Graphics_DrawTextureEx(self->f_sheet, 300, 100, 128, 64, -15, CS2SX_RGB(255, 120, 120), 0, 1);
    CS2SX_LINE(84);
    Graphics_DrawTextureAlpha(self->f_sheet, 460, 100, 90);
    CS2SX_LINE(85);
    if (self->f_frame == 5)
    {
        CS2SX_LINE(85);
        printf("done\n");
    }
}

void GameKitApp_Free(GameKitApp* self)
{
    if (!self) return;
    if (self->f_sheet) { Texture_Dispose(self->f_sheet); self->f_sheet = NULL; }
    free(self);
}
