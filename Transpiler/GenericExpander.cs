// ============================================================================
// CS2SX — Transpiler/GenericExpander.cs
//
// Expandiert generische C#-Klassen zu konkreten C-Structs und Funktionen.
//
// Strategie: Echter Re-Transpile-Pass (nicht Text-Substitution)
//
// Für jede konkrete Instantiierung wird die generische Klasse syntaktisch
// rewritten — Typ-Parameter werden durch konkrete Typen ersetzt — und dann
// durch den normalen CSharpToC-Transpiler gejagt. Dadurch profitiert die
// Expansion von der gesamten ExpressionWriter/StatementWriter-Logik:
// String-Konkatenation, Vererbung, Null-Coalescing, foreach, etc.
//
// Ablauf:
//   1. BuildTypeMap:        T → int,  U → string
//   2. RewriteSyntaxTree:   Roslyn-Rewriter ersetzt alle Typ-Parameter
//                           in Signaturen, Feldern, Parametern, Locals
//   3. CSharpToC.Transpile: normaler Transpiler auf rewritetem Source
//   4. Rename:              generierte Symbole Foo → Foo_int
//
// Beispiel:
//   class Stack<T> {
//       private T[] _items;
//       private int _count;
//       public void Push(T item) { _items[_count++] = item; }
//       public T Pop() { return _items[--_count]; }
//   }
//
//   → nach Rewrite für T=int:
//   class Stack_int {
//       private int[] _items;
//       private int _count;
//       public void Push(int item) { _items[_count++] = item; }
//       public int Pop() { return _items[--_count]; }
//   }
//
//   → CSharpToC erzeugt daraus normalen C-Code:
//   typedef struct Stack_int Stack_int;
//   struct Stack_int { int* f_items; int f_count; };
//   Stack_int* Stack_int_New();
//   void Stack_int_Push(Stack_int* self, int item);
//   int  Stack_int_Pop(Stack_int* self);
// ============================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Logging;

namespace CS2SX.Transpiler;

public sealed class GenericExpander
{
    private readonly GenericInstantiationCollector _collector;

    public GenericExpander(GenericInstantiationCollector collector)
    {
        _collector = collector;
    }

    // ── Öffentliche API ───────────────────────────────────────────────────────

    /// <summary>
    /// Erzeugt für alle gesammelten Instantiierungen den C-Header- und
    /// Implementierungs-Code via echtem Re-Transpile-Pass.
    /// Gibt die Pfade der erzeugten .h und .c Dateien zurück.
    /// </summary>
    public (string headerPath, string implPath) WriteToFiles(string buildDir)
    {
        if (_collector.Instantiations.Count == 0)
            return (string.Empty, string.Empty);

        var headerSb = new System.Text.StringBuilder();
        headerSb.AppendLine("// ── Expanded Generic Types (auto-generated) ──────────────────────────");
        headerSb.AppendLine("// DO NOT EDIT — regenerated on every build");
        headerSb.AppendLine();

        var implSb = new System.Text.StringBuilder();
        implSb.AppendLine("// ── Expanded Generic Implementations (auto-generated) ────────────────");
        implSb.AppendLine("// DO NOT EDIT — regenerated on every build");
        implSb.AppendLine();

        foreach (var inst in _collector.Instantiations)
        {
            if (!_collector.GenericClasses.TryGetValue(inst.BaseName, out var classDef))
                continue;

            Log.Debug($"GenericExpander: Expanding {inst}");

            try
            {
                var (hCode, cCode) = ExpandInstantiation(classDef, inst);
                headerSb.AppendLine(hCode);
                implSb.AppendLine(cCode);
            }
            catch (Exception ex)
            {
                Log.Warning($"GenericExpander: Expansion von {inst} fehlgeschlagen: {ex.Message}");
                // Fallback: leerer Stub damit der Build nicht bricht
                headerSb.AppendLine($"/* expansion failed: {inst} */");
            }
        }

        var headerPath = Path.Combine(buildDir, "_generics.h");
        var implPath = Path.Combine(buildDir, "_generics.c");

        File.WriteAllText(headerPath,
            "#pragma once\n#include \"_forward.h\"\n\n" + headerSb);
        File.WriteAllText(implPath,
            "#include \"_generics.h\"\n\n" + implSb);

        Log.Info($"GenericExpander: {_collector.Instantiations.Count} Expansion(en) → _generics.h / _generics.c");
        return (headerPath, implPath);
    }

    /// <summary>
    /// Gibt alle expandierten C-Typ-Namen zurück (für Forward-Declarations in _forward.h).
    /// z.B. ["Stack_int", "Pair_str_float"]
    /// </summary>
    public IEnumerable<string> GetExpandedTypeNames() =>
        _collector.Instantiations
            .Where(i => _collector.GenericClasses.ContainsKey(i.BaseName))
            .Select(i => i.CName);

    // ── Kern-Expansion ────────────────────────────────────────────────────────

    /// <summary>
    /// Expandiert eine einzelne Instantiierung über einen echten Re-Transpile-Pass.
    /// Gibt (headerCode, implCode) zurück.
    /// </summary>
    private (string header, string impl) ExpandInstantiation(
        ClassDeclarationSyntax originalClass,
        GenericInstantiation inst)
    {
        var typeMap = BuildTypeMap(originalClass, inst);

        // Schritt 1: Syntax-Tree rewriten — Typ-Parameter durch konkrete Typen ersetzen
        //            und Klassenname von "Stack" zu "Stack_int" umbenennen
        var rewrittenSource = RewriteGenericClass(originalClass, inst, typeMap);

        Log.Debug($"GenericExpander: Rewritten source for {inst}:\n{rewrittenSource[..Math.Min(200, rewrittenSource.Length)]}…");

        // Schritt 2: Normaler Transpiler-Pass (Header)
        var hTranspiler = new CSharpToC(CSharpToC.TranspileMode.HeaderOnly);
        var hResult = hTranspiler.Transpile(rewrittenSource, filePath: $"<generic:{inst}>");

        // Schritt 3: Normaler Transpiler-Pass (Implementierung)
        var cTranspiler = new CSharpToC(CSharpToC.TranspileMode.Implementation);
        var cResult = cTranspiler.Transpile(rewrittenSource, filePath: $"<generic:{inst}>");

        // Warnungen aus dem Generic-Transpile loggen
        foreach (var d in hResult.Diagnostics.Concat(cResult.Diagnostics)
            .Where(d => d.Severity == Core.DiagnosticSeverity.Warning))
        {
            Log.Warning($"GenericExpander/{inst}: {d.Message}");
        }

        return (hResult.Code, cResult.Code);
    }

    // ── Syntax-Rewriter ───────────────────────────────────────────────────────

    /// <summary>
    /// Erzeugt C#-Quellcode der die generische Klasse mit substituierten
    /// Typen enthält.  Beispiel für Stack&lt;T&gt; mit T=int:
    ///
    ///   class Stack_int {
    ///       private int[] _items;
    ///       public void Push(int item) { ... }
    ///       public int Pop() { ... }
    ///   }
    /// </summary>
    private static string RewriteGenericClass(
        ClassDeclarationSyntax cls,
        GenericInstantiation inst,
        Dictionary<string, string> typeMap)
    {
        // Den originalen Quelltext der Klasse aus dem SyntaxTree extrahieren
        var originalSource = cls.SyntaxTree.GetText().ToString();

        // Rewriter anwenden
        var rewriter = new TypeParameterRewriter(typeMap, inst.CName, cls.Identifier.Text);
        var newRoot = rewriter.Visit(cls.SyntaxTree.GetRoot());

        // Nur die Klasse selbst als eigenständigen Source extrahieren
        // Dazu: finde die neu geschriebene Klasse im neuen Baum
        var newClass = newRoot.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.Text == inst.CName);

        if (newClass == null)
        {
            // Fallback: gesamten Baum als Source verwenden
            return newRoot.NormalizeWhitespace().ToFullString();
        }

        // Nur die Klasse zurückgeben (ohne namespace wrapper etc.)
        return newClass.NormalizeWhitespace().ToFullString();
    }

    // ── Typ-Map ───────────────────────────────────────────────────────────────

    private static Dictionary<string, string> BuildTypeMap(
        ClassDeclarationSyntax cls, GenericInstantiation inst)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        var typeParams = cls.TypeParameterList?.Parameters.ToList()
                         ?? new List<TypeParameterSyntax>();

        for (int i = 0; i < typeParams.Count && i < inst.TypeArguments.Count; i++)
            map[typeParams[i].Identifier.Text] = inst.TypeArguments[i];

        return map;
    }

    // ── Public Helpers (für BuildPipeline) ───────────────────────────────────

    /// <summary>
    /// Mappt einen C#-Typ-String mit substituierten Typparametern auf C.
    /// Wird von CSharpToC.WriteInstanceFieldDeclarations genutzt.
    /// </summary>
    public static string SubstituteType(string csType, Dictionary<string, string> typeMap)
    {
        // Direkte Substitution: T → int
        if (typeMap.TryGetValue(csType, out var direct))
            return TypeRegistry.MapType(direct);

        // Array: T[] → int*
        if (csType.EndsWith("[]"))
        {
            var baseType = csType[..^2];
            if (typeMap.TryGetValue(baseType, out var arrBase))
                return TypeRegistry.MapType(arrBase) + "*";
        }

        // List<T>
        if (csType.StartsWith("List<") && csType.EndsWith(">"))
        {
            var inner = csType[5..^1].Trim();
            var resolved = typeMap.TryGetValue(inner, out var ri) ? ri : inner;
            var cInner = resolved == "string" ? "str" : TypeRegistry.MapType(resolved);
            return $"List_{cInner}*";
        }

        // Dictionary<TKey, TValue>
        if (csType.StartsWith("Dictionary<") && csType.EndsWith(">"))
        {
            var inner = csType[11..^1].Trim();
            var comma = inner.IndexOf(',');
            if (comma >= 0)
            {
                var k = inner[..comma].Trim();
                var v = inner[(comma + 1)..].Trim();
                var rk = typeMap.TryGetValue(k, out var rkv) ? rkv : k;
                var rv = typeMap.TryGetValue(v, out var rvv) ? rvv : v;
                var ck = rk == "string" ? "str" : TypeRegistry.MapType(rk);
                var cv = rv == "string" ? "str" : TypeRegistry.MapType(rv);
                return $"Dict_{ck}_{cv}*";
            }
        }

        return TypeRegistry.MapType(csType);
    }
}

// ============================================================================
// TypeParameterRewriter — Roslyn CSharpSyntaxRewriter
//
// Ersetzt alle Vorkommen von Typ-Parametern (T, U, TKey, TValue etc.)
// durch die konkreten Typen und benennt die Klasse um.
//
// Behandelt:
//   • Klassen-Name:        Stack<T>     → Stack_int
//   • Felder:              private T[]  → private int[]
//   • Auto-Properties:     public T Foo → public int Foo
//   • Methoden-Parameter:  void Push(T) → void Push(int)
//   • Rückgabetypen:       T Pop()      → int Pop()
//   • Lokale Variablen:    T x = ...    → int x = ...
//   • Cast-Ausdrücke:      (T)value     → (int)value
//   • typeof(T)            → typeof(int)
//   • new T()              → new int()  (selten, aber möglich)
//   • Constraint-Klausel:  where T : ...  → wird entfernt
// ============================================================================
internal sealed class TypeParameterRewriter : CSharpSyntaxRewriter
{
    private readonly Dictionary<string, string> _typeMap;
    private readonly string _newClassName;
    private readonly string _oldClassName;

    public TypeParameterRewriter(
        Dictionary<string, string> typeMap,
        string newClassName,
        string oldClassName)
    {
        _typeMap = typeMap;
        _newClassName = newClassName;
        _oldClassName = oldClassName;
    }

    // ── Klassendeklaration ────────────────────────────────────────────────────

    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        // Nur die generische Klasse selbst umbenennen (nicht nested classes)
        if (node.Identifier.Text == _oldClassName)
        {
            // Typ-Parameter-Liste entfernen, Klasse umbenennen
            var newName = SyntaxFactory.Identifier(_newClassName)
                .WithLeadingTrivia(node.Identifier.LeadingTrivia)
                .WithTrailingTrivia(node.Identifier.TrailingTrivia);

            node = node
                .WithIdentifier(newName)
                .WithTypeParameterList(null)         // <T> entfernen
                .WithConstraintClauses(              // where T : ... entfernen
                    SyntaxFactory.List<TypeParameterConstraintClauseSyntax>());
        }

        return base.VisitClassDeclaration(node);
    }

    // ── Konstruktor ───────────────────────────────────────────────────────────

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        // Konstruktor-Name muss mit Klassennamen übereinstimmen
        if (node.Identifier.Text == _oldClassName)
        {
            node = node.WithIdentifier(
                SyntaxFactory.Identifier(_newClassName)
                    .WithTriviaFrom(node.Identifier));
        }
        return base.VisitConstructorDeclaration(node);
    }

    // ── Destruktor ────────────────────────────────────────────────────────────

    public override SyntaxNode? VisitDestructorDeclaration(DestructorDeclarationSyntax node)
    {
        if (node.Identifier.Text == _oldClassName)
        {
            node = node.WithIdentifier(
                SyntaxFactory.Identifier(_newClassName)
                    .WithTriviaFrom(node.Identifier));
        }
        return base.VisitDestructorDeclaration(node);
    }

    // ── Typen ─────────────────────────────────────────────────────────────────

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        var name = node.Identifier.Text;

        // Typ-Parameter ersetzen: T → int
        if (_typeMap.TryGetValue(name, out var concrete))
        {
            return SyntaxFactory.IdentifierName(concrete)
                .WithTriviaFrom(node);
        }

        // Eigene Klasse umbenennen wenn als Typ verwendet (Rückgabetyp, Parameter etc.)
        if (name == _oldClassName)
        {
            return SyntaxFactory.IdentifierName(_newClassName)
                .WithTriviaFrom(node);
        }

        return base.VisitIdentifierName(node);
    }

    public override SyntaxNode? VisitGenericName(GenericNameSyntax node)
    {
        // Generischen Aufruf der eigenen Klasse: Stack<int> im Body → Stack_int
        if (node.Identifier.Text == _oldClassName)
        {
            // Prüfen ob die Typ-Argumente mit unserer Instantiierung übereinstimmen
            var args = node.TypeArgumentList.Arguments;
            bool matches = args.Count == _typeMap.Count;
            if (matches)
            {
                var typeParams = _typeMap.Keys.ToList();
                for (int i = 0; i < args.Count && matches; i++)
                {
                    var argText = args[i].ToString().Trim();
                    if (_typeMap.TryGetValue(typeParams[i], out var expected))
                        matches = argText == expected;
                }
            }

            if (matches)
            {
                return SyntaxFactory.IdentifierName(_newClassName)
                    .WithTriviaFrom(node);
            }
        }

        // Typ-Argumente in generischen Typen rewriten: List<T> → List<int>
        return base.VisitGenericName(node);
    }

    public override SyntaxNode? VisitArrayType(ArrayTypeSyntax node)
    {
        // T[] → int[]
        if (node.ElementType is IdentifierNameSyntax idType
            && _typeMap.TryGetValue(idType.Identifier.Text, out var concrete))
        {
            return node.WithElementType(
                SyntaxFactory.IdentifierName(concrete)
                    .WithTriviaFrom(idType));
        }
        return base.VisitArrayType(node);
    }

    public override SyntaxNode? VisitNullableType(NullableTypeSyntax node)
    {
        // T? → int?
        if (node.ElementType is IdentifierNameSyntax idType
            && _typeMap.TryGetValue(idType.Identifier.Text, out var concrete))
        {
            return node.WithElementType(
                SyntaxFactory.IdentifierName(concrete)
                    .WithTriviaFrom(idType));
        }
        return base.VisitNullableType(node);
    }

    // ── Objekt-Erstellung ─────────────────────────────────────────────────────

    public override SyntaxNode? VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        // new Stack<T>() im Methodenbody → new Stack_int()
        if (node.Type is GenericNameSyntax gn && gn.Identifier.Text == _oldClassName)
        {
            return node.WithType(
                SyntaxFactory.IdentifierName(_newClassName)
                    .WithTriviaFrom(node.Type));
        }
        return base.VisitObjectCreationExpression(node);
    }

    // ── typeof ────────────────────────────────────────────────────────────────

    public override SyntaxNode? VisitTypeOfExpression(TypeOfExpressionSyntax node)
    {
        if (node.Type is IdentifierNameSyntax idType
            && _typeMap.TryGetValue(idType.Identifier.Text, out var concrete))
        {
            return node.WithType(
                SyntaxFactory.IdentifierName(concrete)
                    .WithTriviaFrom(idType));
        }
        return base.VisitTypeOfExpression(node);
    }
}