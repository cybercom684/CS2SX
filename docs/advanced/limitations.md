# Grenzen und Workarounds

Diese Seite beschreibt, was CS2SX nicht unterstützt und wie man die häufigsten Einschränkungen umgeht. Stand: 0.5, jeder Punkt ist gegen den aktuellen Transpiler geprüft.

---

## Speicher: kein Garbage Collector

Das transpilierte Programm hat keine .NET-Laufzeit und damit keinen Garbage Collector. Objekte, Listen und Strings, die in Feldern liegen, leben, bis die App endet oder das besitzende Objekt freigegeben wird. Für typische Homebrew-Apps ist das unkritisch. Wer aber in jedem Frame neue Objekte erzeugt (`new` in `OnFrame`), verbraucht stetig Speicher.

```csharp
// UNGÜNSTIG — 60 neue Objekte pro Sekunde, die nie freigegeben werden
public override void OnFrame()
{
    var p = new Partikel();
    ...
}

// BESSER — einmal anlegen und wiederverwenden
private readonly List<Partikel> _pool = new List<Partikel>();
```

---

## Strings: Länge und Lebensdauer

Strings aus Verkettung, Interpolation oder `ToString()` entstehen in einem Ring aus 512 Puffern zu je 1 KB.

- **Länge:** Ein so erzeugter String ist auf **1023 Zeichen** begrenzt, längere werden abgeschnitten. Für lange Texte `StringBuilder` verwenden.
- **Felder:** Beim Zuweisen an ein Feld wird der String auf den Heap kopiert und bleibt gültig.
- **Lokale Variablen und Rückgabewerte:** Diese zeigen in den Ring. Sie bleiben für etwa 512 weitere String-Operationen gültig, also für den Rest des Frames, aber nicht dauerhaft.

```csharp
private string _status = "";

public string Status() => $"Punkte: {_score}";   // OK — Rückgabewert für den Aufrufer

public override void OnFrame()
{
    _status = Status();              // OK — Feldzuweisung kopiert den String
    string tmp = Status();           // OK — im selben Frame verwenden
}
```

Problematisch ist nur, einen lokalen String in einer Sammlung oder einem Objekt aufzubewahren, ohne ihn einem `string`-Feld zuzuweisen, z. B. in einer `List<string>`, die über viele Frames wächst. Dann den String über ein Feld oder `StringBuilder` führen.

---

## Lambda: erfasste lokale Variablen sind Kopien

Felder sieht eine Lambda live (über `this`). Erfasste **lokale Variablen** werden aber beim Erzeugen der Lambda kopiert. Schreibt die Lambda in eine solche Variable, sieht die Methode die Änderung nicht. Der Transpiler warnt in diesem Fall.

```csharp
// FALSCH — zaehler ist eine lokale Variable; die Lambda erhöht ihre Kopie
int zaehler = 0;
Action cb = () => zaehler++;
cb();                       // zaehler bleibt 0 (Warnung beim Build)

// RICHTIG — geteilten Zustand in ein Feld legen
private int _zaehler;
Action cb = () => _zaehler++;   // wirkt auf das Feld
```

---

## Keine generischen Methoden

Generische **Klassen** werden für jeden verwendeten konkreten Typ erzeugt (`Stack<int>`, `Pool<Gegner>`). Generische **Methoden** (`T Max<T>(T a, T b)`) werden dagegen nicht unterstützt, auch ohne Constraints.

```csharp
// FALSCH
public static T Max<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) > 0 ? a : b;

// RICHTIG — eine Methode pro Typ
public static int Max(int a, int b) => a > b ? a : b;
public static float Max(float a, float b) => a > b ? a : b;
```

---

## Kein `record with { }`

```csharp
public record Punkt(int X, int Y);

// FALSCH — with-Ausdruck nicht unterstützt
var p2 = p1 with { X = 10 };

// RICHTIG — manuell kopieren
var p2 = new Punkt(10, p1.Y);
```

---

## Kein `foreach` über Ranges

```csharp
// FALSCH
foreach (var i in 0..10) { }

// RICHTIG
for (int i = 0; i < 10; i++) { }
```

---

## Kein `new object()` und kein `lock`

`lock` wird ignoriert (mit Warnung). Der App-Code läuft im Main-Thread; die Laufzeit nutzt Threads nur intern, z. B. für HTTP. `new object()` als Sperrobjekt wird nicht unterstützt.

---

## Kein Async/Await

`async`/`await` wird synchron ausgegeben. Alles läuft im Main-Thread.

```csharp
// FALSCH — await täuscht Asynchronität vor, die nicht existiert
await Task.Delay(1000);

// RICHTIG — Zeit messen
private readonly Stopwatch _uhr = Stopwatch.StartNew();
public override void OnFrame()
{
    if (_uhr.ElapsedMilliseconds >= 1000)
    {
        _uhr.Restart();
        // verzögerte Aktion
    }
}
```

---

## Exceptions als Kontrollfluss

`try`/`catch`/`finally` verhalten sich wie in C#, und die Laufzeitbibliothek wirft dieselben Exceptions wie .NET, auch `NullReferenceException`, siehe [Sprachunterstützung](../language-guide.md#exceptions). Ein `throw` kostet allerdings deutlich mehr als eine Prüfung (`setjmp`/`longjmp`). Für erwartbare Fälle in einer Schleife, die jeden Frame läuft, sind deshalb `?.`, `TryParse` und `TryGetValue` der bessere Weg.

```csharp
// Funktioniert, ist aber teuer, wenn _ziel oft null ist
try { Console.WriteLine(_ziel.Name); }
catch (NullReferenceException) { Console.WriteLine("kein Ziel"); }

// Besser
Console.WriteLine(_ziel?.Name ?? "kein Ziel");
```

---

## Environment.Exit(0)

`Environment.Exit(0)` ruft direkt `exit()` auf und überspringt das libnx-Cleanup. Auf echter Hardware erscheint dann die Meldung **„Software wurde wegen eines Fehlers beendet"**. Stattdessen `App.Quit()` aufrufen: Die Hauptschleife endet nach dem aktuellen Frame, und `OnExit()` läuft noch. Die Plus-Taste beendet die App ohnehin automatisch.

---

## Reflection

Reflection (`Activator.CreateInstance()`, `MethodInfo` usw.) gibt es nicht, weil die .NET-Laufzeit im transpilierten C fehlt. Stattdessen eine explizite Factory-Methode oder ein Dictionary mit Konstruktor-Delegates verwenden.

---

## `ulong` statt `IntPtr` für externe Handles

Siehe [Externe Libraries](external-libs.md#opake-handle-typen).
