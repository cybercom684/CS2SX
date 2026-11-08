#include <stdlib.h>
#include "Program.h"

// ── Struct: Point ────────────────────────────────────────
Point Point_New(int x, int y)
{
    Point _self_value = {0};
    Point* self = &_self_value;
    CS2SX_LINE(7);
    self->X = x;
    CS2SX_LINE(7);
    self->Y = y;
    return _self_value;
}

int Point_ManhattanLength(Point* self)
{
    return ((self->X < 0 ? -self->X : self->X)) + ((self->Y < 0 ? -self->Y : self->Y));
}

int Point_Equals(Point a, Point b)
{
    return memcmp(&a, &b, sizeof(Point)) == 0;
}

Score* Score_New(const char* Player, int Points)
{
    Score* self = (Score*)calloc(1, sizeof(Score));
    if (!self) return NULL;
    self->_rc = 1;
    self->f_Player = Player;
    self->f_Points = Points;
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Score_Free);
    return self;
}

void Score_Free(Score* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Score* Score_Retain(Score* self)
{
    if (self) self->_rc++;
    return self;
}

void StructsRecordsEnumsApp_Init(StructsRecordsEnumsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))StructsRecordsEnumsApp_OnInit;
    self->f_state = State_Menu;
}

void StructsRecordsEnumsApp_OnInit(StructsRecordsEnumsApp* self)
{
    CS2SX_TRACE("StructsRecordsEnumsApp.OnInit", "Program.cs");
    CS2SX_LINE(19);
    Point p = Point_New(3, -4);
    CS2SX_LINE(20);
    Point q = p;
    CS2SX_LINE(21);
    q.X = 10;
    CS2SX_LINE(22);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d %d %d", p.X, q.X, Point_ManhattanLength(&p));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(24);
    Score* s = Score_New("anna", 500);
    CS2SX_LINE(25);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%s %d", s->f_Player, s->f_Points);
    const char* _ps_1 = _sb1;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(27);
    self->f_state = State_Playing;
    CS2SX_LINE(28);
    switch (self->f_state)
    {
        case State_Menu:
            CS2SX_LINE(30);
            printf("menu\n");
            CS2SX_LINE(30);
            break;
        case State_Playing:
            CS2SX_LINE(31);
            printf("playing\n");
            CS2SX_LINE(31);
            break;
        default:
            CS2SX_LINE(32);
            printf("other\n");
            CS2SX_LINE(32);
            break;
    }
    CS2SX_LINE(35);
    const char* label = (self->f_state == State_Paused ? "paused" : (self->f_state == State_GameOver ? "over" : "running"));
    CS2SX_LINE(41);
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "%s %d", label, ((int)self->f_state));
    const char* _ps_2 = _sb2;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(45);
    if (self->f_state == State_Playing || self->f_state == State_Paused)
    {
        printf("active\n");
    }
    CS2SX_LINE(47);
    int lives = 2;
    CS2SX_LINE(48);
    if (lives > 0 && lives < 5)
    {
        printf("few lives\n");
    }
}

void StructsRecordsEnumsApp_Free(StructsRecordsEnumsApp* self)
{
    if (!self) return;
    free(self);
}
