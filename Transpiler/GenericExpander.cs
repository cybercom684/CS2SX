// ============================================================================
// CS2SX — Transpiler/GenericExpander.cs
//
// Expandiert generische C#-Klassen zu konkreten C-Structs und Funktionen.
//
// Strategie: Template-Expansion (wie C++ Templates)
// Für jede konkrete Instantiierung wird die generische Klasse mit substituierten
// Typen neu transpiliert.
//
// Beispiel:
//   class Stack<T> {
//       private T[] _items;
//       private int _count;
//       public void Push(T item) { _items[_count++] = item; }
//       public T Pop() { return _items[--_count]; }
//   }
//
//   → Stack<int> expandiert zu:
//
//   typedef struct Stack_int {
//       int* f_items;
//       int f_count;
//   } Stack_int;
//
//   void Stack_int_Push(Stack_int* self, int item);
//   int  Stack_int_Pop(Stack_int* self);
//
//   void Stack_int_Push(Stack_int* self, int item) {
//       self->f_items[self->f_count++] = item;
//   }
//   int Stack_int_Pop(Stack_int* self) {
//       return self->f_items[--self->f_count];
//   }
// ============================================================================

using CS2SX.Core;
using CS2SX.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    /// Erzeugt für alle gesammelten Instantiierungen den C-Header-Code.
    /// Wird in _forward.h oder eine separate generic_types.h eingebettet.
    /// </summary>
    public string ExpandHeaders()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("// ── Expanded Generic Types (auto-generated) ──────────────────────────");
        sb.AppendLine();

        foreach (var inst in _collector.Instantiations)
        {
            if (!_collector.GenericClasses.TryGetValue(inst.BaseName, out var classDef))
                continue;

            sb.AppendLine(ExpandClassHeader(classDef, inst));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Erzeugt für alle gesammelten Instantiierungen den C-Implementierungs-Code.
    /// Wird als separate .c-Datei geschrieben.
    /// </summary>
    public string ExpandImplementations()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("// ── Expanded Generic Implementations (auto-generated) ────────────────");
        sb.AppendLine();

        foreach (var inst in _collector.Instantiations)
        {
            if (!_collector.GenericClasses.TryGetValue(inst.BaseName, out var classDef))
                continue;

            sb.AppendLine(ExpandClassImpl(classDef, inst));
        }

        return sb.ToString();
    }

    // ── Header-Expansion ──────────────────────────────────────────────────────

    private string ExpandClassHeader(ClassDeclarationSyntax cls, GenericInstantiation inst)
    {
        var sb = new System.Text.StringBuilder();
        var cName = inst.CName;
        var typeMap = BuildTypeMap(cls, inst);

        sb.AppendLine($"// Generic expansion: {inst}");

        // Forward declaration
        sb.AppendLine($"typedef struct {cName} {cName};");
        sb.AppendLine();

        // Struct definition
        sb.AppendLine($"struct {cName}");
        sb.AppendLine("{");

        foreach (var field in cls.Members.OfType<FieldDeclarationSyntax>())
        {
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            var csType = field.Declaration.Type.ToString().Trim();
            var cType = SubstituteType(csType, typeMap);
            var needsPtr = NeedsPointer(csType, typeMap);
            var ptr = needsPtr ? "*" : "";

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                sb.AppendLine($"    {cType}{ptr} f_{fieldName};");
            }
        }

        foreach (var prop in cls.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (prop.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;
            if (!IsAutoProperty(prop)) continue;

            var csType = prop.Type.ToString().Trim();
            var cType = SubstituteType(csType, typeMap);
            var ptr = NeedsPointer(csType, typeMap) ? "*" : "";
            sb.AppendLine($"    {cType}{ptr} f_{prop.Identifier};");
        }

        sb.AppendLine("};");
        sb.AppendLine();

        // _New Funktion
        sb.AppendLine($"{cName}* {cName}_New();");

        // Methoden-Signaturen
        foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
        {
            if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword))) continue;

            var sig = BuildMethodSignature(method, cName, typeMap, withSemicolon: true);
            sb.AppendLine(sig);
        }

        sb.AppendLine();
        return sb.ToString();
    }

    // ── Implementierungs-Expansion ─────────────────────────────────────────────

    private string ExpandClassImpl(ClassDeclarationSyntax cls, GenericInstantiation inst)
    {
        var sb = new System.Text.StringBuilder();
        var cName = inst.CName;
        var typeMap = BuildTypeMap(cls, inst);

        sb.AppendLine($"// Implementation: {inst}");

        // _New
        sb.AppendLine($"{cName}* {cName}_New()");
        sb.AppendLine("{");
        sb.AppendLine($"    {cName}* self = ({cName}*)malloc(sizeof({cName}));");
        sb.AppendLine("    if (!self) return NULL;");
        sb.AppendLine($"    memset(self, 0, sizeof({cName}));");

        // Feld-Initializer
        foreach (var field in cls.Members.OfType<FieldDeclarationSyntax>())
        {
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;
            foreach (var v in field.Declaration.Variables)
            {
                if (v.Initializer == null) continue;
                var fieldName = v.Identifier.Text.TrimStart('_');
                var initVal = SubstituteExpression(v.Initializer.Value.ToString(), typeMap);
                sb.AppendLine($"    self->f_{fieldName} = {initVal};");
            }
        }

        // Expliziter Konstruktor
        var ctor = cls.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
        if (ctor?.Body != null)
        {
            sb.AppendLine("    // Constructor body (text substitution):");
            foreach (var stmt in ctor.Body.Statements)
            {
                var stmtText = SubstituteStatement(stmt.ToString(), typeMap, cName);
                sb.AppendLine("    " + stmtText.Replace("\n", "\n    ").TrimEnd());
            }
        }

        sb.AppendLine("    return self;");
        sb.AppendLine("}");
        sb.AppendLine();

        // Methoden
        foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
        {
            if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword))) continue;
            sb.AppendLine(ExpandMethod(method, cName, typeMap));
        }

        return sb.ToString();
    }

    private string ExpandMethod(MethodDeclarationSyntax method, string cName,
        Dictionary<string, string> typeMap)
    {
        var sb = new System.Text.StringBuilder();
        var sig = BuildMethodSignature(method, cName, typeMap, withSemicolon: false);
        sb.AppendLine(sig);
        sb.AppendLine("{");

        if (method.Body != null)
        {
            foreach (var stmt in method.Body.Statements)
            {
                var stmtText = SubstituteStatement(stmt.ToString(), typeMap, cName);
                // Einrücken
                foreach (var line in stmtText.Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        sb.AppendLine("    " + line.TrimEnd());
                }
            }
        }
        else if (method.ExpressionBody != null)
        {
            var exprText = SubstituteExpression(method.ExpressionBody.Expression.ToString(), typeMap);
            var retType = method.ReturnType.ToString().Trim();
            if (retType != "void")
                sb.AppendLine("    return " + exprText + ";");
            else
                sb.AppendLine("    " + exprText + ";");
        }

        sb.AppendLine("}");
        sb.AppendLine();
        return sb.ToString();
    }

    // ── Typ-Substitution ──────────────────────────────────────────────────────

    /// <summary>
    /// Baut eine Mapping-Tabelle: T → int, U → string etc.
    /// </summary>
    private static Dictionary<string, string> BuildTypeMap(
        ClassDeclarationSyntax cls, GenericInstantiation inst)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        var typeParams = cls.TypeParameterList?.Parameters.ToList()
                         ?? new List<TypeParameterSyntax>();

        for (int i = 0; i < typeParams.Count && i < inst.TypeArguments.Count; i++)
        {
            map[typeParams[i].Identifier.Text] = inst.TypeArguments[i];
        }

        return map;
    }

    /// <summary>
    /// Ersetzt Typ-Parameter in einem C#-Typen durch konkrete Typen,
    /// dann mappt auf C.
    /// </summary>
    public static string SubstituteType(string csType, Dictionary<string, string> typeMap)
    {
        var resolved = csType;

        // Direkte Substitution: T → int
        if (typeMap.TryGetValue(csType, out var direct))
            resolved = direct;

        // Array: T[] → int[]
        if (csType.EndsWith("[]"))
        {
            var baseType = csType[..^2];
            if (typeMap.TryGetValue(baseType, out var arrBase))
                resolved = arrBase + "[]";
        }

        // List<T> → List<int>
        if (csType.StartsWith("List<") && csType.EndsWith(">"))
        {
            var inner = csType[5..^1].Trim();
            var resolvedInner = typeMap.TryGetValue(inner, out var ri) ? ri : inner;
            var cInner = resolvedInner == "string" ? "str" : TypeRegistry.MapType(resolvedInner);
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
                var ck = (typeMap.TryGetValue(k, out var rk) ? rk : k) == "string" ? "str" : TypeRegistry.MapType(typeMap.TryGetValue(k, out _) ? typeMap[k] : k);
                var cv = (typeMap.TryGetValue(v, out var rv) ? rv : v) == "string" ? "str" : TypeRegistry.MapType(typeMap.TryGetValue(v, out _) ? typeMap[v] : v);
                return $"Dict_{ck}_{cv}*";
            }
        }

        // Auf C mappen
        return TypeRegistry.MapType(resolved);
    }

    private static bool NeedsPointer(string csType, Dictionary<string, string> typeMap)
    {
        var resolved = typeMap.TryGetValue(csType, out var r) ? r : csType;
        if (resolved == "string") return false; // const char* bereits Pointer
        return TypeRegistry.NeedsPointerSuffix(resolved)
            || TypeRegistry.IsList(resolved)
            || TypeRegistry.IsDictionary(resolved);
    }

    /// <summary>
    /// Text-basierte Substitution in generierten Ausdrücken.
    /// Ersetzt T durch den konkreten C-Typ in C-Code.
    /// </summary>
    public static string SubstituteExpression(string expr, Dictionary<string, string> typeMap)
    {
        var result = expr;
        foreach (var (typeParam, concreteType) in typeMap)
        {
            var cType = TypeRegistry.MapType(concreteType);
            // Vorsichtige Ersetzung: nur als ganzes Wort
            result = System.Text.RegularExpressions.Regex.Replace(
                result,
                $@"\b{typeParam}\b",
                cType);
        }
        return result;
    }

    /// <summary>
    /// Text-basierte Substitution in Statement-Text.
    /// Ersetzt außerdem Klassen-Namen (Stack → Stack_int) und this-Felder.
    /// </summary>
    private static string SubstituteStatement(string stmt,
        Dictionary<string, string> typeMap, string cName)
    {
        var result = SubstituteExpression(stmt, typeMap);

        // Typ-Casts: (T)x → (int)x
        foreach (var (typeParam, concreteType) in typeMap)
        {
            var cType = TypeRegistry.MapType(concreteType);
            result = result.Replace($"({typeParam})", $"({cType})");
            result = result.Replace($"({typeParam}*)", $"({cType}*)");
        }

        return result;
    }

    // ── Signatur-Bau ───────────────────────────────────────────────────────────

    private string BuildMethodSignature(MethodDeclarationSyntax method,
        string cName, Dictionary<string, string> typeMap, bool withSemicolon)
    {
        var retCsType = method.ReturnType.ToString().Trim();
        var retCType = SubstituteType(retCsType, typeMap);
        var isStatic = method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
        var methodName = $"{cName}_{method.Identifier.Text}";

        var paramList = new List<string>();
        if (!isStatic)
            paramList.Add($"{cName}* self");

        foreach (var p in method.ParameterList.Parameters)
        {
            var pCsType = p.Type?.ToString().Trim() ?? "int";
            var pCType = SubstituteType(pCsType, typeMap);
            var isRef = p.Modifiers.Any(m =>
                m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword));
            var ptr = isRef ? "*" : "";

            // Typ-Parameter direkt im Parametertyp
            if (typeMap.ContainsKey(pCsType))
            {
                var concrete = typeMap[pCsType];
                pCType = TypeRegistry.MapType(concrete);
                if (!TypeRegistry.IsPrimitive(concrete) && concrete != "string")
                    ptr = "*";
            }

            paramList.Add($"{pCType}{ptr} {p.Identifier}");
        }

        var suffix = withSemicolon ? ";" : "";
        return $"{retCType} {methodName}({string.Join(", ", paramList)}){suffix}";
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    private static bool IsAutoProperty(PropertyDeclarationSyntax prop) =>
        prop.AccessorList != null
        && prop.AccessorList.Accessors.All(a => a.Body == null && a.ExpressionBody == null);

    /// <summary>
    /// Schreibt den generierten Code in Dateien.
    /// Gibt die Pfade der erzeugten .h und .c Dateien zurück.
    /// </summary>
    public (string headerPath, string implPath) WriteToFiles(string buildDir)
    {
        if (_collector.Instantiations.Count == 0)
            return (string.Empty, string.Empty);

        var headerContent = ExpandHeaders();
        var implContent = ExpandImplementations();

        var headerPath = Path.Combine(buildDir, "_generics.h");
        var implPath = Path.Combine(buildDir, "_generics.c");

        File.WriteAllText(headerPath, "#pragma once\n#include \"_forward.h\"\n\n" + headerContent);
        File.WriteAllText(implPath, "#include \"_generics.h\"\n\n" + implContent);

        Log.Info($"GenericExpander: {_collector.Instantiations.Count} Expansion(en) → _generics.h / _generics.c");
        return (headerPath, implPath);
    }
}