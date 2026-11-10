# Grenzen und Workarounds

Diese Seite beschreibt, was CS2SX nicht unterstützt und wie man die häufigsten Einschränkungen umgeht. Stand: 0.5, jeder Punkt ist gegen den aktuellen Transpiler geprüft.

---

## Was der Build ablehnt

Für diese Konstrukte gibt es keine Übersetzung nach C. `cs2sx build` und `cs2sx check` melden sie als Fehler mit Datei, Zeile und Workaround. Der C-Compiler läuft dann gar nicht erst.

| Konstrukt | Stattdessen |
|---|---|
| Ranges auf Arrays (`arr[1..3]`), `^` in `foreach`-Ranges, Ranges auf komplexen Ausdrücken | Schleife, `Substring`/`GetRange` auf einer Variable |
| `with` auf Nicht-Records, auf komplexen Ausdrücken oder für Nicht-Positionsparameter | `new Punkt(1, p.Y)` |
| generische Methoden und generische Structs innerhalb generischer Klassen/Interfaces | in eine nicht-generische Klasse verschieben |
| lokale Variablen/Parameter namens `self` in Instanzmethoden | umbenennen (`self` heißt `this` im erzeugten C) |
| lokale Funktionen, die lokale Variablen/Parameter der umgebenden Methode benutzen, generische lokale Funktionen | Werte als Parameter übergeben, private Methode |
| LINQ-Abfragesyntax `from x in … select x` | `list.Where(…).Select(…)` |
| Collection-Ausdrücke mit `..spread` oder für andere Typen als Array/List/HashSet | `new List<int>(…)` |
| `delegate`-Typen mit `ref`/`out`/`params` oder generisch | `Action<…>`, `Func<…>` |
| `goto`, Sprungmarken | Schleifen, `break`/`continue` |
| `yield return` | Werte in einer `List<T>` sammeln |
| `new object()`, `dynamic` | eigene Klasse, konkreter Typ |
| `stackalloc`, `Span<T>`, `unsafe`, Zeiger | normale Arrays |
| `GetType()` (außer bei Exceptions) | `is`/`switch` auf den Typ |

Unterstützt (CS2SX schreibt es vor dem Übersetzen um): lokale Funktionen ohne Zugriff auf lokale Variablen der Methode, `[1, 2, 3]` für Arrays/List/HashSet, `with` bei Records, `foreach (var i in 0..10)`, `s[1..3]`/`s[^1]`/`list[2..]`, eigene `delegate`-Typen, generische Methoden (auch `virtual`/`abstract`/`override` und in Interfaces) und generische Structs (`struct Pair<T>`).

---

## Speicher: Referenzzählung statt Garbage Collector

Das transpilierte Programm hat keine .NET-Laufzeit und damit keinen Garbage Collector. Objekte eigener Klassen, Collections (`List`, `Dictionary`, `Stack`, `Queue`, `HashSet`) und `StringBuilder` werden stattdessen referenzgezählt. Im Alltag verhält sich das wie in C#:

- **Was niemand behält, wird freigegeben.** Ein Objekt, das nur in lokalen Variablen lebt (`var p = new Partikel();` in `OnFrame`), wird am Anfang des nächsten Frames zerstört. Neue Objekte pro Frame sind also kein Speicherleck, das gilt auch für Zwischenergebnisse wie `gegner.Where(…).ToList()`.
- **Was etwas behält, bleibt.** Halten können Felder und Properties, `List`, `Dictionary`, `Stack`, `Queue`, `HashSet`, Arrays, generische Klassen (`Box<Item>`), Structs, Tupel, Interface- und `object`-Werte sowie Lambdas und Event-Handler. Jede dieser Stellen hält eine eigene Referenz.
- **Erst zum nächsten Frame:** Wird ein Objekt losgelassen (`RemoveAt`, `Pop`, `Remove`, `Clear`, ein überschriebenes Feld), stirbt es frühestens am Anfang des nächsten Frames. Lokale Variablen bleiben bis dahin gültig:

  ```csharp
  var n = _queue.Dequeue();
  n.Run();                // OK — n lebt bis zum Ende des Frames
  ```

- **Zerstörung ist virtuell:** Ein `Circle` in einer `List<Shape>` räumt auch seine eigenen Felder auf. Ein C#-Finalizer (`~Circle()`) läuft dabei mit.

- **Zyklen werden gefunden:** Objekte, die sich gegenseitig halten (`kind.Parent = this; Children.Add(kind);`, ein Ring, ein Objekt, das auf sich selbst zeigt), werden freigegeben, sobald von außen nichts mehr auf sie zeigt. Die Suche läuft etwa alle 16 Frames und nur, wenn Referenzen abgegeben wurden. Sie folgt Feldern und Auto-Properties mit Objekten sowie `List<Klasse>`.

**Nicht freigegeben werden:**

- Zyklen, die über ein `Dictionary`, `Stack`, `Queue`, `HashSet`, Array, eine generische Klasse oder ein Lambda laufen. Dort einen der Verweise vor dem Wegwerfen auf `null` setzen.
- Objekte, die über ein Struct, ein Tupel, ein Interface-/`object`-Wert, oder einen Methoden-Delegate (`+= obj.Methode`) gehalten wurden. Diese Stellen geben ihre Referenz nicht zurück.

`GC.GetTotalMemory(false)` liefert den belegten Heap in Bytes, praktisch, um ein Speicherleck zu finden. `GC.Collect()` sucht sofort nach Zyklen; nötig ist der Aufruf nicht.

---

## Abstürze: Exception mit C#-Zeile

Eine nicht abgefangene Exception beendet die App mit Meldung und Aufrufkette, im Fehlerdialog der Switch und im Debug-Log:

```
Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
   at Enemy.Hit (Enemy.cs:10)
   at Game.OnFrame (Program.cs:23)
```

Auch `e.StackTrace` liefert diese Kette.

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

## Lambdas und erfasste Variablen

Lambdas teilen erfasste lokale Variablen und Parameter mit der Methode, wie in C#:

```csharp
int zaehler = 0;
Action cb = () => zaehler++;
cb(); cb();                 // zaehler ist 2
```

Intern wird eine Variable, die erfasst **und** irgendwo verändert wird, zu einem kleinen geteilten Objekt. Nur gelesene Variablen werden einfach kopiert, das ist genauso korrekt. Nicht unterstützt ist das bei mehreren Variablen in einer Deklaration (`int a = 0, b = 0;`) und bei `foreach`-/`out`-/Pattern-Variablen. Dort wird weiterhin kopiert, und der Build warnt.

---

## Kein `new object()` und kein `lock`

`lock` wird ignoriert (mit Warnung). Der App-Code läuft im Main-Thread; die Laufzeit nutzt Threads nur intern, z. B. für HTTP. `new object()` als Sperrobjekt wird nicht unterstützt.

---

## Async/Await: im Main-Thread, über mehrere Frames

`async`-Methoden funktionieren, laufen aber nicht in eigenen Threads. Ein `await` auf einen Task, der noch nicht fertig ist, unterbricht die Methode; sie läuft in einem späteren Frame weiter, vor `OnFrame`. Lokale Variablen, Parameter und Strings bleiben über das `await` hinweg erhalten.

```csharp
private async Task Intro()
{
    _text = "3";
    await Task.Delay(1000);      // die App läuft weiter, OnFrame wird jeden Frame aufgerufen
    _text = "Los!";
    int punkte = await LadePunkte();
}

public override void OnInit() { Intro(); }
```

Unterstützt: `async Task`, `async Task<T>`, `async void`, `await` auf `Task`/`Task<T>`, `Task.Delay(ms)`, `Task.Yield()` (weiter im nächsten Frame), `Task.CompletedTask`, `Task.FromResult`, `Task.WhenAll`, `Task.Run` (führt sofort aus), `IsCompleted`, `IsFaulted`, `Result`, `Wait()`. Eine Exception in einer `async Task`-Methode landet im Task und wird beim `await` oder bei `Result`/`Wait()` weitergeworfen.

Der Build lehnt mit Zeile und Hinweis ab:

| Konstrukt | Stattdessen |
|---|---|
| `await` in `try`/`catch`/`finally`, `using`, `lock` | `await` davor oder dahinter; oder den Task behalten und `IsFaulted` prüfen |
| `await` in einer Schleifenbedingung oder im `for`-Kopf | Ergebnis im Rumpf in eine Variable schreiben |
| `await` rechts von `&&`, `\|\|`, `??`, in `?:` oder einem `switch`-Ausdruck | vorher in eine Variable schreiben |
| `foreach` mit `await` im Rumpf über etwas anderes als `List<T>`/Array | `for`-Schleife |
| `out var`, Muster-Variablen (`is Foo f`) und `using var` vor einem `await` im selben Block | normal deklarieren (`int n;`) |
| `async`-Lambdas, lokale `async`-Funktionen, `await foreach`, `base.` in `async`-Methoden | `async`-Methode der Klasse |
| `async` in Structs und generischen Klassen | normale Klasse |

`Result` und `Wait()` blockieren nicht: Ist der Task noch nicht fertig, werfen sie eine `InvalidOperationException`. Rechenintensive Arbeit blockiert weiterhin den Frame, `await` verteilt nur Wartezeit.

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
