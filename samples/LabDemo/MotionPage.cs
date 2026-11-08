using System;

// Motion sensors: spirit level from the accelerometer, live gyro graph, rumble test.
public class MotionPage : Page
{
    private const int History = 180;
    private readonly float[] _gyro = new float[History];
    private int _head;
    private float _peak;
    private bool _rumbling;

    public override void Frame(Ui ui, float dt)
    {
        bool zr = Input.IsHeld(NpadButton.ZR);
        if (zr) Vibration.RumbleSimple(0.6f);
        else if (_rumbling) Vibration.Stop();
        _rumbling = zr;
        if (Input.IsDown(NpadButton.Y)) { Motion.ResetAngles(); _peak = 0; }

        if (!Motion.IsAvailable())
        {
            ui.Card(40, 104, 1200, 544, "Bewegung");
            ui.TextCentered(640, 330, "Kein Bewegungssensor verfügbar", Theme.Text, 34);
            ui.TextCentered(640, 380, "Joy-Con oder Pro Controller verbinden", Theme.Muted, 24);
            ui.TextCentered(640, 560, zr ? "Vibration läuft …" : "ZR halten = Vibration testen", Theme.Accent, 24);
            return;
        }

        MotionState m = Motion.Get();
        float g = (float)Math.Sqrt(m.gyroX * m.gyroX + m.gyroY * m.gyroY + m.gyroZ * m.gyroZ);
        _gyro[_head] = g;
        _head = (_head + 1) % History;
        if (g > _peak) _peak = g;

        DrawLevel(ui, m);
        DrawAxes(ui, m);
        DrawGraph(ui, g, zr);
    }

    private static void DrawLevel(Ui ui, MotionState m)
    {
        ui.Card(40, 104, 420, 544, "Wasserwaage");
        int cx = 250, cy = 380, r = 150;
        Graphics.FillCircle(cx, cy, r, Theme.Track);
        Graphics.DrawCircle(cx, cy, 40, Theme.Muted);
        Graphics.DrawCircle(cx, cy, 100, Theme.Card);
        int bx = cx + (int)(Clamp(m.accelX, -1, 1) * (r - 30));
        int by = cy + (int)(Clamp(m.accelY, -1, 1) * (r - 30));
        float off = (float)Math.Sqrt(m.accelX * m.accelX + m.accelY * m.accelY);
        uint color = off < 0.08f ? Theme.Good : off < 0.3f ? Theme.Warn : Theme.Bad;
        Graphics.FillCircle(bx, by, 28, color);
        ui.TextCentered(cx, 580, off < 0.08f ? "Waagerecht!" : $"Neigung {off * 90:F0}°", Theme.Text, 26);
    }

    private static void DrawAxes(Ui ui, MotionState m)
    {
        ui.Card(490, 104, 750, 220, "Sensorwerte");
        Axis(ui, 520, 156, "Beschl. X", m.accelX, 2f, Theme.Accent);
        Axis(ui, 520, 196, "Beschl. Y", m.accelY, 2f, Theme.Accent);
        Axis(ui, 520, 236, "Beschl. Z", m.accelZ, 2f, Theme.Accent);
        Axis(ui, 880, 156, "Gyro X", m.gyroX, 4f, Theme.Good);
        Axis(ui, 880, 196, "Gyro Y", m.gyroY, 4f, Theme.Good);
        Axis(ui, 880, 236, "Gyro Z", m.gyroZ, 4f, Theme.Good);
        ui.Text(520, 280, $"Winkel {m.angleX:F2} / {m.angleY:F2} / {m.angleZ:F2}", Theme.Muted, 20);
    }

    // A centered bar: left = negative, right = positive.
    private static void Axis(Ui ui, int x, int y, string label, float value, float range, uint color)
    {
        ui.Text(x, y - 4, label, Theme.Muted, 18);
        int bx = x + 110, w = 180, mid = bx + w / 2;
        Graphics.FillRoundedRect(bx, y, w, 14, 7, Theme.Track);
        int len = (int)(Clamp(value / range, -1, 1) * (w / 2));
        if (len > 0) Graphics.FillRect(mid, y + 2, len, 10, color);
        else if (len < 0) Graphics.FillRect(mid + len, y + 2, -len, 10, color);
        Graphics.FillRect(mid, y - 2, 1, 18, Theme.Text);
        ui.TextRight(x + 345, y - 4, $"{value:F2}", Theme.Text, 18);
    }

    private void DrawGraph(Ui ui, float now, bool rumbling)
    {
        ui.Card(490, 348, 750, 300, "Drehgeschwindigkeit");
        int gx = 520, gy = 600, gw = 690, gh = 190;
        float scale = _peak > 1 ? _peak : 1;
        for (int i = 1; i < History; i++)
        {
            float a = _gyro[(_head + i - 1) % History];
            float b = _gyro[(_head + i) % History];
            int x0 = gx + (i - 1) * gw / History, x1 = gx + i * gw / History;
            int y0 = gy - (int)(a / scale * gh), y1 = gy - (int)(b / scale * gh);
            Graphics.DrawLine(x0, y0, x1, y1, Theme.Good);
        }
        Graphics.DrawLine(gx, gy, gx + gw, gy, Theme.Track);
        ui.TextRight(1210, 384, $"jetzt {now:F2} · Spitze {_peak:F2}", Theme.Text, 20);
        ui.Text(520, 612, rumbling ? "Vibration läuft …" : "Y = zurücksetzen · ZR halten = Vibration", Theme.Muted, 18);
    }

    private static float Clamp(float v, float lo, float hi)
    {
        return v < lo ? lo : v > hi ? hi : v;
    }
}
