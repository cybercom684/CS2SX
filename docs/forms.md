# Forms API (Text-UI)

SwitchForms ist das Text-basierte UI-System von CS2SX. Es nutzt ANSI-Escape-Codes für positionierten Text auf dem Switch-Konsolenausgang und eignet sich für Tools, Menüs und einfache Anwendungen ohne Grafik-Modus.

---

## SwitchApp Lifecycle

```csharp
public class MeineApp : SwitchApp
{
    public override void OnInit()
    {
        // Einmalig beim Programmstart
        // UI-Elemente erstellen, Dateien laden, Zustand initialisieren
    }

    public override void OnFrame()
    {
        // Einmal pro Frame (~60fps)
        // Standardmäßig: Form.UpdateAll() + Form.DrawAll()
        // Überschreiben für eigene Frame-Logik
    }

    public override void OnExit()
    {
        // Einmalig beim Beenden
        // Ressourcen freigeben, Speicherstand schreiben
    }
}
```

---

## Label

Zeigt Text an einer festen Position an.

```csharp
private Label _statusLabel;

public override void OnInit()
{
    _statusLabel = new Label("Bereit");
    _statusLabel.X = 5;    // Spalte (1-basiert)
    _statusLabel.Y = 3;    // Zeile (1-basiert)
    Form.Add(_statusLabel);
}

// Text ändern
_statusLabel.Text = "Verarbeitung...";
_statusLabel.SetText($"Score: {punkte}");
```

---

## Button

Ein anklickbarer Button der auf A-Taste reagiert wenn er fokussiert ist. Buttons können per D-Pad fokussiert werden.

```csharp
private Button _startBtn;

public override void OnInit()
{
    _startBtn = new Button("Spiel starten");
    _startBtn.X = 5;
    _startBtn.Y = 5;
    _startBtn.OnClick = OnStartGeklickt;
    Form.Add(_startBtn);
}

private void OnStartGeklickt()
{
    Console.Clear();
    Console.WriteLine("Spiel startet!");
}
```

---

## ProgressBar

Zeigt einen Fortschrittsbalken an.

```csharp
private ProgressBar _ladebalken;

public override void OnInit()
{
    _ladebalken = new ProgressBar();
    _ladebalken.X = 5;
    _ladebalken.Y = 8;
    _ladebalken.Width = 40;   // Balkenbreite in Zeichen
    _ladebalken.Value = 0;    // 0-100
    Form.Add(_ladebalken);
}

// Fortschritt aktualisieren
_ladebalken.Value = 75;       // 75%
```

---

## Sichtbarkeit

Alle Steuerelemente haben eine `Visible`-Property:

```csharp
_statusLabel.Visible = false;  // ausblenden
_startBtn.Visible = true;      // einblenden
```

---

## Konsolen-Ausgabe

Ergänzend zu den Form-Elementen kann direkt in die Konsole geschrieben werden:

```csharp
Console.WriteLine("Eine neue Zeile");
Console.Write("Ohne Zeilenumbruch");
Console.Clear();                        // Bildschirm leeren

// ANSI-Escape (Cursor positionieren)
Console.Write($"\x1B[{zeile};{spalte}H Text an Position");
```

---

## Vollständiges Beispiel: Einfaches Menü

```csharp
public class MeinMenü : SwitchApp
{
    private Label _titel;
    private Label _info;
    private Button _spielBtn;
    private Button _exitBtn;
    private int _punkte = 0;

    public override void OnInit()
    {
        Console.Clear();

        _titel = new Label("=== Mein Spiel ===");
        _titel.X = 5;
        _titel.Y = 2;
        Form.Add(_titel);

        _info = new Label("Punkte: 0");
        _info.X = 5;
        _info.Y = 4;
        Form.Add(_info);

        _spielBtn = new Button("Punkt sammeln [A]");
        _spielBtn.X = 5;
        _spielBtn.Y = 6;
        _spielBtn.OnClick = PunktSammeln;
        Form.Add(_spielBtn);

        _exitBtn = new Button("Beenden [+]");
        _exitBtn.X = 5;
        _exitBtn.Y = 8;
        _exitBtn.OnClick = Beenden;
        Form.Add(_exitBtn);
    }

    private void PunktSammeln()
    {
        _punkte++;
        _info.Text = $"Punkte: {_punkte}";
    }

    private void Beenden()
    {
        File.WriteAllText("/switch/MeinMenü/highscore.txt", _punkte.ToString());
        Environment.Exit(0);
    }
}
```

---

## Tipps

- **Fokus**: Der erste hinzugefügte focusable Button ist beim Start fokussiert. Mit D-Pad Up/Down kann zwischen Buttons navigiert werden.
- **Mischbetrieb**: Labels sind nicht focusable (`focusable = 0`), Buttons sind es (`focusable = 1`).
- **Kein Graphics.Init()**: Im Forms-Modus nicht `Graphics.Init()` aufrufen — die beiden Modi schließen sich aus.
