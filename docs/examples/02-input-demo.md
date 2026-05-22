# Beispiel 2: Input-Demo

Zeigt alle gedrückten Buttons in Echtzeit an. Gut als Ausgangspunkt zum Verstehen des Input-Systems.

---

## Code

```csharp
// InputDemo.cs
public class InputDemo : SwitchApp
{
    public override void OnInit()
    {
        Graphics.Init(1280, 720);
    }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.RGB(20, 20, 30));

        // Titel
        Graphics.DrawText(40, 30, "Input-Demo — alle Buttons testen", Color.White, 2);
        Graphics.DrawLine(40, 60, 1240, 60, Color.RGB(60, 60, 80));

        // Buttons anzeigen
        int col1 = 80,  col2 = 400, col3 = 720, col4 = 1040;
        int zeile = 90, abstand = 36;

        DrawButton("A",      NpadButton.A,           col1, zeile);
        DrawButton("B",      NpadButton.B,            col1, zeile + abstand);
        DrawButton("X",      NpadButton.X,            col1, zeile + abstand * 2);
        DrawButton("Y",      NpadButton.Y,            col1, zeile + abstand * 3);

        DrawButton("L",      NpadButton.L,            col2, zeile);
        DrawButton("R",      NpadButton.R,            col2, zeile + abstand);
        DrawButton("ZL",     NpadButton.ZL,           col2, zeile + abstand * 2);
        DrawButton("ZR",     NpadButton.ZR,           col2, zeile + abstand * 3);

        DrawButton("Hoch",   NpadButton.Up,           col3, zeile);
        DrawButton("Runter", NpadButton.Down,          col3, zeile + abstand);
        DrawButton("Links",  NpadButton.Left,          col3, zeile + abstand * 2);
        DrawButton("Rechts", NpadButton.Right,         col3, zeile + abstand * 3);

        DrawButton("Plus",   NpadButton.Plus,          col4, zeile);
        DrawButton("Minus",  NpadButton.Minus,         col4, zeile + abstand);
        DrawButton("LStick", NpadButton.StickL,        col4, zeile + abstand * 2);
        DrawButton("RStick", NpadButton.StickR,        col4, zeile + abstand * 3);

        // Stick-Richtungen
        int sy = zeile + abstand * 5;
        Graphics.DrawText(col1, sy, "Linker Stick:", Color.Gray, 1);
        DrawButton("LHoch",   NpadButton.StickLUp,    col1, sy + abstand);
        DrawButton("LRunter", NpadButton.StickLDown,  col1, sy + abstand * 2);
        DrawButton("LLinks",  NpadButton.StickLLeft,  col2, sy + abstand);
        DrawButton("LRechts", NpadButton.StickLRight, col2, sy + abstand * 2);

        // Beenden
        Graphics.DrawText(40, 680, "Drücke + zum Beenden", Color.Gray, 1);
        if (Input.IsDown(NpadButton.Plus))
            Environment.Exit(0);
    }

    private void DrawButton(string name, NpadButton btn, int x, int y)
    {
        bool gedrückt = Input.IsHeld(btn);
        uint farbe = gedrückt ? Color.Green : Color.RGB(80, 80, 80);
        uint textfarbe = gedrückt ? Color.Black : Color.White;

        Graphics.FillRoundedRect(x, y - 2, 140, 28, 5, farbe);
        Graphics.DrawText(x + 8, y + 2, name, textfarbe, 1);
    }
}
```

---

## Was dieses Beispiel zeigt

- Echtzeit-Input-Abfrage mit `Input.IsHeld()` für gedrückte Zustände
- Dynamisches Zeichnen (Farbe ändert sich je nach Zustand)
- `Graphics.FillRoundedRect()` für abgerundete UI-Elemente
- Text messen und positionieren
- Strukturiertes Layout mit Spalten und Abständen
