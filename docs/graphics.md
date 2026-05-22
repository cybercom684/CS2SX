# Graphics API

CS2SX stellt über die `Graphics`-Klasse und `Color`-Klasse eine vollständige 2D-Framebuffer-API zur Verfügung.

---

## Initialisierung

```csharp
public override void OnInit()
{
    Graphics.Init(1280, 720);  // Switch Native Resolution
}
```

Sobald `Graphics.Init()` aufgerufen wurde, rendert jeder `OnFrame()`-Aufruf in den Framebuffer. Konsolen-Ausgabe (`Console.WriteLine`) ist danach nicht mehr sichtbar.

---

## Farben

Farben sind `uint`-Werte im Format `ABGR` (Alpha, Blue, Green, Red).

```csharp
// Vordefinierte Farben
uint schwarz  = Color.Black;
uint weiß     = Color.White;
uint rot      = Color.Red;
uint grün     = Color.Green;
uint blau     = Color.Blue;
uint gelb     = Color.Yellow;
uint cyan     = Color.Cyan;
uint magenta  = Color.Magenta;
uint grau     = Color.Gray;
uint orange   = Color.Orange;

// Eigene Farben
uint lila   = Color.RGB(128, 0, 255);
uint transparent = Color.RGBA(255, 0, 0, 128);  // 50% Transparenz
```

---

## Zeichenfunktionen

### Bildschirm

```csharp
// Gesamten Bildschirm mit Farbe füllen
Graphics.FillScreen(Color.Black);
```

### Rechtecke

```csharp
// Gefülltes Rechteck
Graphics.FillRect(x, y, breite, höhe, Color.Blue);

// Nur Rahmen
Graphics.DrawRect(x, y, breite, höhe, Color.White);

// Abgerundete Ecken
Graphics.FillRoundedRect(x, y, breite, höhe, radius, Color.Red);
```

### Linien und Punkte

```csharp
// Linie von (x0, y0) bis (x1, y1)
Graphics.DrawLine(0, 0, 1280, 720, Color.White);

// Einzelner Pixel
Graphics.SetPixel(640, 360, Color.Red);
```

### Kreise und Ellipsen

```csharp
// Gefüllter Kreis (cx, cy = Mittelpunkt)
Graphics.FillCircle(640, 360, 80, Color.Green);

// Nur Kreisrahmen
Graphics.DrawCircle(640, 360, 80, Color.Green);

// Gefüllte Ellipse
Graphics.FillEllipse(640, 360, 120, 60, Color.Blue);
```

### Dreiecke

```csharp
// Gefülltes Dreieck (drei Eckpunkte)
Graphics.FillTriangle(200, 100, 300, 300, 100, 300, Color.RGB(255, 128, 0));
```

---

## Text

```csharp
// Text zeichnen (scale = Schriftgröße-Faktor)
Graphics.DrawText(x, y, "Hallo Switch!", Color.White, 2);

// Text mit Schatten
Graphics.DrawTextShadow(x, y, "Schatten!", Color.White, Color.Black, 2);

// Einzelnes Zeichen
Graphics.DrawChar(x, y, 'A', Color.White, 2);

// Textbreite messen (für Zentrierung)
int breite = Graphics.MeasureTextWidth("Hallo", 2);
int höhe   = Graphics.MeasureTextHeight(2);
```

**Schriftgrößen (scale):**
| Scale | Ungefähre Pixelhöhe |
|-------|---------------------|
| 1 | ~16 px |
| 2 | ~32 px |
| 3 | ~48 px |

---

## Texturen

```csharp
// Textur laden (z.B. aus dem App-Verzeichnis)
var tex = new Texture("/switch/MeineApp/bild.png");

// Textur zeichnen
Graphics.DrawTexture(tex, x, y);
```

---

## Vollständiges Grafik-Beispiel

```csharp
public class GrafikApp : SwitchApp
{
    private int _winkelGrad = 0;

    public override void OnInit()
    {
        Graphics.Init(1280, 720);
    }

    public override void OnFrame()
    {
        // Hintergrund löschen
        Graphics.FillScreen(Color.Black);

        // Kopfzeile
        Graphics.FillRect(0, 0, 1280, 50, Color.RGB(30, 30, 80));
        Graphics.DrawText(10, 12, "Grafik-Demo", Color.White, 2);

        // Formen
        Graphics.FillRect(100, 100, 200, 100, Color.Blue);
        Graphics.DrawRect(100, 100, 200, 100, Color.White);

        Graphics.FillCircle(640, 360, 60, Color.Green);
        Graphics.FillTriangle(900, 150, 1100, 150, 1000, 300, Color.RGB(255, 128, 0));
        Graphics.FillRoundedRect(100, 400, 300, 80, 15, Color.RGB(80, 80, 200));

        // Info-Text
        Graphics.DrawText(100, 600, "Drücke + zum Beenden", Color.Gray, 1);

        // Beenden
        if (Input.IsDown(NpadButton.Plus))
            Environment.Exit(0);
    }
}
```

---

## Koordinatensystem

```
(0,0) ────────────────────── (1280,0)
  │                               │
  │   Switch Display 1280×720     │
  │                               │
(0,720) ─────────────────── (1280,720)
```

Der Ursprung (0,0) liegt oben links. X wächst nach rechts, Y wächst nach unten.

---

## Tipps

**Text zentrieren:**
```csharp
string text = "Spieler gewinnt!";
int tw = Graphics.MeasureTextWidth(text, 2);
int tx = (1280 - tw) / 2;
Graphics.DrawText(tx, 340, text, Color.Yellow, 2);
```

**Einfaches Fade-to-Black:**
```csharp
private int _alpha = 255;

// In OnFrame():
uint overlay = Color.RGBA(0, 0, 0, (byte)_alpha);
Graphics.FillScreen(overlay);
_alpha = Math.Max(0, _alpha - 5);
```
