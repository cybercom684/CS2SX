# Beispiel 3: Einfaches Spiel — Ball fangen

Ein vollständiges kleines Spiel: Ein Ball fällt von oben herab, ein Schläger muss ihn fangen. Demonstriert Spielschleife, Kollisionserkennung, Punkte und Highscore-Speicherung.

---

## Code

```csharp
// BallFangen.cs
public class BallFangen : SwitchApp
{
    // Bildschirmgröße
    private const int Breite  = 1280;
    private const int Höhe    = 720;

    // Ball
    private float _ballX, _ballY;
    private float _ballVX, _ballVY;
    private const int BallRadius = 15;

    // Schläger
    private float _schlägerX;
    private const int SchlägerBreite = 160;
    private const int SchlägerHöhe  = 18;
    private const int SchlägerY     = 650;
    private const float SchlägerSpeed = 8.0f;

    // Spielzustand
    private int _punkte     = 0;
    private int _highscore  = 0;
    private int _leben      = 3;
    private bool _spielVorbei = false;

    private const string SavePfad = "/switch/BallFangen/save.txt";

    public override void OnInit()
    {
        Graphics.Init(Breite, Höhe);
        LadeHighscore();
        SpielNeustart();
    }

    private void SpielNeustart()
    {
        _ballX  = Breite / 2;
        _ballY  = 100;
        _ballVX = 3.5f;
        _ballVY = 4.0f;
        _schlägerX = (Breite - SchlägerBreite) / 2;
        _spielVorbei = false;
    }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.RGB(10, 10, 20));

        if (_spielVorbei)
        {
            ZeigSpielVorbei();
            return;
        }

        SchlägerBewegen();
        BallBewegen();
        Kollision();
        Zeichnen();
    }

    private void SchlägerBewegen()
    {
        if (Input.IsHeld(NpadButton.Left) || Input.IsHeld(NpadButton.StickLLeft))
            _schlägerX -= SchlägerSpeed;
        if (Input.IsHeld(NpadButton.Right) || Input.IsHeld(NpadButton.StickLRight))
            _schlägerX += SchlägerSpeed;

        _schlägerX = Math.Clamp(_schlägerX, 0, Breite - SchlägerBreite);
    }

    private void BallBewegen()
    {
        _ballX += _ballVX;
        _ballY += _ballVY;

        // Seitenwände
        if (_ballX - BallRadius < 0)      { _ballX = BallRadius;           _ballVX = Math.Abs(_ballVX); }
        if (_ballX + BallRadius > Breite)  { _ballX = Breite - BallRadius;  _ballVX = -Math.Abs(_ballVX); }

        // Decke
        if (_ballY - BallRadius < 0)       { _ballY = BallRadius;            _ballVY = Math.Abs(_ballVY); }

        // Ball fällt durch Boden → Leben verlieren
        if (_ballY > Höhe + 50)
        {
            _leben--;
            if (_leben <= 0)
            {
                SpeichereHighscore();
                _spielVorbei = true;
            }
            else
            {
                // Ball zurücksetzen
                _ballX  = Breite / 2;
                _ballY  = 100;
                _ballVY = Math.Abs(_ballVY);
            }
        }
    }

    private void Kollision()
    {
        // Schläger-Kollision
        bool trifftX = _ballX + BallRadius > _schlägerX
                    && _ballX - BallRadius < _schlägerX + SchlägerBreite;
        bool trifftY = _ballY + BallRadius > SchlägerY
                    && _ballY - BallRadius < SchlägerY + SchlägerHöhe;

        if (trifftX && trifftY && _ballVY > 0)
        {
            _ballVY = -Math.Abs(_ballVY);
            _punkte++;

            // Ball schneller mit jedem Punkt
            if (_punkte % 5 == 0)
            {
                _ballVX *= 1.1f;
                _ballVY *= 1.1f;
            }
        }
    }

    private void Zeichnen()
    {
        // HUD
        Graphics.FillRect(0, 0, Breite, 45, Color.RGB(20, 20, 40));
        Graphics.DrawText(20,  12, $"Punkte: {_punkte}",             Color.White, 2);
        Graphics.DrawText(500, 12, $"Leben: {new string('♥', _leben)}", Color.Red,   2);
        Graphics.DrawText(900, 12, $"Highscore: {_highscore}",       Color.Yellow, 2);

        // Schläger
        Graphics.FillRoundedRect(
            (int)_schlägerX, SchlägerY,
            SchlägerBreite, SchlägerHöhe,
            6, Color.RGB(80, 160, 255));

        // Ball
        Graphics.FillCircle((int)_ballX, (int)_ballY, BallRadius, Color.White);

        // Hinweis
        Graphics.DrawText(20, 685, "Links/Rechts oder Stick zum Bewegen   + = Beenden", Color.Gray, 1);

        if (Input.IsDown(NpadButton.Plus))
            Environment.Exit(0);
    }

    private void ZeigSpielVorbei()
    {
        string titel  = "SPIEL VORBEI";
        string score  = $"Punkte: {_punkte}";
        string hiscore = $"Highscore: {_highscore}";
        string wieder = "A = Nochmal   + = Beenden";

        int tw = Graphics.MeasureTextWidth(titel, 4);
        Graphics.DrawText((Breite - tw) / 2, 240, titel, Color.Red, 4);

        int sw = Graphics.MeasureTextWidth(score, 2);
        Graphics.DrawText((Breite - sw) / 2, 330, score, Color.White, 2);

        int hw = Graphics.MeasureTextWidth(hiscore, 2);
        Graphics.DrawText((Breite - hw) / 2, 370, hiscore, Color.Yellow, 2);

        int ww = Graphics.MeasureTextWidth(wieder, 1);
        Graphics.DrawText((Breite - ww) / 2, 440, wieder, Color.Gray, 1);

        if (Input.IsDown(NpadButton.A))
        {
            _punkte = 0;
            _leben  = 3;
            SpielNeustart();
        }

        if (Input.IsDown(NpadButton.Plus))
            Environment.Exit(0);
    }

    private void LadeHighscore()
    {
        if (!Directory.Exists("/switch/BallFangen"))
            Directory.CreateDirectory("/switch/BallFangen");

        if (File.Exists(SavePfad))
        {
            string inhalt = File.ReadAllText(SavePfad);
            int.TryParse(inhalt, out _highscore);
        }
    }

    private void SpeichereHighscore()
    {
        if (_punkte > _highscore)
        {
            _highscore = _punkte;
            File.WriteAllText(SavePfad, _highscore.ToString());
        }
    }
}
```

---

## Konzepte in diesem Beispiel

| Konzept | Codezeile |
|---------|-----------|
| Framebuffer-Modus | `Graphics.Init(1280, 720)` |
| Physik-Simulation | `_ballX += _ballVX` + Wandkollision |
| AABB-Kollision | `trifftX && trifftY` Check |
| Spielzustand | `_spielVorbei` Flag + Zustandsmaschine |
| Persistenz | `File.WriteAllText` / `File.ReadAllText` |
| Math-Funktionen | `Math.Clamp`, `Math.Abs` |
| Text zentrieren | `MeasureTextWidth` + `(Breite - tw) / 2` |
| Schwierigkeitsanstieg | Ball wird nach je 5 Punkten schneller |

---

## Erweiterungsideen

- Mehrere Bälle gleichzeitig
- Powerups (breiterer Schläger, Zeitlupe)
- Verschiedene Farben je nach Geschwindigkeit
- Sound via LibNX Audio-API
- Highscore-Liste mit Einträgen
