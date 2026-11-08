// Every input at once: sticks with motion trails, buttons, touch, press statistics.
public class ControllerPage : Page
{
    private const int Trail = 24;

    private readonly int[] _lx = new int[Trail];
    private readonly int[] _ly = new int[Trail];
    private readonly int[] _rx = new int[Trail];
    private readonly int[] _ry = new int[Trail];
    private int _trailHead;

    private int _presses;
    private readonly int[] _recent = new int[60];   // presses per frame, last second
    private int _recentHead;
    private int _bestRate;

    public override void Frame(Ui ui, float dt)
    {
        CountPresses();
        StickPos l = Input.GetStickLeft();
        StickPos r = Input.GetStickRight();
        _lx[_trailHead] = l.x; _ly[_trailHead] = l.y;
        _rx[_trailHead] = r.x; _ry[_trailHead] = r.y;
        _trailHead = (_trailHead + 1) % Trail;

        ui.Card(40, 104, 1200, 420, "");
        Shoulder(ui, 90, 124, "ZL", NpadButton.ZL);
        Shoulder(ui, 220, 124, "L", NpadButton.L);
        Shoulder(ui, 960, 124, "R", NpadButton.R);
        Shoulder(ui, 1090, 124, "ZR", NpadButton.ZR);

        Stick(ui, 240, 330, _lx, _ly, l, Input.IsHeld(NpadButton.StickL), "L-Stick");
        Stick(ui, 1040, 330, _rx, _ry, r, Input.IsHeld(NpadButton.StickR), "R-Stick");
        DPad(470, 330);
        FaceButtons(ui, 810, 330);
        Small(ui, 590, 200, "-", NpadButton.Minus);
        Small(ui, 690, 200, "+", NpadButton.Plus);

        DrawStats(ui);
        DrawTouch(ui);
    }

    private void CountPresses()
    {
        int now = 0;
        if (Input.IsDown(NpadButton.A)) now++;
        if (Input.IsDown(NpadButton.B)) now++;
        if (Input.IsDown(NpadButton.X)) now++;
        if (Input.IsDown(NpadButton.Y)) now++;
        if (Input.IsDown(NpadButton.Up)) now++;
        if (Input.IsDown(NpadButton.Down)) now++;
        if (Input.IsDown(NpadButton.Left)) now++;
        if (Input.IsDown(NpadButton.Right)) now++;
        if (Input.IsDown(NpadButton.ZL)) now++;
        if (Input.IsDown(NpadButton.ZR)) now++;
        if (Input.IsDown(NpadButton.Minus)) now++;
        if (Input.IsDown(NpadButton.StickL)) now++;
        if (Input.IsDown(NpadButton.StickR)) now++;
        _presses += now;
        _recent[_recentHead] = now;
        _recentHead = (_recentHead + 1) % 60;
        int rate = Rate();
        if (rate > _bestRate) _bestRate = rate;
    }

    private int Rate()
    {
        int sum = 0;
        foreach (int n in _recent) sum += n;
        return sum;
    }

    private void Stick(Ui ui, int cx, int cy, int[] xs, int[] ys, StickPos now, bool pressed, string label)
    {
        const int radius = 110;
        Graphics.FillCircle(cx, cy, radius, Theme.Track);
        Graphics.DrawCircle(cx, cy, radius, pressed ? Theme.Accent : Theme.Muted);
        Graphics.DrawLine(cx - radius, cy, cx + radius, cy, Theme.Card);
        Graphics.DrawLine(cx, cy - radius, cx, cy + radius, Theme.Card);
        for (int i = 1; i < Trail; i++)
        {
            int idx = (_trailHead + i) % Trail;
            int px = cx + xs[idx] * (radius - 20) / 32767;
            int py = cy - ys[idx] * (radius - 20) / 32767;
            Graphics.FillCircle(px, py, 4 + i / 4, Theme.Accent.WithAlpha((byte)(i * 8)));
        }
        int dx = cx + now.x * (radius - 20) / 32767;
        int dy = cy - now.y * (radius - 20) / 32767;
        Graphics.FillCircle(dx, dy, 22, pressed ? Theme.Accent : Theme.Text);
        ui.TextCentered(cx, cy + radius + 14, label, Theme.Muted, 20);
        ui.TextCentered(cx, cy + radius + 40, $"{now.x * 100 / 32767,4} / {now.y * 100 / 32767,4}", Theme.Text, 20);
    }

    private static void DPad(int cx, int cy)
    {
        DPadKey(cx, cy - 46, NpadButton.Up);
        DPadKey(cx, cy + 46, NpadButton.Down);
        DPadKey(cx - 46, cy, NpadButton.Left);
        DPadKey(cx + 46, cy, NpadButton.Right);
    }

    private static void DPadKey(int cx, int cy, NpadButton button)
    {
        bool held = Input.IsHeld(button);
        Graphics.FillRoundedRect(cx - 22, cy - 22, 44, 44, 8, held ? Theme.Accent : Theme.Track);
    }

    private static void FaceButtons(Ui ui, int cx, int cy)
    {
        Face(ui, cx, cy - 56, "X", NpadButton.X, Theme.Accent);
        Face(ui, cx, cy + 56, "B", NpadButton.B, Theme.Warn);
        Face(ui, cx - 56, cy, "Y", NpadButton.Y, Theme.Good);
        Face(ui, cx + 56, cy, "A", NpadButton.A, Theme.Bad);
    }

    private static void Face(Ui ui, int cx, int cy, string label, NpadButton button, uint color)
    {
        bool held = Input.IsHeld(button);
        Graphics.FillCircle(cx, cy, 30, held ? color : Theme.Track);
        ui.TextCentered(cx, cy - 15, label, held ? Theme.Bg : Theme.Text, 28);
    }

    private static void Shoulder(Ui ui, int x, int y, string label, NpadButton button)
    {
        bool held = Input.IsHeld(button);
        Graphics.FillRoundedRect(x, y, 110, 40, 14, held ? Theme.Accent : Theme.Track);
        ui.TextCentered(x + 55, y + 8, label, held ? Theme.Bg : Theme.Text, 22);
    }

    private static void Small(Ui ui, int cx, int cy, string label, NpadButton button)
    {
        bool held = Input.IsHeld(button);
        Graphics.FillCircle(cx, cy, 20, held ? Theme.Accent : Theme.Track);
        ui.TextCentered(cx, cy - 13, label, Theme.Text, 24);
    }

    private void DrawStats(Ui ui)
    {
        ui.Card(40, 548, 1200, 100, "Auswertung");
        int rate = Rate();
        ui.Text(70, 592, $"{_presses} Tastendrücke", Theme.Text, 28);
        ui.Text(430, 592, $"{rate} pro Sekunde", rate > 0 ? Theme.Accent : Theme.Muted, 28);
        ui.Text(760, 592, $"Rekord: {_bestRate}/s", Theme.Good, 28);
        ui.Bar(1010, 604, 200, 16, rate / 15f, Theme.Accent);
    }

    // Touch points (handheld mode): a ring per finger with its number.
    private static void DrawTouch(Ui ui)
    {
        TouchState t = Input.GetTouch();
        for (int i = 0; i < t.count; i++)
        {
            Graphics.FillCircle(t.x[i], t.y[i], 46, Theme.Warn.WithAlpha(70));
            Graphics.DrawCircle(t.x[i], t.y[i], 46, Theme.Warn);
            ui.TextCentered(t.x[i], t.y[i] - 14, (i + 1).ToString(), Theme.Text, 26);
        }
    }
}
