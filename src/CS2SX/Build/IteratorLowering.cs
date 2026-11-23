using System.Text;
using System.Text.RegularExpressions;
using CS2SX.Logging;
using CS2SX.Transpiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CS2SX.Build.AsyncLowering;

namespace CS2SX.Build;

/// yield return, lowered at the C# level (after the generics are specialised, before
/// AsyncLowering). Two kinds of iterator methods:
///
/// IEnumerable&lt;T&gt; — the body runs when the method is called and collects the
/// values in a List&lt;T&gt; (in C an IEnumerable&lt;T&gt; is a list, so foreach and LINQ
/// work as before):
///
///   IEnumerable&lt;int&gt; Evens(int n) { var cs2sx_yield = new List&lt;int&gt;(); for (…) cs2sx_yield.Add(i); return cs2sx_yield; }
///
/// IEnumerator&lt;T&gt; / IEnumerator — a state machine that runs a piece of the body
/// on each MoveNext() (coroutines: one step per frame). The method creates the
/// object; a nested class holds the parameters and locals as fields:
///
///   CS2SX_Iter_int Blink(int n) { var cs2sx_sm = new Blink_Iter(); …; return cs2sx_sm; }
///   class Blink_Iter : CS2SX_Iter_int {
///       public override bool MoveNext() { if (cs2sx_state &lt; 0) return false; CS2SX_Yield.Begin(1);
///           for (i = 0; i &lt; n; i++) { Current = i; CS2SX_Yield.Return(1); }
///           cs2sx_state = -1; return false; } }
///
/// CS2SX_Yield.Return(k) stores the state and returns true; the next MoveNext() jumps
/// back behind it (a label and a goto in C, like the async state machines).
/// IEnumerator&lt;T&gt; becomes CS2SX_Iter_T, a small class appended to the first
/// source file. Every replacement keeps the number of lines.
public static class IteratorLowering
{
    public static List<string> Rewrite(IReadOnlyList<string> files, string outDir)
    {
        if (!files.Any(f => File.ReadAllText(f).Contains("yield") || File.ReadAllText(f).Contains("IEnumerator")))
            return files.ToList();

        var sem = new SemanticModelBuilder(files);
        var edits = files.ToDictionary(f => f, _ => new Edits());
        var iterTypes = new SortedDictionary<string, string>(StringComparer.Ordinal);   // CS2SX_Iter_int → int
        int lazy = 0, eager = 0;
        bool any = false;

        foreach (var file in files)
        {
            if (sem.GetModel(file) is not { } model) continue;
            var root = model.SyntaxTree.GetCompilationUnitRoot();
            var source = model.SyntaxTree.GetText().ToString();
            var e = edits[file];

            any |= RenameEnumeratorTypes(root, model, e, iterTypes);

            bool needsList = false;
            foreach (var body in root.DescendantNodes().OfType<BlockSyntax>())
            {
                if (!IsIteratorBody(body, out var member)) continue;
                if (Kind(member, model) is not { } kind) continue;
                if (kind == IterKind.Enumerable)
                {
                    if (EagerProblem(member, body, model) != null) continue;
                    LowerEager(body, member, model, e);
                    needsList = true;
                    eager++;
                }
                else
                {
                    if (member is not MethodDeclarationSyntax m || LazyProblem(m, model) != null) continue;
                    LowerLazy(m, model, source, e, iterTypes);
                    lazy++;
                }
                any = true;
            }
            if (needsList)
            {
                int at = root.Usings.FirstOrDefault()?.SpanStart ?? root.Members.FirstOrDefault()?.SpanStart ?? 0;
                e.Insert(at, "using System.Collections.Generic; ", PrioAwaitClose);
            }
        }
        if (!any) return files.ToList();

        if (iterTypes.Count > 0)
        {
            var first = files[0];
            var root = CSharpSyntaxTree.ParseText(File.ReadAllText(first)).GetCompilationUnitRoot();
            int membersAt = root.Members.FirstOrDefault()?.SpanStart ?? root.FullSpan.End;
            int usingsAt = root.Usings.FirstOrDefault()?.SpanStart ?? membersAt;
            edits[first].Insert(usingsAt, "using System; ", PrioAwaitClose);
            edits[first].Insert(membersAt, RuntimeSource(iterTypes) + " ", PrioLoopClose);
        }

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
        Log.Info($"yield: {lazy} enumerator(s), {eager} collected sequence(s)");
        return result;
    }

    // ── Kinds of iterators ──────────────────────────────────────────────────

    private enum IterKind { Enumerable, Enumerator }

    /// A method body or get accessor body with a yield statement of its own (not
    /// one of a lambda or local function inside it).
    private static bool IsIteratorBody(BlockSyntax body, out SyntaxNode member)
    {
        member = body.Parent!;
        if (body.Parent is not (MethodDeclarationSyntax or AccessorDeclarationSyntax { Keyword.Text: "get" })) return false;
        var m = member;
        return body.DescendantNodes().OfType<YieldStatementSyntax>().Any(y => !InNested(y, m));
    }

    private static bool InNested(SyntaxNode node, SyntaxNode member) =>
        node.Ancestors().TakeWhile(a => a != member)
            .Any(a => a is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax);

    private static ITypeSymbol? ReturnType(SyntaxNode member, SemanticModel model) => member switch
    {
        MethodDeclarationSyntax m => model.GetDeclaredSymbol(m)?.ReturnType,
        AccessorDeclarationSyntax a => (model.GetDeclaredSymbol(a) as IMethodSymbol)?.ReturnType,
        _ => null,
    };

    private static IterKind? Kind(SyntaxNode member, SemanticModel model)
    {
        var t = ReturnType(member, model);
        if (IsGenericEnumerable(t)) return IterKind.Enumerable;
        if (IsEnumeratorOrLowered(t)) return IterKind.Enumerator;
        return null;
    }

    private static bool InNamespace(INamedTypeSymbol n, string ns) => n.ContainingNamespace?.ToDisplayString() == ns;

    private static bool IsGenericEnumerable(ITypeSymbol? t) =>
        t is INamedTypeSymbol { Name: "IEnumerable", TypeArguments.Length: 1 } n && InNamespace(n, "System.Collections.Generic");

    private static bool IsEnumerator(ITypeSymbol? t) =>
        t is INamedTypeSymbol { Name: "IEnumerator" } n
        && (n.TypeArguments.Length == 1 && InNamespace(n, "System.Collections.Generic")
            || n.TypeArguments.Length == 0 && InNamespace(n, "System.Collections"));

    /// Also true for the classes the enumerator types were replaced by: the problems
    /// of a method are looked for again in the lowered source.
    private static bool IsEnumeratorOrLowered(ITypeSymbol? t) =>
        IsEnumerator(t) || t is INamedTypeSymbol { Name: "CS2SX_Iter" } || t?.Name.StartsWith("CS2SX_Iter_") == true;

    /// The element type of IEnumerator&lt;T&gt; (or its class); null for the non-generic one.
    private static ITypeSymbol? ElementType(ITypeSymbol t, SemanticModel model)
    {
        if (t is INamedTypeSymbol { TypeArguments.Length: 1 } n) return n.TypeArguments[0];
        if (t.Name == "CS2SX_Iter") return null;
        if (t.Name.StartsWith("CS2SX_Iter_")) return t.GetMembers("Current").OfType<IFieldSymbol>().FirstOrDefault()?.Type;
        return null;
    }

    // ── Enumerator types → CS2SX_Iter classes ───────────────────────────────

    private static string Suffix(string t) =>
        t == "string" ? "str" : Regex.Replace(t.Replace("[]", "Arr"), @"[^A-Za-z0-9]+", "_").Trim('_');

    private static string TypeText(ITypeSymbol t, IDictionary<string, string> iterTypes)
    {
        if (IsEnumerator(t))
        {
            var n = (INamedTypeSymbol)t;
            if (n.TypeArguments.Length == 0) { iterTypes["CS2SX_Iter"] = "object"; return "CS2SX_Iter"; }
            var arg = TypeText(n.TypeArguments[0], iterTypes);
            var name = "CS2SX_Iter_" + Suffix(arg);
            iterTypes[name] = arg;
            return name;
        }
        if (t is IArrayTypeSymbol a) return TypeText(a.ElementType, iterTypes) + "[]";
        if (t is INamedTypeSymbol { IsGenericType: true, IsTupleType: false } g && g.TypeArguments.Any(ContainsEnumerator)
            && g.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
            return g.Name + "<" + string.Join(", ", g.TypeArguments.Select(x => TypeText(x, iterTypes))) + ">";
        return t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
    }

    private static bool ContainsEnumerator(ITypeSymbol t) => t switch
    {
        _ when IsEnumerator(t) => true,
        IArrayTypeSymbol a => ContainsEnumerator(a.ElementType),
        INamedTypeSymbol n => n.TypeArguments.Any(ContainsEnumerator),
        _ => false,
    };

    private static bool RenameEnumeratorTypes(SyntaxNode root, SemanticModel model, Edits e, IDictionary<string, string> iterTypes)
    {
        bool any = false;
        foreach (var name in root.DescendantNodes().OfType<SimpleNameSyntax>())
        {
            if (name.Identifier.Text != "IEnumerator") continue;
            if (name.Ancestors().Any(a => a is UsingDirectiveSyntax or BaseListSyntax)) continue;
            if (model.GetSymbolInfo(name).Symbol is not INamedTypeSymbol type || !IsEnumerator(type)) continue;
            SyntaxNode span = name;
            if (name.Parent is QualifiedNameSyntax q && q.Right == name) span = q;
            e.Replace(span.SpanStart, span.Span.Length, TypeText(type, iterTypes));
            any = true;
        }
        return any;
    }

    // ── What cannot be lowered ──────────────────────────────────────────────

    /// The message for a yield statement that is still in the source after lowering.
    public static string YieldMessage(YieldStatementSyntax y, SemanticModel? model)
    {
        const string fallback = "yield return wird hier nicht unterstützt — die Werte in einer List<T> sammeln und zurückgeben.";
        var member = y.Ancestors().FirstOrDefault(a => a is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax
                                                        or MethodDeclarationSyntax or AccessorDeclarationSyntax);
        if (member is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax)
            return "yield in einer lokalen Funktion oder einem Lambda wird nicht unterstützt — eine Methode der Klasse verwenden.";
        if (member == null || model == null) return fallback;
        if (member is AccessorDeclarationSyntax { Keyword.Text: not "get" })
            return fallback;
        var body = member switch { MethodDeclarationSyntax m => m.Body, AccessorDeclarationSyntax a => a.Body, _ => null };
        if (body == null) return fallback;

        switch (Kind(member, model))
        {
            case IterKind.Enumerable:
                return EagerProblem(member, body, model) ?? fallback;
            case IterKind.Enumerator:
                if (member is not MethodDeclarationSyntax method)
                    return "Eine Eigenschaft mit yield muss IEnumerable<T> zurückgeben — für IEnumerator<T> eine Methode verwenden.";
                return LazyProblem(method, model) ?? fallback;
            default:
                return ReturnType(member, model) is INamedTypeSymbol { Name: "IEnumerable", TypeArguments.Length: 0 }
                    ? "Das nicht-generische IEnumerable wird nicht unterstützt — IEnumerable<T> verwenden (z. B. IEnumerable<int>)."
                    : "Eine Methode mit yield muss IEnumerable<T>, IEnumerator<T> oder IEnumerator zurückgeben.";
        }
    }

    private static string? EagerProblem(SyntaxNode member, BlockSyntax body, SemanticModel model) => null;

    private static string? LazyProblem(MethodDeclarationSyntax m, SemanticModel model)
    {
        var name = m.Identifier.Text;
        if (m.Parent is not ClassDeclarationSyntax cls)
            return $"Die Methode {name} gibt einen IEnumerator zurück und steht nicht in einer Klasse — das geht nur in Klassen (nicht in Structs, Records oder Interfaces).";
        if (m.TypeParameterList != null || cls.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().Any(t => t.TypeParameterList != null))
            return $"Die Methode {name} gibt einen IEnumerator zurück und ist generisch oder steht in einer generischen Klasse — das wird nicht unterstützt.";
        if (m.ParameterList.Parameters.Any(p => p.Modifiers.Count > 0 && !p.Modifiers.Any(SyntaxKind.ParamsKeyword)))
            return $"Die Methode {name} hat ref/out/in-Parameter — Iteratoren erlauben das nicht.";

        var body = m.Body!;
        var yields = Yields(body, m).ToList();
        var ret = model.GetDeclaredSymbol(m)!.ReturnType;
        bool generic = ElementType(ret, model) != null;

        if (body.DescendantNodes().OfType<BaseExpressionSyntax>().Any())
            return $"base. in der Methode {name} (IEnumerator) wird nicht unterstützt — den Aufruf in eine normale Methode auslagern.";

        foreach (var y in yields)
        {
            if (!generic && y.Expression != null && !y.Expression.IsKind(SyntaxKind.NullLiteralExpression))
                return "Das nicht-generische IEnumerator kann nur null liefern (yield return null;) — für Werte IEnumerator<T> verwenden, z. B. IEnumerator<int>.";
            foreach (var up in y.Ancestors().TakeWhile(x => x != m))
                switch (up)
                {
                    case TryStatementSyntax t when t.Block.Span.Contains(y.Span):
                        return "yield in einem try-Block wird bei IEnumerator nicht unterstützt — den try-Block in eine eigene Methode auslagern oder IEnumerable<T> verwenden.";
                    case UsingStatementSyntax:
                        return "yield in einem using-Block wird bei IEnumerator nicht unterstützt — Dispose selbst aufrufen.";
                    case ForEachStatementSyntax fe when fe.Statement.Span.Contains(y.Span):
                        var ct = model.GetTypeInfo(fe.Expression).Type;
                        if (!(IsListOrArray(ct) || IsGenericEnumerable(ct)))
                            return "foreach mit yield im Rumpf geht nur über eine List<T>, ein Array oder ein IEnumerable<T> — sonst eine for-Schleife verwenden.";
                        break;
                    case ForEachVariableStatementSyntax:
                        return "foreach mit Zerlegung (var (a, b)) und yield im Rumpf wird nicht unterstützt — die Elemente im Rumpf zerlegen.";
                }
        }

        const string lost = " behält ihren Wert nicht über ein yield return — vorher normal deklarieren (z. B. int n; … out n).";
        foreach (var d in body.DescendantNodes().OfType<SingleVariableDesignationSyntax>())
        {
            if (InNested(d, m)) continue;
            var scope = d.Ancestors().OfType<BlockSyntax>().FirstOrDefault() ?? body;
            if (yields.Any(y => scope.Span.Contains(y.Span) && y.SpanStart > d.SpanStart))
                return $"Die Variable {d.Identifier.Text} (out var, Muster oder Zerlegung)" + lost;
        }
        foreach (var ld in body.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
        {
            if (InNested(ld, m) || !(ld.UsingKeyword.RawKind != 0 || ld.Declaration.Type is RefTypeSyntax)) continue;
            var scope = ld.Ancestors().OfType<BlockSyntax>().FirstOrDefault() ?? body;
            if (yields.Any(y => scope.Span.Contains(y.Span) && y.SpanStart > ld.SpanStart))
                return "using var / ref-Variablen vor einem yield return werden bei IEnumerator nicht unterstützt.";
        }
        return null;
    }

    private static IEnumerable<YieldStatementSyntax> Yields(SyntaxNode scope, SyntaxNode member) =>
        scope.DescendantNodes().OfType<YieldStatementSyntax>().Where(y => !InNested(y, member));

    private static bool IsListOrArray(ITypeSymbol? t) =>
        t is IArrayTypeSymbol { Rank: 1 }
        || t is INamedTypeSymbol { Name: "List", TypeArguments.Length: 1 } n && InNamespace(n, "System.Collections.Generic");

    // ── IEnumerable<T>: collect the values ──────────────────────────────────

    private static void LowerEager(BlockSyntax body, SyntaxNode member, SemanticModel model, Edits e)
    {
        var elem = ((INamedTypeSymbol)ReturnType(member, model)!).TypeArguments[0]
            .ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        e.Insert(body.OpenBraceToken.Span.End, $" var cs2sx_yield = new List<{elem}>();", PrioLoopOpen);
        foreach (var y in Yields(body, member))
        {
            if (y.Expression == null)
                e.Replace(y.SpanStart, y.Span.Length, "return cs2sx_yield;");
            else
            {
                e.Replace(y.SpanStart, y.Expression.SpanStart - y.SpanStart, "cs2sx_yield.Add(");
                e.Replace(y.SemicolonToken.SpanStart, y.SemicolonToken.Span.Length, ");");
            }
        }
        e.Insert(body.CloseBraceToken.SpanStart, " return cs2sx_yield; ", PrioMethod);
    }

    // ── IEnumerator<T>: a state machine ─────────────────────────────────────

    private static bool IsHoistable(LocalDeclarationStatementSyntax ld) =>
        !ld.IsConst && ld.UsingKeyword.RawKind == 0 && ld.Declaration.Type is not RefTypeSyntax;

    private static readonly HashSet<string> Reserved = new(StringComparer.Ordinal)
        { "Current", "MoveNext", "Reset", "Dispose", "cs2sx_state", "cs2sx_this" };

    private static void LowerLazy(MethodDeclarationSyntax m, SemanticModel model, string source, Edits e,
        IDictionary<string, string> iterTypes)
    {
        var symbol = model.GetDeclaredSymbol(m)!;
        var owner = symbol.ContainingType;
        var body = m.Body!;
        bool isStatic = symbol.IsStatic;
        var baseType = TypeText(symbol.ReturnType, iterTypes);

        var smName = m.Identifier.Text + "_Iter";
        {
            var siblings = ((ClassDeclarationSyntax)m.Parent!).Members.OfType<MethodDeclarationSyntax>()
                .Where(x => x.Identifier.Text == m.Identifier.Text).ToList();
            int index = siblings.IndexOf(m);
            if (index > 0) smName += (index + 1);
            while (owner.GetMembers(smName).Any()) smName += "_";
        }

        // Fields: parameters, then every local of the body.
        var fields = new List<(string type, string name)>();
        var fieldTypes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var r in Reserved) fieldTypes[r] = "\u0001";
        var paramNames = new Dictionary<IParameterSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var p in symbol.Parameters)
        {
            var name = p.Name;
            while (fieldTypes.ContainsKey(name)) name += "_";
            fieldTypes[name] = TypeText(p.Type, iterTypes);
            fields.Add((fieldTypes[name], name));
            paramNames[p] = name;
        }
        var localNames = new Dictionary<ILocalSymbol, string>(SymbolEqualityComparer.Default);
        string FieldFor(ILocalSymbol local)
        {
            if (localNames.TryGetValue(local, out var have)) return have;
            var type = TypeText(local.Type, iterTypes);
            var name = local.Name;
            for (int n = 2; fieldTypes.TryGetValue(name, out var t) && t != type; n++) name = local.Name + "_" + n;
            if (!fieldTypes.ContainsKey(name)) { fieldTypes[name] = type; fields.Add((type, name)); }
            return localNames[local] = name;
        }

        foreach (var ld in body.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
        {
            if (InNested(ld, m) || !IsHoistable(ld)) continue;
            var decl = ld.Declaration;
            e.Replace(decl.Type.SpanStart, decl.Variables[0].SpanStart - decl.Type.SpanStart, "");
            for (int i = 0; i < decl.Variables.Count; i++)
            {
                var v = decl.Variables[i];
                if (model.GetDeclaredSymbol(v) is not ILocalSymbol local) continue;
                var name = FieldFor(local);
                if (v.Initializer == null) e.Replace(v.SpanStart, v.Span.Length, "");
                else
                {
                    if (name != local.Name) e.Replace(v.Identifier.SpanStart, v.Identifier.Span.Length, name);
                    if (v.Initializer.Value is InitializerExpressionSyntax arrayInit)   // int[] a = { 1, 2 };
                        e.Insert(arrayInit.SpanStart, "new " + TypeText(local.Type, iterTypes) + " ", PrioName, statementLevel: false);
                }
                if (i < decl.Variables.SeparatorCount)
                {
                    var comma = decl.Variables.GetSeparator(i);
                    e.Replace(comma.SpanStart, comma.Span.Length, ";");
                }
            }
        }
        foreach (var fs in body.DescendantNodes().OfType<ForStatementSyntax>())
        {
            if (InNested(fs, m) || fs.Declaration is not { } decl) continue;
            e.Replace(decl.Type.SpanStart, decl.Variables[0].SpanStart - decl.Type.SpanStart, "");
            foreach (var v in decl.Variables)
                if (model.GetDeclaredSymbol(v) is ILocalSymbol local && FieldFor(local) is var name && name != local.Name)
                    e.Replace(v.Identifier.SpanStart, v.Identifier.Span.Length, name);
        }

        var loops = body.DescendantNodes().OfType<ForEachStatementSyntax>()
            .Where(fe => !InNested(fe, m) && Yields(fe.Statement, m).Any()).ToList();
        foreach (var fe in loops)
            if (model.GetDeclaredSymbol(fe) is ILocalSymbol local) FieldFor(local);

        // Names: renamed locals and parameters, this, and members of the owner reached without "this.".
        foreach (var t in body.DescendantNodes().OfType<ThisExpressionSyntax>())
            e.Replace(t.SpanStart, t.Span.Length, "cs2sx_this");
        foreach (var id in body.DescendantNodes().OfType<SimpleNameSyntax>())
        {
            if (id.Parent is MemberAccessExpressionSyntax ma && ma.Name == id) continue;
            if (id.Parent is MemberBindingExpressionSyntax or QualifiedNameSyntax or NameColonSyntax or NameEqualsSyntax) continue;
            if (id.Parent is AssignmentExpressionSyntax { Parent: InitializerExpressionSyntax } init && init.Left == id) continue;
            var info = model.GetSymbolInfo(id);
            var sym = info.Symbol ?? info.CandidateSymbols.FirstOrDefault();
            if (sym is ILocalSymbol local)
            {
                if (localNames.TryGetValue(local, out var name) && name != local.Name)
                    e.Replace(id.SpanStart, id.Span.Length, name);
                continue;
            }
            if (sym is IParameterSymbol ps)
            {
                if (paramNames.TryGetValue(ps, out var name) && name != ps.Name)
                    e.Replace(id.SpanStart, id.Span.Length, name);
                continue;
            }
            if (sym is not (IFieldSymbol or IPropertySymbol or IEventSymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary })) continue;
            if (!InheritsFrom(owner, sym.ContainingType)) continue;
            e.Insert(id.SpanStart, sym.IsStatic ? sym.ContainingType.Name + "." : "cs2sx_this.", PrioName, statementLevel: false);
        }

        // yield return x → store it and pause; yield break → done.
        int points = 0;
        foreach (var y in Yields(body, m).OrderBy(y => y.SpanStart))
        {
            if (y.Expression == null)
            {
                e.Replace(y.SpanStart, y.Span.Length, "{ cs2sx_state = -1; return false; }");
                continue;
            }
            int k = ++points;
            e.Replace(y.SpanStart, y.Expression.SpanStart - y.SpanStart, "{ Current = ");
            e.Replace(y.SemicolonToken.SpanStart, y.SemicolonToken.Span.Length, $"; CS2SX_Yield.Return({k}); }}");
        }

        // foreach with a yield in its body → index loop over fields.
        int loopIndex = 0;
        foreach (var fe in loops)
        {
            int n = ++loopIndex;
            var collType = model.GetTypeInfo(fe.Expression).Type!;
            var elem = localNames[(ILocalSymbol)model.GetDeclaredSymbol(fe)!];
            string c = "cs2sx_c" + n, i = "cs2sx_i" + n;
            var expr = e.Render(source, fe.Expression.SpanStart, fe.Expression.Span.End, all: false);
            string collText, count;
            if (IsListOrArray(collType))
            {
                collText = TypeText(collType, iterTypes);
                count = collType is IArrayTypeSymbol ? "Length" : "Count";
            }
            else   // IEnumerable<T>: a copy as a list
            {
                var t = TypeText(((INamedTypeSymbol)collType).TypeArguments[0], iterTypes);
                collText = $"List<{t}>";
                expr = $"new List<{t}>({expr})";
                count = "Count";
            }
            fields.Add((collText, c));
            fields.Add(("int", i));
            var header = $"{{ {c} = {expr}; for ({i} = 0; {i} < {c}.{count}; {i}++)";
            e.Replace(fe.ForEachKeyword.SpanStart, fe.CloseParenToken.Span.End - fe.ForEachKeyword.SpanStart, header);
            var assign = $"{elem} = {c}[{i}]; ";
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

        // The method itself: create the machine; the body moves into MoveNext().
        var head = new StringBuilder();
        head.Append($" var cs2sx_sm = new {smName}(); ");
        if (!isStatic) head.Append("cs2sx_sm.cs2sx_this = this; ");
        foreach (var p in symbol.Parameters) head.Append($"cs2sx_sm.{paramNames[p]} = {p.Name}; ");
        head.Append($"return ({baseType})cs2sx_sm; ");
        head.Append($"}} public sealed class {smName} : {baseType} {{ ");
        if (!isStatic) head.Append($"public {owner.Name} cs2sx_this; ");
        head.Append("public int cs2sx_state; ");
        foreach (var (type, name) in fields) head.Append($"public {type} {name}; ");
        head.Append($"public override bool MoveNext() {{ if (cs2sx_state < 0) return false; CS2SX_Yield.Begin({points});");

        e.Insert(body.OpenBraceToken.Span.End, head.ToString(), PrioLoopOpen);
        e.Insert(body.CloseBraceToken.SpanStart, " cs2sx_state = -1; return false; ", PrioMethod);
        e.Insert(body.CloseBraceToken.Span.End, " }", PrioMethod);
    }

    private static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol? ancestor)
    {
        for (INamedTypeSymbol? t = type; t != null; t = t.BaseType)
            if (SymbolEqualityComparer.Default.Equals(t, ancestor)) return true;
        return false;
    }

    // ── The enumerator classes (C#, transpiled with the project) ────────────

    private static string RuntimeSource(IDictionary<string, string> iterTypes)
    {
        var sb = new StringBuilder("public static class CS2SX_Yield { public static void Begin(int points) { } public static void Return(int point) { } } ");
        foreach (var (name, elem) in iterTypes)
            sb.Append($"public abstract class {name} {{ public {elem} Current; public abstract bool MoveNext(); "
                    + "public void Reset() { throw new InvalidOperationException(\"Reset wird nicht unterstützt — die Methode neu aufrufen.\"); } "
                    + "public void Dispose() { } } ");
        return sb.ToString();
    }
}
