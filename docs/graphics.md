# Graphics API

CS2SX stellt über die `Graphics`-Klasse und `Color`-Klasse eine vollständige 2D-API zur Verfügung.
Gezeichnet wird auf der **GPU** (deko3d): Alle Aufrufe eines Frames werden gesammelt und in
wenigen Draw-Calls gerendert. Runde Formen und Linien sind kantengeglättet, Farben mit
Alpha werden echt überblendet, und eine höhere Ausgabeauflösung (1080p) kostet keine
zusätzliche Rechenzeit.

---

## Initialisierung

```csharp
public override void OnInit()
{
    Graphics.Init(1280, 720);  // Switch Native Resolution
}
```

Sobald `Graphics.Init()` aufgerufen wurde, rendert jeder `OnFrame()`-Aufruf auf die GPU; der Bildschirm wird vor jedem Frame schwarz gelöscht. Konsolen-Ausgabe (`Console.WriteLine`) ist danach nicht mehr sichtbar. `Graphics.BeginFrame()`/`EndFrame()` sind nicht nötig (die Runtime steuert die Frames) und bewirken nichts.

Alle Koordinaten sind **logisch** (die Größe aus `Graphics.Init`). Ausnahmen in **physischen** Ausgabe-Pixeln: `SetPixel`, `SetPixelAlpha`, `BlitPhysical`. Umrisse (`DrawRect`, `DrawCircle`, …) sind einen physischen Pixel breit.

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

`Graphics.LoadTexture` lädt 24-/32-bit **BMP** eingebaut. Für **PNG/JPEG/GIF/TGA** gibt es `Graphics.LoadImage` (benötigt die `stb_image`-Extern-Lib, siehe [Externe Bibliotheken](advanced/external-libs.md)). Dateien im `romfs/`-Ordner werden automatisch in die `.nro` eingebettet.

```csharp
// Aus romfs laden (empfohlen — in .nro eingebettet)
Texture tex = Graphics.LoadTexture("romfs:/bild.bmp");

// Aus absolutem Dateipfad laden
Texture tex2 = Graphics.LoadTexture("/switch/MeineApp/bild.bmp");

// An fester Position rendern
Graphics.DrawTexture(tex, x, y);

// Zentriert in einer Bounding-Box rendern
Graphics.DrawTextureCentered(tex, rx, ry, rw, rh);

// Auf Zielgröße (tw × th Pixel) skalieren
Graphics.DrawTextureScaled(tex, x, y, tw, th);

// Skaliert und in Bounding-Box zentriert (nützlich für Icons)
Graphics.DrawTextureCenteredScaled(tex, rx, ry, rw, rh, tw, th);
```

| Methode | Beschreibung |
|---|---|
| `Graphics.LoadTexture(path)` | **BMP** (24/32-bit) laden → `Texture` (oder `null`) — eingebaut |
| `Graphics.LoadImage(path)` | **PNG/JPEG/BMP/GIF/TGA** laden → `Texture` — benötigt die `stb_image`-Extern-Lib |
| `Graphics.LoadImageThumb(path, maxSize)` | Bild laden **und** auf `maxSize` herunterskalieren (Thumbnail) — `stb_image` |
| `Graphics.ScaleTextureSmooth(src, dw, dh)` | Texture bilinear auf dw × dh resampeln → neue `Texture` |
| `Graphics.DrawTexture(tex, x, y)` | Texture an Position (x, y) rendern |
| `Graphics.DrawTextureCentered(tex, rx, ry, rw, rh)` | Zentriert in Bounding-Box |
| `Graphics.DrawTextureScaled(tex, x, y, tw, th)` | Skaliert auf tw × th Pixel |
| `Graphics.DrawTextureCenteredScaled(tex, rx, ry, rw, rh, tw, th)` | Skaliert + zentriert |

> `null`-Check empfohlen: `if (tex != null) Graphics.DrawTexture(tex, x, y);`

Texturen werden beim ersten Zeichnen automatisch auf die GPU geladen (auch wenn sie schon in
`OnInit` erzeugt wurden). Änderst du später `tex.Pixels`, mache die Änderung mit
`tex.Update()` sichtbar. Gib Texturen mit `tex.Dispose()` frei — mehrfaches `Dispose` ist
unschädlich. Halbtransparente Pixel werden überblendet (früher: nur ganz transparent/deckend).

### Auflösung & weitere Primitive

```csharp
// VOR Graphics.Init() aufrufen: in 1080p ausgeben (gedockt), Logik bleibt 1280×720
Graphics.SetOutputResolution(1920, 1080);

// geschlossener Umriss durch die Punkte (xs[i], ys[i])
int[] xs = { 100, 160, 190, 150, 90 };
int[] ys = { 200, 190, 240, 285, 260 };
Graphics.DrawPolygon(xs, ys, xs.Length, Color.White);
```

`SetOutputResolution(w, h)` rendert intern hochskaliert (z.B. 1080p) während alle Zeichenkoordinaten im logischen 1280×720-Raum bleiben.

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
