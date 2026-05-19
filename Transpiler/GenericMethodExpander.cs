// ============================================================================
// CS2SX — Transpiler/GenericMethodExpander.cs
//
// Specializes generic methods at call sites.
//
// C#:  static T Clamp<T>(T val, T min, T max) where T : IComparable<T> { ... }
//      Clamp(speed, 0f, 100f)
//
// C:   static float MyClass_Clamp_float(float val, float min, float max) { ... }
//      MyClass_Clamp_float(speed, 0.0f, 100.0f)
//
// Strategy:
//   1. GenericInstantiationCollector already captures generic method calls.
//   2. For each call site with concrete type args (or inferred types), we
//      synthesize a concrete method name and, if not yet emitted, emit the body.
// ============================================================================

using CS2SX.Core;
using CS2SX.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler;

public sealed class GenericMethodExpander
{
    private readonly GenericInstantiationCollector _collector;
    // Track which specializations have already been emitted
    private readonly HashSet<string> _emitted = new(StringComparer.Ordinal);

    public GenericMethodExpander(GenericInstantiationCollector collector)
    {
        _collector = collector;
    }

    /// <summary>
    /// Tries to resolve a generic method call to a concrete C function name.
    /// Returns null if the method is not a known generic method.
    /// If the specialization hasn't been emitted yet, queues it in pendingSpecializations.
    /// </summary>
    public string? TryResolve(
        string className,
        string methodName,
        IReadOnlyList<string> typeArgs,
        out string? specializationCode)
    {
        specializationCode = null;
        var key = className + "." + methodName;
        if (!_collector.GenericMethods.TryGetValue(key, out var methodDef))
            return null;

        if (typeArgs.Count == 0) return null;

        var suffix = string.Join("_", typeArgs.Select(GenericInstantiation.MapToCSuffix));
        var cFuncName = className + "_" + methodName + "_" + suffix;

        if (_emitted.Add(cFuncName))
        {
            // Generate the specialization
            try
            {
                specializationCode = SpecializeMethod(methodDef, className, methodName,
                    suffix, typeArgs);
                Log.Debug($"GenericMethodExpander: Specialized {cFuncName}");
            }
            catch (Exception ex)
            {
                Log.Warning($"GenericMethodExpander: Failed to specialize {cFuncName}: {ex.Message}");
                specializationCode = $"/* specialization of {cFuncName} failed: {ex.Message} */\n";
            }
        }

        return cFuncName;
    }

    private string SpecializeMethod(
        MethodDeclarationSyntax method,
        string className,
        string methodName,
        string suffix,
        IReadOnlyList<string> typeArgs)
    {
        var typeParams = method.TypeParameterList?.Parameters.ToList()
                         ?? new List<TypeParameterSyntax>();

        var typeMap = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int i = 0; i < typeParams.Count && i < typeArgs.Count; i++)
            typeMap[typeParams[i].Identifier.Text] = typeArgs[i];

        // Rewrite the method body substituting type parameters
        var rewriter = new TypeParameterRewriter(typeMap,
            className + "_" + methodName + "_" + suffix,
            className + "_" + methodName);

        // Wrap method in a class so the rewriter can find it
        var wrappedSource = $"class {className} {{ {method.NormalizeWhitespace().ToFullString()} }}";
        var tree = CSharpSyntaxTree.ParseText(wrappedSource,
            new CSharpParseOptions(LanguageVersion.CSharp12));
        var newRoot = rewriter.Visit(tree.GetRoot());

        var dummyCollector = new GenericInstantiationCollector();
        var dummyExpander = new InterfaceExpander(dummyCollector);

        var cTranspiler = new CSharpToC(
            CSharpToC.TranspileMode.Implementation,
            dummyCollector,
            dummyExpander);

        var result = cTranspiler.Transpile(
            newRoot.NormalizeWhitespace().ToFullString(),
            filePath: $"<generic_method:{className}.{methodName}<{suffix}>>",
            semanticModel: null);

        return result.Code;
    }
}