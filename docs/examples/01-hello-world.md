# Beispiel 1: Hello World

Das einfachste mögliche CS2SX-Programm — Text ausgeben und auf Plus zum Beenden warten.

---

## Konsolen-Version

```csharp
// HelloApp.cs
public class HelloApp : SwitchApp
{
    public override void OnInit()
    {
        Console.Clear();
        Console.WriteLine("==============================");
        Console.WriteLine("   Hallo von der Switch!");
        Console.WriteLine("==============================");
        Console.WriteLine();
        Console.WriteLine("Drücke + zum Beenden.");
    }

    public override void OnFrame()
    {
        if (Input.IsDown(NpadButton.Plus))
            Environment.Exit(0);
    }
}
```

**Erstellen und bauen:**
```bash
cs2sx new HelloApp
# HelloApp.cs mit obigem Code ersetzen
cs2sx build HelloApp/HelloApp.csproj
```

---

## Grafik-Version

```csharp
// HelloApp.cs
public class HelloApp : SwitchApp
{
    public override void OnInit()
    {
        Graphics.Init(1280, 720);
    }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.Black);

        // Zentrierter Titel
        string titel = "Hallo von der Switch!";
        int tw = Graphics.MeasureTextWidth(titel, 3);
        Graphics.DrawText((1280 - tw) / 2, 300, titel, Color.White, 3);

        // Untertitel
        string hint = "Drücke + zum Beenden";
        int hw = Graphics.MeasureTextWidth(hint, 1);
        Graphics.DrawText((1280 - hw) / 2, 380, hint, Color.Gray, 1);

        if (Input.IsDown(NpadButton.Plus))
            Environment.Exit(0);
    }
}
```

---

## Was passiert hier?

| Methode | Bedeutung |
|---------|-----------|
| `OnInit()` | Wird einmal beim Programmstart aufgerufen |
| `OnFrame()` | Wird ~60 Mal pro Sekunde aufgerufen |
| `Input.IsDown()` | Gibt `true` nur im ersten Frame zurück, in dem der Button gedrückt wird |
| `Environment.Exit(0)` | Beendet die App sauber |
| `Graphics.Init(1280, 720)` | Aktiviert den Framebuffer-Modus |
