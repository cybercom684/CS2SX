# Input API

CS2SX liest Controller-Eingaben über die `Input`-Klasse. Alle Buttons des Nintendo Switch Joy-Con und Pro Controllers werden unterstützt.

---

## Grundfunktionen

```csharp
// Taste wurde in diesem Frame gedrückt (einmalig)
bool gedrueckt = Input.IsDown(NpadButton.A);

// Taste wird gehalten (jeder Frame solange gedrückt)
bool gehalten = Input.IsHeld(NpadButton.Right);

// Taste wurde in diesem Frame losgelassen
bool losgelassen = Input.IsUp(NpadButton.B);
```

---

## Button-Referenz

| Button | Beschreibung |
|--------|-------------|
| `NpadButton.A` | A-Taste (rechts) |
| `NpadButton.B` | B-Taste (unten) |
| `NpadButton.X` | X-Taste (oben) |
| `NpadButton.Y` | Y-Taste (links) |
| `NpadButton.L` | Linke Schultertaste |
| `NpadButton.R` | Rechte Schultertaste |
| `NpadButton.ZL` | Linker Trigger |
| `NpadButton.ZR` | Rechter Trigger |
| `NpadButton.Plus` | Plus-Taste (Start) |
| `NpadButton.Minus` | Minus-Taste (Select) |
| `NpadButton.Up` | D-Pad Oben |
| `NpadButton.Down` | D-Pad Unten |
| `NpadButton.Left` | D-Pad Links |
| `NpadButton.Right` | D-Pad Rechts |
| `NpadButton.StickL` | Linker Stick gedrückt |
| `NpadButton.StickR` | Rechter Stick gedrückt |
| `NpadButton.StickLUp` | Linker Stick nach oben |
| `NpadButton.StickLDown` | Linker Stick nach unten |
| `NpadButton.StickLLeft` | Linker Stick nach links |
| `NpadButton.StickLRight` | Linker Stick nach rechts |
| `NpadButton.StickRUp` | Rechter Stick nach oben |
| `NpadButton.StickRDown` | Rechter Stick nach unten |
| `NpadButton.StickRLeft` | Rechter Stick nach links |
| `NpadButton.StickRRight` | Rechter Stick nach rechts |

---

## Typische Muster

### App beenden

```csharp
public override void OnFrame()
{
    if (Input.IsDown(NpadButton.Plus))
        Environment.Exit(0);
}
```

### Menü-Navigation

```csharp
private int _auswahl = 0;
private string[] _optionen = { "Spiel starten", "Optionen", "Beenden" };

public override void OnFrame()
{
    if (Input.IsDown(NpadButton.Down))
        _auswahl = Math.Min(_auswahl + 1, _optionen.Length - 1);

    if (Input.IsDown(NpadButton.Up))
        _auswahl = Math.Max(_auswahl - 1, 0);

    if (Input.IsDown(NpadButton.A))
        AuswahlBestätigen(_auswahl);
}
```

### Figur bewegen (gehalten)

```csharp
private int _x = 640, _y = 360;
private const int Geschwindigkeit = 5;

public override void OnFrame()
{
    if (Input.IsHeld(NpadButton.StickLRight)) _x += Geschwindigkeit;
    if (Input.IsHeld(NpadButton.StickLLeft))  _x -= Geschwindigkeit;
    if (Input.IsHeld(NpadButton.StickLDown))  _y += Geschwindigkeit;
    if (Input.IsHeld(NpadButton.StickLUp))    _y -= Geschwindigkeit;

    // Bildschirmgrenzen
    _x = Math.Clamp(_x, 0, 1280);
    _y = Math.Clamp(_y, 0, 720);
}
```

### IsDown vs IsHeld

```csharp
// IsDown: Aktion genau einmal auslösen
if (Input.IsDown(NpadButton.A))
    Springen();   // Sprung startet nur beim ersten Frame

// IsHeld: Dauerhaft während Button gedrückt ist
if (Input.IsHeld(NpadButton.ZR))
    Schießen();   // Schießt jeden Frame
```

---

## Analog-Sticks

```csharp
StickPos left  = Input.GetStickLeft();
StickPos right = Input.GetStickRight();

// Rohwerte: -32767 .. +32767
// x: links=negativer Wert, rechts=positiver Wert
// y: unten=negativer Wert, oben=positiver Wert
if (left.x > 5000) Graphics.DrawText(100, 100, "Stick rechts", Color.Green, 2);
if (left.y < -5000) Graphics.DrawText(100, 130, "Stick unten", Color.Red, 2);

// Normierte Werte: 0..100 (Betrag)
int stärkeX = left.NormX;
int stärkeY = left.NormY;
```

---

## Touch-Screen

```csharp
TouchState touch = Input.GetTouch();

// Anzahl aktiver Finger
if (touch.count > 0)
{
    // Position des ersten Fingers
    int x = touch.X0;
    int y = touch.Y0;
    Graphics.FillCircle(x, y, 20, Color.Red);
}

// Treffer-Test für einen Bereich (idx = Finger-Index)
if (touch.HitRect(0, 100, 200, 80, 40))  // Finger 0 in Rect(100,200,80×40)?
    DoSomething();

// Alle aktiven Finger durchlaufen
for (int i = 0; i < touch.count; i++)
    Graphics.FillCircle(touch.x[i], touch.y[i], 15, Color.Blue);
```

> Touch und Sticks stehen sowohl über `Input.GetTouch()` / `Input.GetStickLeft()` als auch automatisch als `_touch` / `_stickL` in `SwitchAppEx`-Subklassen zur Verfügung.

---

## Input in beliebigen Klassen

`Input.IsDown()`, `Input.IsHeld()` und `Input.IsUp()` funktionieren in **jeder Klasse** — nicht nur in direkten `SwitchApp`-Unterklassen. Buttons werden über globale Frame-Variablen (`g_cs2sx_kDown/kHeld/kUp`) bereitgestellt, die der App-Loop jede Frame aktualisiert.

```csharp
// Funktioniert in SwitchApp, Hilfsklassen, UI-Komponenten, überall
public class MeinPanel
{
    public void Update()
    {
        if (Input.IsDown(NpadButton.A))
            Aktion();
    }
}
```

Direkten Zugriff auf `kDown`/`kHeld` vermeiden — die Instanzfelder existieren nur in `SwitchApp` selbst:

```csharp
// FALSCH — kDown ist kein globales Feld
if ((kDown & NpadButton.A) != 0) { }

// RICHTIG — immer die Input-Methoden verwenden
if (Input.IsDown(NpadButton.A)) { }
```

## Sauberes Beenden der App

`Environment.Exit(0)` bricht den Prozess hart ab und überspringt libnx-Cleanup. Auf echter Hardware erscheint die Fehlermeldung „Software wurde wegen eines Fehlers beendet".

```csharp
// FALSCH — harter Abbruch, Fehlermeldung auf Hardware
if (Input.IsDown(NpadButton.Plus))
    Environment.Exit(0);

// RICHTIG — Quit-Flag setzen, Loop endet sauber
private bool _quit;

public override void OnFrame()
{
    if (Input.IsDown(NpadButton.Plus))
        _quit = true;

    if (_quit) return;   // appletMainLoop() gibt beim nächsten Aufruf false zurück

    // ... restlicher Frame-Code
}
```
