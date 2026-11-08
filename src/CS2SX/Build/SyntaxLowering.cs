using System.Text;
using CS2SX.Logging;
using CS2SX.Transpiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Build;

/// C# conveniences rewritten into constructs the transpiler handles, at the C# level
/// and before anything else runs (like GenericMethodMonomorphizer):
///
///   local function (no captured locals) → private method of the type
///   [1, 2, 3]                            → new List&lt;int&gt; { 1, 2, 3 } / new int[] { … }
///   p with { X = 5 }                     → new Punkt(5, p.Y)               (records)
///   foreach (var i in a..b)              → for (int i = a; i &lt; b; i++)
///   s[a..b] / s[^n] / list[a..]          → Substring / Length - n / GetRange
///   delegate int Op(int a);              → Func&lt;int, int&gt; at every use
///
/// Every replacement keeps the number of lines, so diagnostics point at the user's
/// lines. What cannot be lowered (a local function using the method's locals, …)
/// is left alone and reported by UnsupportedSyntax.
public static class SyntaxLowering
{
    private sealed record Edit(int Start, int Length, string Text);

    public static List<string> Rewrite(IReadOnlyList<string> files, string outDir)
    {
        static bool Candidate(SyntaxNode n) => n is LocalFunctionStatementSyntax or CollectionExpressionSyntax
            or WithExpressionSyntax or RangeExpressionSyntax or DelegateDeclarationSyntax
            or AnonymousFunctionExpressionSyntax
            || n.IsKind(SyntaxKind.IndexExpression);
        if (!files.Any(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f)).GetRoot()
                .DescendantNodes().Any(Candidate)))
            return files.ToList();

        var sem = new SemanticModelBuilder(files);
        var edits = files.ToDictionary(f => f, _ => new List<Edit>());
        var refTypes = new Dictionary<string, string>(StringComparer.Ordinal);   // __Ref_int → int
        var inserts = new Dictionary<(string file, int pos), StringBuilder>();
        int lowered = 0;

        // Delegate types → Func/Action, project-wide (uses may be in any file).
        var delegateTypes = new Dictionary<INamedTypeSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var f in files)
        {
            if (sem.GetModel(f) is not { } model) continue;
            foreach (var dd in model.SyntaxTree.GetRoot().DescendantNodes().OfType<DelegateDeclarationSyntax>())
            {
                if (model.GetDeclaredSymbol(dd) is not INamedTypeSymbol sym || DelegateAsFunc(sym) is not { } func)
                    continue;
                delegateTypes[sym] = func;
                edits[f].Add(Blank(dd, model));
                lowered++;
            }
        }

        foreach (var file in files)
        {
            if (sem.GetModel(file) is not { } model) continue;
            var root = model.SyntaxTree.GetRoot();
            var list = edits[file];

            foreach (var id in root.DescendantNodes().OfType<IdentifierNameSyntax>())
                if (delegateTypes.Count > 0 && model.GetSymbolInfo(id).Symbol is INamedTypeSymbol t
                    && delegateTypes.TryGetValue(t, out var func)
                    && id.Parent is not DelegateDeclarationSyntax)
                    list.Add(new Edit(id.SpanStart, id.Span.Length, func));

            lowered += LowerLocalFunctions(root, model, list, inserts, file);
            lowered += BoxMutatedCaptures(root, model, list, refTypes);

            foreach (var node in root.DescendantNodes())
            {
                // Nodes inside a local function that is being lifted are handled there.
                if (node.Ancestors().OfType<LocalFunctionStatementSyntax>().Any(lf => IsLiftable(lf, model)))
                    continue;
                var text = node switch
                {
                    CollectionExpressionSyntax ce => LowerCollection(ce, model),
                    WithExpressionSyntax we => LowerWith(we, model),
                    ForEachStatementSyntax fe => LowerForEachRange(fe, model),
                    ElementAccessExpressionSyntax ea => LowerIndexing(ea, model),
                    _ => null,
                };
                if (text == null) continue;
                var span = node is ForEachStatementSyntax fes
                    ? Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(fes.ForEachKeyword.SpanStart, fes.CloseParenToken.Span.End)
                    : node.Span;
                list.Add(new Edit(span.Start, span.Length, KeepLines(model, span, text)));
                lowered++;
            }
        }

        foreach (var ((file, pos), sb) in inserts)
            edits[file].Add(new Edit(pos, 0, sb.ToString()));

        // The shared-variable classes (__Ref_int, …), at the end of the first file.
        if (refTypes.Count > 0)
        {
            var first = files[0];
            var sb = new StringBuilder("\n");
            foreach (var (name, type) in refTypes.OrderBy(kv => kv.Key))
                sb.Append($"public sealed class {name} {{ public {type} V; public {name}({type} v) {{ V = v; }} }}\n");
            edits[first].Add(new Edit(File.ReadAllText(first).Length, 0, sb.ToString()));
        }

        if (lowered == 0) return files.ToList();

        var result = new List<string>();
        var dir = Path.Combine(outDir, "_cs");
        Directory.CreateDirectory(dir);
        foreach (var file in files)
        {
            var list = edits[file];
            if (list.Count == 0) { result.Add(file); continue; }
            var text = File.ReadAllText(file);
            text = Apply(text, list);
            var target = Path.Combine(dir, Path.GetFileName(file));
            if (!File.Exists(target) || File.ReadAllText(target) != text)
                File.WriteAllText(target, text);
            result.Add(target);
        }
        Log.Info($"Lowered {lowered} C# construct(s)");
        return result;
    }

    // Non-overlapping edits; nested ones (inside a replaced span) are dropped.
    private static string Apply(string text, List<Edit> edits)
    {
        var kept = new List<Edit>();
        foreach (var e in edits.OrderBy(e => e.Start).ThenByDescending(e => e.Length))
        {
            if (kept.Count > 0 && e.Length > 0)
            {
                var last = kept[^1];
                if (e.Start < last.Start + last.Length) continue;   // inside a replaced span
            }
            kept.Add(e);
        }
        foreach (var e in kept.OrderByDescending(e => e.Start).ThenByDescending(e => e.Length))
            text = text.Remove(e.Start, e.Length).Insert(e.Start, e.Text);
        return text;
    }

    /// <paramref name="text"/> padded with line breaks so it spans as many lines as
    /// the replaced span (whitespace is fine inside any C# construct).
    private static string KeepLines(SemanticModel model, Microsoft.CodeAnalysis.Text.TextSpan span, string text)
    {
        var lines = model.SyntaxTree.GetText().Lines;
        int had = lines.GetLineFromPosition(span.End).LineNumber - lines.GetLineFromPosition(span.Start).LineNumber;
        int has = text.Count(c => c == '\n');
        return has < had ? text + new string('\n', had - has) : text;
    }

    /// Replaces a node with only its line breaks.
    private static Edit Blank(SyntaxNode node, SemanticModel model)
    {
        var span = node.Span;
        return new Edit(span.Start, span.Length, KeepLines(model, span, ""));
    }

    // ── delegate types ──────────────────────────────────────────────────────

    private static string? DelegateAsFunc(INamedTypeSymbol d)
    {
        var invoke = d.DelegateInvokeMethod;
        if (invoke == null || d.IsGenericType || invoke.Parameters.Length > 16
            || invoke.Parameters.Any(p => p.RefKind != RefKind.None || p.IsParams)
            || invoke.ReturnsByRef)
            return null;
        var args = invoke.Parameters.Select(p => p.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)).ToList();
        if (invoke.ReturnsVoid)
            return args.Count == 0 ? "Action" : "Action<" + string.Join(", ", args) + ">";
        args.Add(invoke.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        return "Func<" + string.Join(", ", args) + ">";
    }

    // ── local functions ─────────────────────────────────────────────────────

    /// A local function that only uses its own parameters/locals, fields and `this`.
    private static bool IsLiftable(LocalFunctionStatementSyntax lf, SemanticModel model)
    {
        if (lf.TypeParameterList != null) return false;
        if (lf.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault() == null) return false;
        if (model.GetDeclaredSymbol(lf) is not IMethodSymbol self) return false;
        foreach (var id in lf.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            var s = model.GetSymbolInfo(id).Symbol;
            if (s is ILocalSymbol or IParameterSymbol
                && !SymbolEqualityComparer.Default.Equals(s.ContainingSymbol, self)
                && !IsInside(s.ContainingSymbol, self))
                return false;   // uses a local of the enclosing method
        }
        return true;
    }

    private static bool IsInside(ISymbol? s, IMethodSymbol self)
    {
        for (; s != null; s = s.ContainingSymbol)
            if (SymbolEqualityComparer.Default.Equals(s, self)) return true;
        return false;
    }

    private static int LowerLocalFunctions(SyntaxNode root, SemanticModel model, List<Edit> edits,
        Dictionary<(string, int), StringBuilder> inserts, string file)
    {
        var liftable = root.DescendantNodes().OfType<LocalFunctionStatementSyntax>()
            .Where(lf => IsLiftable(lf, model)).ToList();
        if (liftable.Count == 0) return 0;

        var names = new Dictionary<IMethodSymbol, string>(SymbolEqualityComparer.Default);
        foreach (var lf in liftable)
        {
            var outer = lf.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
            var outerName = outer switch
            {
                MethodDeclarationSyntax m => m.Identifier.Text,
                ConstructorDeclarationSyntax => "Ctor",
                _ => "Member",
            };
            names[(IMethodSymbol)model.GetDeclaredSymbol(lf)!] = outerName + "__" + lf.Identifier.Text;
        }

        // Renamed calls (outside the lifted bodies; inside they are renamed below).
        foreach (var id in root.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            if (model.GetSymbolInfo(id).Symbol is not IMethodSymbol m || !names.TryGetValue(m, out var n)) continue;
            if (liftable.Any(lf => lf.Span.Contains(id.Span))) continue;
            edits.Add(new Edit(id.SpanStart, id.Span.Length, n));
        }

        foreach (var lf in liftable)
        {
            var self = (IMethodSymbol)model.GetDeclaredSymbol(lf)!;
            var outerStatic = lf.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault()?
                .Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)) == true;
            var isStatic = outerStatic || lf.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

            // The body with nested renames, relative to the local function.
            var local = new List<Edit>();
            int Rel(int p) => p - lf.SpanStart;
            foreach (var id in lf.DescendantNodes().OfType<IdentifierNameSyntax>())
                if (model.GetSymbolInfo(id).Symbol is IMethodSymbol m && names.TryGetValue(m, out var n))
                    local.Add(new Edit(Rel(id.SpanStart), id.Span.Length, n));
            local.Add(new Edit(Rel(lf.Identifier.SpanStart), lf.Identifier.Span.Length, names[self]));
            foreach (var mod in lf.Modifiers)
                local.Add(new Edit(Rel(mod.SpanStart), mod.Span.Length, ""));
            var text = Apply(lf.ToString(), local).Trim();

            var type = lf.Ancestors().OfType<TypeDeclarationSyntax>().First();
            var at = (file, type.CloseBraceToken.SpanStart);
            if (!inserts.TryGetValue(at, out var sb)) inserts[at] = sb = new StringBuilder();
            sb.Append("\n    private ").Append(isStatic ? "static " : "").Append(text).Append('\n');

            edits.Add(Blank(lf, model));
        }
        return liftable.Count;
    }

    // ── variables shared with lambdas ───────────────────────────────────────

    /// A local or parameter captured by a lambda and modified anywhere becomes a
    /// shared object, as the C# compiler does it (closures copy captured values):
    ///   int z = 0; Action a = () => z++;   →   var z = new __Ref_int(0); … z.V++ …
    private static int BoxMutatedCaptures(SyntaxNode root, SemanticModel model, List<Edit> edits,
        Dictionary<string, string> refTypes)
    {
        var captured = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (var lam in root.DescendantNodes().OfType<AnonymousFunctionExpressionSyntax>())
        {
            DataFlowAnalysis? flow;
            try { flow = model.AnalyzeDataFlow(lam); } catch { continue; }
            if (flow is not { Succeeded: true }) continue;
            foreach (var s in flow.CapturedInside)
                if (s is ILocalSymbol or IParameterSymbol) captured.Add(s);
        }

        int count = 0;
        foreach (var sym in captured)
        {
            if (!IsWritten(sym, root, model)) continue;   // read-only: a copy is exact
            var type = sym is ILocalSymbol l ? l.Type : ((IParameterSymbol)sym).Type;
            if (type is IErrorTypeSymbol || sym is IParameterSymbol { RefKind: not RefKind.None }) continue;
            var typeText = Display(type);
            var refName = "__Ref_" + Suffix(typeText);

            string access;
            if (sym is ILocalSymbol)
            {
                if (sym.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not VariableDeclaratorSyntax decl
                    || decl.Parent is not VariableDeclarationSyntax { Variables.Count: 1 } vd
                    || vd.Parent is not LocalDeclarationStatementSyntax)
                    continue;   // foreach/out/pattern variables, multiple declarators
                edits.Add(new Edit(vd.Type.SpanStart, vd.Type.Span.Length, "var"));
                if (decl.Initializer != null)
                    edits.Add(new Edit(decl.Initializer.Value.SpanStart, decl.Initializer.Value.Span.Length,
                        $"new {refName}({decl.Initializer.Value})"));
                else
                    edits.Add(new Edit(decl.Identifier.Span.End, 0, $" = new {refName}(default({typeText}))"));
                access = sym.Name + ".V";
            }
            else
            {
                // A parameter: copied into a shared object at the start of the body.
                var owner = sym.ContainingSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                var body = owner switch
                {
                    BaseMethodDeclarationSyntax m => m.Body,
                    LocalFunctionStatementSyntax lf => lf.Body,
                    _ => null,
                };
                if (body == null) continue;
                var boxed = sym.Name + "__r";
                edits.Add(new Edit(body.OpenBraceToken.Span.End, 0, $" var {boxed} = new {refName}({sym.Name});"));
                access = boxed + ".V";
            }

            foreach (var id in root.DescendantNodes().OfType<IdentifierNameSyntax>())
                if (SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(id).Symbol, sym))
                    edits.Add(new Edit(id.SpanStart, id.Span.Length, access));
            refTypes[refName] = typeText;
            count++;
        }
        return count;
    }

    private static bool IsWritten(ISymbol sym, SyntaxNode root, SemanticModel model)
    {
        bool Is(ExpressionSyntax e) => SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(e).Symbol, sym);
        foreach (var n in root.DescendantNodes())
        {
            switch (n)
            {
                case AssignmentExpressionSyntax a when Is(a.Left):
                    return true;
                case PostfixUnaryExpressionSyntax p when (p.IsKind(SyntaxKind.PostIncrementExpression)
                                                          || p.IsKind(SyntaxKind.PostDecrementExpression)) && Is(p.Operand):
                    return true;
                case PrefixUnaryExpressionSyntax p when (p.IsKind(SyntaxKind.PreIncrementExpression)
                                                         || p.IsKind(SyntaxKind.PreDecrementExpression)) && Is(p.Operand):
                    return true;
                case ArgumentSyntax arg when !arg.RefKindKeyword.IsKind(SyntaxKind.None) && Is(arg.Expression):
                    return true;
            }
        }
        return false;
    }

    /// Class-name suffix for a type: int → int, string → str, List<int> → List_int.
    private static string Suffix(string t) =>
        t == "string" ? "str"
        : System.Text.RegularExpressions.Regex.Replace(t.Replace("[]", "Arr"), @"[^A-Za-z0-9]+", "_").Trim('_');

    // ── collection expressions ──────────────────────────────────────────────

    private static string? LowerCollection(CollectionExpressionSyntax ce, SemanticModel model)
    {
        if (ce.Elements.Any(e => e is not ExpressionElementSyntax)) return null;   // spread ..x
        var target = model.GetTypeInfo(ce).ConvertedType;
        var items = string.Join(", ", ce.Elements.Cast<ExpressionElementSyntax>().Select(e => e.Expression.ToString()));
        if (target is IArrayTypeSymbol arr)
        {
            var elem = Display(arr.ElementType);
            return ce.Elements.Count == 0 ? $"new {elem}[0]" : $"new {elem}[] {{ {items} }}";
        }
        if (target is INamedTypeSymbol { TypeArguments.Length: 1 } nt)
        {
            var elem = Display(nt.TypeArguments[0]);
            var kind = nt.Name switch
            {
                "List" or "IList" or "IEnumerable" or "ICollection" or "IReadOnlyList" or "IReadOnlyCollection" => "List",
                "HashSet" or "ISet" => "HashSet",
                "Stack" => "Stack",
                "Queue" => "Queue",
                _ => null,
            };
            if (kind == null) return null;
            return ce.Elements.Count == 0 || kind is "Stack" or "Queue"
                ? (ce.Elements.Count == 0 ? $"new {kind}<{elem}>()" : null)
                : $"new {kind}<{elem}> {{ {items} }}";
        }
        return null;
    }

    // ── with ────────────────────────────────────────────────────────────────

    private static string? LowerWith(WithExpressionSyntax we, SemanticModel model)
    {
        if (!IsSimple(we.Expression)) return null;   // would be evaluated more than once
        if (model.GetTypeInfo(we).Type is not INamedTypeSymbol { IsRecord: true } rec) return null;
        var decl = rec.DeclaringSyntaxReferences.Select(r => r.GetSyntax()).OfType<RecordDeclarationSyntax>()
            .FirstOrDefault(r => r.ParameterList != null);
        if (decl == null) return null;
        var parameters = decl.ParameterList!.Parameters.Select(p => p.Identifier.Text).ToList();
        var given = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var e in we.Initializer.Expressions)
        {
            if (e is not AssignmentExpressionSyntax { Left: IdentifierNameSyntax name } a
                || !parameters.Contains(name.Identifier.Text))
                return null;   // only the positional members
            given[name.Identifier.Text] = a.Right.ToString();
        }
        var src = we.Expression.ToString();
        var args = parameters.Select(p => given.TryGetValue(p, out var v) ? v : src + "." + p);
        return $"new {Display(rec)}({string.Join(", ", args)})";
    }

    private static bool IsSimple(ExpressionSyntax e) => e switch
    {
        IdentifierNameSyntax or ThisExpressionSyntax => true,
        MemberAccessExpressionSyntax ma => IsSimple(ma.Expression),
        ParenthesizedExpressionSyntax p => IsSimple(p.Expression),
        _ => false,
    };

    // ── foreach over a range ────────────────────────────────────────────────

    private static string? LowerForEachRange(ForEachStatementSyntax fe, SemanticModel model)
    {
        if (fe.Expression is not RangeExpressionSyntax { LeftOperand: { } from, RightOperand: { } to }) return null;
        if (from.IsKind(SyntaxKind.IndexExpression) || to.IsKind(SyntaxKind.IndexExpression)) return null;
        var v = fe.Identifier.Text;
        return $"for (int {v} = ({from}); {v} < ({to}); {v}++)";
    }

    // ── ranges and ^ on strings, lists, arrays ──────────────────────────────

    private static string? LowerIndexing(ElementAccessExpressionSyntax ea, SemanticModel model)
    {
        if (ea.ArgumentList.Arguments.Count != 1) return null;
        var arg = ea.ArgumentList.Arguments[0].Expression;
        if (arg is not RangeExpressionSyntax && !arg.IsKind(SyntaxKind.IndexExpression)) return null;
        if (!IsSimple(ea.Expression)) return null;
        var recvType = model.GetTypeInfo(ea.Expression).Type;
        var recv = ea.Expression.ToString();
        string? length = recvType switch
        {
            { SpecialType: SpecialType.System_String } => recv + ".Length",
            IArrayTypeSymbol => recv + ".Length",
            INamedTypeSymbol { Name: "List" } => recv + ".Count",
            _ => null,
        };
        if (length == null) return null;

        string Index(ExpressionSyntax e) => e is PrefixUnaryExpressionSyntax p && p.IsKind(SyntaxKind.IndexExpression)
            ? $"{length} - ({p.Operand})" : $"({e})";

        if (arg is PrefixUnaryExpressionSyntax hat)   // s[^1]
            return $"{recv}[{Index(hat)}]";

        var r = (RangeExpressionSyntax)arg;
        var start = r.LeftOperand != null ? Index(r.LeftOperand) : "0";
        var end = r.RightOperand != null ? Index(r.RightOperand) : length;
        return recvType switch
        {
            { SpecialType: SpecialType.System_String } => $"{recv}.Substring({start}, ({end}) - ({start}))",
            INamedTypeSymbol { Name: "List" } => $"{recv}.GetRange({start}, ({end}) - ({start}))",
            _ => null,   // array slices: no cheap equivalent
        };
    }

    private static string Display(ITypeSymbol t) => t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
}
