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
        Graphics.Init(1280, 720);
    }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.Black);
        Graphics.DrawText(100, 100, "Hello Switch!", Color.White, 2);
        Graphics.FillRect(100, 200, 200, 50, Color.Blue);
        Graphics.DrawLine(0, 0, 400, 400, Color.Red);
        Graphics.FillCircle(640, 360, 80, Color.Green);

        // Neue Primitiven
        Graphics.FillTriangle(200, 100, 300, 300, 100, 300, Color.RGB(255, 128, 0));
        Graphics.FillEllipse(640, 360, 120, 60, Color.RGB(80, 200, 120));
        Graphics.FillRoundedRect(50, 50, 300, 150, 20, Color.RGB(60, 60, 180));
        Graphics.DrawTextShadow(100, 400, "Shadow!", Color.White, Color.RGB(0,0,0), 2);
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
| `T?` Nullable-Typen | ✅ | `HasValue`, `Value`, `??`, `?.` |
| `List<T>` | ✅ | `Add`, `Remove`, `Clear`, `Contains`, Index-Zugriff |
| `List<string>` | ✅ | `foreach`, `string.Join`, `string.Split` |
| `Dictionary<K,V>` | ✅ | `Add`, `Remove`, `ContainsKey`, `TryGetValue`, Indexer |
| `StringBuilder` | ✅ | `Append`, `AppendLine`, `Clear`, `ToString`, `Insert`, `Replace`, `IndexOf` |
| `StickPos` | ✅ | Analog-Stick-Position (`x`, `y`) |
| `TouchState` | ✅ | Touch-Screen-Zustand (`count`, `x[]`, `y[]`) |
| `BatteryInfo` | ✅ | Akkustand (`percent`, `charging`, `connected`) |

### Nullable-Typen

```csharp
int? x = null;             // → int* x = NULL;
int? x = 5;                // → int* x = &(int){5};
bool hasVal = x.HasValue;  // → (x != NULL)
int val = x.Value;         // → (*x)
int v = x ?? 0;            // → (x != NULL ? *x : 0)
```

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

---

## Grafik (Framebuffer)

Aktivierung: `Graphics.Init(1280, 720)` in `OnInit()` aufrufen.

### Basis-Primitiven

| Methode | Beschreibung |
|---|---|
| `Graphics.Init(w, h)` | Framebuffer-Modus aktivieren |
| `Graphics.FillScreen(color)` | Bildschirm füllen |
| `Graphics.SetPixel(x, y, color)` | Einzelnen Pixel setzen |
| `Graphics.DrawRect(x, y, w, h, color)` | Rechteck-Outline |
| `Graphics.FillRect(x, y, w, h, color)` | Gefülltes Rechteck |
| `Graphics.DrawLine(x0, y0, x1, y1, color)` | Linie (Bresenham) |
| `Graphics.DrawCircle(cx, cy, r, color)` | Kreis-Outline |
| `Graphics.FillCircle(cx, cy, r, color)` | Gefüllter Kreis |
| `Graphics.DrawText(x, y, text, color, scale)` | Text (8×8 Bitmap-Font) |
| `Graphics.DrawChar(x, y, c, color, scale)` | Einzelnes Zeichen |
| `Graphics.MeasureTextWidth(text, scale)` | Text-Breite in Pixeln |
| `Graphics.MeasureTextHeight(scale)` | Text-Höhe in Pixeln |
| `Graphics.DrawTexture(tex, x, y)` | Texture rendern |

### Erweiterte Primitiven

| Methode | Beschreibung |
|---|---|
| `Graphics.DrawTriangle(x0,y0, x1,y1, x2,y2, color)` | Dreieck-Outline |
| `Graphics.FillTriangle(x0,y0, x1,y1, x2,y2, color)` | Gefülltes Dreieck (Scanline-Fill) |
| `Graphics.DrawEllipse(cx, cy, rx, ry, color)` | Ellipse-Outline |
| `Graphics.FillEllipse(cx, cy, rx, ry, color)` | Gefüllte Ellipse |
| `Graphics.DrawRoundedRect(x, y, w, h, r, color)` | Rechteck mit abgerundeten Ecken |
| `Graphics.FillRoundedRect(x, y, w, h, r, color)` | Gefülltes abgerundetes Rechteck |
| `Graphics.DrawGrid(x, y, w, h, cellW, cellH, color)` | Gitter zeichnen |
| `Graphics.DrawTextShadow(x, y, text, color, shadow, scale)` | Text mit 1px-Schatten |

### Alpha-Blending

| Methode | Beschreibung |
|---|---|
| `Graphics.SetPixelAlpha(x, y, color, alpha)` | Pixel mit Alpha (0=transparent, 255=deckend) |
| `Graphics.FillRectAlpha(x, y, w, h, color, alpha)` | Rechteck mit Alpha |
| `Graphics.DrawTextAlpha(x, y, text, color, scale, alpha)` | Text mit Alpha |

```csharp
// Halbtransparentes Overlay
Graphics.FillRectAlpha(0, 0, 1280, 720, Color.RGB(0, 0, 0), 128);

// Text mit Schatten für bessere Lesbarkeit
Graphics.DrawTextShadow(100, 100, "Score: 42",
    Color.White, Color.RGB(0, 0, 0), 2);
```

### Farb-Konstanten

```csharp
Color.Black   Color.White   Color.Red     Color.Green
Color.Blue    Color.Yellow  Color.Cyan    Color.Magenta
Color.Gray    Color.Orange

// Eigene Farben
uint myColor  = Color.RGB(255, 128, 0);
uint myColorA = Color.RGBA(255, 128, 0, 200);
```

### Texture & IDisposable

```csharp
using (Texture tex = new Texture(64, 64, pixels))
{
    Graphics.DrawTexture(tex, 100, 100);
} // → Texture_Dispose(tex) wird automatisch aufgerufen
```

---

## Input

### Buttons

```csharp
public override void OnFrame()
{
    if (Input.IsDown(NpadButton.A))   { /* einmalig beim Drücken */ }
    if (Input.IsHeld(NpadButton.ZR))  { /* solange gehalten      */ }
    if (Input.IsUp(NpadButton.B))     { /* einmalig beim Loslassen */ }
}
```

Verfügbare Buttons: `A`, `B`, `X`, `Y`, `L`, `R`, `ZL`, `ZR`, `Plus`, `Minus`, `Up`, `Down`, `Left`, `Right`, `StickL`, `StickR` sowie alle `StickL/RUp/Down/Left/Right`-Richtungen.

### Analog-Sticks

```csharp
StickPos left  = Input.GetStickLeft();   // x/y: -32767..+32767
StickPos right = Input.GetStickRight();

if (left.x > 5000)
    Console.WriteLine("Stick rechts");

// Normierter Wert 0..100 (Betrag, Deadzone bereits herausgefiltert)
// int norm = CS2SX_StickNorm(left.x < 0 ? -left.x : left.x);
```

> **Deadzone:** Werte innerhalb von ±3000 werden automatisch auf 0 gesetzt.

> **Achsen:** X negativ = links, positiv = rechts. Y positiv = oben, negativ = unten.

### Touch-Screen

```csharp
TouchState touch = Input.GetTouch();

if (touch.count > 0)
{
    int x = touch.x[0];   // 0..1280
    int y = touch.y[0];   // 0..720
    Graphics.FillCircle(x, y, 20, Color.Red);
}

// Bis zu 10 simultane Berührungspunkte
for (int i = 0; i < touch.count && i < 10; i++)
{
    Graphics.FillCircle(touch.x[i], touch.y[i], 15, Color.Green);
}
```

---

## System

### Akkustand

```csharp
BatteryInfo battery = System.GetBattery();

Graphics.DrawText(10, 10,
    $"Akku: {battery.percent}%  Lädt: {battery.charging}",
    Color.White, 1);
```

| Feld | Typ | Beschreibung |
|---|---|---|
| `percent` | `int` | Ladezustand 0–100 |
| `charging` | `bool` | `true` wenn geladen wird |
| `connected` | `bool` | `true` wenn Ladegerät angesteckt |

> `System.GetBattery()` ruft intern `psmInitialize()` auf — kein manuelles Init nötig.

---

## File I/O (SD-Karte)

Alle Pfade müssen absolut sein und mit `/switch/` beginnen.

### Dateien

| Methode | Beschreibung |
|---|---|
| `File.ReadAllText(path)` | Datei lesen (max. 8192 Bytes) |
| `File.WriteAllText(path, content)` | Datei schreiben (überschreibt) |
| `File.AppendAllText(path, content)` | An Datei anhängen |
| `File.Exists(path)` | Prüft ob Datei existiert |
| `File.Delete(path)` | Datei löschen |
| `File.Copy(src, dst)` | Datei kopieren |

### Verzeichnisse

| Methode | Beschreibung |
|---|---|
| `Directory.Exists(path)` | Prüft ob Verzeichnis existiert |
| `Directory.CreateDirectory(path)` | Verzeichnis anlegen |
| `Directory.Delete(path)` | Verzeichnis löschen |
| `Directory.GetFiles(path, pattern)` | Dateien auflisten → `List<string>` |
| `Directory.GetDirectories(path)` | Unterverzeichnisse auflisten → `List<string>` |
| `Directory.GetEntries(path)` | Dateien + Verzeichnisse → `List<string>` |

### Pfad-Hilfsmethoden

| Methode | Beispiel | Ergebnis |
|---|---|---|
| `Path.GetFileName(path)` | `"/switch/app.nro"` | `"app.nro"` |
| `Path.GetExtension(path)` | `"/switch/app.nro"` | `".nro"` |
| `Path.GetDirectoryName(path)` | `"/switch/app.nro"` | `"/switch"` |
| `Path.Combine(a, b)` | `"/switch"`, `"save.txt"` | `"/switch/save.txt"` |
| `Path.IsDirectory(path)` | `"/switch/mydir"` | `true` |

```csharp
List<string> dirs = Directory.GetDirectories("/switch");
for (int i = 0; i < dirs.Count; i++)
{
    string name = Path.GetFileName(dirs[i]);
    Graphics.DrawText(20, 100 + i * 20, name, Color.White, 1);
}

string savePath = Path.Combine("/switch/MeinSpiel", "save.dat");
File.WriteAllText(savePath, "42");
```

---

## Kontrollfluss

| Feature | Status |
|---|---|
| `if`, `else if`, `else` | ✅ |
| `for`, `foreach`, `while`, `do...while` | ✅ |
| `switch` (Wert und Pattern) | ✅ |
| `break`, `continue`, `return` | ✅ |
| `try` / `catch` | ✅ (via `setjmp`) |
| `using` (mit `IDisposable`) | ✅ |
| `??` Null-Coalescing | ✅ |
| `??=` Null-Coalescing-Zuweisung | ✅ |

---

## Pattern Matching

```csharp
// is-Pattern mit Typ und Binding-Variable
if (obj is Dog d)
{
    d.Bark();
}

// switch-Expression
string label = value switch
{
    0 => "zero",
    1 => "one",
    _ => "other",
};

// Relational Pattern
string category = score switch
{
    >= 90 => "A",
    >= 70 => "B",
    _     => "C",
};

if (x is not null) { ... }
```

| Pattern | Status |
|---|---|
| Konstant (`case 1:`, `1 =>`) | ✅ |
| Discard (`_`) | ✅ |
| `is`-Pattern mit Binding (`obj is Dog d`) | ✅ |
| Relational (`>= 5`, `< 10`) | ✅ |
| `not null` / `is null` | ✅ |
| `and` / `or` Pattern | ✅ |
| `when`-Klausel | ✅ |

---

## Properties

```csharp
// Auto-Property → einfaches Struct-Feld
public int Speed { get; set; }

// Expliziter Body → Player_get_Speed() / Player_set_Speed()
public int Speed
{
    get => _speed * 2;
    set => _speed = value / 2;
}
```

---

## Lambda-Ausdrücke

```csharp
_button.OnClick = () => DoSomething();

Action<int> handler = x => Console.WriteLine($"Value: {x}");
```

Lambdas werden automatisch zu statischen C-Funktionen geliftet. Captures werden als Capture-Struct realisiert.

---

## Klassen & OOP

| Feature | Status | Hinweis |
|---|---|---|
| Klassen mit Feldern und Methoden | ✅ | → C-Structs |
| Vererbung (einzeln) | ✅ | `SwitchApp`, `Control` als Basis |
| `abstract`-Klassen | ✅ | → vtable-Infrastruktur |
| `virtual` / `override` | ✅ | → vtable-Funktionszeiger |
| Eigene Controls (erbt von `Control`) | ✅ | `Draw()` + `Update()` |
| `static`-Felder und -Methoden | ✅ | → globale C-Variablen |
| `IDisposable` / `using` | ✅ | → `Dispose()`-Aufruf am Blockende |
| Enums mit Werten | ✅ | |
| `interface` | ❌ | |
| Generics | ❌ | |

### Vererbung & virtuelle Methoden

```csharp
// Animal.cs
public abstract class Animal
{
    private int _health;
    public abstract void Speak();
    public virtual void Update() { _health++; }
}

// Dog.cs
public class Dog : Animal
{
    public override void Speak()
    {
        Console.WriteLine("Woof!");
    }
}
```

Virtuelle Aufrufe: `animal.Speak()` → `animal->vtable->Speak(animal)`

### Eigene Controls

```csharp
// ValueMeter.cs
public class ValueMeter : Control
{
    private int _value;
    private int _maxValue;
    private int _width;

    public void SetValue(int v)  { _value    = v; }
    public void SetMax(int max)  { _maxValue = max; }
    public void SetWidth(int w)  { _width    = w; }

    public override void Draw()
    {
        int filled = _maxValue > 0 ? (_value * _width) / _maxValue : 0;
        Console.Write(string.Format("\x1b[{0};{1}H[", base.Y, base.X));
        for (int i = 0; i < _width; i++)
            Console.Write(i < filled ? "#" : "-");
        Console.Write("]");
    }

    public override void Update(ulong kDown, ulong kHeld) { }
}
```

```csharp
// Nutzung in OnInit()
_meter = new ValueMeter();
_meter.X = 14;
_meter.Y = 4;
_meter.SetMax(100);
_meter.SetWidth(20);
Form.Add(_meter);
```

---

## Render-Modi

| Modus | Aktivierung | Beschreibung |
|---|---|---|
| **Console** | Standard (kein `Graphics.Init`) | ANSI-Terminal, `Label`, `Button`, `ProgressBar` |
| **Framebuffer** | `Graphics.Init(1280, 720)` in `OnInit()` | Direktes Pixel-Rendering, 1280×720 RGBA8888 |

Im Framebuffer-Modus sind Console-Controls nicht sichtbar — der gesamte Output läuft über `Graphics.*`.

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
│   │   ├── InputExtHandler.cs       — Sticks, Touch
│   │   ├── ConsoleHandler.cs
│   │   ├── FormHandler.cs
│   │   ├── GraphicsHandler.cs
│   │   ├── GraphicsExtHandler.cs    — neue Primitiven, Alpha
│   │   ├── ColorHandler.cs
│   │   ├── FileHandler.cs
│   │   ├── DirectoryExtHandler.cs   — GetDirectories, GetEntries
│   │   ├── PathHandler.cs           — Path.GetFileName, Combine etc.
│   │   ├── SystemExtHandler.cs      — System.GetBattery
│   │   ├── ParseHandler.cs
│   │   ├── ListHandler.cs
│   │   ├── DictionaryHandler.cs
│   │   ├── StringBuilderHandler.cs
│   │   ├── StringMethodHandler.cs
│   │   ├── FieldMethodHandler.cs
│   │   ├── OwnMethodHandler.cs
│   │   └── MathHandler.cs
│   ├── Strategies/
│   │   ├── IConstructorStrategy.cs
│   │   ├── SwitchAppConstructorStrategy.cs
│   │   ├── ControlSubclassConstructorStrategy.cs
│   │   └── DefaultConstructorStrategy.cs
│   ├── Writers/
│   │   ├── ExpressionWriter.cs
│   │   ├── StatementWriter.cs
│   │   ├── FormatStringBuilder.cs
│   │   ├── StringEscaper.cs
│   │   ├── TypeInferrer.cs
│   │   └── NullableAndPatternWriter.cs
│   ├── CSharpToC.cs
│   ├── LambdaLifter.cs
│   ├── PropertyWriter.cs
│   ├── VTableBuilder.cs
│   └── TypeMapper.cs
├── Build/
│   ├── BuildPipeline.cs
│   ├── CCompiler.cs
│   ├── EntryPointGenerator.cs
│   ├── NacpBuilder.cs
│   ├── NroBuilder.cs
│   ├── ProjectConfig.cs
│   ├── ProjectCreator.cs
│   └── ProjectReader.cs
└── Runtime/
    ├── switchforms.h    — UI-Controls, Collections, String-Utils, File I/O
    ├── switchforms.c    — globale Variablendefinitionen
    └── switchapp.h      — SwitchApp-Loop, Framebuffer, Graphics, Input, System
```

### Neuen Feature-Handler hinzufügen

```csharp
// 1. Transpiler/Handlers/MeinHandler.cs anlegen
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

// 2. In InvocationDispatcher.cs eintragen
new MeinHandler(),
```

### Neuen Typ hinzufügen

Eintrag in `Core/TypeRegistry.cs` in der entsprechenden Kategorie ergänzen — `s_primitives`, `s_controlTypes` oder `s_libNxStructs`.

Für Struct-Rückgabetypen (Stack-Allokation, kein Pointer) zusätzlich in `TypeInferrer.cs` → `InferInvocation()` eintragen.

---

## Bekannte Einschränkungen

- **Ein `SwitchApp`-Subtyp pro Projekt** — der Einstiegspunkt wird automatisch erkannt
- **Eine Klasse pro `.cs`-Datei** — keine verschachtelten Klassen
- **`string`-Puffer 512 Bytes** — interne String-Puffer; Dateipuffer 8192 Bytes
- **Bitmap-Font 8×8** — `Graphics.DrawText` nutzt einen eingebauten Font ohne Anti-Aliasing
- **Kein Heap-GC** — allokierte Objekte (`*_New()`) müssen manuell freigegeben werden
- **Lambda-Captures** — nur Werttypen und primitive Captures zuverlässig unterstützt
- **`is`-Typ-Pattern** — erfordert `TypeName_Is()`-Hilfsfunktion in der Runtime
- **Char-Literale in Vergleichen** — `s[i] == '\n'` funktioniert; für komplexe Fälle `int`-Konstanten nutzen (`int nl = 10;`)
- **Statische String-Puffer** — `String_Trim`, `String_ToUpper` etc. nutzen statische interne Puffer; verschachtelte Aufrufe wie `String_Trim(String_ToUpper(s))` können sich gegenseitig überschreiben

---

## Nicht unterstützt

| Feature |
|---|
| `async` / `await` |
| LINQ |
| `params`-Parameter (nur teilweise) |
| Tuple-Return / Dekonstruktion |
| `interface` |
| Generics |
| `Console.ReadLine` / Keyboard-Input |

---

## Lizenz

MIT