using System.Text;
using System.Text.RegularExpressions;
using CS2SX.Logging;
using CS2SX.Transpiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Build;

/// Generic methods, specialised per type at the C# level before transpiling:
///
///   static T Max&lt;T&gt;(T a, T b) where T : IComparable&lt;T&gt; …   Max(3, 7)   Max("a", "b")
///   →  static int Max_int(int a, int b) …                    Max_int(3, 7)
///      static string Max_str(string a, string b) …           Max_str("a", "b")
///
/// Type arguments come from the semantic model, so inferred ones work too; a generic
/// method called from another generic method is specialised transitively. Call sites
/// are renamed in place, the generic declaration is commented out line by line and
/// the specialisations are appended at the end of the declaring type — line numbers
/// of the user's code stay the same in diagnostics.
///
/// Not handled (UnsupportedSyntax reports them): virtual/abstract/override and
/// interface generic methods, generic methods of generic types, method groups.
public static class GenericMethodMonomorphizer
{
    private sealed record Edit(int Start, int Length, string Text);

    private sealed class Spec
    {
        public required GenericDecl Decl { get; init; }
        public required IReadOnlyList<string> Args { get; init; }
        public required string Name { get; init; }
    }

    private sealed class GenericDecl
    {
        public required MethodDeclarationSyntax Syntax { get; init; }
        public required IMethodSymbol Symbol { get; init; }
        public required SemanticModel Model { get; init; }
        public required string File { get; init; }
    }

    /// Returns the source files to transpile: rewritten copies (under
    /// <paramref name="outDir"/>) for files that changed, the originals otherwise.
    public static List<string> Rewrite(IReadOnlyList<string> files, string outDir)
    {
        // Nothing to do without a generic method declaration (cheap syntax check).
        if (!files.Any(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f)).GetRoot()
                .DescendantNodes().OfType<MethodDeclarationSyntax>().Any(m => m.TypeParameterList != null)))
            return files.ToList();

        var sem = new SemanticModelBuilder(files);
        var models = files.ToDictionary(f => f, f => sem.GetModel(f));

        // Supported generic method declarations, by their symbol.
        var decls = new Dictionary<IMethodSymbol, GenericDecl>(SymbolEqualityComparer.Default);
        foreach (var (file, model) in models)
        {
            if (model == null) continue;
            foreach (var m in model.SyntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (m.TypeParameterList == null || !IsSupported(m)) continue;
                if (model.GetDeclaredSymbol(m) is IMethodSymbol sym)
                    decls[sym] = new GenericDecl { Syntax = m, Symbol = sym, Model = model, File = file };
            }
        }
        if (decls.Count == 0) return files.ToList();

        var edits = files.ToDictionary(f => f, _ => new List<Edit>());
        var specs = new Dictionary<string, Spec>(StringComparer.Ordinal);
        var queue = new Queue<Spec>();

        Spec SpecFor(GenericDecl d, IReadOnlyList<string> args)
        {
            var name = d.Symbol.Name + "_" + string.Join("_", args.Select(Suffix));
            var key = d.File + "|" + d.Syntax.SpanStart + "|" + name;
            if (!specs.TryGetValue(key, out var s))
            {
                s = new Spec { Decl = d, Args = args, Name = name };
                specs[key] = s;
                queue.Enqueue(s);
            }
            return s;
        }

        // 1. Call sites outside generic method bodies: concrete type arguments.
        foreach (var (file, model) in models)
        {
            if (model == null) continue;
            foreach (var inv in model.SyntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (inv.Ancestors().OfType<MethodDeclarationSyntax>().Any(a => a.TypeParameterList != null))
                    continue;   // handled when its generic method is specialised
                if (Target(inv, model, decls) is not var (decl, method) || decl == null) continue;
                if (method.TypeArguments.Any(ContainsTypeParameter)) continue;
                var spec = SpecFor(decl, method.TypeArguments.Select(Display).ToList());
                if (NameNode(inv) is { } nameNode)
                    edits[file].Add(new Edit(nameNode.SpanStart, nameNode.Span.Length, spec.Name));
            }
        }

        // 2. Specialise (transitively), grouped by declaring type for insertion.
        var inserts = new Dictionary<(string file, int pos), StringBuilder>();
        while (queue.Count > 0)
        {
            var spec = queue.Dequeue();
            var text = Specialise(spec, decls, SpecFor);
            var type = (TypeDeclarationSyntax)spec.Decl.Syntax.Parent!;
            var at = (spec.Decl.File, type.CloseBraceToken.SpanStart);
            if (!inserts.TryGetValue(at, out var sb)) inserts[at] = sb = new StringBuilder();
            sb.Append("\n    // specialisation of ").Append(spec.Decl.Symbol.Name).Append('<')
              .Append(string.Join(", ", spec.Args)).Append(">\n    ").Append(text).Append('\n');
        }
        foreach (var ((file, pos), sb) in inserts)
            edits[file].Add(new Edit(pos, 0, sb.ToString()));

        // 3. Comment out the generic declarations (same number of lines).
        foreach (var d in decls.Values)
        {
            var source = d.Model.SyntaxTree.GetText();
            var first = source.Lines.GetLineFromPosition(d.Syntax.SpanStart).LineNumber;
            var last = source.Lines.GetLineFromPosition(d.Syntax.Span.End).LineNumber;
            for (int l = first; l <= last; l++)
                edits[d.File].Add(new Edit(source.Lines[l].Start, 0, "// "));
        }

        // 4. Apply and write changed files.
        var result = new List<string>();
        var dir = Path.Combine(outDir, "_cs");
        Directory.CreateDirectory(dir);
        foreach (var file in files)
        {
            var list = edits[file];
            if (list.Count == 0) { result.Add(file); continue; }
            var text = File.ReadAllText(file);
            foreach (var e in list.OrderByDescending(e => e.Start).ThenByDescending(e => e.Length))
                text = text.Remove(e.Start, e.Length).Insert(e.Start, e.Text);
            var target = Path.Combine(dir, Path.GetFileName(file));
            // Unchanged content keeps its timestamp, so incremental builds skip it.
            if (!File.Exists(target) || File.ReadAllText(target) != text)
                File.WriteAllText(target, text);
            result.Add(target);
        }
        Log.Info($"Generic methods: {specs.Count} specialisation(s)");
        return result;
    }

    /// The generic method's text with type parameters replaced, renamed, without
    /// type parameter list and constraints; nested generic calls are specialised too.
    private static string Specialise(Spec spec, Dictionary<IMethodSymbol, GenericDecl> decls,
        Func<GenericDecl, IReadOnlyList<string>, Spec> specFor)
    {
        var m = spec.Decl.Syntax;
        var model = spec.Decl.Model;
        var typeParams = spec.Decl.Symbol.TypeParameters;
        var local = new List<Edit>();
        int Rel(int pos) => pos - m.SpanStart;

        local.Add(new Edit(Rel(m.Identifier.SpanStart), m.Identifier.Span.Length, spec.Name));
        local.Add(new Edit(Rel(m.TypeParameterList!.SpanStart), m.TypeParameterList.Span.Length, ""));
        foreach (var c in m.ConstraintClauses)
            local.Add(new Edit(Rel(c.SpanStart), c.Span.Length, ""));

        string Subst(ITypeSymbol t)
        {
            if (t is ITypeParameterSymbol tp && tp.DeclaringMethod is { } dm
                && SymbolEqualityComparer.Default.Equals(dm.OriginalDefinition, spec.Decl.Symbol))
                return spec.Args[tp.Ordinal];
            var s = Display(t);
            foreach (var p in typeParams)
                s = Regex.Replace(s, $@"\b{Regex.Escape(p.Name)}\b", spec.Args[p.Ordinal]);
            return s;
        }

        // Nested calls to generic methods (renamed whole, so type-parameter edits
        // inside their name are skipped).
        var renamed = new List<TextSpanRange>();
        foreach (var inv in m.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (Target(inv, model, decls) is not var (decl, method) || decl == null) continue;
            var nested = specFor(decl, method.TypeArguments.Select(Subst).ToList());
            if (NameNode(inv) is { } nameNode)
            {
                local.Add(new Edit(Rel(nameNode.SpanStart), nameNode.Span.Length, nested.Name));
                renamed.Add(new TextSpanRange(nameNode.SpanStart, nameNode.Span.End));
            }
        }

        // Type parameter references: T → int, T? → int (value type) or string?
        var inHeader = new List<TextSpanRange> { new(m.TypeParameterList.SpanStart, m.TypeParameterList.Span.End) };
        inHeader.AddRange(m.ConstraintClauses.Select(c => new TextSpanRange(c.SpanStart, c.Span.End)));
        foreach (var id in m.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            if (renamed.Concat(inHeader).Any(r => r.Contains(id.SpanStart))) continue;
            if (model.GetSymbolInfo(id).Symbol is not ITypeParameterSymbol tp
                || !SymbolEqualityComparer.Default.Equals(tp.DeclaringMethod?.OriginalDefinition, spec.Decl.Symbol))
                continue;
            var arg = spec.Args[tp.Ordinal];
            if (id.Parent is NullableTypeSyntax nt)
            {
                // Unconstrained T? is just T for value types (C# semantics).
                var isValue = model.GetTypeInfo(nt).Type is { IsValueType: true }
                              || IsValueTypeName(arg);
                local.Add(new Edit(Rel(nt.SpanStart), nt.Span.Length, isValue ? arg : arg + "?"));
            }
            else
                local.Add(new Edit(Rel(id.SpanStart), id.Span.Length, arg));
        }

        var text = m.ToString();
        foreach (var e in local.OrderByDescending(e => e.Start).ThenByDescending(e => e.Length))
            text = text.Remove(e.Start, e.Length).Insert(e.Start, e.Text);
        return text;
    }

    private readonly record struct TextSpanRange(int Start, int End)
    {
        public bool Contains(int pos) => pos >= Start && pos < End;
    }

    private static (GenericDecl? decl, IMethodSymbol method)? Target(
        InvocationExpressionSyntax inv, SemanticModel model, Dictionary<IMethodSymbol, GenericDecl> decls)
    {
        if (model.GetSymbolInfo(inv).Symbol is not IMethodSymbol { IsGenericMethod: true } m) return null;
        var def = (m.ReducedFrom ?? m).OriginalDefinition;
        return decls.TryGetValue(def, out var d) ? (d, m) : null;
    }

    /// The name token(s) of the called method: Max, Max<int>, Util.Max, x.Ext<T>.
    private static SyntaxNode? NameNode(InvocationExpressionSyntax inv) => inv.Expression switch
    {
        SimpleNameSyntax s => s,
        MemberAccessExpressionSyntax ma => ma.Name,
        MemberBindingExpressionSyntax mb => mb.Name,
        _ => null,
    };

    private static bool IsSupported(MethodDeclarationSyntax m) =>
        m.Parent is ClassDeclarationSyntax { TypeParameterList: null } or StructDeclarationSyntax { TypeParameterList: null }
        && (m.Body != null || m.ExpressionBody != null)
        && !m.Modifiers.Any(x => x.IsKind(SyntaxKind.VirtualKeyword) || x.IsKind(SyntaxKind.AbstractKeyword)
                               || x.IsKind(SyntaxKind.OverrideKeyword));

    private static bool ContainsTypeParameter(ITypeSymbol t) => t switch
    {
        ITypeParameterSymbol => true,
        IArrayTypeSymbol a => ContainsTypeParameter(a.ElementType),
        INamedTypeSymbol n => n.TypeArguments.Any(ContainsTypeParameter),
        _ => false,
    };

    private static string Display(ITypeSymbol t) =>
        t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

    private static bool IsValueTypeName(string t) => t is "int" or "uint" or "long" or "ulong" or "short"
        or "ushort" or "byte" or "sbyte" or "float" or "double" or "bool" or "char";

    /// Method-name suffix for a type argument: int → int, string → str, List<int> → List_int.
    private static string Suffix(string t) =>
        t == "string" ? "str" : Regex.Replace(t.Replace("[]", "Arr"), @"[^A-Za-z0-9]+", "_").Trim('_');
}
