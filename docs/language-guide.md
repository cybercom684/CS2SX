# Sprachunterstützung

CS2SX unterstützt einen großen Teil der C#-Sprache. Diese Seite zeigt, was funktioniert, was eingeschränkt ist und was nicht unterstützt wird.

---

## Typen

### Primitive Typen

| C# | C | Anmerkung |
|----|---|-----------|
| `int` | `int` | |
| `uint` | `unsigned int` | |
| `long` | `long long` | |
| `ulong` | `unsigned long long` | Gut für opake Handles |
| `float` | `float` | |
| `double` | `double` | |
| `bool` | `int` | `true` → `1`, `false` → `0` |
| `char` | `char` | |
| `byte` | `unsigned char` | |
| `short` | `short` | |
| `string` | `const char*` | Unveränderlich in C |
| `IntPtr` / `nint` | `intptr_t` | |
| `void` | `void` | |

### Klassen

```csharp
public class Spieler
{
    public string Name;
    public int Leben;
    private float _speed;

    public Spieler(string name, int leben)
    {
        Name = name;
        Leben = leben;
        _speed = 1.0f;
    }

    public void Schaden(int dmg)
    {
        Leben -= dmg;
    }
}
```

Klassen werden als C-Structs mit Funktionszeiger-VTable transpiliert. Instanzen werden auf dem Heap allokiert und über `_Free`-Funktionen freigegeben.

**Konstruktoren** dürfen überladen werden und sich gegenseitig aufrufen — `: this(...)` und `: base(...)` funktionieren wie in C#, ebenso optionale Parameter. Die Reihenfolge ist die von C#: Feld-Initialisierer, dann der Basis-Konstruktor, dann der Rumpf.

```csharp
public class Punkt
{
    public int X, Y;
    public Punkt() : this(0, 0) { }
    public Punkt(int v) : this(v, v) { }
    public Punkt(int x, int y) { X = x; Y = y; }
}
```

### Structs

```csharp
public struct Punkt
{
    public int X;
    public int Y;
}

// Verwendung
var p = new Punkt { X = 10, Y = 20 };
```

Structs werden direkt zu C-Structs — kein Heap, kein Freiräumen nötig.

### Vererbung

```csharp
public class Tier
{
    public virtual string LautMachen() => "...";
}

public class Hund : Tier
{
    public override string LautMachen() => "Wuff";
}

Tier t = new Hund();
Console.WriteLine(t.LautMachen()); // → "Wuff" (VTable-Dispatch)
```

Einfache Vererbung mit virtuellen Methoden funktioniert über eine VTable (Funktionszeiger-Struct).

### Interfaces

```csharp
public interface IDrawable
{
    void Draw();
    string Name { get; }
}

public class Box : IDrawable
{
    public void Draw() => Graphics.FillRect(0, 0, 100, 100, Color.Red);
    public string Name => "Box";
}
```

Interfaces werden als VTable expandiert.

### Enums

```csharp
public enum Richtung { Nord, Sued, Ost, West }

public enum Status : byte { Idle = 0, Running = 1, Done = 2 }

// Flags
[Flags]
public enum Optionen { Keine = 0, Sound = 1, Musik = 2, Alle = 3 }
```

In Strings, bei `Console.WriteLine` und mit `ToString()` erscheint der Name des Werts wie in .NET: `"Stufe " + Rang.Gold` ergibt `Stufe Gold`. Bei `[Flags]`-Enums werden die gesetzten Werte aufgezählt (`Read, Write`), ein Wert ohne Namen erscheint als Zahl. Für die Zahl selbst `(int)wert` schreiben. `List<MeinEnum>` funktioniert wie eine Liste von Zahlen.

Mehrere Enums dürfen gleichnamige Werte haben (`Modus.Keine`, `Stufe.Keine`). Im generierten C heißen die Konstanten `Modus_Keine` — wichtig nur, wenn man eigenen C-Code dazu schreibt.

---

## Kontrollfluss

```csharp
// if / else
if (leben > 0)
    Console.WriteLine("Am Leben");
else
    Console.WriteLine("Game Over");

// for
for (int i = 0; i < 10; i++)
    Console.WriteLine(i);

// foreach
var liste = new List<int> { 1, 2, 3 };
foreach (var x in liste)
    Console.WriteLine(x);

// while / do-while
while (spielLäuft) OnFrame();
do { lesen(); } while (!fertig);

// switch expression
string text = richtung switch
{
    Richtung.Nord => "Norden",
    Richtung.Sued => "Süden",
    _ => "Unbekannt",
};

// Pattern matching: prüft den echten Typ zur Laufzeit, auch durch ein Interface
if (tier is Hund hund)
    hund.LautMachen();
switch (tier)
{
    case Dackel: …; break;        // abgeleitete Klassen zuerst
    case Hund h: …; break;
}
```

`is`, Typ-Muster und `switch` auf den Typ funktionieren für eigene Klassen, Interface-Werte und Exceptions. Bei `object` (z. B. `object o = …; o is Hund`) prüft CS2SX nur auf `null`.

---

## Strings

```csharp
string name = "Switch";

// Interpolation
Console.WriteLine($"Hallo {name}!");
Console.WriteLine($"Leben: {leben:D3}");     // → "Leben: 007"
Console.WriteLine($"Wert: {pi:F2}");         // → "Wert: 3.14"
Console.WriteLine($"{name,10}");             // rechts ausrichten
Console.WriteLine($"{name,-10}");            // links ausrichten

// Methoden
bool b = name.Contains("wi");
string oben = name.ToUpper();
string teil = name.Substring(2, 3);
int idx = name.IndexOf('t');
string[] teile = name.Split(',');
string getrimmt = "  hallo  ".Trim();
string ersetzt = name.Replace("Switch", "NX");
```

> **Hinweis:** Interpolierte Strings sind auf 1023 Zeichen begrenzt. Feldzuweisungen kopieren sie dauerhaft, lokale Variablen gelten für den laufenden Frame, siehe [Grenzen](advanced/limitations.md#strings-länge-und-lebensdauer).

---

## Collections

### List\<T\>

```csharp
var punkte = new List<int>();
punkte.Add(10);
punkte.Add(20);
punkte.Remove(10);
bool hat = punkte.Contains(20);
int anz = punkte.Count;

// Sortieren
punkte.Sort();
punkte.Sort((a, b) => b - a);  // absteigend

// ForEach
punkte.ForEach(p => Console.WriteLine(p));
```

### Dictionary\<K, V\>

```csharp
var scores = new Dictionary<string, int>();
scores["Alice"] = 100;
scores["Bob"] = 80;

if (scores.TryGetValue("Alice", out int s))
    Console.WriteLine($"Alice: {s}");

foreach (var kv in scores)
    Console.WriteLine($"{kv.Key}: {kv.Value}");
```

### Arrays

```csharp
int[] werte = new int[10];
werte[0] = 42;

Array.Sort(werte);
Array.Reverse(werte);
Array.Fill(werte, 0);
int idx = Array.IndexOf(werte, 42);
Array.Resize(ref werte, 20);

int Summe(params int[] xs) { int s = 0; foreach (var x in xs) s += x; return s; }
Summe(1, 2, 3);          // ausgeschriebene Form
Summe(werte);            // Array direkt

string[] teile = "a,b,c".Split(',');
Console.WriteLine(teile.Length + " " + string.Join("-", teile));
```

Arrays kennen ihre Länge überall — als Parameter, Feld oder Rückgabewert (`.Length` funktioniert immer).
Kleine lokale Arrays mit konstanter Größe, die die Methode nicht verlassen, liegen auf dem Stack;
alle anderen (Felder, Rückgabewerte, an eigene Methoden übergebene) auf dem Heap. Heap-Arrays werden
nicht automatisch freigegeben — wiederholt pro Frame neu erzeugte große Arrays besser als Feld halten
und wiederverwenden.

### Nullable-Werttypen (`int?`)

```csharp
int? best = null;
best = 120;
int anzeige = best ?? 0;
if (best.HasValue && best > 100) Console.WriteLine(best.Value);
int? summe = best + bonus;   // null, wenn einer der beiden null ist (wie in C#)
```

`int?`, `float?`, `MeinEnum?` und `MeinStruct?` sind Werttypen ohne Heap-Allokation.
`string?` oder `Gegner?` sind nur Nullable-Annotationen für Referenztypen und ändern nichts.

---

## LINQ

```csharp
var zahlen = new List<int> { 1, 2, 3, 4, 5, 6 };

var gerade = zahlen.Where(n => n % 2 == 0).ToList();
var quadrate = zahlen.Select(n => n * n).ToList();
int summe = zahlen.Sum();
int max = zahlen.Max();
bool hatGerade = zahlen.Any(n => n % 2 == 0);
int erst = zahlen.First(n => n > 3);

var sortiert = zahlen.OrderByDescending(n => n).ToList();
```

LINQ-Abfragen werden inline zu C-Schleifen expandiert — kein IQueryable, keine Lazy Evaluation.

---

## Iteratoren (yield return)

Eine Methode mit `yield return` liefert ihre Werte nacheinander. Was dabei passiert, hängt vom Rückgabetyp ab.

**`IEnumerable<T>`:** Der Rumpf läuft beim Aufruf ganz durch und sammelt die Werte in einer Liste. `foreach`, LINQ und `string.Join` funktionieren damit wie gewohnt, auch rekursiv und in Eigenschaften:

```csharp
IEnumerable<int> Gerade(int max)
{
    for (int i = 0; i <= max; i += 2) yield return i;
}

public IEnumerable<Gegner> Lebende
{
    get { foreach (var g in _gegner) if (g.Leben > 0) yield return g; }
}

int summe = Gerade(10).Sum();
```

> Weil die Werte sofort gesammelt werden, darf ein `IEnumerable<T>`-Iterator nicht endlos laufen (`while (true) yield return …` hängt). Dafür `IEnumerator<T>` verwenden.

**`IEnumerator<T>` und `IEnumerator`:** Der Rumpf läuft schrittweise, bei jedem `MoveNext()` bis zum nächsten `yield return`. Lokale Variablen behalten dazwischen ihren Wert. So lassen sich Abläufe über mehrere Frames schreiben (Coroutines):

```csharp
private IEnumerator? _intro;

private IEnumerator Intro()
{
    _text = "3";
    for (int i = 0; i < 60; i++) yield return null;   // 60 Frames warten
    _text = "Los!";
}

public override void OnInit() { _intro = Intro(); }

public override void OnFrame()
{
    if (_intro != null && !_intro.MoveNext()) _intro = null;   // ein Schritt pro Frame
}
```

Mit `IEnumerator<T>` liefert jeder Schritt einen Wert in `Current`, zum Beispiel die Anzahl der Frames, die der Ablauf warten will. `yield break` beendet einen Iterator, danach gibt `MoveNext()` immer `false` zurück. Jeder Aufruf der Methode erzeugt einen eigenen, unabhängigen Ablauf.

Bei `IEnumerator<T>` und `IEnumerator` lehnt der Build ab, mit Zeile und Hinweis:
- `yield` in `try` oder `using`
- Methoden in Structs oder generischen Klassen
- `out var` und Mustervariablen, die über ein `yield return` hinweg gebraucht werden
- beim nicht-generischen `IEnumerator` andere Werte als `null`

`yield` in Lambdas und in lokalen Funktionen, die lokale Variablen der Methode benutzen, geht nicht.

---

## Lambdas und Delegates

```csharp
// Action
Action<string> drucke = s => Console.WriteLine(s);
drucke("Hallo");

// Func
Func<int, int, int> addiere = (a, b) => a + b;
int ergebnis = addiere(3, 4);

// Als Parameter
var liste = new List<int> { 3, 1, 2 };
liste.Sort((a, b) => a - b);
```

Delegates (`Action`, `Func<…>`, Events) sind echte Objekte: Lambdas, Methoden (`OnTreffer`, `Hilfe.Parse`) und Kombinationen daraus lassen sich speichern, übergeben, in Listen ablegen und mit `+=`/`-=` verketten. Ein Aufruf ruft alle Einträge nacheinander auf und liefert das letzte Ergebnis. Ein Aufruf auf `null` wirft `NullReferenceException`, deshalb `?.Invoke(...)` für Events.

```csharp
public class Zaehler
{
    public event Action<int> Geaendert;
    private int _wert;
    public void Plus(int n) { _wert += n; Geaendert?.Invoke(_wert); }
}

var z = new Zaehler();
z.Geaendert += OnGeaendert;                 // Methode dieser Klasse
z.Geaendert += w => Console.WriteLine(w);   // zusätzlich eine Lambda
z.Plus(1);                                  // ruft beide auf
z.Geaendert -= OnGeaendert;
```

**Captures:** Felder (`_wert`, `this`) sieht eine Lambda immer live. Erfasste **lokale Variablen** werden beim Erzeugen der Lambda kopiert. Jede Lambda, die in einer Schleife entsteht, hat deshalb ihre eigenen Werte (`for (…) { int k = i; liste.Add(() => k); }` liefert 0, 1, 2 …). Weist eine Lambda einer erfassten lokalen Variable etwas zu, warnt `cs2sx check`, weil die Änderung außen nicht ankommt. Für geteilten Zustand ein Feld verwenden.

Lambdas, die direkt an LINQ oder `List`/`Array`-Methoden gehen (`Where`, `Select`, `ForEach`, `Sort`, `Find`, …), werden zu einer Schleife um eine statische Funktion. Das kostet keine Allokation.

---

## Exceptions

```csharp
public class ParseFehler : Exception
{
    public int Position;
    public ParseFehler(string msg, int pos) : base(msg) { Position = pos; }
}

try
{
    Lade(datei);
}
catch (ParseFehler e) when (e.Position > 0)
{
    Console.WriteLine("Zeile " + e.Position + ": " + e.Message);
}
catch (IOException)
{
    throw;                          // weiterreichen
}
finally
{
    Aufraeumen();
}
```

`throw` funktioniert über Methodengrenzen hinweg, `catch` wählt nach Typ (inklusive Basisklassen und `when`-Filter), `finally` läuft auch bei `return`, `break`, `continue` und weitergeworfenen Exceptions. Eigene Exception-Klassen leiten von `Exception` oder einer System-Exception ab. Verfügbar sind unter anderem `ArgumentException`, `ArgumentNullException`, `ArgumentOutOfRangeException`, `InvalidOperationException`, `NotSupportedException`, `NotImplementedException`, `FormatException`, `IOException`, `FileNotFoundException` und `KeyNotFoundException`, jeweils mit `Message`, `InnerException` und `GetType().Name`.

Eine nicht gefangene Exception beendet die App. Vorher zeigt die Switch die Meldung im System-Fehlerdialog, und sie wird nach `stderr` geschrieben (nxlink).

Die Laufzeitbibliothek wirft dieselben Exceptions wie .NET:

| Situation | Exception |
|---|---|
| `int.Parse("abc")`, `bool.Parse("1")` · Wert zu groß (`byte.Parse("300")`) · `null` | `FormatException` · `OverflowException` · `ArgumentNullException` |
| `list[i]`, `RemoveAt`, `Insert`, `Substring` außerhalb des Bereichs | `ArgumentOutOfRangeException` |
| `array[i]` außerhalb des Bereichs · auf ein `null`-Array | `IndexOutOfRangeException` · `NullReferenceException` |
| `dict[key]` ohne den Schlüssel · `Add` mit vorhandenem Schlüssel | `KeyNotFoundException` · `ArgumentException` |
| `Pop`/`Peek`/`Dequeue` auf leerem Stack/Queue · `First`/`Single`/`Last`/`Min`/`Max`/`Average` auf leerer Folge | `InvalidOperationException` |
| Ganzzahl-Division oder `%` durch 0 | `DivideByZeroException` |
| `File.ReadAllText`/`ReadAllLines`/`ReadAllBytes`/`OpenRead`/`Copy` auf nicht vorhandene Datei | `FileNotFoundException` |
| `BinaryReader.ReadInt32` & Co. am Dateiende | `EndOfStreamException` |

`TryParse`, `TryGetValue`, `FirstOrDefault` usw. werfen nicht und sind für erwartbare Fehlschläge der günstigere Weg.

Ein Zugriff über eine `null`-Referenz wirft `NullReferenceException`, statt die App abstürzen zu lassen. Das gilt für Felder, Methoden, virtuelle Methoden, Interfaces, Strings und Collections. Wo `null` ausgeschlossen ist, kostet die Prüfung nichts: bei `this`, bei `new …` und bei lokalen Variablen, die nur mit `new …` belegt werden.

Zahlen und Wahrheitswerte erscheinen in Strings wie in .NET: `2.5` statt `2.500000`, `0.1`, `1E+21`, `True`/`False`.

---

## Math

```csharp
double wurzel = Math.Sqrt(16.0);      // → 4.0
float sin = MathF.Sin(MathF.PI / 2); // → 1.0f
int abs = Math.Abs(-5);              // → abs()
float absF = Math.Abs(-3.14f);       // → fabsf()
double log = Math.Log(Math.E);       // → log()
double pow = Math.Pow(2.0, 10.0);    // → pow()
int min = Math.Min(3, 7);
int geklammert = Math.Clamp(x, 0, 100);
```

---

## Generics

```csharp
// Generische Klasse
public class Behälter<T>
{
    private T _wert;
    public Behälter(T wert) { _wert = wert; }
    public T HoleWert() => _wert;
}

var b = new Behälter<int>(42);
int w = b.HoleWert();

// Generische Methoden — Typargumente werden wie in C# abgeleitet
public static T Max<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) > 0 ? a : b;
public static void Tausche<T>(ref T a, ref T b) { T t = a; a = b; b = t; }

int m = Max(3, 7);             // → Max_int
string s = Max("Apfel", "Birne");   // → Max_str
```

Generics werden zur Compilezeit expandiert, ähnlich wie C++ Templates: Für jeden verwendeten Typ entsteht eine eigene Kopie der Klasse bzw. Methode. Das gilt auch für generische Methoden, die andere generische Methoden aufrufen, und für generische Erweiterungsmethoden.

Generische Methoden dürfen auch `virtual`/`abstract`/`override` sein oder in einem Interface stehen, und generische Structs (`struct Pair<T>`) funktionieren ebenso. Generische Methoden gehen auch in generischen Klassen und Structs (`class Box<T> { U Map<U>(Func<T, U> f) … }`). Nicht unterstützt sind generische Methoden in generischen Interfaces und generische Structs innerhalb generischer Typen. Der Build meldet sie mit Zeile und Hinweis.

---

## Statische Klassen und Methoden

```csharp
public static class Hilfe
{
    public static int Clamp(int wert, int min, int max)
        => Math.Max(min, Math.Min(max, wert));

    public static string Wiederholen(string s, int n)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < n; i++) sb.Append(s);
        return sb.ToString();
    }
}

int x = Hilfe.Clamp(150, 0, 100); // → 100
```

---

## Properties

```csharp
public class Rechteck
{
    public int Breite { get; set; }
    public int Höhe { get; set; }
    public int Fläche => Breite * Höhe;  // read-only computed
}
```

---

## Datei-I/O

```csharp
// Lesen
string inhalt = File.ReadAllText("/switch/MeineApp/save.txt");
string[] zeilen = File.ReadAllLines("/switch/MeineApp/log.txt");

// Schreiben
File.WriteAllText("/switch/MeineApp/save.txt", "Spielstand: 100");

// Prüfen
if (!Directory.Exists("/switch/MeineApp"))
    Directory.CreateDirectory("/switch/MeineApp");

if (File.Exists("/switch/MeineApp/save.txt"))
    File.Delete("/switch/MeineApp/save.txt");

// Bytes
byte[] daten = File.ReadAllBytes("/switch/MeineApp/level.bin");
File.WriteAllBytes("/switch/MeineApp/kopie.bin", daten);
File.AppendAllBytes("/switch/MeineApp/log.bin", new byte[] { 1, 2 });
```

### Binäre Dateien: BinaryWriter / BinaryReader

Für Spielstände und eigene Dateiformate. Das Format ist dasselbe wie in .NET (Little Endian, Strings mit vorangestellter Länge), Dateien vom PC lassen sich also direkt lesen.

```csharp
using (var w = new BinaryWriter(File.Create("/switch/MeineApp/stand.dat")))
{
    w.Write(_level);        // int
    w.Write(_name);         // string
    w.Write(_leben > 0);    // bool
    w.Write(_position);     // float
}

using (var r = new BinaryReader(File.OpenRead("/switch/MeineApp/stand.dat")))
{
    _level = r.ReadInt32();
    _name = r.ReadString();
    bool lebt = r.ReadBoolean();
    _position = r.ReadSingle();
}

// im Speicher
var ms = new MemoryStream();
new BinaryWriter(ms).Write(42);
byte[] bytes = ms.ToArray();
var leser = new BinaryReader(new MemoryStream(bytes));
```

| | |
|---|---|
| Öffnen | `File.OpenRead(pfad)`, `File.Create(pfad)` (neu/leeren), `File.OpenWrite(pfad)` (ohne Leeren), `new MemoryStream()`, `new MemoryStream(bytes)` |
| Stream | `Read`, `ReadByte`, `Write`, `WriteByte`, `Seek`, `Position` (lesen und setzen), `Length`, `Flush`, `ToArray` (MemoryStream), `Dispose` |
| BinaryWriter | `Write(…)` für `byte`, `sbyte`, `bool`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `string`, `byte[]`, `(byte[], index, count)` |
| BinaryReader | `ReadByte`, `ReadSByte`, `ReadBoolean`, `ReadInt16`/`UInt16`, `ReadInt32`/`UInt32`, `ReadInt64`/`UInt64`, `ReadSingle`, `ReadDouble`, `ReadString`, `ReadBytes(n)`, `BaseStream` |

`Dispose` (oder das Ende eines `using`) schließt die Datei; `Dispose` von Reader oder Writer schließt auch den Stream. Liest ein `ReadInt32` & Co. über das Dateiende hinaus, kommt eine `EndOfStreamException`.

> Pfade auf der Switch beginnen immer mit `/switch/`. Erstelle deinen App-Ordner in `OnInit()`.

---

## Was nicht unterstützt wird

| Feature | Alternative |
|---------|-------------|
| Threads hinter `async`/`await` | `async`/`await` läuft im Main-Thread über mehrere Frames, siehe [Grenzen](advanced/limitations.md#asyncawait-im-main-thread-über-mehrere-frames) |
| Reflection | Nicht verfügbar (kein .NET Runtime) |
| `dynamic` | Nicht verfügbar |
| `event` mit eigenen `add`/`remove`-Accessoren | Feld-artiges `event` |
| `IQueryable` / LINQ-Query-Syntax | Method-Syntax LINQ verwenden |
| Ranges auf Arrays (`arr[1..3]`) | Manuell mit Index-Schleife |

Ausführlichere Infos: [Grenzen und Workarounds](advanced/limitations.md)
