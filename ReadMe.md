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

    public override void OnInit()
    {
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
        if (Input.IsDown(NpadButton.B))
            SwitchApp_RequestExit();
    }

    public void OnPress()
    {
        _label.Text = "Button pressed!";
    }
}
```

---

## Unterstützte Features

| Feature | Status |
|---|---|
| Labels, Buttons, ProgressBar | ✅ |
| Input (alle NpadButtons) | ✅ |
| LibNX-Funktionsaufrufe | ✅ |
| `List<T>` | ✅ |
| `Dictionary<K,V>` | ✅ |
| `StringBuilder` | ✅ |
| `string`-Methoden | ✅ |
| `foreach`, `for`, `while` | ✅ |
| Mehrere Klassen pro Projekt | ✅ |
| String-Interpolation `$"..."` | ✅ |
| `try/catch/finally` | ✅ |
| libnx Structs als Stack-Variable | ✅ |
| `async/await`, LINQ, Generics | ❌ |

---

## Architektur

```
CS2SX/
├── Core/
│   ├── TypeRegistry.cs        — einzige Quelle aller Typ-Mappings
│   └── TranspilerContext.cs   — geteilter Zustand, kein globaler State
├── Transpiler/
│   ├── Handlers/              — pluggable Methoden-Aufruf-Handler
│   │   ├── IInvocationHandler.cs
│   │   ├── InvocationDispatcher.cs
│   │   ├── LibNxHandler.cs
│   │   ├── InputHandler.cs
│   │   ├── ConsoleHandler.cs
│   │   ├── FormHandler.cs
│   │   ├── ListHandler.cs
│   │   ├── DictionaryHandler.cs
│   │   ├── StringBuilderHandler.cs
│   │   ├── StringMethodHandler.cs
│   │   ├── FieldMethodHandler.cs
│   │   ├── OwnMethodHandler.cs
│   │   └── MathHandler.cs
│   ├── Writers/
│   │   ├── ExpressionWriter.cs
│   │   ├── StatementWriter.cs
│   │   ├── FormatStringBuilder.cs
│   │   ├── StringEscaper.cs
│   │   └── TypeInferrer.cs
│   ├── CSharpToC.cs           — dünner Orchestrator
│   └── TypeMapper.cs          — Backward-Compatibility-Shim
├── Build/
│   ├── BuildPipeline.cs
│   ├── CCompiler.cs
│   ├── EntryPointGenerator.cs
│   └── ProjectConfig.cs
└── Runtime/
    ├── switchforms.h          — UI-Controls, List<T>, Dictionary<K,V>, StringBuilder
    └── switchapp.h            — SwitchApp-Loop
```

### Neuen Feature-Handler hinzufügen

1. Neue Datei `Transpiler/Handlers/MeinHandler.cs` anlegen die `IInvocationHandler` implementiert
2. In `InvocationDispatcher.cs` eine Zeile `new MeinHandler()` in die Handler-Liste eintragen

Kein bestehender Code muss angefasst werden.

### Neuen Typ hinzufügen

Eintrag in `Core/TypeRegistry.cs` in der entsprechenden Kategorie ergänzen.

---

## Bei Update

> Version in `.csproj` hochsetzen, dann packen und global installieren:

```bash
dotnet pack -c Release
dotnet tool update --global --add-source ./bin/Release CS2SX
```

---

## Lizenz

MIT