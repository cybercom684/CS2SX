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

### Neues Projekt erstellen

```bash
cs2sx new MeinProjekt
```

### Projekt bauen

```bash
cs2sx build MeinProjekt
```

Die fertige `.nro`-Datei liegt danach im Projektverzeichnis.

---

## Beispiel-App

```csharp
public class MyApp : SwitchApp
{
    private Label _label;
    private Button _button;
    private List<int> _values;

    public override void OnInit()
    {
        _values ??= new List<int>();

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
    }
}
```

> Wichtig: **eine Klasse pro `.cs`-Datei**. Der Transpiler verarbeitet jede Datei separat.

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
| `abstract class` (außer SwitchApp) | ❌ | |

### UI & Input

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
| `Console.ReadLine` / Keyboard-Input |
| File I/O |

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

    public void SetValue(int v)   { _value    = v; }
    public void SetRange(int min, int max) { _minValue = min; _maxValue = max; }
    public void SetWidth(int w)   { _width    = w; }

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

## Architektur

```
CS2SX/
├── Core/
│   ├── TypeRegistry.cs         — einzige Quelle aller Typ-Mappings
│   └── TranspilerContext.cs    — geteilter Zustand, kein globaler State
├── Transpiler/
│   ├── Handlers/               — pluggable Methoden-Aufruf-Handler
│   │   ├── IInvocationHandler.cs
│   │   ├── InvocationHandlerBase.cs  — gemeinsame Hilfsmethoden (ArgAt, TryResolveList, …)
│   │   ├── InvocationDispatcher.cs
│   │   ├── LibNxHandler.cs
│   │   ├── InputHandler.cs
│   │   ├── ConsoleHandler.cs
│   │   ├── FormHandler.cs
│   │   ├── GraphicsHandler.cs
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
│   ├── CSharpToC.cs            — dünner Orchestrator
│   └── TypeMapper.cs           — Backward-Compatibility-Shim
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
    ├── switchforms.h           — UI-Controls, List<T>, Dictionary<K,V>, StringBuilder, String-Utils
    └── switchapp.h             — SwitchApp-Loop, Framebuffer
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

Kein bestehender Code muss angefasst werden.

### Neuen Typ hinzufügen

Eintrag in `Core/TypeRegistry.cs` in der entsprechenden Kategorie ergänzen — `s_primitives`, `s_controlTypes` oder `s_libNxStructs`.

---

## Lizenz

MIT