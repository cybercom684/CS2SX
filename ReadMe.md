# CS2SX — C# to Nintendo Switch Transpiler

CS2SX transpiliert C#-Quellcode zu C und kompiliert ihn via DevkitPro zu einer Nintendo Switch Homebrew `.nro`-Datei.

---

## Voraussetzungen

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [DevkitPro](https://devkitpro.org/wiki/Getting_Started) mit `devkitA64` und `libnx`
- Umgebungsvariable `DEVKITPRO` muss gesetzt sein

---

## Installation

```bash
dotnet tool install --global --add-source ./bin/Release CS2SX
```

## Update

```bash
dotnet pack -c Release
dotnet tool update --global --add-source ./bin/Release CS2SX
```

---

## Verwendung

```bash
# Neues Projekt erstellen
cs2sx new MeinProjekt

# Projekt bauen
cs2sx build MeinProjekt/MeinProjekt.csproj

# LibNX-Stubs generieren (optional)
cs2sx genstubs <libnx-include> <output>
```

Die fertige `.nro`-Datei liegt danach im Projektverzeichnis und kann direkt auf die Switch SD-Karte kopiert werden.

> Der Build ist **inkrementell** — nur geänderte `.cs`-Dateien werden neu transpiliert. Unveränderte Dateien werden übersprungen.

---

## Beispiel-Apps

### Console-App (Text-UI)

```csharp
public class MyApp : SwitchApp
{
    private Label _label;
    private Button _button;
    private List<int> _values;

    public override void OnInit()
    {
        _values ??= new List<int>();

        if (!Directory.Exists("/switch/MeinProjekt"))
            Directory.CreateDirectory("/switch/MeinProjekt");

        _label = new Label("Hello from C#!");
        _label.X = 5;
        _label.Y = 5;
        Form.Add(_label);

        _button = new Button("Press A");
        _button.X = 5;
        _button.Y = 8;
        _button.OnClick = OnPress;
        Form.Add(_button);
    }

    public override void OnFrame()
    {
    }

    public void OnPress()
    {
        _values.Add(_values.Count);
        _label.Text = $"Pressed {_values.Count} times!";
        File.WriteAllText("/switch/MeinProjekt/save.txt",
            _values.Count.ToString());
    }
}
```

### Grafik-App (Framebuffer)

```csharp
public class MyApp : SwitchApp
{
    public override void OnInit()
    {
        // Grafik-Modus aktivieren (1280x720)
        Graphics.Init(1280, 720);
    }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.Black);
        Graphics.DrawText(100, 100, "Hello Switch!", Color.White, 2);
        Graphics.FillRect(100, 200, 200, 50, Color.Blue);
        Graphics.DrawRect(100, 200, 200, 50, Color.White);
        Graphics.DrawLine(0, 0, 400, 400, Color.Red);
        Graphics.FillCircle(640, 360, 80, Color.Green);
        Graphics.DrawCircle(640, 360, 80, Color.White);
    }
}
```

> **Wichtig:** Wird `Graphics.Init()` in `OnInit()` aufgerufen, wechselt CS2SX automatisch in den Framebuffer-Modus. Ohne `Graphics.Init()` läuft die App im Console/ANSI-Modus.

> **Wichtig:** **Eine Klasse pro `.cs`-Datei.** Der Transpiler verarbeitet jede Datei separat.

---

## Unterstützte Features

### Typen & Collections

| Feature | Status | Hinweis |
|---|---|---|
| `string` | ✅ | als `const char*` |
| `int`, `float`, `bool`, `char` | ✅ | direkt gemappt |
| `u8`, `u16`, `u32`, `u64` | ✅ | libnx-Typen |
| `List<T>` | ✅ | `Add`, `Remove`, `Clear`, `Contains`, Index-Zugriff |
| `List<string>` | ✅ | `foreach`, `string.Join`, `string.Split` |
| `Dictionary<K,V>` | ✅ | `Add`, `Remove`, `ContainsKey`, `TryGetValue`, Indexer |
| `StringBuilder` | ✅ | `Append`, `AppendLine`, `Clear`, `ToString`, `Insert`, `Replace`, `IndexOf` |

### String-Methoden

| Methode | Status |
|---|---|
| `Trim`, `TrimStart`, `TrimEnd` | ✅ |
| `ToUpper`, `ToLower` | ✅ |
| `Replace`, `Substring`, `IndexOf`, `LastIndexOf` | ✅ |
| `StartsWith`, `EndsWith`, `Contains`, `Equals` | ✅ |
| `PadLeft`, `PadRight` | ✅ |
| `Split`, `string.Join` | ✅ |
| `string.Format`, `string.Concat` | ✅ |
| `IsNullOrEmpty`, `IsNullOrWhiteSpace` | ✅ |
| String-Interpolation `$"..."` | ✅ |

### Parsing

| Methode | Status |
|---|---|
| `int.Parse(s)` | ✅ |
| `int.TryParse(s, out val)` | ✅ |
| `float.Parse(s)` | ✅ |
| `float.TryParse(s, out val)` | ✅ |

```csharp
int val = int.Parse("42");

int result = 0;
if (int.TryParse(someString, out result))
{
    // result enthält den geparsten Wert
}
```

### Grafik (Framebuffer)

Aktivierung: `Graphics.Init(1280, 720)` in `OnInit()` aufrufen.

| Methode | Status | Beschreibung |
|---|---|---|
| `Graphics.Init(w, h)` | ✅ | Framebuffer-Modus aktivieren |
| `Graphics.FillScreen(color)` | ✅ | Bildschirm füllen |
| `Graphics.SetPixel(x, y, color)` | ✅ | Einzelnen Pixel setzen |
| `Graphics.DrawRect(x, y, w, h, color)` | ✅ | Rechteck-Outline |
| `Graphics.FillRect(x, y, w, h, color)` | ✅ | Gefülltes Rechteck |
| `Graphics.DrawLine(x0, y0, x1, y1, color)` | ✅ | Linie (Bresenham) |
| `Graphics.DrawCircle(cx, cy, r, color)` | ✅ | Kreis-Outline (Midpoint) |
| `Graphics.FillCircle(cx, cy, r, color)` | ✅ | Gefüllter Kreis |
| `Graphics.DrawText(x, y, text, color, scale)` | ✅ | Text (8x8 Bitmap-Font) |
| `Graphics.DrawChar(x, y, c, color, scale)` | ✅ | Einzelnes Zeichen |
| `Graphics.MeasureTextWidth(text, scale)` | ✅ | Text-Breite in Pixeln |
| `Graphics.MeasureTextHeight(scale)` | ✅ | Text-Höhe in Pixeln |
| `Graphics.DrawTexture(tex, x, y)` | ✅ | Texture rendern |

### Farb-Konstanten

```csharp
Color.Black   Color.White   Color.Red     Color.Green
Color.Blue    Color.Yellow  Color.Cyan    Color.Magenta
Color.Gray    Color.Orange

// Eigene Farben
uint myColor = Color.RGB(255, 128, 0);
uint myColorA = Color.RGBA(255, 128, 0, 200);
```

### File I/O (SD-Karte)

Alle Pfade müssen absolut sein und mit `/switch/` beginnen.

| Methode | Status | Beschreibung |
|---|---|---|
| `File.ReadAllText(path)` | ✅ | Datei lesen |
| `File.WriteAllText(path, content)` | ✅ | Datei schreiben (überschreibt) |
| `File.AppendAllText(path, content)` | ✅ | An Datei anhängen |
| `File.Exists(path)` | ✅ | Prüft ob Datei existiert |
| `File.Delete(path)` | ✅ | Datei löschen |
| `File.Copy(src, dst)` | ✅ | Datei kopieren |
| `Directory.Exists(path)` | ✅ | Prüft ob Verzeichnis existiert |
| `Directory.CreateDirectory(path)` | ✅ | Verzeichnis anlegen |
| `Directory.Delete(path)` | ✅ | Verzeichnis löschen |
| `Directory.GetFiles(path)` | ✅ | Dateien auflisten → `List<string>` |

```csharp
Directory.CreateDirectory("/switch/MeinSpiel");

File.WriteAllText("/switch/MeinSpiel/save.txt", "42;1337");

if (File.Exists("/switch/MeinSpiel/save.txt"))
{
    string content = File.ReadAllText("/switch/MeinSpiel/save.txt");
    List<string> parts = content.Split(";");
    int val = int.Parse(parts[0]);
}
```

### Kontrollfluss

| Feature | Status |
|---|---|
| `if`, `else if`, `else` | ✅ |
| `for`, `foreach`, `while`, `do` | ✅ |
| `switch` | ✅ |
| `break`, `continue`, `return` | ✅ |
| `try` / `catch` | ✅ (via `setjmp`) |
| `??` Null-Coalescing | ✅ |
| `??=` Null-Coalescing-Zuweisung | ✅ |

### Klassen & OOP

| Feature | Status | Hinweis |
|---|---|---|
| Klassen mit Feldern und Methoden | ✅ | → C-Structs |
| Vererbung (einzeln) | ✅ | `SwitchApp`, `Control` als Basis |
| `static`-Felder und -Methoden | ✅ | → globale C-Variablen |
| `override` | ✅ | → Funktionszeiger |
| Eigene Controls (erbt von `Control`) | ✅ | `Draw()` + `Update()` |
| Enums mit Werten | ✅ | |
| `interface` | ❌ | |
| Generics | ❌ | |

### UI & Input (Console-Modus)

| Feature | Status |
|---|---|
| `Label`, `Button`, `ProgressBar` | ✅ |
| Eigene Controls (erbt von `Control`) | ✅ |
| DPad-Navigation zwischen Buttons | ✅ |
| `Input.IsDown`, `IsHeld`, `IsUp` | ✅ |
| Alle `NpadButton`-Werte | ✅ |
| LibNX-Funktionsaufrufe direkt | ✅ |
| libnx Structs als Stack-Variable (`PadState` etc.) | ✅ |

### Nicht unterstützt

| Feature |
|---|
| `async` / `await` |
| LINQ |
| `params`-Parameter (nur teilweise) |
| Tuple-Return / Dekonstruktion |
| `is`-Pattern-Matching |
| `interface` |
| Generics |
| `Console.ReadLine` / Keyboard-Input |

---

## Render-Modi

CS2SX unterstützt zwei exklusive Render-Modi:

| Modus | Aktivierung | Beschreibung |
|---|---|---|
| **Console** | Standard (kein `Graphics.Init`) | ANSI-Terminal, `Label`, `Button`, `ProgressBar` |
| **Framebuffer** | `Graphics.Init(1280, 720)` in `OnInit()` | Direktes Pixel-Rendering, 1280×720 RGBA8888 |

Im Framebuffer-Modus sind Console-Controls (`Label`, `Button`) nicht sichtbar — der gesamte Output läuft über `Graphics.*`.

---

## Eigene Controls schreiben

Klassen die von `Control` erben bekommen automatisch `Draw()` und `Update()` als Funktionszeiger verdrahtet:

```csharp
// ValueMeter.cs
public class ValueMeter : Control
{
    private int _value;
    private int _minValue;
    private int _maxValue;
    private int _width;

    public void SetValue(int v)            { _value    = v; }
    public void SetRange(int min, int max) { _minValue = min; _maxValue = max; }
    public void SetWidth(int w)            { _width    = w; }

    public override void Draw()
    {
        int px = base.X;
        int py = base.Y;
        Console.Write(string.Format("\x1b[{0};{1}H", py, px));

        int half   = _width / 2;
        int filled = _maxValue > 0 ? (_value * half) / _maxValue : 0;

        if (_value < 0)
        {
            int abs = -filled;
            Console.Write(string.Format("<{0}|{1}>", abs, half - abs));
            return;
        }
        Console.Write(string.Format("<{0}|{1}>", half, filled));
    }

    public override void Update(ulong kDown, ulong kHeld)
    {
    }
}
```

Nutzung:

```csharp
_meter = new ValueMeter();
_meter.X = 14;
_meter.Y = 4;
_meter.SetRange(-10, 10);
_meter.SetWidth(20);
Form.Add(_meter);
```

---

## Projektstruktur

```
MeinProjekt/
├── MeinProjekt.csproj
├── cs2sx.json              — Projektkonfiguration
├── Program.cs              — Haupt-App (eine Klasse pro Datei!)
├── MeineKlasse.cs          — weitere Klassen
├── cs2sx_out/              — generierter C-Code (nicht manuell bearbeiten)
└── MeinProjekt.nro         — fertige Switch-Homebrew-Datei
```

`cs2sx.json`:

```json
{
    "name": "MeinProjekt",
    "author": "Dein Name",
    "version": "1.0.0",
    "mainClass": "MyApp",
    "icon": "icon.jpg"
}
```

---

## Architektur

```
CS2SX/
├── Core/
│   ├── TypeRegistry.cs         — einzige Quelle aller Typ-Mappings
│   └── TranspilerContext.cs    — geteilter Zustand, kein globaler State
├── Transpiler/
│   ├── Handlers/               — pluggable Methoden-Aufruf-Handler
│   │   ├── IInvocationHandler.cs
│   │   ├── InvocationHandlerBase.cs
│   │   ├── InvocationDispatcher.cs
│   │   ├── LibNxHandler.cs
│   │   ├── InputHandler.cs
│   │   ├── ConsoleHandler.cs
│   │   ├── FormHandler.cs
│   │   ├── GraphicsHandler.cs
│   │   ├── ColorHandler.cs
│   │   ├── FileHandler.cs
│   │   ├── ParseHandler.cs
│   │   ├── ListHandler.cs
│   │   ├── DictionaryHandler.cs
│   │   ├── StringBuilderHandler.cs
│   │   ├── StringMethodHandler.cs
│   │   ├── FieldMethodHandler.cs
│   │   ├── OwnMethodHandler.cs
│   │   └── MathHandler.cs
│   ├── Strategies/             — Konstruktor-Generierung
│   │   ├── IConstructorStrategy.cs
│   │   ├── SwitchAppConstructorStrategy.cs
│   │   ├── ControlSubclassConstructorStrategy.cs
│   │   └── DefaultConstructorStrategy.cs
│   ├── Writers/
│   │   ├── ExpressionWriter.cs
│   │   ├── StatementWriter.cs
│   │   ├── FormatStringBuilder.cs
│   │   ├── StringEscaper.cs
│   │   └── TypeInferrer.cs
│   ├── CSharpToC.cs
│   └── TypeMapper.cs
├── Build/
│   ├── BuildPipeline.cs        — inkrementeller Build
│   ├── CCompiler.cs
│   ├── EntryPointGenerator.cs
│   ├── NacpBuilder.cs
│   ├── NroBuilder.cs
│   ├── ProjectConfig.cs
│   ├── ProjectCreator.cs
│   └── ProjectReader.cs
└── Runtime/
    ├── switchforms.h           — UI-Controls, Collections, String-Utils, File I/O
    ├── switchforms.c           — globale Variablendefinitionen (g_fb_addr, etc.)
    └── switchapp.h             — SwitchApp-Loop, Framebuffer, Graphics-API
```

### Neuen Feature-Handler hinzufügen

1. Neue Datei `Transpiler/Handlers/MeinHandler.cs` anlegen:

```csharp
public sealed class MeinHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (calleeStr != "Mein.Methode")
            return NotHandled(out result);

        result = "mein_c_aufruf(" + ArgAt(args, 0) + ")";
        return true;
    }
}
```

2. In `InvocationDispatcher.cs` eintragen:

```csharp
new MeinHandler(),
```

### Neuen Typ hinzufügen

Eintrag in `Core/TypeRegistry.cs` in der entsprechenden Kategorie ergänzen — `s_primitives`, `s_controlTypes` oder `s_libNxStructs`.

---

## Bekannte Einschränkungen

- **Ein `SwitchApp`-Subtyp pro Projekt** — der Einstiegspunkt wird automatisch erkannt
- **Keine verschachtelten Klassen** — jede Klasse in einer eigenen `.cs`-Datei
- **`string`-Puffer begrenzt** — interne statische Puffer sind 512 Bytes, Dateipuffer 8192 Bytes
- **Bitmap-Font 8x8** — `Graphics.DrawText` nutzt einen eingebauten 8×8-Pixel-Font ohne Anti-Aliasing
- **Kein Heap-GC** — allokierte Objekte (`*_New()`) müssen manuell freigegeben werden wenn nötig

---

## Lizenz

MIT