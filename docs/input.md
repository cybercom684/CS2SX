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

## Input in Unterklassen

In `SwitchApp`-Unterklassen immer über `Input.IsDown()` und `Input.IsHeld()` arbeiten — nicht direkt über `kDown`/`kHeld`:

```csharp
// FALSCH — kDown direkt in Unterklassen-Methoden
if ((kDown & NpadButton.A) != 0) { }

// RICHTIG
if (Input.IsDown(NpadButton.A)) { }
```
