using System;
using System.Collections.Generic;
using System.Diagnostics;

// Reaction test: wait for green, press A. Keeps the last ten results and evaluates them.
public class ReactionPage : Page
{
    private const int Idle = 0, Waiting = 1, Go = 2, Result = 3, TooEarly = 4;

    private int _state = Idle;
    private readonly Stopwatch _timer = new Stopwatch();
    private int _delayMs;
    private int _lastMs;
    private float _pulse;
    private readonly List<int> _results = new List<int>();
    private int _attempts;
    private int _early;

    public override void Frame(Ui ui, float dt)
    {
        Update();
        _pulse += dt * 0.004f;
        DrawArena(ui);
        DrawHistory(ui);
        DrawSummary(ui);
    }

    private void Update()
    {
        bool a = Input.IsDown(NpadButton.A);
        if (_state == Idle || _state == Result || _state == TooEarly)
        {
            if (a) Start();
        }
        else if (_state == Waiting)
        {
            if (a)
            {
                _state = TooEarly;
                _early++;
                Vibration.Pulse(0.9f, 160);
            }
            else if (_timer.ElapsedMilliseconds >= _delayMs)
            {
                _state = Go;
                _timer.Restart();
                Vibration.Pulse(0.5f, 60);
                Audio.PlayTone(880f, 0.25f, 90);
            }
        }
        else if (_state == Go && a)
        {
            _lastMs = (int)_timer.Elapsed.TotalMilliseconds;
            _state = Result;
            _attempts++;
            _results.Add(_lastMs);
            if (_results.Count > 10) _results.RemoveAt(0);
        }
    }

    private void Start()
    {
        _state = Waiting;
        _delayMs = Random.Shared.Next(1200, 3800);
        _timer.Restart();
    }

    private void DrawArena(Ui ui)
    {
        ui.Card(40, 104, 560, 544, "Reaktionstest");
        int cx = 320, cy = 360;
        uint color = Theme.Track;
        string big = "A";
        string hint = "A drücken zum Starten";
        if (_state == Waiting) { color = Theme.Bad; big = "…"; hint = "Warten, bis es grün wird"; }
        else if (_state == Go) { color = Theme.Good; big = "JETZT!"; hint = "A drücken!"; }
        else if (_state == Result) { color = RatingColor(_lastMs); big = _lastMs + " ms"; hint = Rating(_lastMs) + "  ·  A = nochmal"; }
        else if (_state == TooEarly) { color = Theme.Warn; big = "Zu früh!"; hint = "A = nochmal"; }

        int r = 150;
        if (_state == Waiting) r = 150 + (int)(Math.Sin(_pulse) * 8);
        Graphics.FillCircle(cx, cy, r + 18, color.WithAlpha(50));
        Graphics.FillCircle(cx, cy, r, color);
        int size = big.Length > 4 ? 48 : 72;
        ui.TextCentered(cx, cy - size / 2 - 6, big, Theme.Bg, size);
        ui.TextCentered(cx, 572, hint, Theme.Text, 26);
    }

    private void DrawHistory(Ui ui)
    {
        ui.Card(640, 104, 600, 330, "Letzte Versuche");
        int baseY = 400, maxH = 230;
        Graphics.DrawLine(670, baseY, 1210, baseY, Theme.Track);
        if (_results.Count == 0)
        {
            ui.TextCentered(940, 250, "Noch keine Messung", Theme.Muted, 24);
            return;
        }
        for (int i = 0; i < _results.Count; i++)
        {
            int ms = _results[i];
            int h = ms * maxH / 600;
            if (h > maxH) h = maxH;
            int x = 680 + i * 52;
            Graphics.FillRoundedRect(x, baseY - h, 40, h, 8, RatingColor(ms));
            ui.TextCentered(x + 20, baseY - h - 26, ms.ToString(), Theme.Text, 18);
        }
    }

    private void DrawSummary(Ui ui)
    {
        ui.Card(640, 458, 600, 190, "Auswertung");
        if (_results.Count == 0)
        {
            ui.Text(670, 512, "Bestwert, Schnitt und Median erscheinen", Theme.Muted, 22);
            ui.Text(670, 546, "nach der ersten Messung.", Theme.Muted, 22);
            return;
        }
        int best = int.MaxValue, worst = 0, sum = 0;
        foreach (int ms in _results)
        {
            if (ms < best) best = ms;
            if (ms > worst) worst = ms;
            sum += ms;
        }
        var sorted = new List<int>(_results);
        sorted.Sort();
        int median = sorted[sorted.Count / 2];
        int avg = sum / _results.Count;

        Metric(ui, 670, 500, "Bestwert", best + " ms", Theme.Good);
        Metric(ui, 860, 500, "Schnitt", avg + " ms", Theme.Accent);
        Metric(ui, 1050, 500, "Median", median + " ms", Theme.Text);
        ui.Text(670, 596, $"{_attempts} {(_attempts == 1 ? "Versuch" : "Versuche")} ·{_early}× zu früh · langsamster {worst} ms", Theme.Muted, 20);
    }

    private static void Metric(Ui ui, int x, int y, string label, string value, uint color)
    {
        ui.Text(x, y, label, Theme.Muted, 20);
        ui.Text(x, y + 28, value, color, 34);
    }

    private static uint RatingColor(int ms)
    {
        return ms < 230 ? Theme.Good : ms < 320 ? Theme.Accent : ms < 450 ? Theme.Warn : Theme.Bad;
    }

    private static string Rating(int ms)
    {
        if (ms < 200) return "Blitzschnell!";
        if (ms < 260) return "Sehr gut";
        if (ms < 340) return "Gut";
        return "Geht noch schneller";
    }
}
