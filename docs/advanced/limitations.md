# Grenzen und Workarounds

Diese Seite beschreibt was CS2SX nicht unterstützt und wie man die häufigsten Einschränkungen umgeht.

---

## Strings als Felder

**Problem:** Interpolierte Strings erzeugen Stack-Buffer. Werden sie in Feldern gespeichert, entsteht ein dangling pointer im generierten C.

```csharp
// FALSCH
private string _text;
public void Update() { _text = $"Wert: {_wert}"; }

// RICHTIG — String-Literal oder direktes Ausgeben
private string _text = "Standardwert";
public void Update() { Console.WriteLine($"Wert: {_wert}"); }

// RICHTIG — wenn Feldwert dynamisch sein muss: Hilfsklasse/Methode
public string GetText() => $"Wert: {_wert}";
```

---

## Keine Exceptions

`try`/`catch` wird transpiliert, aber der `catch`-Block ist ein no-op — Exceptions werden nicht abgefangen.

```csharp
// FALSCH — catch wird nicht ausgeführt
try { var x = int.Parse(eingabe); }
catch { Console.WriteLine("Kein int!"); }

// RICHTIG — bool-Rückgabe nutzen
if (int.TryParse(eingabe, out int x))
    Console.WriteLine($"Zahl: {x}");
else
    Console.WriteLine("Kein int!");
```

---

## Kein Async/Await

`async`/`await` wird synchron ausgegeben. Alle Aufgaben laufen im Main-Thread.

```csharp
// FALSCH — await täuscht Asynchronität vor, die nicht existiert
await Task.Delay(1000);

// RICHTIG — Frame-Counter als Timer
private int _timer = 0;
public override void OnFrame()
{
    _timer++;
    if (_timer >= 60)  // 60 Frames ≈ 1 Sekunde
    {
        _timer = 0;
        // verzögerte Aktion
    }
}
```

---

## Kein `foreach` über Ranges

```csharp
// FALSCH — Range-Expression in foreach nicht unterstützt
foreach (var i in 0..10) { }

// RICHTIG
for (int i = 0; i < 10; i++) { }
```

---

## Generics nur mit konkreten Typen

```csharp
// FALSCH — Generic-Constraint zur Laufzeit
public T Max<T>(T a, T b) where T : IComparable<T>
    => a.CompareTo(b) > 0 ? a : b;

// RICHTIG — separate Methoden für jeden Typ
public int MaxInt(int a, int b) => a > b ? a : b;
public float MaxFloat(float a, float b) => a > b ? a : b;
```

---

## Kein `record with { }` Ausdruck

```csharp
public record Punkt(int X, int Y);

// FALSCH — with-Expression nicht unterstützt
var p2 = p1 with { X = 10 };

// RICHTIG — manuell kopieren
var p2 = new Punkt(10, p1.Y);
```

---

## Events: Nur ein Subscriber

```csharp
// Nur der letzte Subscriber wird ausgeführt
_button.OnClick += HandlerA;
_button.OnClick += HandlerB;  // HandlerA wird überschrieben!

// RICHTIG — wrapper Methode
_button.OnClick = () => { HandlerA(); HandlerB(); };
```

---

## Kein `lock`

`lock` wird als no-op transpiliert. Nintendo Switch Homebrew läuft sowieso single-threaded.

---

## String-Interpolation in Rückgabewerten

```csharp
// FALSCH — gibt Pointer auf lokalen Stack-Buffer zurück
public string GetStatus() => $"Score: {_score}";

// RICHTIG — statischen Puffer oder String-Literal zurückgeben
private string _statusBuf = "";
public string GetStatus()
{
    _statusBuf = _score > 0 ? "Aktiv" : "Bereit";
    return _statusBuf;
}
```

---

## Reflection

Reflection (`.GetType()`, `Activator.CreateInstance()`, `MethodInfo` etc.) ist grundsätzlich nicht verfügbar — die .NET Runtime existiert im transpilierten C nicht. Es gibt keine Alternative; Reflection-lastigen Code muss anders strukturiert werden (z.B. mit einer expliziten Factory-Methode oder einem Registry-Dictionary).

---

## `StringBuilder`

`StringBuilder` wird grundsätzlich unterstützt, aber die Ausgabe von `ToString()` hat die gleiche Einschränkung wie interpolierte Strings — nicht in Feldern speichern.

```csharp
// RICHTIG — direkt ausgeben
var sb = new System.Text.StringBuilder();
sb.Append("Hallo");
sb.Append(" Switch");
Console.WriteLine(sb.ToString());
```

---

## `ulong` statt `IntPtr` für externe Handles

Siehe [Externe Libraries](external-libs.md#opake-handle-typen) für eine ausführliche Erklärung.
