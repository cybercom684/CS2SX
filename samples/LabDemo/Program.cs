using System.Collections.Generic;
using System.Diagnostics;

// CS2SX Lab — an interactive tour of what a CS2SX app can do on the Switch.
// L / R or tapping a tab switches pages, + quits.
public class LabApp : SwitchApp
{
    private Ui? _ui;
    private readonly Stats _stats = new Stats();
    private readonly List<Page> _pages = new List<Page>();
    private readonly string[] _titles = { "Übersicht", "Controller", "Reaktion", "Bewegung" };
    private int _page;
    private float _tabPos;
    private bool _wasTouching;
    private Stopwatch? _frameTimer;

    private const int TabW = 170, TabX = 400;

    public override void OnInit()
    {
        Graphics.Init(1280, 720);
        // romfs:/font.ttf when the app ships one, otherwise the console's own font
        Font? font = File.Exists("romfs:/font.ttf") ? Font.Load("romfs:/font.ttf") : Font.LoadSystem();
        _ui = new Ui(font);
        _pages.Add(new DashboardPage(_stats));
        _pages.Add(new ControllerPage());
        _pages.Add(new ReactionPage());
        _pages.Add(new MotionPage());
        Audio.Init(48000);
        _frameTimer = Stopwatch.StartNew();
    }

    public override void OnFrame()
    {
        var ui = _ui!;
        float dt = (float)_frameTimer!.Elapsed.TotalMilliseconds;
        _frameTimer.Restart();
        _stats.AddFrame(dt);
        Audio.Update();

        HandleNavigation();
        _tabPos += (_page - _tabPos) * 0.25f;

        Graphics.FillScreen(Theme.Bg);
        DrawHeader(ui);
        _pages[_page].Frame(ui, dt);
        DrawFooter(ui);
    }

    private void HandleNavigation()
    {
        if (Input.IsDown(NpadButton.R)) _page = (_page + 1) % _pages.Count;
        if (Input.IsDown(NpadButton.L)) _page = (_page + _pages.Count - 1) % _pages.Count;

        TouchState t = Input.GetTouch();
        bool touching = t.count > 0;
        if (touching && !_wasTouching && t.y[0] < 84)
        {
            int idx = (t.x[0] - TabX) / TabW;
            if (t.x[0] >= TabX && idx < _pages.Count) _page = idx;
        }
        _wasTouching = touching;
    }

    private void DrawHeader(Ui ui)
    {
        Graphics.FillRect(0, 0, 1280, 84, Theme.Header);
        Graphics.FillCircle(52, 42, 18, Theme.Accent);
        Graphics.FillCircle(52, 42, 8, Theme.Header);
        ui.Text(82, 24, "CS2SX Lab", Theme.Text, 32);

        int ix = TabX + (int)(_tabPos * TabW);
        Graphics.FillRoundedRect(ix + 6, 18, TabW - 12, 48, 24, Theme.Accent.WithAlpha(60));
        Graphics.FillRoundedRect(ix + 30, 72, TabW - 60, 5, 2, Theme.Accent);
        for (int i = 0; i < _titles.Length; i++)
        {
            uint color = i == _page ? Theme.Text : Theme.Muted;
            ui.TextCentered(TabX + i * TabW + TabW / 2, 30, _titles[i], color, 24);
        }

        TimeInfo t = NX.GetTime();
        BatteryInfo b = NX.GetBattery();
        ui.TextRight(1240, 18, $"{t.hour:D2}:{t.minute:D2}", Theme.Text, 26);
        ui.TextRight(1240, 50, b.percent + " %" + (b.charging ? " · lädt" : ""), Theme.Muted, 18);
    }

    private void DrawFooter(Ui ui)
    {
        Graphics.FillRect(0, 676, 1280, 44, Theme.Header);
        ui.Text(40, 686, "L / R  Seite wechseln", Theme.Muted, 20);
        ui.Text(330, 686, "Tippen auf die Reiter geht auch", Theme.Muted, 20);
        ui.TextRight(1240, 686, "+  Beenden", Theme.Muted, 20);
    }
}
