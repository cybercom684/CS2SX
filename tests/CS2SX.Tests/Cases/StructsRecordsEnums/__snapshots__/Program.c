#include <stdlib.h>
#include "Program.h"

// ── Struct: Point ────────────────────────────────────────
typedef struct Point {
    int X;
    int Y;
} Point;

int Point_ManhattanLength(Point* self)
{
    return ((self->f_X < 0 ? -self->f_X : self->f_X)) + ((self->f_Y < 0 ? -self->f_Y : self->f_Y));
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
    self->f_state = Menu;
}

void StructsRecordsEnumsApp_OnInit(StructsRecordsEnumsApp* self)
{
    Point* p = (Point){ 3, -4 };
    Point q = p;
    q->f_X = 10;
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d %d %d", p->f_X, q->f_X, Point_ManhattanLength(p));
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d %d %d", p->f_X, q->f_X, Point_ManhattanLength(p));
    const char* _ps_0 = _sb1;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    Score* s = Score_New("anna", 500);
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "%s %d", s->f_Player, s->f_Points);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "%s %d", s->f_Player, s->f_Points);
    const char* _ps_1 = _sb3;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    self->f_state = Playing;
    switch (self->f_state)
    {
        case Menu:
            printf("menu\n");
            break;
        case Playing:
            printf("playing\n");
            break;
        default:
            printf("other\n");
            break;
    }
    const char* label = (self->f_state == Paused ? "paused" : (self->f_state == GameOver ? "over" : "running"));
    char* _sb4 = _cs2sx_next_buf();
    snprintf(_sb4, CS2SX_STRBUF_SIZE, "%s %d", label, ((int)self->f_state));
    char* _sb5 = _cs2sx_next_buf();
    snprintf(_sb5, CS2SX_STRBUF_SIZE, "%s %d", label, ((int)self->f_state));
    const char* _ps_2 = _sb5;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    void* o = 5;
    if (1 && n > 3)
    {
        char* _sb6 = _cs2sx_next_buf();
        snprintf(_sb6, CS2SX_STRBUF_SIZE, "pattern matched %d", n);
        char* _sb7 = _cs2sx_next_buf();
        snprintf(_sb7, CS2SX_STRBUF_SIZE, "pattern matched %d", n);
        const char* _ps_3 = _sb7;
        printf("%s\n", _ps_3 ? _ps_3 : "");
    }
}

void StructsRecordsEnumsApp_Free(StructsRecordsEnumsApp* self)
{
    if (!self) return;
    free(self);
}
