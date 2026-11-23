using System;
using System.Collections.Generic;

// Funkenjagd — a small local multiplayer game for 1 to 4 players.
// Fly your drone with the left stick, catch sparks, dodge the meteors.
// One round lasts 60 seconds; the best rounds are kept in a high score list.
//
// Shows: sprite sheets (frames, tint, rotation, mirroring), several controllers,
// coroutines with yield return, JSON in SaveData, enums in text, audio and rumble.

public enum Phase { Title, Countdown, Playing, Results }
public enum Medal { Gold, Silber, Bronze }

public class FunkenjagdApp : SwitchApp
{
    private const int RoundFrames = 60 * 60;
    private static readonly uint[] Tints =
    {
        Color.RGB(92, 170, 255), Color.RGB(255, 110, 120), Color.RGB(110, 230, 140), Color.RGB(255, 200, 80),
    };

    private Hud? _hud;
    private readonly Random _rng = new Random();
    private readonly Scheduler _routines = new Scheduler();
    private readonly List<Player> _players = new List<Player>();
    private readonly List<Spark> _sparks = new List<Spark>();
    private readonly List<Rock> _rocks = new List<Rock>();
    private readonly List<Particle> _particles = new List<Particle>();
    private readonly int[] _starX = new int[90], _starY = new int[90], _starSpeed = new int[90];

    private Phase _phase = Phase.Title;
    private int _frame;
    private int _timeLeft;
    private string _banner = "";
    private int _bannerFrames;
    private int _wave;
    private int _place;
    private bool _demo;          // the computer plays (after a while on the title screen)
    private int _idle;

    public override void OnInit()
    {
        Graphics.Init(1280, 720);
        Font? font = File.Exists("romfs:/font.ttf") ? Font.Load("romfs:/font.ttf") : Font.LoadSystem();
        _hud = new Hud(font);
        Sprites.Build();
        Highscores.Load();
        Audio.Init(48000);
        for (int i = 0; i < _starX.Length; i++)
        {
            _starX[i] = _rng.Next(0, 1280);
            _starY[i] = _rng.Next(0, 720);
            _starSpeed[i] = _rng.Next(1, 4);
        }
        Console.WriteLine("Funkenjagd bereit: " + _phase + ", " + Highscores.Table.Best.Count + " Bestwerte");
    }

    public override void OnFrame()
    {
        _frame++;
        Audio.Update();
        _routines.Tick();

        if (_phase == Phase.Title) UpdateTitle();
        else if (_phase == Phase.Playing) UpdatePlaying();
        else if (_phase == Phase.Results) UpdateResults();
        UpdateParticles();
        if (_bannerFrames > 0) _bannerFrames--;

        DrawBackground();
        if (_phase == Phase.Title) DrawTitle();
        else DrawPlayfield();
        if (_phase == Phase.Results) DrawResults();
    }

    // ── Title ────────────────────────────────────────────────────────────────

    private void UpdateTitle()
    {
        // Y: connect another controller. The console's dialog only opens when it is
        // needed, so it asks for one more player than are connected now.
        if (Input.IsDown(NpadButton.Y)) Input.ShowControllerSupport(Math.Min(4, Input.ConnectedCount() + 1), 4);
        if (Input.IsDown(NpadButton.A)) StartRound(false);
        else if (++_idle > 600) StartRound(true);   // 10 s nothing pressed: demo round
    }

    private void StartRound(bool demo)
    {
        _demo = demo;
        _idle = 0;
        _players.Clear();
        _sparks.Clear();
        _rocks.Clear();
        _particles.Clear();
        for (int p = 0; p < 4; p++)
        {
            if (p > 0 && !demo && !Input.IsConnected(p)) continue;
            _players.Add(new Player(p, Tints[p], 340 + p * 200, 420));
        }
        _timeLeft = RoundFrames;
        _wave = 0;
        _place = 0;
        _phase = Phase.Countdown;
        _routines.StopAll();
        _routines.Start(Countdown());
    }

    /// 3, 2, 1, Los! — then the round and the meteor waves start.
    private IEnumerator<int> Countdown()
    {
        for (int n = 3; n > 0; n--)
        {
            Show(n.ToString(), 50);
            Audio.PlayTone(440f, 0.25f, 120);
            yield return 60;
        }
        Show("Los!", 60);
        Audio.PlayTone(880f, 0.3f, 250);
        _phase = Phase.Playing;
        _routines.Start(Waves());
        _routines.Start(SparkRain());
    }

    /// Ever bigger meteor waves with a short break in between.
    private IEnumerator<int> Waves()
    {
        yield return 120;
        while (true)
        {
            _wave++;
            Show("Welle " + _wave, 90);
            Console.WriteLine("Welle " + _wave + (_demo ? " (Demo)" : ""));
            int count = 4 + _wave * 2;
            for (int i = 0; i < count; i++)
            {
                SpawnRock();
                yield return Math.Max(10, 46 - _wave * 5);
            }
            yield return 150;
        }
    }

    /// Keeps a few sparks on the field; now and then a golden one.
    private IEnumerator<int> SparkRain()
    {
        while (true)
        {
            if (_sparks.Count < 3 + _players.Count)
            {
                var s = new Spark();
                s.X = _rng.Next(60, 1220);
                s.Y = _rng.Next(Field.Top + 40, Field.Bottom - 40);
                s.Golden = _rng.Next(0, 10) == 0;
                _sparks.Add(s);
            }
            yield return _rng.Next(20, 50);
        }
    }

    private void SpawnRock()
    {
        var r = new Rock();
        r.Size = _rng.Next(36, 84);
        bool fromLeft = _rng.Next(0, 2) == 0;
        r.X = fromLeft ? -r.Size : 1280 + r.Size;
        r.Y = _rng.Next(Field.Top + 20, Field.Bottom - 20);
        float speed = 2.5f + _wave * 0.6f + _rng.Next(0, 100) / 50f;
        r.Vx = fromLeft ? speed : -speed;
        r.Vy = (_rng.Next(0, 100) - 50) / 40f;
        r.Spin = (_rng.Next(0, 100) - 50) / 12f;
        _rocks.Add(r);
    }

    private void Show(string text, int frames)
    {
        _banner = text;
        _bannerFrames = frames;
    }

    // ── Round ────────────────────────────────────────────────────────────────

    private void UpdatePlaying()
    {
        // any button ends a demo round
        if (_demo && (Input.IsDown(NpadButton.A) || Input.IsDown(NpadButton.B)))
        {
            _routines.StopAll();
            _phase = Phase.Title;
            return;
        }
        _timeLeft--;
        foreach (var p in _players) UpdatePlayer(p);

        for (int i = _rocks.Count - 1; i >= 0; i--)
        {
            var r = _rocks[i];
            r.X += r.Vx;
            r.Y += r.Vy;
            r.Angle += r.Spin;
            if (r.X < -150 || r.X > 1430 || r.Y < Field.Top - 150 || r.Y > Field.Bottom + 150)
            {
                _rocks.RemoveAt(i);
                continue;
            }
            foreach (var p in _players)
                if (!p.Out && p.Shield == 0 && Field.Distance(p.X, p.Y, r.X, r.Y) < r.Size / 2 + 16)
                    Hit(p);
        }

        for (int i = _sparks.Count - 1; i >= 0; i--)
        {
            var s = _sparks[i];
            s.Age++;
            foreach (var p in _players)
            {
                if (p.Out || Field.Distance(p.X, p.Y, s.X, s.Y) > 30) continue;
                p.Points += s.Golden ? 5 : 1;
                Burst(s.X, s.Y, s.Golden ? Color.RGB(255, 210, 80) : p.Tint, s.Golden ? 26 : 12);
                Audio.PlayTone(s.Golden ? 1320f : 990f, 0.2f, 70);
                _sparks.RemoveAt(i);
                break;
            }
        }

        bool anyoneLeft = false;
        foreach (var p in _players) if (!p.Out) anyoneLeft = true;
        if (_timeLeft <= 0 || !anyoneLeft) EndRound();
    }

    private void UpdatePlayer(Player p)
    {
        if (p.Out) return;
        if (p.Shield > 0) p.Shield--;
        float ax, ay;
        if (_demo)
        {
            Steer(p, out ax, out ay);
        }
        else
        {
            StickPos stick = Input.GetStickLeft(p.Index);
            ax = stick.x / 32767f;
            ay = -stick.y / 32767f;
        }
        // the d-pad works too
        if (Input.IsHeld(p.Index, NpadButton.Left)) ax = -1;
        if (Input.IsHeld(p.Index, NpadButton.Right)) ax = 1;
        if (Input.IsHeld(p.Index, NpadButton.Up)) ay = -1;
        if (Input.IsHeld(p.Index, NpadButton.Down)) ay = 1;
        p.Vx = p.Vx * 0.85f + ax * 1.2f;
        p.Vy = p.Vy * 0.85f + ay * 1.2f;
        p.X = Field.Clamp(p.X + p.Vx, 24, 1256);
        p.Y = Field.Clamp(p.Y + p.Vy, Field.Top + 24, Field.Bottom - 24);
    }

    /// The demo pilot: towards the nearest spark, away from close meteors.
    private void Steer(Player p, out float ax, out float ay)
    {
        ax = 0;
        ay = 0;
        Spark? target = null;
        float best = 99999;
        foreach (var s in _sparks)
        {
            float d = Field.Distance(p.X, p.Y, s.X, s.Y);
            if (d < best) { best = d; target = s; }
        }
        if (target != null && best > 1)
        {
            ax = (target.X - p.X) / best;
            ay = (target.Y - p.Y) / best;
        }
        foreach (var r in _rocks)
        {
            float d = Field.Distance(p.X, p.Y, r.X, r.Y);
            float danger = r.Size / 2 + 90;
            if (d < danger && d > 1)
            {
                ax += (p.X - r.X) / d * 2.5f;
                ay += (p.Y - r.Y) / d * 2.5f;
            }
        }
        float len = (float)Math.Sqrt(ax * ax + ay * ay);
        if (len > 1) { ax /= len; ay /= len; }
    }

    private void Hit(Player p)
    {
        p.Lives--;
        p.Shield = 100;
        Burst(p.X, p.Y, Color.RGB(255, 120, 60), 30);
        Vibration.Pulse(0.8f, 180);
        Audio.PlayTone(150f, 0.35f, 220);
        if (p.Out) Show(p.Name + " ist raus", 90);
    }

    private void Burst(float x, float y, uint color, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var q = new Particle();
            q.X = x;
            q.Y = y;
            q.Vx = (_rng.Next(0, 200) - 100) / 18f;
            q.Vy = (_rng.Next(0, 200) - 100) / 18f;
            q.Life = _rng.Next(18, 40);
            q.Color = color;
            _particles.Add(q);
        }
    }

    private void UpdateParticles()
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var q = _particles[i];
            q.X += q.Vx;
            q.Y += q.Vy;
            q.Vx *= 0.92f;
            q.Vy *= 0.92f;
            if (--q.Life <= 0) _particles.RemoveAt(i);
        }
    }

    private void EndRound()
    {
        _routines.StopAll();
        _phase = Phase.Results;
        Player best = _players[0];
        foreach (var p in _players) if (p.Points > best.Points) best = p;
        Console.WriteLine("Runde vorbei: " + best.Name + " mit " + best.Points + " Punkten, Welle " + _wave);
        if (_demo)
        {
            _routines.Start(BackToTitle());
            return;
        }
        if (Highscores.Qualifies(best.Points))
        {
            string name = Keyboard.Show("Neuer Bestwert! Name für die Liste", best.Name);
            if (name.Length == 0) name = best.Name;
            _place = Highscores.Add(name, best.Points, _players.Count);
        }
    }

    private void UpdateResults()
    {
        if (Input.IsDown(NpadButton.A)) StartRound(false);
        if (Input.IsDown(NpadButton.B)) _phase = Phase.Title;
    }

    /// After a demo round the results stay for a few seconds.
    private IEnumerator<int> BackToTitle()
    {
        yield return 300;
        if (_phase == Phase.Results) _phase = Phase.Title;
    }

    // ── Drawing ──────────────────────────────────────────────────────────────

    private void DrawBackground()
    {
        Graphics.FillScreen(Color.RGB(10, 12, 22));
        for (int i = 0; i < _starX.Length; i++)
        {
            int x = (_starX[i] - _frame * _starSpeed[i] / 2) % 1280;
            if (x < 0) x += 1280;
            byte b = (byte)(60 + _starSpeed[i] * 50);
            Graphics.FillRect(x, _starY[i], _starSpeed[i], _starSpeed[i], Color.RGB(b, b, (byte)Math.Min(255, b + 30)));
        }
    }

    private void DrawDrone(Player p, float x, float y, int size)
    {
        int frame = (_frame / 3 + p.Index) % Sprites.DroneFrames;
        uint tint = p.Tint;
        if (p.Out) tint = Color.RGBA(120, 120, 140, 90);
        else if (p.Shield > 0 && _frame / 4 % 2 == 0) tint = (p.Tint & 0x00FFFFFF) | 0x60000000;
        // tilt into the direction of flight, mirror when flying left
        Graphics.DrawTextureRegion(Sprites.Drones!, frame * Sprites.DroneSize, 0, Sprites.DroneSize, Sprites.DroneSize,
            (int)x - size / 2, (int)y - size / 2, size, size, p.Vx * 2.5f, tint, p.Vx < -0.5f);
    }

    private void DrawPlayfield()
    {
        var hud = _hud!;
        foreach (var s in _sparks)
        {
            int f = (s.Age / 8) % Sprites.SparkFrames;
            int size = 22 + (s.Age / 4) % 3 * 2;
            uint tint = s.Golden ? Color.RGB(255, 205, 70) : Color.RGB(170, 235, 255);
            Graphics.DrawTextureRegion(Sprites.Sparks!, f * Sprites.SparkSize, 0, Sprites.SparkSize, Sprites.SparkSize,
                (int)s.X - size / 2, (int)s.Y - size / 2, size, size, s.Age * 3, tint);
        }
        foreach (var r in _rocks)
            Graphics.DrawTextureEx(Sprites.Rock!, (int)r.X - r.Size / 2, (int)r.Y - r.Size / 2, r.Size, r.Size, r.Angle);
        foreach (var p in _players) DrawDrone(p, p.X, p.Y, 48);
        foreach (var q in _particles)
            Graphics.FillCircle((int)q.X, (int)q.Y, 1 + q.Life / 10, q.Color);

        // top bar: every player's points and lives, the time in the middle
        Graphics.FillRect(0, 0, 1280, Field.Top, Color.RGB(18, 20, 34));
        for (int i = 0; i < _players.Count; i++)
        {
            var p = _players[i];
            int x = 30 + i * 230;
            if (i >= 2) x += 340;
            DrawDrone(p, x + 18, 40, 36);
            hud.Text(x + 44, 14, p.Points.ToString(), p.Out ? Theme.Muted : Theme.Text, 30);
            for (int l = 0; l < 3; l++)
                Graphics.FillCircle(x + 50 + l * 18, 60, 6, l < p.Lives ? p.Tint : Theme.Track);
        }
        int seconds = (_timeLeft + 59) / 60;
        hud.TextCentered(640, 12, seconds.ToString(), seconds <= 10 ? Theme.Bad : Theme.Text, 40);
        hud.TextCentered(640, 56, _wave > 0 ? "Welle " + _wave : "", Theme.Muted, 18);

        if (_bannerFrames > 0)
            hud.TextCentered(640, 300, _banner, Theme.Text, 72);
        if (_demo && _phase != Phase.Results)
            hud.TextCentered(640, 676, "Demo  ·  A  zum Titel", Theme.Muted, 22);
    }

    private void DrawTitle()
    {
        var hud = _hud!;
        hud.TextCentered(640, 110, "FUNKENJAGD", Theme.Accent, 96);
        hud.TextCentered(640, 220, "Fange Funken, weiche den Meteoren aus", Theme.Muted, 26);

        // the players who would join, flying in a circle
        int joined = 0;
        for (int p = 0; p < 4; p++)
        {
            if (p > 0 && !Input.IsConnected(p)) continue;
            joined++;
            var demo = new Player(p, Tints[p], 0, 0);
            float a = (_frame + p * 40) / 40f;
            demo.Vx = (float)Math.Cos(a) * 3;
            DrawDrone(demo, 640 + (p - 1.5f) * 120 + (float)Math.Sin(a) * 14, 320 + (float)Math.Cos(a) * 10, 64);
        }
        hud.TextCentered(640, 380, joined + " Spieler bereit", Theme.Text, 24);

        // high score list
        Graphics.FillRoundedRect(390, 430, 500, 220, 18, Theme.Card);
        hud.TextCentered(640, 444, "Bestenliste", Theme.Muted, 22);
        var best = Highscores.Table.Best;
        if (best.Count == 0) hud.TextCentered(640, 530, "noch leer", Theme.Muted, 22);
        for (int i = 0; i < best.Count; i++)
        {
            int y = 484 + i * 32;
            hud.Text(420, y, (i + 1) + ".", Theme.Muted, 22);
            hud.Text(460, y, best[i].Name, Theme.Text, 22);
            hud.TextRight(780, y, best[i].Points.ToString(), Theme.Text, 22);
            hud.TextRight(860, y, best[i].Players + "P", Theme.Muted, 18);
        }

        hud.TextCentered(640, 676, "A  Start      Y  Controller      +  Beenden", Theme.Muted, 22);
    }

    private void DrawResults()
    {
        var hud = _hud!;
        Graphics.FillRectAlpha(0, 0, 1280, 720, Color.RGB(0, 0, 0), 150);
        hud.TextCentered(640, 120, "Runde vorbei", Theme.Text, 64);

        // ranking: most points first
        var order = new List<Player>(_players);
        for (int i = 1; i < order.Count; i++)
            for (int j = i; j > 0 && order[j].Points > order[j - 1].Points; j--)
            {
                var t = order[j];
                order[j] = order[j - 1];
                order[j - 1] = t;
            }
        for (int i = 0; i < order.Count; i++)
        {
            var p = order[i];
            int y = 240 + i * 70;
            string medal = i < 3 ? ((Medal)i).ToString() : "";
            hud.Text(360, y, medal, i == 0 ? Theme.Warn : Theme.Muted, 28);
            DrawDrone(p, 520, y + 18, 48);
            hud.Text(570, y, p.Name, Theme.Text, 30);
            hud.TextRight(920, y, p.Points + " Punkte", Theme.Text, 30);
        }
        if (_place > 0) hud.TextCentered(640, 560, "Platz " + _place + " der Bestenliste!", Theme.Good, 30);
        hud.TextCentered(640, 650, _demo ? "Demo" : "A  Nochmal      B  Titel", Theme.Muted, 24);
    }
}
