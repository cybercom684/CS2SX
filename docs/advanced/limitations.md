# Grenzen und Workarounds

Diese Seite beschreibt, was CS2SX nicht unterstützt und wie man die häufigsten Einschränkungen umgeht. Stand: 0.5, jeder Punkt ist gegen den aktuellen Transpiler geprüft.

---

## Was der Build ablehnt

Für diese Konstrukte gibt es keine Übersetzung nach C. `cs2sx build` und `cs2sx check` melden sie als Fehler mit Datei, Zeile und Workaround. Der C-Compiler läuft dann gar nicht erst.

| Konstrukt | Stattdessen |
|---|---|
| `foreach (var i in 0..10)`, Ranges `s[1..3]` | `for`-Schleife, `Substring`, `GetRange` |
| `p with { X = 1 }` | `new Punkt(1, p.Y)` |
| generische Methoden, die `virtual`/`abstract`/`override` sind oder in Interfaces bzw. generischen Klassen stehen; generische Structs | nicht-virtuelle generische Methode, generische Klasse |
| lokale Funktionen | private Methode |
| LINQ-Abfragesyntax `from x in … select x` | `list.Where(…).Select(…)` |
| Collection-Ausdrücke `[1, 2, 3]` | `new List<int> { 1, 2, 3 }` |
| eigene `delegate`-Typen | `Action<…>`, `Func<…>` |
| `goto`, Sprungmarken | Schleifen, `break`/`continue` |
| `yield return` | Werte in einer `List<T>` sammeln |
| `new object()`, `dynamic` | eigene Klasse, konkreter Typ |
| `stackalloc`, `Span<T>`, `unsafe`, Zeiger | normale Arrays |
| `GetType()` (außer bei Exceptions) | `is`/`switch` auf den Typ |

---

## Speicher: Referenzzählung statt Garbage Collector

Das transpilierte Programm hat keine .NET-Laufzeit und damit keinen Garbage Collector. Objekte eigener Klassen werden stattdessen referenzgezählt. Im Alltag verhält sich das wie in C#:

- **Was niemand behält, wird freigegeben.** Ein Objekt, das nur in lokalen Variablen lebt (`var p = new Partikel();` in `OnFrame`), wird am Anfang des nächsten Frames zerstört. Neue Objekte pro Frame sind also kein Speicherleck.
- **Was etwas behält, bleibt.** Halten können Felder und Properties, `List`, `Dictionary`, `Stack`, `Queue`, `HashSet`, Arrays, generische Klassen (`Box<Item>`), Structs, Tupel, Interface- und `object`-Werte sowie Lambdas und Event-Handler. Jede dieser Stellen hält eine eigene Referenz.
- **Erst zum nächsten Frame:** Wird ein Objekt losgelassen (`RemoveAt`, `Pop`, `Remove`, `Clear`, ein überschriebenes Feld), stirbt es frühestens am Anfang des nächsten Frames. Lokale Variablen bleiben bis dahin gültig:

  ```csharp
  var n = _queue.Dequeue();
  n.Run();                // OK — n lebt bis zum Ende des Frames
  ```

- **Zerstörung ist virtuell:** Ein `Circle` in einer `List<Shape>` räumt auch seine eigenen Felder auf. Ein C#-Finalizer (`~Circle()`) läuft dabei mit.

**Nicht freigegeben werden:**

- Objekte, die sich gegenseitig referenzieren (Zyklen, z. B. `a.Next = b; b.Prev = a;`). Bei solchen Strukturen die Verweise vor dem Wegwerfen auf `null` setzen.
- Objekte, die über ein Struct, ein Tupel, ein Interface-/`object`-Wert, oder einen Methoden-Delegate (`+= obj.Methode`) gehalten wurden. Diese Stellen geben ihre Referenz nicht zurück.
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
