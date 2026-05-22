# Beispiel 4: Einfacher Datei-Browser (Text-UI)

Ein praktisches Tool im Konsolen-Modus: Zeigt den Inhalt eines Verzeichnisses und erlaubt Navigation. Demonstriert die Forms-API, Listen und Datei-I/O.

---

## Code

```csharp
// DateiBrowser.cs
public class DateiBrowser : SwitchApp
{
    private string _pfad = "/switch";
    private List<string> _einträge;
    private int _auswahl   = 0;
    private int _scrollOff = 0;
    private Label _pfadLabel;
    private Label _infoLabel;

    private const int MaxSichtbar = 20;

    public override void OnInit()
    {
        Console.Clear();

        _pfadLabel = new Label($"Pfad: {_pfad}");
        _pfadLabel.X = 1;
        _pfadLabel.Y = 1;
        Form.Add(_pfadLabel);

        _infoLabel = new Label("");
        _infoLabel.X = 1;
        _infoLabel.Y = 2;
        Form.Add(_infoLabel);

        OrdnerLaden();
    }

    public override void OnFrame()
    {
        HandleInput();
        Zeichnen();

        if (Input.IsDown(NpadButton.Plus))
            Environment.Exit(0);
    }

    private void HandleInput()
    {
        if (_einträge.Count == 0) return;

        if (Input.IsDown(NpadButton.Down))
        {
            _auswahl = Math.Min(_auswahl + 1, _einträge.Count - 1);
            if (_auswahl >= _scrollOff + MaxSichtbar)
                _scrollOff++;
        }

        if (Input.IsDown(NpadButton.Up))
        {
            _auswahl = Math.Max(_auswahl - 1, 0);
            if (_auswahl < _scrollOff)
                _scrollOff--;
        }

        if (Input.IsDown(NpadButton.A))
            OrdnerÖffnen();

        if (Input.IsDown(NpadButton.B))
            OrdnerHoch();
    }

    private void OrdnerÖffnen()
    {
        if (_auswahl >= _einträge.Count) return;

        string name = _einträge[_auswahl];
        string vollPfad = _pfad + "/" + name;

        if (Directory.Exists(vollPfad))
        {
            _pfad = vollPfad;
            _auswahl = 0;
            _scrollOff = 0;
            OrdnerLaden();
        }
        else if (File.Exists(vollPfad))
        {
            // Dateigröße anzeigen
            string inhalt = File.ReadAllText(vollPfad);
            _infoLabel.Text = $"Datei: {name} ({inhalt.Length} Zeichen)";
        }
    }

    private void OrdnerHoch()
    {
        int letzterSlash = _pfad.LastIndexOf('/');
        if (letzterSlash > 0)
        {
            _pfad = _pfad.Substring(0, letzterSlash);
            _auswahl = 0;
            _scrollOff = 0;
            OrdnerLaden();
        }
    }

    private void OrdnerLaden()
    {
        _einträge = new List<string>();

        // Unterverzeichnisse
        foreach (var dir in Directory.GetDirectories(_pfad))
        {
            string name = dir.Substring(dir.LastIndexOf('/') + 1);
            _einträge.Add("[" + name + "]");
        }

        // Dateien
        foreach (var file in Directory.GetFiles(_pfad))
        {
            string name = file.Substring(file.LastIndexOf('/') + 1);
            _einträge.Add(name);
        }

        _pfadLabel.Text = "Pfad: " + _pfad;
        _infoLabel.Text = $"{_einträge.Count} Einträge";
    }

    private void Zeichnen()
    {
        // Einträge rendern (ab Zeile 4)
        for (int i = 0; i < MaxSichtbar; i++)
        {
            int idx = _scrollOff + i;
            int zeile = i + 4;

            // Zeile leeren
            Console.Write($"\x1B[{zeile};1H                                        ");

            if (idx >= _einträge.Count) continue;

            string eintrag = _einträge[idx];
            bool markiert = (idx == _auswahl);

            if (markiert)
                Console.Write($"\x1B[{zeile};1H> {eintrag}");
            else
                Console.Write($"\x1B[{zeile};1H  {eintrag}");
        }

        // Steuerungshinweise unten
        Console.Write($"\x1B[26;1H Hoch/Runter=Navigate  A=Öffnen  B=Hoch  +=Beenden");
    }
}
```

---

## Was dieses Beispiel zeigt

- **Konsolen-Modus** mit ANSI-Escape-Codes für Cursor-Positionierung
- **Listen-Navigation** mit Scroll-Offset
- **Verzeichnis-Traversierung** via `Directory.GetDirectories` und `Directory.GetFiles`
- **Zustandsverwaltung** (aktueller Pfad, Auswahl, Scroll)
- **Label** als dynamisches Statusanzeige-Element
- **String-Manipulation** mit `LastIndexOf` und `Substring`

---

## Hinweis: Pfade auf der Switch

```
/             ← Root (eingeschränkt)
/switch/      ← Homebrew-Verzeichnis (schreib-/lesbar)
/atmosphere/  ← CFW-Daten
/sdcard/      ← SD-Karte Root
```

Eigene App-Daten immer unter `/switch/<AppName>/` ablegen.
