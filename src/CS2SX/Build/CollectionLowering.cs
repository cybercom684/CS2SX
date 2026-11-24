using System.Text;
using System.Text.RegularExpressions;
using CS2SX.Logging;
using CS2SX.Transpiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CS2SX.Build.AsyncLowering;

namespace CS2SX.Build;

/// SortedDictionary, SortedSet, PriorityQueue and LinkedList, at the C# level (after
/// the generics are specialised, before JSON, iterators and async): every use of
/// one becomes a class written for its type arguments, in a file appended to the
/// project — ordinary C# the transpiler handles like the app's own classes:
///
///   SortedDictionary&lt;string, int&gt;   → CS2SX_SortedDictionary_str_int  (sorted lists, binary search)
///   SortedSet&lt;int&gt;                   → CS2SX_SortedSet_int
///   PriorityQueue&lt;Enemy, float&gt;      → CS2SX_PriorityQueue_Enemy_float (binary heap)
///   LinkedList&lt;T&gt; / LinkedListNode&lt;T&gt; → CS2SX_LinkedList_T / CS2SX_LinkedListNode_T
///
/// foreach over them becomes an index loop (LinkedList: a node walk), a collection
/// initializer a call of Of(…), and a LINQ call on a SortedSet / LinkedList /
/// SortedDictionary goes through a List copy (ToList / ToEntries). Keys and
/// priorities are compared like .NET's default comparer (numbers, enums, chars,
/// IComparable&lt;T&gt;); strings ordinally. Every replacement keeps the number of lines.
public static class CollectionLowering
{
    private enum Kind { SortedDictionary, SortedSet, PriorityQueue, LinkedList, LinkedListNode, Grouping, DictLinq }

    private static Kind? KindOf(ITypeSymbol? t) =>
        t is INamedTypeSymbol { Name: "IGrouping", TypeArguments.Length: 2 } g
            && g.ContainingNamespace?.ToDisplayString() == "System.Linq"
            ? Kind.Grouping
        : t is INamedTypeSymbol n && n.ContainingNamespace?.ToDisplayString() == "System.Collections.Generic"
            ? (n.Name, n.TypeArguments.Length) switch
            {
                ("SortedDictionary", 2) => Kind.SortedDictionary,
                ("SortedSet", 1) => Kind.SortedSet,
                ("PriorityQueue", 2) => Kind.PriorityQueue,
                ("LinkedList", 1) => Kind.LinkedList,
                ("LinkedListNode", 1) => Kind.LinkedListNode,
                _ => null,
            }
            : null;

    private static readonly Dictionary<Kind, HashSet<string>> Members = new()
    {
        [Kind.SortedDictionary] = new() { "Count", "Add", "TryAdd", "ContainsKey", "ContainsValue", "TryGetValue", "Remove", "Clear", "Keys", "Values" },
        [Kind.SortedSet] = new() { "Count", "Add", "Remove", "Contains", "Clear", "Min", "Max", "UnionWith", "ExceptWith", "IntersectWith" },
        [Kind.PriorityQueue] = new() { "Count", "Enqueue", "Dequeue", "Peek", "TryDequeue", "TryPeek", "Clear" },
        [Kind.LinkedList] = new() { "Count", "First", "Last", "AddFirst", "AddLast", "AddBefore", "AddAfter", "Remove", "RemoveFirst", "RemoveLast", "Find", "FindLast", "Contains", "Clear" },
        [Kind.LinkedListNode] = new() { "Value", "Next", "Previous", "List" },
        [Kind.Grouping] = new() { "Key" },
    };

    public static List<string> Rewrite(IReadOnlyList<string> files, string outDir)
    {
        if (!files.Any(f => Regex.IsMatch(File.ReadAllText(f), @"\b(SortedDictionary|SortedSet|PriorityQueue|LinkedList|GroupBy|Dictionary)\b")))
            return files.ToList();

        var sem = new SemanticModelBuilder(files);
        var edits = files.ToDictionary(f => f, _ => new Edits());
        var gen = new Generator();
        int loops = 0;
        bool any = false;

        foreach (var file in files)
        {
            if (sem.GetModel(file) is not { } model) continue;
            var root = model.SyntaxTree.GetCompilationUnitRoot();
            var source = model.SyntaxTree.GetText().ToString();
            var e = edits[file];
            if (Problems(root, model).Any()) continue;   // reported by UnsupportedSyntax; left as it is

            // Types → generated classes (outermost names only; the class name covers its arguments).
            foreach (var name in root.DescendantNodes().OfType<GenericNameSyntax>())
            {
                if (name.Ancestors().OfType<GenericNameSyntax>().Any(a => KindOf(model.GetSymbolInfo(a).Symbol as ITypeSymbol) != null)) continue;
                if (model.GetSymbolInfo(name).Symbol is not INamedTypeSymbol t || KindOf(t) == null) continue;
                SyntaxNode span = name.Parent is QualifiedNameSyntax q && q.Right == name ? q : name;
                e.Replace(span.SpanStart, span.Span.Length, gen.Name(t));
                any = true;
            }

            // new SortedSet<int> { 3, 1 } / new SortedDictionary<K, V> { { k, v } } → Of(…)
            foreach (var oc in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
            {
                if (oc.Initializer is not { } init || !init.IsKind(SyntaxKind.CollectionInitializerExpression)) continue;
                if (model.GetTypeInfo(oc).Type is not INamedTypeSymbol t || KindOf(t) is not { } kind) continue;
                string R(ExpressionSyntax x) => e.Render(source, x.SpanStart, x.Span.End, all: false);
                var lines = new string('\n', source.Substring(oc.SpanStart, oc.Span.Length).Count(c => c == '\n'));
                if (kind == Kind.SortedSet)
                {
                    var elem = gen.Text(t.TypeArguments[0]);
                    e.Replace(oc.SpanStart, oc.Span.Length,
                        $"{gen.Name(t)}.Of(new {elem}[] {{ {string.Join(", ", init.Expressions.Select(R))} }}){lines}");
                }
                else if (kind == Kind.SortedDictionary)
                {
                    var pairs = init.Expressions.OfType<InitializerExpressionSyntax>().ToList();
                    e.Replace(oc.SpanStart, oc.Span.Length,
                        $"{gen.Name(t)}.Of(new {gen.Text(t.TypeArguments[0])}[] {{ {string.Join(", ", pairs.Select(p => R(p.Expressions[0])))} }}, "
                        + $"new {gen.Text(t.TypeArguments[1])}[] {{ {string.Join(", ", pairs.Select(p => R(p.Expressions[1])))} }}){lines}");
                }
            }

            // list.GroupBy(x => key) → CS2SX_IGrouping_K_T.By(list, x => key): a list of groups
            // (Key, Items), in the order the keys first appear, like .NET.
            foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (inv.Expression is not MemberAccessExpressionSyntax gma || inv.ArgumentList.Arguments.Count != 1) continue;
                if (model.GetSymbolInfo(inv).Symbol is not IMethodSymbol { Name: "GroupBy", IsExtensionMethod: true } gb
                    || gb.ContainingType?.ToDisplayString() != "System.Linq.Enumerable") continue;
                if (gb.ReturnType is not INamedTypeSymbol { TypeArguments.Length: 1 } ret
                    || ret.TypeArguments[0] is not INamedTypeSymbol groupT || KindOf(groupT) != Kind.Grouping) continue;
                var srcType = model.GetTypeInfo(gma.Expression).Type;
                var src = e.Render(source, gma.Expression.SpanStart, gma.Expression.Span.End, all: false);
                if (srcType is not INamedTypeSymbol { Name: "List", TypeArguments.Length: 1 }) src = "(" + src + ").ToList()";
                var sel = e.Render(source, inv.ArgumentList.Arguments[0].SpanStart, inv.ArgumentList.Arguments[0].Span.End, all: false);
                var glines = new string('\n', source.Substring(inv.SpanStart, inv.Span.Length).Count(c => c == '\n'));
                e.Replace(inv.SpanStart, inv.Span.Length, $"{gen.Name(groupT)}.By({src}, {sel}){glines}");
                any = true;
            }
            // LINQ on a Dictionary<K, V> (d.Select(kv => …), d.Where(…)): through a list of
            // entries with Key and Value; d.Keys / d.Values as a sequence: through a list.
            foreach (var x in root.DescendantNodes().OfType<ExpressionSyntax>())
            {
                var xt = model.GetTypeInfo(x).Type as INamedTypeSymbol;
                bool linqReceiver = x.Parent is MemberAccessExpressionSyntax lr && lr.Expression == x
                    && model.GetSymbolInfo(lr).Symbol is IMethodSymbol { IsExtensionMethod: true } lm
                    && lm.ContainingType?.ToDisplayString() == "System.Linq.Enumerable";
                bool joined = x.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax ji } }
                    && model.GetSymbolInfo(ji).Symbol is IMethodSymbol { Name: "Join" or "Concat", ContainingType.SpecialType: SpecialType.System_String };
                if (!linqReceiver && !joined) continue;
                if (xt is { Name: "Dictionary", TypeArguments.Length: 2 } dt
                    && dt.ContainingNamespace?.ToDisplayString() == "System.Collections.Generic" && linqReceiver)
                {
                    var r = e.Render(source, x.SpanStart, x.Span.End, all: false);
                    e.Replace(x.SpanStart, x.Span.Length, $"{gen.DictLinq(dt)}.Entries({r})");
                    any = true;
                }
                else if (xt is { Name: "KeyCollection" or "ValueCollection", ContainingType: { Name: "Dictionary" } owner }
                         && x is MemberAccessExpressionSyntax kv)
                {
                    var r = e.Render(source, kv.Expression.SpanStart, kv.Expression.Span.End, all: false);
                    e.Replace(x.SpanStart, x.Span.Length,
                        $"{gen.DictLinq(owner)}.{(xt.Name == "KeyCollection" ? "Keys" : "Values")}({r})");
                    any = true;
                }
            }

            // A group used as a sequence (g.Count(), foreach (var x in g), string.Join(",", g)) is its Items.
            foreach (var x in root.DescendantNodes().OfType<ExpressionSyntax>())
            {
                if (KindOf(model.GetTypeInfo(x).Type) != Kind.Grouping) continue;
                if (x is TypeSyntax && model.GetSymbolInfo(x).Symbol is ITypeSymbol) continue;      // `var`, a type name
                if (x.Parent is MemberAccessExpressionSyntax pm && pm.Name == x) continue;          // name part of a.B
                if (x.Parent is MemberAccessExpressionSyntax km && km.Expression == x
                    && km.Name.Identifier.Text == "Key") continue;
                if (x.Parent is ParenthesizedExpressionSyntax) continue;
                if (x.Parent is AssignmentExpressionSyntax asg && asg.Left == x) continue;
                if (x.Parent is ArgumentSyntax or EqualsValueClauseSyntax or ReturnStatementSyntax or ArrowExpressionClauseSyntax
                    && model.GetTypeInfo(x).ConvertedType is INamedTypeSymbol ct && KindOf(ct) == Kind.Grouping) continue;   // passed on as a group
                e.Insert(x.Span.End, ".Items", PrioName, statementLevel: false);
            }

            // LINQ on them: through a List copy.
            foreach (var ma in root.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
            {
                if (model.GetSymbolInfo(ma).Symbol is not IMethodSymbol { IsExtensionMethod: true } ext) continue;
                if (KindOf(model.GetTypeInfo(ma.Expression).Type) is not { } kind) continue;
                if (kind is Kind.SortedSet or Kind.LinkedList)
                {
                    if (ext.Name is "ToList" or "ToArray") continue;   // the class has them itself
                    e.Insert(ma.Expression.Span.End, ".ToList()", PrioName, statementLevel: false);
                }
                else if (kind == Kind.SortedDictionary)
                    e.Insert(ma.Expression.Span.End, ".ToEntries()", PrioName, statementLevel: false);
            }
            // …and where one is passed on as IEnumerable<T> (string.Join(", ", set)).
            foreach (var x in root.DescendantNodes().OfType<ExpressionSyntax>())
            {
                if (x.Parent is MemberAccessExpressionSyntax recv && recv.Expression == x) continue;   // LINQ receiver: above
                var ti = model.GetTypeInfo(x);
                if (KindOf(ti.Type) is not (Kind.SortedSet or Kind.LinkedList) || IsEnumerableOf(ti.ConvertedType) == false) continue;
                if (SymbolEqualityComparer.Default.Equals(ti.Type, ti.ConvertedType) || ti.ConvertedType == null) continue;
                e.Insert(x.Span.End, ".ToList()", PrioName, statementLevel: false);
            }

            // foreach → index loop / node walk
            foreach (var fe in root.DescendantNodes().OfType<CommonForEachStatementSyntax>())
            {
                if (model.GetTypeInfo(fe.Expression).Type is not INamedTypeSymbol t || KindOf(t) is not { } kind) continue;
                if (kind == Kind.Grouping) continue;   // its .Items, above
                int n = ++loops;
                string c = "cs2sx_fc" + n, i = "cs2sx_fi" + n;
                var expr = e.Render(source, fe.Expression.SpanStart, fe.Expression.Span.End, all: false);
                string header, assign;
                if (kind == Kind.LinkedList)
                {
                    header = $"{{ var {c} = {expr}; for (var {i} = {c}.First; {i} != null; {i} = {i}.Next)";
                    assign = fe is ForEachStatementSyntax one ? $"var {one.Identifier.Text} = {i}.Value; " : "";
                }
                else
                {
                    header = $"{{ var {c} = {expr}; for (int {i} = 0; {i} < {c}.Count; {i}++)";
                    var item = kind == Kind.SortedDictionary ? $"{c}.EntryAt({i})" : $"{c}.ItemAt({i})";
                    assign = fe switch
                    {
                        ForEachStatementSyntax one => $"var {one.Identifier.Text} = {item}; ",
                        ForEachVariableStatementSyntax { Variable: DeclarationExpressionSyntax { Designation: ParenthesizedVariableDesignationSyntax pd } }
                            when kind == Kind.SortedDictionary && pd.Variables.Count == 2
                            => $"var {pd.Variables[0]} = {c}.KeyAt({i}); var {pd.Variables[1]} = {c}.ValueAt({i}); ",
                        _ => "",
                    };
                }
                if (assign.Length == 0) continue;
                e.Replace(fe.ForEachKeyword.SpanStart, fe.CloseParenToken.Span.End - fe.ForEachKeyword.SpanStart, header);
                if (fe.Statement is BlockSyntax block)
                {
                    e.Insert(block.OpenBraceToken.Span.End, " " + assign, PrioLoopOpen);
                    e.Insert(fe.Span.End, " }", PrioLoopClose);
                }
                else
                {
                    e.Insert(fe.Statement.SpanStart, "{ " + assign, PrioLoopOpen);
                    e.Insert(fe.Statement.Span.End, " } }", PrioLoopClose);
                }
            }
        }
        if (!any) return files.ToList();

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
            if (!File.Exists(target) || File.ReadAllText(target) != text) File.WriteAllText(target, text);
            result.Add(target);
        }
        var code = gen.Source();
        var usings = new StringBuilder("using System;\nusing System.Collections.Generic;\nusing System.Linq;\n");
        foreach (var ns in gen.Namespaces) usings.Append($"using {ns};\n");
        var collFile = Path.Combine(dir, "CS2SX_Collections.cs");
        var collText = usings + "\n" + code + "\n";
        if (!File.Exists(collFile) || File.ReadAllText(collFile) != collText) File.WriteAllText(collFile, collText);
        result.Add(collFile);
        Log.Info($"collections: {gen.Count} class(es)");
        return result;
    }

    private static bool? IsEnumerableOf(ITypeSymbol? t) =>
        t is INamedTypeSymbol { TypeKind: TypeKind.Interface } n && n.ContainingNamespace?.ToDisplayString() is "System.Collections.Generic" or "System.Collections"
            ? true : t == null ? null : false;

    // ── What cannot be lowered ──────────────────────────────────────────────

    public static IEnumerable<(SyntaxNode node, string message)> Problems(SyntaxNode root, SemanticModel model)
    {
        foreach (var node in root.DescendantNodes())
        {
            switch (node)
            {
                case MemberAccessExpressionSyntax ma:
                {
                    var recv = model.GetTypeInfo(ma.Expression).Type;
                    if (KindOf(recv) is not { } kind) break;
                    var sym = model.GetSymbolInfo(ma).Symbol;
                    if (sym is IMethodSymbol { IsExtensionMethod: true })
                    {
                        if (kind is Kind.PriorityQueue or Kind.LinkedListNode)
                            yield return (ma, $"LINQ auf {recv!.Name} wird nicht unterstützt — die Werte vorher in eine List<T> holen.");
                        break;
                    }
                    if (!Members[kind].Contains(ma.Name.Identifier.Text))
                        yield return (ma, $"{recv!.Name}.{ma.Name.Identifier.Text} wird nicht unterstützt — unterstützt: {string.Join(", ", Members[kind].OrderBy(x => x))}.");
                    break;
                }
                case ObjectCreationExpressionSyntax oc:
                {
                    if (model.GetTypeInfo(oc).Type is not INamedTypeSymbol t || KindOf(t) is not { } kind) break;
                    var args = oc.ArgumentList?.Arguments ?? default;
                    if (args.Count > 1 || args.Count == 1 && !OkSource(model.GetTypeInfo(args[0].Expression).Type, kind))
                        yield return (oc, $"Dieser Konstruktor von {t.Name} wird nicht unterstützt — ohne Argumente erzeugen (eigene Vergleicher gehen nicht; die Werte als List<T> oder Array übergeben geht).");
                    if (kind is Kind.SortedDictionary or Kind.SortedSet or Kind.PriorityQueue
                        && Generator.CompareCode(t.TypeArguments[kind == Kind.PriorityQueue ? 1 : 0], "a", "b") == null)
                        yield return (oc, $"Der Typ {t.TypeArguments[kind == Kind.PriorityQueue ? 1 : 0].Name} kann nicht verglichen werden — Schlüssel und Prioritäten brauchen Zahlen, Strings, Enums oder IComparable<T>.");
                    break;
                }
                case ExpressionSyntax x when x is not (ObjectCreationExpressionSyntax or MemberAccessExpressionSyntax)
                                             && !(x.Parent is MemberAccessExpressionSyntax r && r.Expression == x):
                {
                    var ti = model.GetTypeInfo(x);
                    if (KindOf(ti.Type) is not { } kind || ti.ConvertedType == null
                        || SymbolEqualityComparer.Default.Equals(ti.Type, ti.ConvertedType)) break;
                    if (ti.ConvertedType.SpecialType == SpecialType.System_Object) break;
                    if (kind == Kind.Grouping) break;   // used through its Items
                    if (kind is Kind.SortedSet or Kind.LinkedList && IsEnumerableOf(ti.ConvertedType) == true) break;
                    yield return (x, $"Ein {ti.Type!.Name} kann hier nicht als {ti.ConvertedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} verwendet werden — den Typ direkt verwenden (oder .ToList()).");
                    break;
                }
            }
        }
    }

    private static bool OkSource(ITypeSymbol? t, Kind kind) =>
        kind is Kind.SortedSet or Kind.LinkedList
            ? t is IArrayTypeSymbol || t is INamedTypeSymbol { Name: "List", TypeArguments.Length: 1 }
            : false;

    // ── The classes ─────────────────────────────────────────────────────────

    private sealed class Generator
    {
        private readonly Dictionary<string, (Kind kind, INamedTypeSymbol type)> _types = new(StringComparer.Ordinal);
        private readonly Queue<string> _todo = new();
        public readonly SortedSet<string> Namespaces = new(StringComparer.Ordinal);
        public int Count => _types.Count;

        private static string Suffix(string text) =>
            Regex.Replace(text.Replace("[]", "Arr").Replace("?", "N"), @"[^A-Za-z0-9]+", "_").Trim('_').Replace("string", "str");

        /// C# text of a type, with these collections replaced by their classes.
        public string Text(ITypeSymbol t)
        {
            if (t is INamedTypeSymbol n && KindOf(n) != null) return Name(n);
            if (t is IArrayTypeSymbol a) return Text(a.ElementType) + "[]";
            if (t is INamedTypeSymbol { IsGenericType: true, IsTupleType: false } g
                && g.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
                return g.Name + "<" + string.Join(", ", g.TypeArguments.Select(Text)) + ">";
            if (t is INamedTypeSymbol { ContainingNamespace: { IsGlobalNamespace: false } ns } && !ns.ToDisplayString().StartsWith("System"))
                Namespaces.Add(ns.ToDisplayString());
            var shown = t.IsReferenceType ? t.WithNullableAnnotation(NullableAnnotation.NotAnnotated) : t;
            return shown.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        }

        /// Entries / Keys / Values of a Dictionary<K, V> as lists (LINQ on dictionaries).
        public string DictLinq(INamedTypeSymbol dict)
        {
            var name = "CS2SX_DictLinq_" + string.Join("_", dict.TypeArguments.Select(a => Suffix(Text(a))));
            if (!_types.ContainsKey(name))
            {
                _types[name] = (Kind.DictLinq, dict);
                _todo.Enqueue(name);
            }
            return name;
        }

        public string Name(INamedTypeSymbol t)
        {
            var kind = KindOf(t)!.Value;
            var name = "CS2SX_" + t.Name + "_" + string.Join("_", t.TypeArguments.Select(a => Suffix(Text(a))));
            if (!_types.ContainsKey(name))
            {
                _types[name] = (kind, t);
                _todo.Enqueue(name);
                if (kind == Kind.LinkedList) Name(NodeOf(t));
                if (kind == Kind.LinkedListNode) Name(ListOf(t));
            }
            return name;
        }

        private static INamedTypeSymbol NodeOf(INamedTypeSymbol list) =>
            list.ContainingNamespace.GetTypeMembers("LinkedListNode", 1)[0].Construct(list.TypeArguments[0]);
        private static INamedTypeSymbol ListOf(INamedTypeSymbol node) =>
            node.ContainingNamespace.GetTypeMembers("LinkedList", 1)[0].Construct(node.TypeArguments[0]);

        /// C# code comparing a and b like .NET's default comparer (<0, 0, >0), or null.
        public static string? CompareCode(ITypeSymbol t, string a, string b)
        {
            if (t.SpecialType == SpecialType.System_String) return $"string.CompareOrdinal({a}, {b})";
            if (t.SpecialType == SpecialType.System_Boolean) return $"({a} == {b} ? 0 : {a} ? 1 : -1)";
            if (t.TypeKind == TypeKind.Enum || t.SpecialType is SpecialType.System_Char or SpecialType.System_Single or SpecialType.System_Double
                or SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_Int16 or SpecialType.System_UInt16
                or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64)
                return $"({a} < {b} ? -1 : {a} > {b} ? 1 : 0)";
            if (t.AllInterfaces.Any(i => i.Name == "IComparable" && i.TypeArguments.Length == 1
                    && SymbolEqualityComparer.Default.Equals(i.TypeArguments[0], t)))
                return $"{a}.CompareTo({b})";
            return null;
        }

        private static string EqualsCode(ITypeSymbol t, string a, string b) =>
            t.TypeKind == TypeKind.Struct && t.SpecialType == SpecialType.None && t.TypeKind != TypeKind.Enum
                ? $"{a}.Equals({b})" : $"{a} == {b}";

        private string Default(ITypeSymbol t) =>
            t.IsReferenceType ? "null!"
            : t.SpecialType == SpecialType.System_Boolean ? "false"
            : t.SpecialType == SpecialType.System_Char ? "'\\0'"
            : t.TypeKind == TypeKind.Enum ? $"({Text(t)})0"
            : t.SpecialType != SpecialType.None ? "0"
            : $"new {Text(t)}()";

        private readonly StringBuilder _out = new();

        public string Source()
        {
            while (_todo.Count > 0)
            {
                var name = _todo.Dequeue();
                var (kind, t) = _types[name];
                _out.Append(kind switch
                {
                    Kind.SortedDictionary => SortedDictionary(name, t),
                    Kind.SortedSet => SortedSet(name, t),
                    Kind.PriorityQueue => PriorityQueue(name, t),
                    Kind.LinkedList => LinkedList(name, t),
                    Kind.Grouping => Grouping(name, t),
                    Kind.DictLinq => DictLinqClass(name, t),
                    _ => LinkedListNode(name, t),
                }).Append('\n');
            }
            return _out.ToString();
        }

        private static string Fill(string template, params (string key, string value)[] values)
        {
            foreach (var (k, v) in values) template = template.Replace(k, v);
            return template;
        }

        private string SortedDictionary(string name, INamedTypeSymbol t)
        {
            var (k, v) = (t.TypeArguments[0], t.TypeArguments[1]);
            var kv = "CS2SX_KV_" + name["CS2SX_SortedDictionary_".Length..];
            return Fill(@"
public struct @KV
{
    public @K Key;
    public @V Value;
    public @KV(@K key, @V value) { Key = key; Value = value; }
}
public class @N
{
    private List<@K> _keys = new List<@K>();
    private List<@V> _vals = new List<@V>();
    public @N() { }
    public static @N Of(@K[] keyArr, @V[] valArr) { var d = new @N(); for (int i = 0; i < keyArr.Length; i++) d.Add(keyArr[i], valArr[i]); return d; }
    public int Count => _keys.Count;
    private static int Cmp(@K a, @K b) { return @CMP; }
    private int Find(@K key)
    {
        int lo = 0;
        int hi = _keys.Count - 1;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            int c = Cmp(_keys[mid], key);
            if (c == 0) return mid;
            if (c < 0) lo = mid + 1; else hi = mid - 1;
        }
        return -(lo + 1);
    }
    public @V this[@K key]
    {
        get { int i = Find(key); if (i < 0) throw new KeyNotFoundException(""The given key was not present in the dictionary.""); return _vals[i]; }
        set { int i = Find(key); if (i >= 0) { _vals[i] = value; return; } _keys.Insert(-(i + 1), key); _vals.Insert(-(i + 1), value); }
    }
    public void Add(@K key, @V value)
    {
        int i = Find(key);
        if (i >= 0) throw new ArgumentException(""An item with the same key has already been added."");
        _keys.Insert(-(i + 1), key);
        _vals.Insert(-(i + 1), value);
    }
    public bool TryAdd(@K key, @V value) { int i = Find(key); if (i >= 0) return false; _keys.Insert(-(i + 1), key); _vals.Insert(-(i + 1), value); return true; }
    public bool ContainsKey(@K key) { return Find(key) >= 0; }
    public bool ContainsValue(@V value) { for (int i = 0; i < _vals.Count; i++) if (@EQV) return true; return false; }
    public bool TryGetValue(@K key, out @V value) { int i = Find(key); if (i < 0) { value = @DV; return false; } value = _vals[i]; return true; }
    public bool Remove(@K key) { int i = Find(key); if (i < 0) return false; _keys.RemoveAt(i); _vals.RemoveAt(i); return true; }
    public void Clear() { _keys.Clear(); _vals.Clear(); }
    public List<@K> Keys => new List<@K>(_keys);
    public List<@V> Values => new List<@V>(_vals);
    public @K KeyAt(int i) { return _keys[i]; }
    public @V ValueAt(int i) { return _vals[i]; }
    public @KV EntryAt(int i) { return new @KV(_keys[i], _vals[i]); }
    public List<@KV> ToEntries() { var l = new List<@KV>(); for (int i = 0; i < _keys.Count; i++) l.Add(new @KV(_keys[i], _vals[i])); return l; }
}", ("@KV", kv), ("@N", name), ("@K", Text(k)), ("@V", Text(v)), ("@CMP", CompareCode(k, "a", "b")!),
                ("@EQV", EqualsCode(v, "_vals[i]", "value")), ("@DV", Default(v)));
        }

        private string SortedSet(string name, INamedTypeSymbol t)
        {
            var e = t.TypeArguments[0];
            return Fill(@"
public class @N
{
    private List<@T> _items = new List<@T>();
    public @N() { }
    public @N(List<@T> source) { for (int i = 0; i < source.Count; i++) Add(source[i]); }
    public @N(@T[] source) { for (int i = 0; i < source.Length; i++) Add(source[i]); }
    public static @N Of(@T[] itemArr) { var s = new @N(); for (int i = 0; i < itemArr.Length; i++) s.Add(itemArr[i]); return s; }
    public int Count => _items.Count;
    private static int Cmp(@T a, @T b) { return @CMP; }
    private int Find(@T item)
    {
        int lo = 0;
        int hi = _items.Count - 1;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            int c = Cmp(_items[mid], item);
            if (c == 0) return mid;
            if (c < 0) lo = mid + 1; else hi = mid - 1;
        }
        return -(lo + 1);
    }
    public bool Add(@T item) { int i = Find(item); if (i >= 0) return false; _items.Insert(-(i + 1), item); return true; }
    public bool Remove(@T item) { int i = Find(item); if (i < 0) return false; _items.RemoveAt(i); return true; }
    public bool Contains(@T item) { return Find(item) >= 0; }
    public void Clear() { _items.Clear(); }
    public @T Min => _items.Count > 0 ? _items[0] : @DT;
    public @T Max => _items.Count > 0 ? _items[_items.Count - 1] : @DT;
    public @T ItemAt(int i) { return _items[i]; }
    public List<@T> ToList() { return new List<@T>(_items); }
    public @T[] ToArray() { var a = new @T[_items.Count]; for (int i = 0; i < a.Length; i++) a[i] = _items[i]; return a; }
    public void UnionWith(List<@T> other) { for (int i = 0; i < other.Count; i++) Add(other[i]); }
    public void UnionWith(@N other) { for (int i = 0; i < other.Count; i++) Add(other.ItemAt(i)); }
    public void ExceptWith(List<@T> other) { for (int i = 0; i < other.Count; i++) Remove(other[i]); }
    public void ExceptWith(@N other) { for (int i = 0; i < other.Count; i++) Remove(other.ItemAt(i)); }
    public void IntersectWith(@N other) { for (int i = _items.Count - 1; i >= 0; i--) if (!other.Contains(_items[i])) _items.RemoveAt(i); }
    public void IntersectWith(List<@T> other) { IntersectWith(new @N(other)); }
}", ("@N", name), ("@T", Text(e)), ("@CMP", CompareCode(e, "a", "b")!), ("@DT", Default(e)));
        }

        private string PriorityQueue(string name, INamedTypeSymbol t)
        {
            var (el, pr) = (t.TypeArguments[0], t.TypeArguments[1]);
            return Fill(@"
public class @N
{
    private List<@E> _el = new List<@E>();
    private List<@P> _pr = new List<@P>();
    public int Count => _el.Count;
    private static int Cmp(@P a, @P b) { return @CMP; }
    private void Swap(int a, int b)
    {
        @E e = _el[a]; _el[a] = _el[b]; _el[b] = e;
        @P p = _pr[a]; _pr[a] = _pr[b]; _pr[b] = p;
    }
    public void Enqueue(@E element, @P priority)
    {
        _el.Add(element);
        _pr.Add(priority);
        int i = _el.Count - 1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (Cmp(_pr[i], _pr[parent]) >= 0) break;
            Swap(i, parent);
            i = parent;
        }
    }
    private void RemoveTop()
    {
        int last = _el.Count - 1;
        _el[0] = _el[last];
        _pr[0] = _pr[last];
        _el.RemoveAt(last);
        _pr.RemoveAt(last);
        int i = 0;
        while (true)
        {
            int l = 2 * i + 1;
            int r = l + 1;
            int m = i;
            if (l < _el.Count && Cmp(_pr[l], _pr[m]) < 0) m = l;
            if (r < _el.Count && Cmp(_pr[r], _pr[m]) < 0) m = r;
            if (m == i) break;
            Swap(i, m);
            i = m;
        }
    }
    public @E Peek() { if (_el.Count == 0) throw new InvalidOperationException(""Queue empty.""); return _el[0]; }
    public @E Dequeue()
    {
        if (_el.Count == 0) throw new InvalidOperationException(""Queue empty."");
        @E top = _el[0];
        RemoveTop();
        return top;
    }
    public bool TryDequeue(out @E element, out @P priority)
    {
        if (_el.Count == 0) { element = @DE; priority = @DP; return false; }
        element = _el[0];
        priority = _pr[0];
        RemoveTop();
        return true;
    }
    public bool TryPeek(out @E element, out @P priority)
    {
        if (_el.Count == 0) { element = @DE; priority = @DP; return false; }
        element = _el[0];
        priority = _pr[0];
        return true;
    }
    public void Clear() { _el.Clear(); _pr.Clear(); }
}", ("@N", name), ("@E", Text(el)), ("@P", Text(pr)), ("@CMP", CompareCode(pr, "a", "b")!),
                ("@DE", Default(el)), ("@DP", Default(pr)));
        }

        private string LinkedList(string name, INamedTypeSymbol t)
        {
            var e = t.TypeArguments[0];
            var node = Name(NodeOf(t));
            return Fill(@"
public class @L
{
    private @Node? _first;
    private @Node? _last;
    private int _count;
    public @L() { }
    public @L(List<@T> source) { for (int i = 0; i < source.Count; i++) AddLast(source[i]); }
    public @L(@T[] source) { for (int i = 0; i < source.Length; i++) AddLast(source[i]); }
    public int Count => _count;
    public @Node? First => _first;
    public @Node? Last => _last;
    private void Check(@Node node)
    {
        if (node == null) throw new ArgumentNullException(""node"");
        if (node.list != this) throw new InvalidOperationException(""The LinkedList node does not belong to current LinkedList."");
    }
    private void Fresh(@Node node)
    {
        if (node == null) throw new ArgumentNullException(""node"");
        if (node.list != null) throw new InvalidOperationException(""The LinkedList node already belongs to a LinkedList."");
    }
    private void Link(@Node node, @Node? before, @Node? after)
    {
        node.list = this;
        node.prev = before;
        node.next = after;
        if (before != null) before.next = node; else _first = node;
        if (after != null) after.prev = node; else _last = node;
        _count++;
    }
    public @Node AddLast(@T value) { var n = new @Node(value); Link(n, _last, null); return n; }
    public void AddLast(@Node node) { Fresh(node); Link(node, _last, null); }
    public @Node AddFirst(@T value) { var n = new @Node(value); Link(n, null, _first); return n; }
    public void AddFirst(@Node node) { Fresh(node); Link(node, null, _first); }
    public @Node AddAfter(@Node node, @T value) { Check(node); var n = new @Node(value); Link(n, node, node.next); return n; }
    public @Node AddBefore(@Node node, @T value) { Check(node); var n = new @Node(value); Link(n, node.prev, node); return n; }
    public void Remove(@Node node)
    {
        Check(node);
        if (node.prev != null) node.prev.next = node.next; else _first = node.next;
        if (node.next != null) node.next.prev = node.prev; else _last = node.prev;
        node.prev = null;
        node.next = null;
        node.list = null;
        _count--;
    }
    public bool Remove(@T value) { var n = Find(value); if (n == null) return false; Remove(n); return true; }
    public void RemoveFirst() { if (_first == null) throw new InvalidOperationException(""The LinkedList is empty.""); Remove(_first); }
    public void RemoveLast() { if (_last == null) throw new InvalidOperationException(""The LinkedList is empty.""); Remove(_last); }
    public @Node? Find(@T value) { for (var n = _first; n != null; n = n.next) if (@EQ) return n; return null; }
    public @Node? FindLast(@T value) { for (var n = _last; n != null; n = n.prev) if (@EQ) return n; return null; }
    public bool Contains(@T value) { return Find(value) != null; }
    public void Clear()
    {
        var n = _first;
        while (n != null) { var next = n.next; n.prev = null; n.next = null; n.list = null; n = next; }
        _first = null;
        _last = null;
        _count = 0;
    }
    public List<@T> ToList() { var l = new List<@T>(); for (var n = _first; n != null; n = n.next) l.Add(n.Value); return l; }
    public @T[] ToArray() { var l = ToList(); var a = new @T[l.Count]; for (int i = 0; i < a.Length; i++) a[i] = l[i]; return a; }
}", ("@L", name), ("@Node", node), ("@T", Text(e)), ("@EQ", EqualsCode(e, "n.Value", "value")));
        }

        private string DictLinqClass(string name, INamedTypeSymbol t)
        {
            var (k, v) = (t.TypeArguments[0], t.TypeArguments[1]);
            var entry = "CS2SX_Entry_" + name["CS2SX_DictLinq_".Length..];
            return Fill(@"
public class @E
{
    public @K Key;
    public @V Value;
    public @E(@K key, @V value) { Key = key; Value = value; }
}
public static class @N
{
    public static List<@E> Entries(Dictionary<@K, @V> d) { var l = new List<@E>(); foreach (var kv in d) l.Add(new @E(kv.Key, kv.Value)); return l; }
    public static List<@K> Keys(Dictionary<@K, @V> d) { var l = new List<@K>(); foreach (var kv in d) l.Add(kv.Key); return l; }
    public static List<@V> Values(Dictionary<@K, @V> d) { var l = new List<@V>(); foreach (var kv in d) l.Add(kv.Value); return l; }
}", ("@N", name), ("@E", entry), ("@K", Text(k)), ("@V", Text(v)));
        }

        private string Grouping(string name, INamedTypeSymbol t)
        {
            var (k, e) = (t.TypeArguments[0], t.TypeArguments[1]);
            return Fill(@"
public class @N
{
    public @K Key = @DEF;
    public List<@T> Items = new List<@T>();
    public static List<@N> By(List<@T> src, Func<@T, @K> key)
    {
        var groups = new List<@N>();
        for (int i = 0; i < src.Count; i++)
        {
            var item = src[i];
            var k = key(item);
            @N? g = null;
            for (int j = 0; j < groups.Count; j++) if (@EQ) { g = groups[j]; break; }
            if (g == null) { g = new @N(); g.Key = k; groups.Add(g); }
            g.Items.Add(item);
        }
        return groups;
    }
}", ("@N", name), ("@K", Text(k)), ("@T", Text(e)), ("@DEF", Default(k)), ("@EQ", EqualsCode(k, "groups[j].Key", "k")));
        }

        private string LinkedListNode(string name, INamedTypeSymbol t)
        {
            var list = Name(ListOf(t));
            return Fill(@"
public class @Node
{
    public @T Value;
    internal @Node? next;
    internal @Node? prev;
    internal @L? list;
    public @Node(@T value) { Value = value; }
    public @Node? Next => next;
    public @Node? Previous => prev;
    public @L? List => list;
}", ("@Node", name), ("@L", list), ("@T", Text(t.TypeArguments[0])));
        }
    }
}
