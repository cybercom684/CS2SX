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

// Pattern matching
if (obj is Hund hund)
    hund.LautMachen();
```

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

> **Wichtig:** Interpolierte Strings erzeugen Stack-Buffer. Nicht in Feldern speichern — nur in lokalen Variablen oder direkt in Ausgabefunktionen verwenden.

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
```

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

Lambdas werden zu statischen C-Funktionen geliftet (`_lambda_N`). Captures von lokalen Variablen werden in einer Capture-Struct übergeben.

### Lambda-Einschränkung: Feld-Mutation schlägt nicht zurück

Felder der äußeren Klasse werden beim Erstellen der Lambda in die Capture-Struct **kopiert**. Zuweisungen an diese Felder innerhalb der Lambda ändern nur die Kopie — das Original bleibt unverändert.

```csharp
// FALSCH — _log wird in der Kopie gesetzt, nicht im Feld
private string _log = "";
private Action _cb;

public void Init()
{
    _cb = () => { _log = "fertig"; };  // schreibt in Capture-Kopie!
}

public void OnFrame()
{
    _cb();
    // _log ist weiterhin "" — die Zuweisung in der Lambda hatte keinen Effekt
}

// RICHTIG — Felder NACH dem Lambda-Aufruf setzen
public void OnFrame()
{
    _cb();                   // Lambda läuft (gut für Seiteneffekte auf Objekten)
    _log = "fertig";         // Feld direkt nach dem Aufruf setzen
}
```

**Was funktioniert:** Pointer-Parameter innerhalb einer Lambda werden korrekt dereferenziert. Wenn die Lambda ein Objekt als Parameter empfängt (`Action<MyClass>`), können dessen Felder über den Pointer mutiert werden:

```csharp
Action<Spieler> heilen = (sp) => { sp.Leben += 10; };  // sp->f_Leben += 10 — funktioniert!
heilen(_spieler);
// _spieler.Leben ist jetzt +10 — Mutation via Pointer-Param wirkt sich aus
```

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
```

Generics werden zur Compilezeit expandiert — ähnlich wie C++ Templates. Nur konkrete Typen (kein `T` zur Laufzeit).

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
```

> Pfade auf der Switch beginnen immer mit `/switch/`. Erstelle deinen App-Ordner in `OnInit()`.

---

## Was nicht unterstützt wird

| Feature | Alternative |
|---------|-------------|
| `async`/`await` | Synchrone Logik — kein echtes Multithreading auf Switch Homebrew |
| `try`/`catch` | Fehler durch Rückgabewerte / Flags behandeln |
| Reflection | Nicht verfügbar (kein .NET Runtime) |
| `dynamic` | Nicht verfügbar |
| `event` mit mehreren Subscribern | Einfacher `Action`-Delegate (ein Subscriber) |
| `IQueryable` / LINQ-Query-Syntax | Method-Syntax LINQ verwenden |
| `record with { }` Expressions | Manuell kopieren |
| Ranges `arr[1..3]` in foreach | Manuell mit Index-Schleife |

Ausführlichere Infos: [Grenzen und Workarounds](advanced/limitations.md)
