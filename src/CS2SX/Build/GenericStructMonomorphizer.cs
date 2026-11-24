using System.Text;
using System.Text.RegularExpressions;
using CS2SX.Logging;
using CS2SX.Transpiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Build;

/// Generic structs, specialised per type at the C# level (after the generic
/// methods, so their specialisations are seen too):
///
///   struct Pair&lt;T&gt; { public T A, B; public Pair(T a, T b) { … } }   Pair&lt;int&gt; p;
///   →  struct Pair_int { public int A, B; public Pair_int(int a, int b) { … } }   Pair_int p;
///
/// Uses are renamed in place; the specialisations replace the generic declaration
/// (on its first line, the rest blank) — line numbers of user code stay.
///
/// The same for generic interfaces (IRepo&lt;int&gt; → IRepo_int: C has no generic
/// vtables) and generic classes that another class derives from (class IntBox :
/// Box&lt;int&gt;): the derived class embeds a concrete base struct. Other generic
/// classes are expanded later at the C level (GenericExpander).
public static class GenericStructMonomorphizer
{
    private sealed record Edit(int Start, int Length, string Text);

    private sealed class Decl
    {
        public required TypeDeclarationSyntax Syntax { get; init; }
        public required INamedTypeSymbol Symbol { get; init; }
        public required SemanticModel Model { get; init; }
        public required string File { get; init; }
    }

    private sealed record Spec(Decl Decl, IReadOnlyList<string> Args, string Name);

    public static List<string> Rewrite(IReadOnlyList<string> files, string outDir)
    {
        if (!files.Any(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f)).GetRoot()
                .DescendantNodes().OfType<TypeDeclarationSyntax>().Any(s => s.TypeParameterList != null
                    && s is StructDeclarationSyntax or InterfaceDeclarationSyntax or ClassDeclarationSyntax)))
            return files.ToList();

        var sem = new SemanticModelBuilder(files);
        var models = files.ToDictionary(f => f, f => sem.GetModel(f));

        // Generic classes some class derives from.
        var baseClasses = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var (_, model) in models)
        {
            if (model == null) continue;
            foreach (var c in model.SyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                if (model.GetDeclaredSymbol(c) is INamedTypeSymbol { BaseType: { IsGenericType: true } bt }
                    && bt.OriginalDefinition.Locations.Any(l => l.IsInSource))
                    baseClasses.Add(bt.OriginalDefinition);
        }

        var decls = new Dictionary<INamedTypeSymbol, Decl>(SymbolEqualityComparer.Default);
        foreach (var (file, model) in models)
        {
            if (model == null) continue;
            foreach (var s in model.SyntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (s.TypeParameterList == null) continue;
                if (s is ClassDeclarationSyntax && !(model.GetDeclaredSymbol(s) is INamedTypeSymbol cs && baseClasses.Contains(cs)))
                    continue;
                if (s is not (StructDeclarationSyntax or InterfaceDeclarationSyntax or ClassDeclarationSyntax)) continue;
                // Generic methods in a generic interface: not supported (UnsupportedSyntax reports them).
                if (s is InterfaceDeclarationSyntax && s.Members.OfType<MethodDeclarationSyntax>().Any(m => m.TypeParameterList != null))
                    continue;
                if (s.Ancestors().OfType<TypeDeclarationSyntax>().Any(t => t.TypeParameterList != null)) continue;
                if (model.GetDeclaredSymbol(s) is INamedTypeSymbol sym)
                    decls[sym] = new Decl { Syntax = s, Symbol = sym, Model = model, File = file };
            }
        }
        if (decls.Count == 0) return files.ToList();

        var edits = files.ToDictionary(f => f, _ => new List<Edit>());
        var specs = new Dictionary<string, Spec>(StringComparer.Ordinal);
        var queue = new Queue<Spec>();

        Spec SpecFor(Decl d, IReadOnlyList<string> args)
        {
            var name = d.Symbol.Name + "_" + string.Join("_", args.Select(Suffix));
            if (!specs.TryGetValue(name, out var s))
            {
                s = new Spec(d, args, name);
                specs[name] = s;
                queue.Enqueue(s);
            }
            return s;
        }

        // 1. Uses outside the generic declarations.
        foreach (var (file, model) in models)
        {
            if (model == null) continue;
            foreach (var gn in model.SyntaxTree.GetRoot().DescendantNodes().OfType<GenericNameSyntax>())
            {
                if (gn.Ancestors().OfType<TypeDeclarationSyntax>().Any(s => s.TypeParameterList != null
                        && decls.Values.Any(d => d.Syntax == s))) continue;
                if (Target(gn, model, decls) is not var (decl, type) || decl == null) continue;
                if (type.TypeArguments.Any(ContainsTypeParameter)) continue;
                var spec = SpecFor(decl, type.TypeArguments.Select(Display).ToList());
                edits[file].Add(new Edit(gn.SpanStart, gn.Span.Length, spec.Name));
            }
        }

        // 2. Specialise (transitively).
        var specTexts = new Dictionary<Decl, StringBuilder>();
        while (queue.Count > 0)
        {
            var spec = queue.Dequeue();
            if (!specTexts.TryGetValue(spec.Decl, out var sb)) specTexts[spec.Decl] = sb = new StringBuilder();
            sb.Append(OneLine(Specialise(spec, decls, SpecFor))).Append(' ');
        }

        // 3. The specialisations replace the generic declaration, on its first line;
        //    its other lines stay as blank lines. So the types keep their place (a base
        //    class before the classes deriving from it) and no line of user code moves.
        foreach (var d in decls.Values)
        {
            var source = d.Model.SyntaxTree.GetText();
            var span = d.Syntax.Span;
            var lines = source.ToString(span).Count(c => c == '\n');
            var text = (specTexts.TryGetValue(d, out var sb) ? sb.ToString().TrimEnd() : "") + new string('\n', lines);
            edits[d.File].Add(new Edit(span.Start, span.Length, text));
        }

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
            if (!File.Exists(target) || File.ReadAllText(target) != text)
                File.WriteAllText(target, text);
            result.Add(target);
        }
        Log.Info($"Generic types: {specs.Count} specialisation(s)");
        return result;
    }

    /// C# declarations on one line: comments dropped, line breaks as spaces.
    private static string OneLine(string code)
    {
        var root = CSharpSyntaxTree.ParseText(code).GetRoot();
        var clean = root.ReplaceTrivia(root.DescendantTrivia(descendIntoTrivia: true).Where(t =>
                t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia)
                || t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
                || t.IsKind(SyntaxKind.EndOfLineTrivia)),
            (_, _) => SyntaxFactory.Space);
        return clean.ToFullString().Replace("\r", " ").Replace("\n", " ");
    }

    private static string Specialise(Spec spec, Dictionary<INamedTypeSymbol, Decl> decls,
        Func<Decl, IReadOnlyList<string>, Spec> specFor)
    {
        var s = spec.Decl.Syntax;
        var model = spec.Decl.Model;
        var typeParams = spec.Decl.Symbol.TypeParameters;
        var local = new List<Edit>();
        int Rel(int pos) => pos - s.SpanStart;

        local.Add(new Edit(Rel(s.Identifier.SpanStart), s.Identifier.Span.Length, spec.Name));
        local.Add(new Edit(Rel(s.TypeParameterList!.SpanStart), s.TypeParameterList.Span.Length, ""));
        foreach (var c in s.ConstraintClauses)
            local.Add(new Edit(Rel(c.SpanStart), c.Span.Length, ""));
        foreach (var ctor in s.Members.OfType<ConstructorDeclarationSyntax>())
            local.Add(new Edit(Rel(ctor.Identifier.SpanStart), ctor.Identifier.Span.Length, spec.Name));

        string Subst(ITypeSymbol t)
        {
            if (t is ITypeParameterSymbol tp && SymbolEqualityComparer.Default.Equals(tp.ContainingSymbol, spec.Decl.Symbol))
                return spec.Args[tp.Ordinal];
            var str = Display(t);
            foreach (var p in typeParams)
                str = Regex.Replace(str, $@"\b{Regex.Escape(p.Name)}\b", spec.Args[p.Ordinal]);
            return str;
        }

        // Generic struct uses inside (incl. the struct itself, Pair<T> → Pair_int).
        var renamed = new List<(int start, int end)>();
        foreach (var gn in s.DescendantNodes().OfType<GenericNameSyntax>())
        {
            if (Target(gn, model, decls) is not var (decl, type) || decl == null) continue;
            var nested = specFor(decl, type.TypeArguments.Select(Subst).ToList());
            local.Add(new Edit(Rel(gn.SpanStart), gn.Span.Length, nested.Name));
            renamed.Add((gn.SpanStart, gn.Span.End));
        }

        var header = new List<(int start, int end)> { (s.TypeParameterList.SpanStart, s.TypeParameterList.Span.End) };
        header.AddRange(s.ConstraintClauses.Select(c => (c.SpanStart, c.Span.End)));
        foreach (var id in s.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            if (renamed.Concat(header).Any(r => id.SpanStart >= r.start && id.SpanStart < r.end)) continue;
            if (model.GetSymbolInfo(id).Symbol is not ITypeParameterSymbol tp
                || !SymbolEqualityComparer.Default.Equals(tp.ContainingSymbol, spec.Decl.Symbol))
                continue;
            local.Add(new Edit(Rel(id.SpanStart), id.Span.Length, spec.Args[tp.Ordinal]));
        }

        var text = s.ToString();
        foreach (var e in local.OrderByDescending(e => e.Start).ThenByDescending(e => e.Length))
            text = text.Remove(e.Start, e.Length).Insert(e.Start, e.Text);
        return text;
    }

    private static (Decl? decl, INamedTypeSymbol type)? Target(GenericNameSyntax gn, SemanticModel model,
        Dictionary<INamedTypeSymbol, Decl> decls)
    {
        var sym = model.GetSymbolInfo(gn).Symbol as INamedTypeSymbol
                  ?? model.GetTypeInfo(gn).Type as INamedTypeSymbol;
        if (sym == null || !sym.IsGenericType) return null;
        return decls.TryGetValue(sym.OriginalDefinition, out var d) ? (d, sym) : null;
    }

    private static bool ContainsTypeParameter(ITypeSymbol t) => t switch
    {
        ITypeParameterSymbol => true,
        IArrayTypeSymbol a => ContainsTypeParameter(a.ElementType),
        INamedTypeSymbol n => n.TypeArguments.Any(ContainsTypeParameter),
        _ => false,
    };

    private static string Display(ITypeSymbol t) =>
        t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

    private static string Suffix(string t) =>
        t == "string" ? "str" : Regex.Replace(t.Replace("[]", "Arr"), @"[^A-Za-z0-9]+", "_").Trim('_');
}
