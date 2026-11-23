using System.Text;
using System.Text.RegularExpressions;
using CS2SX.Logging;
using CS2SX.Transpiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CS2SX.Build.AsyncLowering;

namespace CS2SX.Build;

/// System.Text.Json's JsonSerializer without reflection: at the C# level (after the
/// generics are specialised) every Serialize / Deserialize call becomes a call of a
/// method written for its type, in a class appended to the first source file:
///
///   JsonSerializer.Serialize(player)                 → CS2SX_Json.S_Player(player, null)
///   JsonSerializer.Deserialize&lt;Player&gt;(text, opts)   → CS2SX_Json.D_Player(text, opts)
///
/// The writer of a type writes its public properties (and fields with [JsonInclude]
/// or IncludeFields) like System.Text.Json does; the reader matches the names and
/// fills a new object (parameterless constructor, or the one public constructor
/// whose parameters are named like the members — positional records). Types inside
/// (List, arrays, Dictionary, nullable values, other classes) get their own methods.
/// JsonSerializerOptions becomes CS2SX_JsonOptions (WriteIndented,
/// PropertyNameCaseInsensitive, IncludeFields, PropertyNamingPolicy = CamelCase).
/// Every replacement keeps the number of lines.
public static class JsonLowering
{
    private const string Ns = "System.Text.Json";

    public static List<string> Rewrite(IReadOnlyList<string> files, string outDir)
    {
        if (!files.Any(f => File.ReadAllText(f).Contains("Json"))) return files.ToList();

        var sem = new SemanticModelBuilder(files);
        var edits = files.ToDictionary(f => f, _ => new Edits());
        var gen = new Generator();
        int calls = 0;

        foreach (var file in files)
        {
            if (sem.GetModel(file) is not { } model) continue;
            var root = model.SyntaxTree.GetCompilationUnitRoot();
            var source = model.SyntaxTree.GetText().ToString();
            var e = edits[file];

            // Converters = { new JsonStringEnumConverter() } → StringEnums = true (also .Converters.Add(…))
            foreach (var asg in root.DescendantNodes().OfType<AssignmentExpressionSyntax>())
                if (asg.Left is IdentifierNameSyntax { Identifier.Text: "Converters" } && asg.Parent is InitializerExpressionSyntax
                    && asg.Right is InitializerExpressionSyntax list && list.Expressions.Count > 0 && list.Expressions.All(IsStringEnumConverterNew))
                    e.Replace(asg.SpanStart, asg.Span.Length, "StringEnums = true");
            foreach (var call in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
                if (call.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: "Add", Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Converters" } conv }
                    && call.ArgumentList.Arguments.Count == 1 && IsStringEnumConverterNew(call.ArgumentList.Arguments[0].Expression))
                    e.Replace(call.SpanStart, call.Span.Length,
                        e.Render(source, conv.Expression.SpanStart, conv.Expression.Span.End, all: false) + ".StringEnums = true");

            // JsonSerializerOptions → CS2SX_JsonOptions, JsonNamingPolicy.CamelCase → 1
            foreach (var n in root.DescendantNodes().OfType<SimpleNameSyntax>())
            {
                if (n.Ancestors().OfType<UsingDirectiveSyntax>().Any()) continue;
                var sym = model.GetSymbolInfo(n).Symbol;
                if (sym is IPropertySymbol { Name: "CamelCase", ContainingType.Name: "JsonNamingPolicy" }
                    && n.Parent is MemberAccessExpressionSyntax camel && camel.Name == n)
                {
                    e.Replace(camel.SpanStart, camel.Span.Length, "1");
                    continue;
                }
                if (sym is not INamedTypeSymbol { Name: "JsonSerializerOptions" or "JsonNamingPolicy" } type
                    || type.ContainingNamespace?.ToDisplayString() != Ns) continue;
                if (n.Parent is MemberAccessExpressionSyntax { Expression: var left } && left == n) continue;   // JsonNamingPolicy.CamelCase
                SyntaxNode span = n.Parent is QualifiedNameSyntax q && q.Right == n ? q : n;
                e.Replace(span.SpanStart, span.Span.Length, type.Name == "JsonNamingPolicy" ? "int" : "CS2SX_JsonOptions");
            }
            foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (Call(inv, model) is not { } call || Problem(call, model) != null) continue;
                var (method, type) = call;
                var suf = gen.Need(type);
                var args = inv.ArgumentList.Arguments;
                string Arg(int i) => e.Render(source, args[i].Expression.SpanStart, args[i].Expression.Span.End, all: false);
                var opts = args.Count > 1 ? Arg(1) : "null";
                var name = method.Name == "Serialize" ? "S_" : "D_";
                e.Replace(inv.SpanStart, inv.Span.Length, $"CS2SX_Json.{name}{suf}({Arg(0)}, {opts})");
                calls++;
            }

        }
        if (calls == 0) return files.ToList();

        var result = new List<string>();
        var dir = Path.Combine(outDir, "_cs");
        Directory.CreateDirectory(dir);
        foreach (var file in files)
        {
            var e = edits[file];
            if (e.List.Count == 0) { result.Add(file); continue; }
            var source = File.ReadAllText(file);
            var text = e.Render(source, 0, source.Length, all: true);
            var target = Path.Combine(dir, Path.GetFileName(file));
            if (!File.Exists(target) || File.ReadAllText(target) != text)
                File.WriteAllText(target, text);
            result.Add(target);
        }
        // The writers and readers go in a file of their own, after the project's files:
        // the structs they use are known by then.
        var usings = new StringBuilder("using System;\nusing System.Collections.Generic;\nusing System.Text;\nusing System.Text.Json;\n");
        var code = gen.Source();
        foreach (var ns in gen.Namespaces) usings.Append($"using {ns};\n");
        var jsonFile = Path.Combine(dir, "CS2SX_Json.cs");
        var jsonText = usings + "\n" + Runtime + "\n" + code + "\n";
        if (!File.Exists(jsonFile) || File.ReadAllText(jsonFile) != jsonText) File.WriteAllText(jsonFile, jsonText);
        result.Add(jsonFile);
        Log.Info($"json: {calls} call(s), {gen.Count} type(s)");
        return result;
    }

    private static bool IsStringEnumConverterNew(ExpressionSyntax e) =>
        e is ObjectCreationExpressionSyntax oc
        && (oc.ArgumentList == null || oc.ArgumentList.Arguments.Count == 0)
        && oc.Type.ToString() is "JsonStringEnumConverter" or "System.Text.Json.Serialization.JsonStringEnumConverter";

    private static bool IsNsQualifier(ExpressionSyntax e, SemanticModel model) =>
        model.GetSymbolInfo(e).Symbol is INamespaceSymbol;

    // ── Calls ───────────────────────────────────────────────────────────────

    /// JsonSerializer.Serialize(x[, options]) / Deserialize&lt;T&gt;(text[, options]) with
    /// the type the call is about.
    private static (IMethodSymbol method, ITypeSymbol type)? Call(InvocationExpressionSyntax inv, SemanticModel model)
    {
        var info = model.GetSymbolInfo(inv);
        if ((info.Symbol ?? info.CandidateSymbols.FirstOrDefault()) is not IMethodSymbol m
            || m.ContainingType is not { Name: "JsonSerializer" } c || c.ContainingNamespace?.ToDisplayString() != Ns)
            return null;
        if (m.Name == "Serialize")
        {
            var t = m.TypeArguments.Length == 1 ? m.TypeArguments[0]
                : inv.ArgumentList.Arguments.Count > 0 ? model.GetTypeInfo(inv.ArgumentList.Arguments[0].Expression).Type : null;
            return t == null ? null : (m, t);
        }
        if (m.Name == "Deserialize" && m.TypeArguments.Length == 1) return (m, m.TypeArguments[0]);
        return (m, m.ReturnType);
    }

    private static string? Problem((IMethodSymbol method, ITypeSymbol type) call, SemanticModel model)
    {
        var (m, t) = call;
        if (m.Name is not ("Serialize" or "Deserialize"))
            return $"JsonSerializer.{m.Name} wird nicht unterstützt — JsonSerializer.Serialize(wert) und JsonSerializer.Deserialize<T>(text) verwenden.";
        if (m.Name == "Deserialize" && m.TypeArguments.Length != 1)
            return "JsonSerializer.Deserialize braucht den Typ als Typargument: JsonSerializer.Deserialize<Spielstand>(text).";
        var p = m.Parameters;
        if (p.Length == 0 || p.Length > 2
            || m.Name == "Deserialize" && p[0].Type.SpecialType != SpecialType.System_String
            || m.Name == "Serialize" && p.Length == 2 && p[1].Type.Name != "JsonSerializerOptions"
            || m.Name == "Deserialize" && p.Length == 2 && p[1].Type.Name != "JsonSerializerOptions"
            || m.Name == "Serialize" && m.TypeArguments.Length == 0 && p.Length == 2 && p[1].Type.Name == "Type")
            return $"Diese Form von JsonSerializer.{m.Name} wird nicht unterstützt — nur ({(m.Name == "Serialize" ? "wert" : "string text")}) und (…, JsonSerializerOptions).";
        return new Generator().Check(t);
    }

    /// The message for a JsonSerializer call that is still in the source after lowering.
    public static IEnumerable<(SyntaxNode node, string message)> Problems(SyntaxNode root, SemanticModel model)
    {
        foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (!inv.Expression.ToString().Contains("JsonSerializer")) continue;
            if (Call(inv, model) is not { } call) continue;
            yield return (inv, Problem(call, model) ?? "Dieser JsonSerializer-Aufruf wird nicht unterstützt.");
        }
    }

    // ── Per-type code ───────────────────────────────────────────────────────

    private sealed record Member(string Name, string Json, string Camel, ITypeSymbol Type, bool Field, bool CanSet, bool InitOnly, bool StringEnum = false);

    private sealed class Generator
    {
        private readonly Dictionary<string, ITypeSymbol> _types = new(StringComparer.Ordinal);
        private readonly Queue<string> _todo = new();
        public readonly SortedSet<string> Namespaces = new(StringComparer.Ordinal);
        public int Count => _types.Count;

        public static string Text(ITypeSymbol t) =>
            (t.IsReferenceType ? t.WithNullableAnnotation(NullableAnnotation.NotAnnotated) : t)
                .ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        public static string Suffix(ITypeSymbol t) =>
            Regex.Replace(Text(t).Replace("[]", "Arr").Replace("?", "N"), @"[^A-Za-z0-9]+", "_").Trim('_')
                .Replace("string", "str");

        public string Need(ITypeSymbol t)
        {
            if (t.IsReferenceType) t = t.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            var s = Suffix(t);
            if (_types.TryAdd(s, t)) _todo.Enqueue(s);
            return s;
        }

        // ── what is supported ──

        private static bool IsNamed(ITypeSymbol t, string name, int arity, string ns) =>
            t is INamedTypeSymbol n && n.Name == name && n.TypeArguments.Length == arity && n.ContainingNamespace?.ToDisplayString() == ns;
        private static bool IsList(ITypeSymbol t) => IsNamed(t, "List", 1, "System.Collections.Generic");
        private static bool IsDict(ITypeSymbol t) => IsNamed(t, "Dictionary", 2, "System.Collections.Generic");
        private static bool IsNullableValue(ITypeSymbol t) => t is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };
        private static ITypeSymbol Arg(ITypeSymbol t, int i) => ((INamedTypeSymbol)t).TypeArguments[i];

        private static bool IsScalar(ITypeSymbol t) => t.SpecialType is SpecialType.System_String or SpecialType.System_Boolean
            or SpecialType.System_Char or SpecialType.System_Double or SpecialType.System_Single
            || IsInteger(t) || t.TypeKind == TypeKind.Enum;

        private static bool IsInteger(ITypeSymbol t) => t.SpecialType is SpecialType.System_Int32 or SpecialType.System_Int64
            or SpecialType.System_Int16 or SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_UInt16
            or SpecialType.System_UInt32 or SpecialType.System_UInt64;

        /// Why t cannot be written / read, or null.
        public string? Check(ITypeSymbol t) => Check(t, Text(t), new HashSet<string>());

        private string? Check(ITypeSymbol t, string path, HashSet<string> seen)
        {
            if (IsScalar(t)) return null;
            if (IsNullableValue(t)) return Check(Arg(t, 0), path, seen);
            if (t is IArrayTypeSymbol a) return a.Rank != 1 ? $"JSON: mehrdimensionale Arrays ({path}) werden nicht unterstützt." : Check(a.ElementType, path + "[]", seen);
            if (IsList(t)) return Check(Arg(t, 0), path + "[]", seen);
            if (IsDict(t))
                return Arg(t, 0).SpecialType == SpecialType.System_String || IsInteger(Arg(t, 0))
                    ? Check(Arg(t, 1), path + "[]", seen)
                    : $"JSON: Dictionary-Schlüssel müssen string oder ganze Zahlen sein ({path}).";
            if (t is not INamedTypeSymbol n || n.TypeKind is not (TypeKind.Class or TypeKind.Struct)
                || n.IsAbstract || n.SpecialType == SpecialType.System_Object
                || n.ContainingNamespace?.ToDisplayString() is { } ns && (ns == "System" || ns.StartsWith("System.")))
                return $"JSON: Der Typ {Text(t)} ({path}) wird nicht unterstützt — erlaubt sind Zahlen, bool, char, string, Enums, List<T>, Arrays, Dictionary<string, T>, Nullable und eigene Klassen/Structs/Records.";
            if (!seen.Add(Text(t))) return null;
            if (Constructor(n) == null)
                return $"JSON: {Text(t)} braucht einen öffentlichen parameterlosen Konstruktor oder genau einen öffentlichen Konstruktor, dessen Parameter wie die Eigenschaften heißen.";
            foreach (var m in Members(n))
                if (Check(m.Type, path + "." + m.Name, seen) is { } why) return why;
            return null;
        }

        // ── members ──

        private static AttributeData? Attr(ISymbol s, string name) =>
            s.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == name
                && a.AttributeClass.ContainingNamespace?.ToDisplayString() == Ns + ".Serialization");

        public static List<Member> Members(INamedTypeSymbol t)
        {
            var chain = new List<INamedTypeSymbol>();
            for (var b = t; b != null && b.SpecialType != SpecialType.System_Object && b.SpecialType != SpecialType.System_ValueType; b = b.BaseType)
                chain.Insert(0, b);
            var list = new List<Member>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var type in chain)
                foreach (var s in type.GetMembers())
                {
                    if (s.IsStatic || s.IsImplicitlyDeclared || Attr(s, "JsonIgnoreAttribute") != null) continue;
                    bool include = Attr(s, "JsonIncludeAttribute") != null;
                    ITypeSymbol? mt = null; bool field = false, canSet = false, initOnly = false;
                    if (s is IPropertySymbol { IsIndexer: false, GetMethod: not null } p
                        && (p.DeclaredAccessibility == Accessibility.Public || include)
                        && p.Name != "EqualityContract")
                    {
                        mt = p.Type;
                        canSet = p.SetMethod is { } set && (set.DeclaredAccessibility == Accessibility.Public || include);
                        initOnly = p.SetMethod?.IsInitOnly == true;
                    }
                    else if (s is IFieldSymbol { IsConst: false } f && (f.DeclaredAccessibility == Accessibility.Public || include))
                    {
                        mt = f.Type; field = !include; canSet = !f.IsReadOnly;
                    }
                    if (mt == null || !names.Add(s.Name)) continue;
                    var json = Attr(s, "JsonPropertyNameAttribute")?.ConstructorArguments.FirstOrDefault().Value as string;
                    list.Add(new Member(s.Name, json ?? s.Name, json ?? Camel(s.Name), mt, field, canSet, initOnly, IsStringEnumConverter(s)));
                }
            return list;
        }

        /// [JsonConverter(typeof(JsonStringEnumConverter))] on a member or an enum type.
        public static bool IsStringEnumConverter(ISymbol s) =>
            Attr(s, "JsonConverterAttribute") is { } a && a.ConstructorArguments.FirstOrDefault().Value is INamedTypeSymbol c
            && c.Name == "JsonStringEnumConverter";

        /// JsonNamingPolicy.CamelCase: the leading capitals become small ("URLName" → "urlName").
        private static string Camel(string name)
        {
            if (name.Length == 0 || !char.IsUpper(name[0])) return name;
            var c = name.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (i == 1 && !char.IsUpper(c[i])) break;
                bool hasNext = i + 1 < c.Length;
                if (i > 0 && hasNext && !char.IsUpper(c[i + 1]))
                {
                    if (c[i + 1] == ' ') c[i] = char.ToLowerInvariant(c[i]);
                    break;
                }
                c[i] = char.ToLowerInvariant(c[i]);
            }
            return new string(c);
        }

        /// null: parameterless (or a struct); otherwise the constructor to call.
        private static (bool ok, IMethodSymbol? ctor) CtorInfo(INamedTypeSymbol t)
        {
            var ctors = t.InstanceConstructors.Where(c => c.DeclaredAccessibility == Accessibility.Public
                && !(c.Parameters.Length == 1 && SymbolEqualityComparer.Default.Equals(c.Parameters[0].Type, t))).ToList();
            if (t.TypeKind == TypeKind.Struct && ctors.All(c => c.Parameters.Length > 0) || ctors.Any(c => c.Parameters.Length == 0))
                return (true, null);
            if (ctors.Count != 1) return (false, null);
            var members = Members(t);
            foreach (var p in ctors[0].Parameters)
                if (!members.Any(m => string.Equals(m.Name, p.Name, StringComparison.OrdinalIgnoreCase)
                                      && SymbolEqualityComparer.Default.Equals(m.Type, p.Type)))
                    return (false, null);
            return (true, ctors[0]);
        }

        private static object? Constructor(INamedTypeSymbol t) => CtorInfo(t).ok ? (object?)(CtorInfo(t).ctor ?? (object)true) : null;

        // ── source ──

        private readonly StringBuilder _out = new();

        public string Source()
        {
            while (_todo.Count > 0)
            {
                var s = _todo.Dequeue();
                Emit(s, _types[s]);
            }
            return "public static class CS2SX_Json { " + _out + "}";
        }

        private static string Default(ITypeSymbol t) =>
            t.IsReferenceType || IsNullableValue(t) ? "null"
            : t.SpecialType == SpecialType.System_Boolean ? "false"
            : t.SpecialType == SpecialType.System_Char ? "'\\0'"
            : IsScalar(t) ? (t.TypeKind == TypeKind.Enum ? $"({Text(t)})0" : "0")
            : $"new {Text(t)}()";

        private void Emit(string s, ITypeSymbol t)
        {
            var T = Text(t);
            if (t is INamedTypeSymbol { ContainingNamespace: { IsGlobalNamespace: false } ns } && !ns.ToDisplayString().StartsWith("System"))
                Namespaces.Add(ns.ToDisplayString());
            var nullable = t.IsReferenceType ? "?" : "";
            _out.Append($"public static string S_{s}({T}{nullable} v, CS2SX_JsonOptions? o) {{ StringBuilder sb = new StringBuilder(); W_{s}(sb, v, o, 0); return sb.ToString(); }} ");
            _out.Append($"public static {T} D_{s}(string json, CS2SX_JsonOptions? o) {{ if (json == null) throw new ArgumentNullException(\"json\"); "
                      + $"CS2SX_JsonR r = new CS2SX_JsonR(json, o != null && o.PropertyNameCaseInsensitive); {T} v = R_{s}(r, o, 0); r.End(); return v; }} ");
            _out.Append($"public static void W_{s}(StringBuilder sb, {T}{nullable} v, CS2SX_JsonOptions? o, int d) {{ {WriteBody(t)} }} ");
            _out.Append($"public static {T} R_{s}(CS2SX_JsonR r, CS2SX_JsonOptions? o, int d) {{ {ReadBody(t)} }} ");
        }

        private string WriteBody(ITypeSymbol t)
        {
            if (t.SpecialType == SpecialType.System_String) return "CS2SX_JsonW.Str(sb, v);";
            if (t.SpecialType == SpecialType.System_Boolean) return "sb.Append(v ? \"true\" : \"false\");";
            if (t.SpecialType == SpecialType.System_Char) return "CS2SX_JsonW.Str(sb, v.ToString());";
            if (t.SpecialType == SpecialType.System_Double) return "CS2SX_JsonW.Dbl(sb, v);";
            if (t.SpecialType == SpecialType.System_Single) return "CS2SX_JsonW.Flt(sb, v);";
            if (t.SpecialType is SpecialType.System_Int64 or SpecialType.System_UInt64) return "sb.Append(v);";
            if (t.SpecialType == SpecialType.System_UInt32) return "sb.Append((long)v);";
            if (IsInteger(t)) return "sb.Append((int)v);";
            if (t.TypeKind == TypeKind.Enum)
            {
                var number = ((INamedTypeSymbol)t).EnumUnderlyingType!.SpecialType is SpecialType.System_Int64 or SpecialType.System_UInt64 or SpecialType.System_UInt32
                    ? "sb.Append((long)v);" : "sb.Append((int)v);";
                // JsonStringEnumConverter (on the enum, or in the options): the name.
                return IsStringEnumConverter(t) ? "CS2SX_JsonW.Str(sb, v.ToString());"
                    : $"if (o != null && o.StringEnums) CS2SX_JsonW.Str(sb, v.ToString()); else {number}";
            }
            if (IsNullableValue(t))
                return $"if (v.HasValue) W_{Need(Arg(t, 0))}(sb, v.Value, o, d); else sb.Append(\"null\");";
            var guard = "if (v == null) { sb.Append(\"null\"); return; } if (d > 64) throw new JsonException(\"JSON: zu tief verschachtelt (mehr als 64 Ebenen) — enthält das Objekt sich selbst?\"); ";
            if (t is IArrayTypeSymbol a)
                return guard + $"sb.Append('['); for (int i = 0; i < v.Length; i++) {{ CS2SX_JsonW.Item(sb, o, d, i == 0); W_{Need(a.ElementType)}(sb, v[i], o, d + 1); }} CS2SX_JsonW.End(sb, o, d, v.Length > 0, ']');";
            if (IsList(t))
                return guard + $"sb.Append('['); for (int i = 0; i < v.Count; i++) {{ CS2SX_JsonW.Item(sb, o, d, i == 0); W_{Need(Arg(t, 0))}(sb, v[i], o, d + 1); }} CS2SX_JsonW.End(sb, o, d, v.Count > 0, ']');";
            if (IsDict(t))
            {
                var key = Arg(t, 0).SpecialType == SpecialType.System_String ? "kv.Key" : "kv.Key.ToString()";
                return guard + $"sb.Append('{{'); int n = 0; foreach (var kv in v) {{ CS2SX_JsonW.Key(sb, o, d, n == 0, {key}); n++; W_{Need(Arg(t, 1))}(sb, kv.Value, o, d + 1); }} CS2SX_JsonW.End(sb, o, d, n > 0, '}}');";
            }
            if (t.IsValueType) guard = "if (d > 64) throw new JsonException(\"JSON: zu tief verschachtelt (mehr als 64 Ebenen).\"); ";
            var b = new StringBuilder(guard);
            b.Append("bool camel = o != null && o.PropertyNamingPolicy == 1; bool fields = o != null && o.IncludeFields; int n = 0; sb.Append('{'); ");
            foreach (var m in Members((INamedTypeSymbol)t))
            {
                var name = m.Json == m.Camel ? Lit(m.Json) : $"camel ? {Lit(m.Camel)} : {Lit(m.Json)}";
                var value = m.StringEnum && m.Type.TypeKind == TypeKind.Enum
                    ? $"CS2SX_JsonW.Str(sb, v.{m.Name}.ToString());"
                    : $"W_{Need(m.Type)}(sb, v.{m.Name}, o, d + 1);";
                var write = $"CS2SX_JsonW.Key(sb, o, d, n == 0, {name}); n++; {value}";
                b.Append(m.Field ? $"if (fields) {{ {write} }} " : write + " ");
            }
            b.Append("CS2SX_JsonW.End(sb, o, d, n > 0, '}');");
            return b.ToString();
        }

        private string ReadBody(ITypeSymbol t)
        {
            var T = Text(t);
            if (t.SpecialType == SpecialType.System_String) return "if (r.Null()) return null!; return r.Str();";
            if (t.SpecialType == SpecialType.System_Boolean) return "return r.Bool();";
            if (t.SpecialType == SpecialType.System_Char) return "return r.Chr();";
            if (t.SpecialType == SpecialType.System_Double) return "return r.Dbl();";
            if (t.SpecialType == SpecialType.System_Single) return "return (float)r.Dbl();";
            if (t.SpecialType == SpecialType.System_UInt64) return "return r.ULng();";
            if (t.SpecialType == SpecialType.System_Int64) return "return r.Lng();";
            if (IsInteger(t) || t.TypeKind == TypeKind.Enum)
            {
                var under = t.TypeKind == TypeKind.Enum ? ((INamedTypeSymbol)t).EnumUnderlyingType! : t;
                var byName = "";
                if (t.TypeKind == TypeKind.Enum)
                {
                    var names = new StringBuilder();
                    foreach (var f in t.GetMembers().OfType<IFieldSymbol>().Where(f => f.HasConstantValue))
                        names.Append($"if (p == {Lit(f.Name.ToLowerInvariant())}) acc |= (long){T}.{f.Name}; else ");
                    byName = "if (r.IsStr()) { string[] parts = r.Str().Split(','); long acc = 0; "
                           + "for (int i = 0; i < parts.Length; i++) { string p = parts[i].Trim().ToLower(); "
                           + names + $"throw r.Err(\"unbekannter Wert für {T}\"); }} return ({T})acc; }} ";
                }
                if (under.SpecialType == SpecialType.System_UInt64) return $"return ({T})r.ULng();";
                var (lo, hi) = under.SpecialType switch
                {
                    SpecialType.System_Int32 => ("-2147483648L", "2147483647L"),
                    SpecialType.System_UInt32 => ("0L", "4294967295L"),
                    SpecialType.System_Int16 => ("-32768L", "32767L"),
                    SpecialType.System_UInt16 => ("0L", "65535L"),
                    SpecialType.System_SByte => ("-128L", "127L"),
                    SpecialType.System_Byte => ("0L", "255L"),
                    _ => ("-9223372036854775807L - 1L", "9223372036854775807L"),
                };
                return byName + $"long x = r.Lng(); if (x < {lo} || x > {hi}) throw r.Err(\"Zahl zu groß für {Text(under)}\"); return ({T})x;";
            }
            if (IsNullableValue(t)) return $"if (r.Null()) return null; return R_{Need(Arg(t, 0))}(r, o, d);";
            var guard = (t.IsReferenceType ? "if (r.Null()) return null!; " : "if (r.Null()) throw r.Err(\"null für " + T + "\"); ")
                      + "if (d > 64) throw r.Err(\"zu tief verschachtelt\"); ";
            if (t is IArrayTypeSymbol a)
            {
                var et = Text(a.ElementType);
                return guard + $"List<{et}> l = new List<{et}>(); r.Expect('['); if (!r.Close(']')) do {{ l.Add(R_{Need(a.ElementType)}(r, o, d + 1)); }} while (r.More(']')); "
                     + $"{et}[] arr = new {et}[l.Count]; for (int i = 0; i < l.Count; i++) arr[i] = l[i]; return arr;";
            }
            if (IsList(t))
                return guard + $"{T} l = new {T}(); r.Expect('['); if (!r.Close(']')) do {{ l.Add(R_{Need(Arg(t, 0))}(r, o, d + 1)); }} while (r.More(']')); return l;";
            if (IsDict(t))
            {
                var kt = Arg(t, 0);
                var key = kt.SpecialType == SpecialType.System_String ? "k" : $"{Text(kt)}.Parse(k)";
                return guard + $"{T} m = new {T}(); r.Expect('{{'); if (!r.Close('}}')) do {{ string k = r.Key(); m[{key}] = R_{Need(Arg(t, 1))}(r, o, d + 1); }} while (r.More('}}')); return m;";
            }

            var nt = (INamedTypeSymbol)t;
            var members = Members(nt);
            var ctor = CtorInfo(nt).ctor;
            var b = new StringBuilder(guard);
            b.Append("bool camel = o != null && o.PropertyNamingPolicy == 1; bool fields = o != null && o.IncludeFields; ");
            for (int i = 0; i < members.Count; i++)
                b.Append($"{Text(members[i].Type)} m{i} = {Default(members[i].Type)}; bool h{i} = false; ");
            b.Append("r.Expect('{'); if (!r.Close('}')) do { string k = r.Key(); ");
            for (int i = 0; i < members.Count; i++)
            {
                var m = members[i];
                bool readable = m.CanSet || ctor?.Parameters.Any(p => string.Equals(p.Name, m.Name, StringComparison.OrdinalIgnoreCase)) == true;
                if (!readable) continue;
                var name = m.Json == m.Camel ? Lit(m.Json) : $"camel ? {Lit(m.Camel)} : {Lit(m.Json)}";
                var cond = (m.Field ? "fields && " : "") + $"r.Is(k, {name})";
                b.Append($"if ({cond}) {{ m{i} = R_{Need(m.Type)}(r, o, d + 1); h{i} = true; }} else ");
            }
            b.Append("r.Skip(); } while (r.More('}')); ");

            var used = new HashSet<int>();
            string args = "";
            if (ctor != null)
                args = string.Join(", ", ctor.Parameters.Select(p =>
                {
                    int i = members.FindIndex(m => string.Equals(m.Name, p.Name, StringComparison.OrdinalIgnoreCase));
                    used.Add(i);
                    return "m" + i;
                }));
            var init = new List<string>();
            for (int i = 0; i < members.Count; i++)
                if (!used.Contains(i) && members[i].CanSet && members[i].InitOnly)
                {
                    init.Add($"{members[i].Name} = m{i}");
                    used.Add(i);
                }
            b.Append($"{T} v = new {T}({args})" + (init.Count > 0 ? " { " + string.Join(", ", init) + " }" : "") + "; ");
            for (int i = 0; i < members.Count; i++)
                if (!used.Contains(i) && members[i].CanSet)
                    b.Append($"if (h{i}) v.{members[i].Name} = m{i}; ");
            b.Append("return v;");
            return b.ToString();
        }

        private static string Lit(string s) => SymbolDisplay.FormatLiteral(s, quote: true);
    }

    // ── The helpers (C#, transpiled with the project) ───────────────────────

    private static readonly string Runtime = string.Join(" ", @"
public class CS2SX_JsonOptions
{
    public bool WriteIndented;
    public bool PropertyNameCaseInsensitive;
    public bool IncludeFields;
    public int PropertyNamingPolicy;
    public bool StringEnums;
}
public static class CS2SX_JsonW
{
    public static void Str(StringBuilder sb, string? s)
    {
        if (s == null) { sb.Append(""null""); return; }
        sb.Append('""');
        char hi = (char)0;
        foreach (char c in s)
        {
            int code = (int)c;
            if (hi != (char)0) { sb.Append(new string(new char[] { hi, c })); hi = (char)0; continue; }
            if (code >= 0xD800 && code <= 0xDBFF) { hi = c; continue; }
            if (c == '""') sb.Append(""\\\"""");
            else if (c == '\\') sb.Append(""\\\\"");
            else if (c == '\n') sb.Append(""\\n"");
            else if (c == '\r') sb.Append(""\\r"");
            else if (c == '\t') sb.Append(""\\t"");
            else if (code >= 0 && code < 32) { sb.Append(""\\u00""); sb.Append(Hex(code >> 4)); sb.Append(Hex(code & 15)); }
            else sb.Append(c);
        }
        sb.Append('""');
    }
    private static char Hex(int n) { return n < 10 ? (char)('0' + n) : (char)('A' + n - 10); }
    public static void Dbl(StringBuilder sb, double v)
    {
        if (double.IsNaN(v) || double.IsInfinity(v)) throw new ArgumentException(""JSON kann NaN und Unendlich nicht darstellen."");
        sb.Append(v.ToString());
    }
    public static void Flt(StringBuilder sb, float v)
    {
        if (float.IsNaN(v) || float.IsInfinity(v)) throw new ArgumentException(""JSON kann NaN und Unendlich nicht darstellen."");
        sb.Append(v.ToString());
    }
    private static void Line(StringBuilder sb, int depth)
    {
        sb.Append('\n');
        for (int i = 0; i < depth; i++) sb.Append(""  "");
    }
    public static void Key(StringBuilder sb, CS2SX_JsonOptions? o, int d, bool first, string name)
    {
        if (!first) sb.Append(',');
        bool ind = o != null && o.WriteIndented;
        if (ind) Line(sb, d + 1);
        Str(sb, name);
        sb.Append(ind ? "": "" : "":"");
    }
    public static void Item(StringBuilder sb, CS2SX_JsonOptions? o, int d, bool first)
    {
        if (!first) sb.Append(',');
        if (o != null && o.WriteIndented) Line(sb, d + 1);
    }
    public static void End(StringBuilder sb, CS2SX_JsonOptions? o, int d, bool any, char close)
    {
        if (any && o != null && o.WriteIndented) Line(sb, d);
        sb.Append(close);
    }
}
public sealed class CS2SX_JsonR
{
    private char[] s;
    private int p;
    private int len;
    private bool ci;
    public CS2SX_JsonR(string text, bool caseInsensitive) { s = text.ToCharArray(); p = 0; len = s.Length; ci = caseInsensitive; }
    public JsonException Err(string what) { return new JsonException(""JSON: "" + what + "" (Zeichen "" + p + "")""); }
    private void Ws() { while (p < len && (s[p] == ' ' || s[p] == '\n' || s[p] == '\r' || s[p] == '\t')) p++; }
    private char Peek()
    {
        Ws();
        if (p >= len) throw Err(""unerwartetes Ende"");
        return s[p];
    }
    public void Expect(char c)
    {
        if (Peek() != c) throw Err(""'"" + c + ""' erwartet"");
        p++;
    }
    public bool Close(char c)
    {
        if (Peek() != c) return false;
        p++;
        return true;
    }
    public bool More(char close)
    {
        char c = Peek();
        if (c == ',') { p++; return true; }
        if (c == close) { p++; return false; }
        throw Err(""',' oder '"" + close + ""' erwartet"");
    }
    public void End()
    {
        Ws();
        if (p < len) throw Err(""Text nach dem Ende des Werts"");
    }
    private bool Word(string w)
    {
        if (p + w.Length > len) return false;
        for (int i = 0; i < w.Length; i++) if (s[p + i] != w[i]) return false;
        p += w.Length;
        return true;
    }
    public bool Null() { Peek(); return Word(""null""); }
    public bool IsStr() { return Peek() == '""'; }
    public bool Bool()
    {
        Peek();
        if (Word(""true"")) return true;
        if (Word(""false"")) return false;
        throw Err(""true oder false erwartet"");
    }
    public string Key()
    {
        string k = Str();
        Expect(':');
        return k;
    }
    public bool Is(string key, string name)
    {
        if (key == name) return true;
        return ci && key.ToLower() == name.ToLower();
    }
    private int HexDigit(char c)
    {
        if (c >= '0' && c <= '9') return c - '0';
        if (c >= 'a' && c <= 'f') return c - 'a' + 10;
        if (c >= 'A' && c <= 'F') return c - 'A' + 10;
        throw Err(""ungültige \\u-Folge"");
    }
    private int Hex4()
    {
        if (p + 4 > len) throw Err(""ungültige \\u-Folge"");
        int v = 0;
        for (int i = 0; i < 4; i++) v = v * 16 + HexDigit(s[p + i]);
        p += 4;
        return v;
    }
    private static void Utf8(StringBuilder sb, int cp)
    {
        if (cp < 0x10000) { sb.Append((char)cp); return; }
        int v = cp - 0x10000;
        sb.Append(new string(new char[] { (char)(0xD800 + (v >> 10)), (char)(0xDC00 + (v & 0x3FF)) }));
    }
    public string Str()
    {
        if (Peek() != '""') throw Err(""Text in Anführungszeichen erwartet"");
        p++;
        int start = p;
        while (p < len && s[p] != '""' && s[p] != '\\') p++;
        if (p < len && s[p] == '""') { string plain = new string(s, start, p - start); p++; return plain; }
        var sb = new StringBuilder();
        sb.Append(new string(s, start, p - start));
        while (true)
        {
            if (p >= len) throw Err(""Text ohne schließendes Anführungszeichen"");
            char c = s[p++];
            if (c == '""') break;
            if (c != '\\') { sb.Append(c); continue; }
            if (p >= len) throw Err(""Text ohne schließendes Anführungszeichen"");
            char e = s[p++];
            if (e == 'n') sb.Append('\n');
            else if (e == 't') sb.Append('\t');
            else if (e == 'r') sb.Append('\r');
            else if (e == 'b') sb.Append('\b');
            else if (e == 'f') sb.Append('\f');
            else if (e == '/' || e == '\\' || e == '""') sb.Append(e);
            else if (e == 'u')
            {
                int cp = Hex4();
                if (cp >= 0xD800 && cp <= 0xDBFF && p + 6 <= len && s[p] == '\\' && s[p + 1] == 'u')
                {
                    p += 2;
                    int low = Hex4();
                    cp = 0x10000 + ((cp - 0xD800) << 10) + (low - 0xDC00);
                }
                Utf8(sb, cp);
            }
            else throw Err(""ungültige Escape-Folge"");
        }
        return sb.ToString();
    }
    public char Chr()
    {
        string t = Str();
        if (t.Length != 1) throw Err(""ein einzelnes Zeichen erwartet"");
        return t[0];
    }
    private string Number(bool whole)
    {
        Peek();
        int start = p;
        if (p < len && s[p] == '-') p++;
        bool frac = false;
        while (p < len)
        {
            char c = s[p];
            if (c >= '0' && c <= '9') { p++; continue; }
            if (c == '.' || c == 'e' || c == 'E' || c == '+' || c == '-') { frac = true; p++; continue; }
            break;
        }
        if (p == start || p == start + 1 && s[start] == '-') throw Err(""Zahl erwartet"");
        if (whole && frac) throw Err(""ganze Zahl erwartet"");
        return new string(s, start, p - start);
    }
    public long Lng() { return long.Parse(Number(true)); }
    public ulong ULng() { return ulong.Parse(Number(true)); }
    public double Dbl() { return double.Parse(Number(false)); }
    public void Skip()
    {
        char c = Peek();
        if (c == '{') { p++; if (!Close('}')) do { Key(); Skip(); } while (More('}')); }
        else if (c == '[') { p++; if (!Close(']')) do { Skip(); } while (More(']')); }
        else if (c == '""') Str();
        else if (c == 't' || c == 'f') Bool();
        else if (c == 'n') { if (!Null()) throw Err(""unbekannter Wert""); }
        else Number(false);
    }
}".Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0));
}
