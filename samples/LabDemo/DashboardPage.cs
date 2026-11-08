using System;

// Live system overview: frame rate + frame-time graph, battery, clock, animation.
public class DashboardPage : Page
{
    private readonly Stats _stats;
    private float _angle;

    public DashboardPage(Stats stats)
    {
        _stats = stats;
    }

    public override void Frame(Ui ui, float dt)
    {
        _angle += dt * 0.0025f;
        DrawFps(ui, 40, 104);
        DrawBattery(ui, 460, 104);
        DrawClock(ui, 880, 104);
        DrawOrbit(ui, 40, 364);
        DrawInfo(ui, 660, 364);
    }

    private void DrawFps(Ui ui, int x, int y)
    {
        ui.Card(x, y, 380, 230, "Bildrate");
        uint color = _stats.Fps >= 58 ? Theme.Good : _stats.Fps >= 45 ? Theme.Warn : Theme.Bad;
        ui.Text(x + 22, y + 46, $"{_stats.Fps:F1}", color, 56);
        ui.Text(x + 22 + ui.Width($"{_stats.Fps:F1}", 56) + 10, y + 74, "FPS", Theme.Muted, 22);
        ui.TextRight(x + 358, y + 52, $"max {_stats.WorstMs:F1} ms", Theme.Muted, 18);

        // frame times of the last two seconds, oldest left; the line marks 16.7 ms
        int gx = x + 22, gy = y + 206, gh = 80;
        Graphics.DrawLine(gx, gy - 40, gx + 336, gy - 40, Theme.Track);
        for (int i = 0; i < Stats.History; i++)
        {
            float ms = _stats.FrameMs[(_stats.Head + i) % Stats.History];
            int h = (int)(ms / 33.4f * gh);
            if (h > gh) h = gh;
            if (h < 1) h = 1;
            uint c = ms <= 17.5f ? Theme.Good : ms <= 25f ? Theme.Warn : Theme.Bad;
            Graphics.FillRect(gx + i * 28 / 10, gy - h, 2, h, c);
        }
    }

    private void DrawBattery(Ui ui, int x, int y)
    {
        ui.Card(x, y, 380, 230, "Akku");
        BatteryInfo b = NX.GetBattery();
        uint color = b.percent > 50 ? Theme.Good : b.percent > 20 ? Theme.Warn : Theme.Bad;
        ui.Text(x + 22, y + 46, b.percent + " %", Theme.Text, 56);
        ui.Bar(x + 22, y + 140, 336, 22, b.percent / 100f, color);
        string state = b.charging ? "wird geladen" : b.connected ? "am Netz" : "Akkubetrieb";
        ui.Text(x + 22, y + 178, state, Theme.Muted, 22);
    }

    private void DrawClock(Ui ui, int x, int y)
    {
        ui.Card(x, y, 360, 230, "Uhrzeit");
        TimeInfo t = NX.GetTime();
        ui.Text(x + 22, y + 46, $"{t.hour:D2}:{t.minute:D2}", Theme.Text, 64);
        ui.Text(x + 22 + ui.Width($"{t.hour:D2}:{t.minute:D2}", 64) + 8, y + 80, $"{t.second:D2}", Theme.Accent, 30);
        int up = (int)_stats.UptimeSec;
        ui.Text(x + 22, y + 178, $"App läuft seit {up / 60}:{up % 60:D2}", Theme.Muted, 22);
    }

    // Twelve dots orbiting — any stutter is visible immediately.
    private void DrawOrbit(Ui ui, int x, int y)
    {
        ui.Card(x, y, 580, 296, "Flüssigkeit");
        int cx = x + 200, cy = y + 160;
        Graphics.DrawCircle(cx, cy, 96, Theme.Track);
        for (int i = 0; i < 12; i++)
        {
            float a = _angle - i * 0.18f;
            int px = cx + (int)(Math.Cos(a) * 96);
            int py = cy + (int)(Math.Sin(a) * 96);
            byte alpha = (byte)(255 - i * 20);
            Graphics.FillCircle(px, py, 14 - i, Theme.Accent.WithAlpha(alpha));
        }
        ui.Text(x + 340, y + 100, "Gleichmäßige Bewegung", Theme.Text, 22);
        ui.Text(x + 340, y + 132, "= stabile 60 Bilder/s.", Theme.Muted, 20);
        ui.Text(x + 340, y + 178, $"Ø {_stats.AverageMs():F2} ms", Theme.Accent, 30);
        ui.Text(x + 340, y + 220, "pro Bild", Theme.Muted, 20);
    }

    private void DrawInfo(Ui ui, int x, int y)
    {
        ui.Card(x, y, 580, 296, "System");
        int row = y + 56;
        InfoRow(ui, x, row, "Laufzeit", "CS2SX 0.5 · C# zu C");
        InfoRow(ui, x, row + 40, "Grafik", "GPU · deko3d");
        InfoRow(ui, x, row + 80, "Auflösung", "1280 × 720");
        InfoRow(ui, x, row + 120, "Schrift", ui.FontName());
        InfoRow(ui, x, row + 160, "Bilder", _stats.Frames.ToString());
    }

    private static void InfoRow(Ui ui, int x, int y, string label, string value)
    {
        ui.Text(x + 22, y, label, Theme.Muted, 22);
        ui.Text(x + 180, y, value, Theme.Text, 22);
    }
}
