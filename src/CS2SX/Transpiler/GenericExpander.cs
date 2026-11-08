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

    /// Stack/Queue/HashSet/Dictionary over user types (CS2SX_*_DEFINE_EX). Elements of
    /// reference-counted classes are owned by the collection (CS2SX_RC_HOOKS); string
    /// keys and values are kept as heap copies.
    private void WriteUserCollections(System.Text.StringBuilder sb)
    {
        if (_collector.UserCollectionInstantiations.Count == 0) return;
        sb.AppendLine();
        sb.AppendLine("// ── Stack / Queue / HashSet / Dictionary over user types ─────────────");

        // (name suffix, C element type, keep hook, drop hook) — null when unsupported
        (string name, string cType, string keep, string drop)? Element(string cs)
        {
            if (cs == "string") return ("str", "const char*", "CS2SX_STR_KEEP", "CS2SX_STR_DROP");
            if (TypeRegistry.IsPrimitive(cs) || TypeRegistry.IsUserDefinedEnum(cs))
            {
                var c = TypeRegistry.MapType(cs);
                return (c.Replace(" ", "_"), c, "CS2SX_ID", "CS2SX_NOREF");
            }
            if (TypeRegistry.IsUserStruct(cs)) return null;   // needs the full struct definition
            var cls = TypeRegistry.MapType(cs);
            var root = TypeRegistry.ArcRoot(cs);
            if (root == null) return (cls, cls + "*", "CS2SX_ID", "CS2SX_NOREF");
            var guard = "CS2SX_RCHOOKS_" + cls;
            sb.AppendLine($"#ifndef {guard}");
            sb.AppendLine($"#define {guard}");
            sb.AppendLine($"CS2SX_RC_HOOKS({cls}, {root})");
            sb.AppendLine("#endif");
            return (cls, cls + "*", cls + "_RcKeep", cls + "_RcDrop");
        }

        foreach (var (kind, key, elem) in _collector.UserCollectionInstantiations
                     .OrderBy(c => c.Kind).ThenBy(c => c.Key).ThenBy(c => c.Elem))
        {
            if (Element(elem) is not { } e) continue;
            if (kind == "Dictionary")
            {
                if (Element(key!) is not { } k) continue;
                var name = $"Dict_{k.name}_{e.name}";
                var isStrKey = key == "string";
                sb.AppendLine($"#ifndef CS2SX_{name}_DEFINED");
                sb.AppendLine($"#define CS2SX_{name}_DEFINED");
                sb.AppendLine($"CS2SX_DICT_DEFINE_EX({k.name}, {k.cType}, {e.name}, {e.cType}, {k.keep}, {k.drop}, {e.keep}, {e.drop}, "
                            + (isStrKey ? "CS2SX_EQ_STR, CS2SX_KEYCHECK_STR)" : "CS2SX_EQ, CS2SX_NOCHECK)"));
                sb.AppendLine("#endif");
            }
            else
            {
                var macro = kind switch { "Stack" => "STACK", "Queue" => "QUEUE", _ => "HASHSET" };
                var prefix = kind == "HashSet" ? "HashSet" : kind;
                sb.AppendLine($"#ifndef CS2SX_{prefix}_{e.name}_DEFINED");
                sb.AppendLine($"#define CS2SX_{prefix}_{e.name}_DEFINED");
                sb.AppendLine($"CS2SX_{macro}_DEFINE_EX({e.name}, {e.cType}, {e.keep}, {e.drop})");
                sb.AppendLine("#endif");
            }
        }
    }

    public (string headerPath, string implPath) WriteToFiles(string buildDir, IReadOnlyList<string>? userSources = null)
    {
        if (_collector.Instantiations.Count == 0
            && _collector.ListOfUserClassInstantiations.Count == 0
            && _collector.ListOfDelegateInstantiations.Count == 0
            && _collector.UserCollectionInstantiations.Count == 0)
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
                // With the user's sources, so the type arguments (Box<Node> → Node) resolve.
                sharedSemanticBuilder = new SemanticModelBuilder(
                    allGenericSources.Concat(userSources ?? Array.Empty<string>()).ToList());
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

        // CS2SX_LIST_DEFINE_PTR für alle List<UserClass>-Nutzungen emittieren
        if (_collector.ListOfUserClassInstantiations.Count > 0)
        {
            headerSb.AppendLine();
            headerSb.AppendLine("// ── List<UserClass> Instantiierungen ─────────────────────────────────");
            foreach (var userClass in _collector.ListOfUserClassInstantiations.OrderBy(s => s))
            {
                // For tuple struct types (_Tuple2_*): emit struct definition first
                if (userClass.StartsWith("_Tuple"))
                {
                    var tupleStructDef = TypeRegistry.GetTupleStructDefinition(userClass);
                    if (!string.IsNullOrEmpty(tupleStructDef))
                    {
                        var tupleGuard = "CS2SX_TUPLE_" + userClass + "_DEFINED";
                        headerSb.AppendLine($"#ifndef {tupleGuard}");
                        headerSb.AppendLine($"#define {tupleGuard}");
                        headerSb.Append(tupleStructDef);
                        headerSb.AppendLine($"#endif");
                    }
                }
                var guard = "CS2SX_LIST_PTR_" + userClass + "_DEFINED";
                headerSb.AppendLine($"#ifndef {guard}");
                headerSb.AppendLine($"#define {guard}");
                var arcRoot = TypeRegistry.ArcRoot(userClass);
                headerSb.AppendLine(arcRoot != null
                    ? $"CS2SX_LIST_DEFINE_RC({userClass}, {arcRoot})"
                    : $"CS2SX_LIST_DEFINE_PTR({userClass})");
                headerSb.AppendLine($"#endif");
                Log.Info($"GenericExpander: List<{userClass}> → CS2SX_LIST_DEFINE_PTR({userClass})");
            }
        }

        foreach (var dg in _collector.ListOfDelegateInstantiations.OrderBy(s => s))
        {
            var guard = "CS2SX_LIST_" + dg + "_DEFINED";
            headerSb.AppendLine($"#ifndef {guard}");
            headerSb.AppendLine($"#define {guard}");
            headerSb.AppendLine($"CS2SX_LIST_DEFINE({dg})");
            headerSb.AppendLine("#endif");
        }

        WriteUserCollections(headerSb);

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
            // The specialised source declares the expanded class (Box_Node).
            semanticModel = semanticBuilder.GetModelForBaseName(inst.CName)
                            ?? semanticBuilder.GetModelForBaseName(inst.BaseName);
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
            // Use top-level comma scan to handle nested generics like Dictionary<string, List<int>>
            var comma = TypeRegistry.FindTopLevelComma(inner);
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