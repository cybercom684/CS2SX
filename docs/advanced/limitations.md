# Grenzen und Workarounds

Diese Seite beschreibt, was CS2SX nicht unterstützt und wie man die häufigsten Einschränkungen umgeht. Stand: 0.7, jeder Punkt ist gegen den aktuellen Transpiler geprüft.

---

## Was der Build ablehnt

Für diese Konstrukte gibt es keine Übersetzung nach C. `cs2sx build` und `cs2sx check` melden sie als Fehler mit Datei, Zeile und Workaround. Der C-Compiler läuft dann gar nicht erst.

| Konstrukt | Stattdessen |
|---|---|
| Ranges auf Arrays (`arr[1..3]`), `^` in `foreach`-Ranges, Ranges auf komplexen Ausdrücken | Schleife, `Substring`/`GetRange` auf einer Variable |
| `with` auf Nicht-Records, auf komplexen Ausdrücken oder für Nicht-Positionsparameter | `new Punkt(1, p.Y)` |
| generische Methoden in generischen Interfaces, generische Structs innerhalb generischer Typen | in eine Klasse oder ein Struct verschieben (dort gehen generische Methoden, auch in `Box<T>` und `Pair<T>`) |
| lokale Variablen/Parameter namens `self` in Instanzmethoden | umbenennen (`self` heißt `this` im erzeugten C) |
| lokale Funktionen, die lokale Variablen/Parameter der umgebenden Methode benutzen, generische lokale Funktionen | Werte als Parameter übergeben, private Methode |
| LINQ-Abfragesyntax `from x in … select x` | `list.Where(…).Select(…)` |
| Collection-Ausdrücke mit `..spread` oder für andere Typen als Array/List/HashSet | `new List<int>(…)` |
| `delegate`-Typen mit `ref`/`out`/`params` oder generisch | `Action<…>`, `Func<…>` |
| `goto`, Sprungmarken | Schleifen, `break`/`continue` |
| `yield` in Lambdas/lokalen Funktionen mit Zugriff auf lokale Variablen, nicht-generisches `IEnumerable`; bei `IEnumerator<T>`: `yield` in `try`/`using`, in generischen Klassen | Methode der Klasse, `IEnumerable<T>`, `try` in eine eigene Methode |
| `new object()`, `dynamic` | eigene Klasse, konkreter Typ |
| `stackalloc`, `Span<T>`, `unsafe`, Zeiger | normale Arrays |
| `GetType()` (außer bei Exceptions) | `is`/`switch` auf den Typ (prüfen den echten Typ zur Laufzeit) |

Unterstützt (CS2SX schreibt es vor dem Übersetzen um): lokale Funktionen ohne Zugriff auf lokale Variablen der Methode, `[1, 2, 3]` für Arrays/List/HashSet, `with` bei Records, `foreach (var i in 0..10)`, `s[1..3]`/`s[^1]`/`list[2..]`, eigene `delegate`-Typen, generische Methoden (auch `virtual`/`abstract`/`override` und in Interfaces) und generische Structs (`struct Pair<T>`). Iteratoren mit `yield return` und `yield break`, siehe [Sprachunterstützung → Iteratoren](../language-guide.md#iteratoren-yield-return).

---

## Speicher: Referenzzählung statt Garbage Collector

Das transpilierte Programm hat keine .NET-Laufzeit und damit keinen Garbage Collector. Objekte eigener Klassen, Collections (`List`, `Dictionary`, `Stack`, `Queue`, `HashSet`), `StringBuilder`, Delegates (Lambdas, Methoden als Delegate, Events) und Interface-Werte werden stattdessen referenzgezählt. Im Alltag verhält sich das wie in C#:

- **Was niemand behält, wird freigegeben.** Ein Objekt, das nur in lokalen Variablen lebt (`var p = new Partikel();` in `OnFrame`), wird am Anfang des nächsten Frames zerstört. Neue Objekte pro Frame sind also kein Speicherleck, das gilt auch für Zwischenergebnisse wie `gegner.Where(…).ToList()` und für Lambdas, die jeden Frame neu entstehen.
- **Was etwas behält, bleibt.** Halten können Felder und Properties, Events, `List`, `Dictionary`, `Stack`, `Queue`, `HashSet`, Arrays, generische Klassen (`Box<Item>`), Lambdas (was sie erfassen), Delegates (das Objekt einer Methode wie `obj.Update`) und Interface-Werte (ihr Objekt). Jede dieser Stellen hält eine eigene Referenz.
- **Structs und Tupel** halten selbst nichts, wie eine lokale Variable. Steckt ein Struct oder Tupel in einem Feld oder einer Liste, hält diese Stelle die Objekte, Strings und Collections darin.
- **Erst zum nächsten Frame:** Wird ein Objekt losgelassen (`RemoveAt`, `Pop`, `Remove`, `Clear`, ein überschriebenes Feld), stirbt es frühestens am Anfang des nächsten Frames. Lokale Variablen bleiben bis dahin gültig:

  ```csharp
  var n = _queue.Dequeue();
  n.Run();                // OK — n lebt bis zum Ende des Frames
  ```

- **Zerstörung ist virtuell:** Ein `Circle` in einer `List<Shape>` räumt auch seine eigenen Felder auf. Ein C#-Finalizer (`~Circle()`) läuft dabei mit.

- **Zyklen werden gefunden:** Objekte, die sich gegenseitig halten (`kind.Parent = this; Children.Add(kind);`, ein Ring, ein Objekt, das auf sich selbst zeigt), werden freigegeben, sobald von außen nichts mehr auf sie zeigt. Die Suche läuft etwa alle 16 Frames und nur, wenn Referenzen abgegeben wurden. Sie folgt Feldern, Auto-Properties und Events, Arrays in Feldern, `List`, `Dictionary` (Werte), `Stack`, `Queue` und `HashSet`, Structs und Tupeln in Feldern, Lambdas (was sie erfassen), Delegates und Interface-Werten. Typischer Fall: ein Objekt, dessen eigener Event-Handler `this` erfasst (`Changed += () => Count++;`).

**Nicht freigegeben werden:**

- Zyklen, die über eine generische Klasse (`Box<T>`) laufen, und Objekte, die ein `object`-Wert hält. Dort einen der Verweise vor dem Wegwerfen auf `null` setzen.
- Objekte in Arrays, die nur in einer lokalen Variable liegen, und in Tupeln, die in ein Array geschrieben werden.

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

Strings aus Verkettung, Interpolation oder `ToString()` entstehen in einem Zwischenspeicher, der zu jedem Frame geleert wird.

- **Länge:** Verkettung (`a + b`, `+=`), Interpolation (`$"…"`), `string.Format`, `string.Join`, `new string('x', n)` und String-Methoden wie `Substring`, `Replace`, `ToUpper`, `Trim` oder `PadLeft` sind nicht begrenzt.
- **Felder und Sammlungen:** Beim Zuweisen an ein Feld oder eine Auto-Property (auch an ein Struct- oder Tupel-Element in einem Feld) und beim Einfügen in `List<string>`, `Dictionary` oder ein `string[]`-Feld wird der String kopiert und bleibt gültig.
- **Lokale Variablen und Rückgabewerte:** Diese zeigen in den Zwischenspeicher. Sie bleiben für den Rest des Frames gültig, aber nicht dauerhaft. In `async`-Methoden sind lokale Variablen Felder und überleben das `await`.

```csharp
private string _status = "";

public string Status() => $"Punkte: {_score}";   // OK — Rückgabewert für den Aufrufer

public override void OnFrame()
{
    _status = Status();              // OK — Feldzuweisung kopiert den String
    string tmp = Status();           // OK — im selben Frame verwenden
}
```

Sehr viele oder sehr lange Strings in einem einzigen Frame (zusammen über 512 KB) überschreiben die ältesten davon. Für große Texte, die schrittweise entstehen, `StringBuilder` verwenden.

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

Unterstützt: `async Task`, `async Task<T>`, `async void`, `async`-Lambdas (`Func<Task>`, `Action`, `Func<int, Task<int>>`), `await` auf `Task`/`Task<T>`, `Task.Delay(ms)`, `Task.Delay(TimeSpan)`, `Task.Yield()` (weiter im nächsten Frame), `Task.CompletedTask`, `Task.FromResult`, `Task.WhenAll` (auch mit Ergebnis: `int[] r = await Task.WhenAll(a, b)`), `Task.WhenAny(a, b)`, `Task.Run` (führt sofort aus), `IsCompleted`, `IsFaulted`, `Result`, `Wait()`.

`await` geht auch in `try`, `catch` und `finally`, in `using`-Blöcken, in den Bedingungen von `while`, `do-while` und `for`, und rechts von `&&`, `||`, `??` sowie in `?:`, wenn der Ausdruck die ganze Bedingung, Zuweisung oder Rückgabe ist (`if (ok && await Pruefe())`).

Eine Exception in einer `async Task`-Methode landet im Task und wird beim `await` oder bei `Result`/`Wait()` weitergeworfen; ein `catch` um das `await` fängt sie. Schaut niemand auf einen fehlgeschlagenen Task, erscheint beim Freigeben `Unobserved task exception: …` mit Aufrufkette im Log.

Der Build lehnt mit Zeile und Hinweis ab:

| Konstrukt | Stattdessen |
|---|---|
| `await` in `catch`/`finally`, wenn `try`/`catch` mit `return`, `break` oder `continue` verlassen werden oder ein `catch` einen Filter (`when`) hat | Ergebnis merken und das `await` hinter den `try`-Block ziehen |
| `await` in `do-while`- oder `for`-Kopf zusammen mit `continue` im Rumpf | `while` |
| `await` rechts von `&&`, `\|\|`, `??` oder in `?:` mitten in einem größeren Ausdruck (z. B. als Argument), in einem `switch`-Ausdruck | vorher in eine Variable schreiben |
| Ergebnis von `await Task.WhenAll(…)` ohne Ergebnistyp, `Task.WhenAny` mit `Task<T>` oder einer Liste | Tasks einzeln mit `await`/`.Result` auslesen |
| `foreach` mit `await` im Rumpf über etwas anderes als `List<T>`/Array | `for`-Schleife |
| `out var`, Muster-Variablen (`is Foo f`) und `using var` vor einem `await` im selben Block | normal deklarieren (`int n;`) |
| `async`-Lambdas in anderen Lambdas oder in generischen Klassen, `await foreach`, `base.` in `async`-Methoden | `async`-Methode der Klasse |
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
