# Erste Schritte mit CS2SX

CS2SX transpiliert C#-Code zu C und kompiliert ihn via DevkitPro zur fertigen Nintendo Switch Homebrew-App (`.nro`).

---

## Voraussetzungen

| Tool | Download |
|------|----------|
| [.NET 8 SDK](https://dotnet.microsoft.com/download) | Mindestens .NET 8 |
| [DevkitPro](https://devkitpro.org/wiki/Getting_Started) | Mit `devkitA64` und `libnx` |

Die Umgebungsvariable `DEVKITPRO` muss auf das DevkitPro-Verzeichnis zeigen (z.B. `/opt/devkitpro` auf Linux/macOS oder `C:/devkitPro` auf Windows).

---

## Installation

```bash
# Aus dem Repository bauen und als globales Tool installieren
dotnet pack -c Release
dotnet tool install --global --add-source ./bin/Release CS2SX
```

Nach der Installation ist `cs2sx` in jedem Terminal verfügbar.

**Update:**
```bash
dotnet pack -c Release
dotnet tool update --global --add-source ./bin/Release CS2SX
```

---

## Erstes Projekt

### 1. Projekt anlegen

```bash
cs2sx new MeineApp
```

Das erzeugt folgende Struktur:

```
MeineApp/
├── MeineApp.csproj       ← .NET-Projektdatei (für IDE)
├── cs2sx.json            ← CS2SX-Konfiguration (Name, Icon, Version)
├── Program.cs            ← Einstiegspunkt (wird nicht transpiliert)
├── MeineApp.cs           ← Deine App-Klasse
├── icon.jpg              ← App-Icon (256×256)
└── romfs/                ← optionale eingebettete Assets (BMPs etc.)
```

### 2. App-Klasse

`MeineApp.cs` enthält eine Klasse die von `SwitchApp` erbt:

```csharp
public class MeineApp : SwitchApp
{
    public override void OnInit()
    {
        // Einmalig beim Start
        Console.WriteLine("Hallo Switch!");
    }

    public override void OnFrame()
    {
        // Einmal pro Frame (~60fps)
        if (Input.IsDown(NpadButton.Plus))
            Environment.Exit(0);   // Beenden mit Plus
    }
}
```

### 3. Bauen

```bash
cs2sx build MeineApp/MeineApp.csproj
```

Die fertige `MeineApp.nro` liegt danach im Projektverzeichnis und kann direkt auf die SD-Karte der Switch kopiert werden (`/switch/MeineApp/MeineApp.nro`).

---

## CLI-Überblick

| Befehl | Beschreibung |
|--------|-------------|
| `cs2sx new <Name>` | Neues Projekt erstellen |
| `cs2sx build <csproj>` | Vollständiger Build → `.nro` |
| `cs2sx check <csproj>` | Nur transpilieren, kein GCC (schnelle Fehlerprüfung) |
| `cs2sx watch <csproj>` | Datei-Watcher mit automatischem Rebuild bei Änderungen |
| `cs2sx clean <csproj>` | Build-Artefakte löschen (behebt verwaiste Symbole nach Umbenennen) |
| `cs2sx addLib <libName>` | Externe C-Library einbinden |

---

## Projektstruktur verstehen

### cs2sx.json

```json
{
  "name": "MeineApp",
  "author": "Dein Name",
  "version": "1.0.0",
  "icon": "icon.jpg"
}
```

### Was wird transpiliert?

Alle `.cs`-Dateien im Projektverzeichnis **außer**:
- `Program.cs` (Einstiegspunkt, bleibt unverändert)
- Dateien in Stub-Ordnern (`Stubs/`, `*Stubs/`)
- Dateien aus `SwitchFormsLib/` (werden als C-Runtime mitgeliefert)

### Inkrementeller Build

Nur geänderte Dateien werden neu transpiliert. Bei Änderungen an Generics oder Interfaces triggert CS2SX automatisch einen vollständigen Rebuild der abhängigen Dateien.

---

## Zwei App-Modi

CS2SX unterstützt zwei Ausgabe-Modi:

### Konsolen-Modus (Text-UI)

Standardmodus — gibt Text über den Switch-Konsolentreiber aus. Gut für Tools, Menüs und einfache Apps.

```csharp
public class MeineApp : SwitchApp
{
    public override void OnInit()
    {
        Console.WriteLine("Willkommen!");
    }
}
```

### Grafik-Modus (Framebuffer)

Aktiviert durch `Graphics.Init(width, height)` in `OnInit()`. Ab diesem Punkt werden alle Frames über den Framebuffer gerendert.

```csharp
public class MeineApp : SwitchApp
{
    public override void OnInit()
    {
        Graphics.Init(1280, 720);
    }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.Black);
        Graphics.DrawText(100, 100, "Hallo Grafik!", Color.White, 2);
    }
}
```

---

## Nächste Schritte

- [Sprachunterstützung](language-guide.md) — Welche C#-Features funktionieren
- [Graphics API](graphics.md) — Alle Zeichenfunktionen
- [Input API](input.md) — Controller-Eingaben lesen
- [Forms API](forms.md) — UI-Steuerelemente
- [Beispiel-Apps](examples/) — Fertige Beispiele zum Ausprobieren
- [Externe Libraries](advanced/external-libs.md) — C-Libraries einbinden
