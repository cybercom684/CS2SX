// ============================================================================
// CS2SX — Transpiler/GenericExpander.cs  (FIXED v2)
//
// FIX: SemanticModel-Lookup nutzt jetzt inst.BaseName (den originalen
// Klassennamen) statt inst.CName (dem expandierten Namen) — denn die
// SemanticModelBuilder-Compilation kennt "Stack", nicht "Stack_int".
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

        var allGenericSources = BuildGenericSourceFiles(buildDir);
        SemanticModelBuilder? sharedSemanticBuilder = null;
        if (allGenericSources.Count > 0)
        {
            try
            {
                sharedSemanticBuilder = new SemanticModelBuilder(allGenericSources);
                Log.Debug($"GenericExpander: SemanticModel für {allGenericSources.Count} Generic-Quelle(n) aufgebaut");
            }
            catch (Exception ex)
            {
                Log.Warning($"GenericExpander: SemanticModel-Aufbau fehlgeschlagen: {ex.Message}");
            }
        }

        foreach (var inst in _collector.Instantiations)
        {
            if (!_collector.GenericClasses.TryGetValue(inst.BaseName, out var classDef))
                continue;

            Log.Debug($"GenericExpander: Expanding {inst}");

            try
            {
                var (hCode, cCode) = ExpandInstantiation(classDef, inst, sharedSemanticBuilder);
                headerSb.AppendLine(hCode);
                implSb.AppendLine(cCode);
            }
            catch (Exception ex)
            {
                Log.Warning($"GenericExpander: Expansion von {inst} fehlgeschlagen: {ex.Message}");
                headerSb.AppendLine($"/* expansion failed: {inst} — {ex.Message} */");
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

    public IEnumerable<string> GetExpandedTypeNames() =>
        _collector.Instantiations
            .Where(i => _collector.GenericClasses.ContainsKey(i.BaseName))
            .Select(i => i.CName);

    private (string header, string impl) ExpandInstantiation(
        ClassDeclarationSyntax originalClass,
        GenericInstantiation inst,
        SemanticModelBuilder? semanticBuilder)
    {
        var typeMap = BuildTypeMap(originalClass, inst);
        var rewrittenSource = RewriteGenericClass(originalClass, inst, typeMap);

        Log.Debug($"GenericExpander: Rewritten source for {inst} ({rewrittenSource.Length} chars)");

        // FIX: Lookup nach inst.BaseName (originalem Klassennamen), nicht inst.CName
        // Die SemanticModelBuilder-Compilation kennt "Stack", nicht "Stack_int"
        SemanticModel? semanticModel = null;
        if (semanticBuilder != null)
        {
            semanticModel = semanticBuilder.GetModelForBaseName(inst.BaseName);
            if (semanticModel == null)
                Log.Debug($"GenericExpander: Kein SemanticModel für '{inst.BaseName}' — Transpile ohne Typ-Info");
        }

        var dummyCollector = new GenericInstantiationCollector();
        var dummyExpander = new InterfaceExpander(dummyCollector);

        var hTranspiler = new CSharpToC(
            CSharpToC.TranspileMode.HeaderOnly,
            dummyCollector,
            dummyExpander);
        var hResult = hTranspiler.Transpile(rewrittenSource,
            filePath: $"<generic:{inst}>",
            semanticModel: semanticModel);

        var cTranspiler = new CSharpToC(
            CSharpToC.TranspileMode.Implementation,
            dummyCollector,
            dummyExpander);
        var cResult = cTranspiler.Transpile(rewrittenSource,
            filePath: $"<generic:{inst}>",
            semanticModel: semanticModel);

        foreach (var d in hResult.Diagnostics.Concat(cResult.Diagnostics)
            .Where(d => d.Severity == Core.DiagnosticSeverity.Warning))
        {
            Log.Warning($"GenericExpander/{inst}: {d.Message}");
        }

        return (hResult.Code, cResult.Code);
    }

    private List<string> BuildGenericSourceFiles(string buildDir)
    {
        var paths = new List<string>();
        var tmpDir = Path.Combine(buildDir, "_generics_tmp");
        Directory.CreateDirectory(tmpDir);

        foreach (var inst in _collector.Instantiations)
        {
            if (!_collector.GenericClasses.TryGetValue(inst.BaseName, out var classDef))
                continue;

            try
            {
                var typeMap = BuildTypeMap(classDef, inst);
                var source = RewriteGenericClass(classDef, inst, typeMap);
                // FIX: Dateiname nach BaseName (für GetModelForBaseName-Lookup)
                var tmpPath = Path.Combine(tmpDir, inst.BaseName + "_" + inst.CName + ".cs");
                File.WriteAllText(tmpPath, source);
                paths.Add(tmpPath);
            }
            catch (Exception ex)
            {
                Log.Debug($"GenericExpander: Temp-Datei für {inst} fehlgeschlagen: {ex.Message}");
            }
        }

        return paths;
    }

    private static string RewriteGenericClass(
        ClassDeclarationSyntax cls,
        GenericInstantiation inst,
        Dictionary<string, string> typeMap)
    {
        var rewriter = new TypeParameterRewriter(typeMap, inst.CName, cls.Identifier.Text);
        var newRoot = rewriter.Visit(cls.SyntaxTree.GetRoot());

        var newClass = newRoot.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.Text == inst.CName);

        if (newClass == null)
            return newRoot.NormalizeWhitespace().ToFullString();

        return newClass.NormalizeWhitespace().ToFullString();
    }

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

    public static string SubstituteType(string csType, Dictionary<string, string> typeMap)
    {
        if (typeMap.TryGetValue(csType, out var direct))
            return TypeRegistry.MapType(direct);

        if (csType.EndsWith("[]"))
        {
            var baseType = csType[..^2];
            if (typeMap.TryGetValue(baseType, out var arrBase))
                return TypeRegistry.MapType(arrBase) + "*";
        }

        if (csType.StartsWith("List<") && csType.EndsWith(">"))
        {
            var inner = csType[5..^1].Trim();
            var resolved = typeMap.TryGetValue(inner, out var ri) ? ri : inner;
            var cInner = resolved == "string" ? "str" : TypeRegistry.MapType(resolved);
            return $"List_{cInner}*";
        }

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

// ── TypeParameterRewriter — unverändert ──────────────────────────────────────
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

    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (node.Identifier.Text == _oldClassName)
        {
            var newName = SyntaxFactory.Identifier(_newClassName)
                .WithLeadingTrivia(node.Identifier.LeadingTrivia)
                .WithTrailingTrivia(node.Identifier.TrailingTrivia);

            node = node
                .WithIdentifier(newName)
                .WithTypeParameterList(null)
                .WithConstraintClauses(
                    SyntaxFactory.List<TypeParameterConstraintClauseSyntax>());
        }
        return base.VisitClassDeclaration(node);
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        if (node.Identifier.Text == _oldClassName)
            node = node.WithIdentifier(
                SyntaxFactory.Identifier(_newClassName).WithTriviaFrom(node.Identifier));
        return base.VisitConstructorDeclaration(node);
    }

    public override SyntaxNode? VisitDestructorDeclaration(DestructorDeclarationSyntax node)
    {
        if (node.Identifier.Text == _oldClassName)
            node = node.WithIdentifier(
                SyntaxFactory.Identifier(_newClassName).WithTriviaFrom(node.Identifier));
        return base.VisitDestructorDeclaration(node);
    }

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        var name = node.Identifier.Text;
        if (_typeMap.TryGetValue(name, out var concrete))
            return SyntaxFactory.IdentifierName(concrete).WithTriviaFrom(node);
        if (name == _oldClassName)
            return SyntaxFactory.IdentifierName(_newClassName).WithTriviaFrom(node);
        return base.VisitIdentifierName(node);
    }

    public override SyntaxNode? VisitGenericName(GenericNameSyntax node)
    {
        if (node.Identifier.Text == _oldClassName)
        {
            var args = node.TypeArgumentList.Arguments;
            if (args.Count == _typeMap.Count)
            {
                bool matches = true;
                var typeParams = _typeMap.Keys.ToList();
                for (int i = 0; i < args.Count && matches; i++)
                {
                    if (_typeMap.TryGetValue(typeParams[i], out var expected))
                        matches = args[i].ToString().Trim() == expected;
                }
                if (matches)
                    return SyntaxFactory.IdentifierName(_newClassName).WithTriviaFrom(node);
            }
        }
        return base.VisitGenericName(node);
    }

    public override SyntaxNode? VisitArrayType(ArrayTypeSyntax node)
    {
        if (node.ElementType is IdentifierNameSyntax idType
            && _typeMap.TryGetValue(idType.Identifier.Text, out var concrete))
            return node.WithElementType(
                SyntaxFactory.IdentifierName(concrete).WithTriviaFrom(idType));
        return base.VisitArrayType(node);
    }

    public override SyntaxNode? VisitNullableType(NullableTypeSyntax node)
    {
        if (node.ElementType is IdentifierNameSyntax idType
            && _typeMap.TryGetValue(idType.Identifier.Text, out var concrete))
            return node.WithElementType(
                SyntaxFactory.IdentifierName(concrete).WithTriviaFrom(idType));
        return base.VisitNullableType(node);
    }

    public override SyntaxNode? VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        if (node.Type is GenericNameSyntax gn && gn.Identifier.Text == _oldClassName)
            return node.WithType(
                SyntaxFactory.IdentifierName(_newClassName).WithTriviaFrom(node.Type));
        return base.VisitObjectCreationExpression(node);
    }

    public override SyntaxNode? VisitTypeOfExpression(TypeOfExpressionSyntax node)
    {
        if (node.Type is IdentifierNameSyntax idType
            && _typeMap.TryGetValue(idType.Identifier.Text, out var concrete))
            return node.WithType(
                SyntaxFactory.IdentifierName(concrete).WithTriviaFrom(idType));
        return base.VisitTypeOfExpression(node);
    }
}