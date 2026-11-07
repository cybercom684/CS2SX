using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler;

/// <summary>
/// C names of class constructors. Every instance constructor becomes two functions:
/// <code>
///   void T_Ctor{sfx}(T* self, params)   initialises an allocated object in place —
///                                       the target of `: this(...)` and `: base(...)`
///   T*   T_New{sfx}(params)             allocates, sets refcount/vtable, calls T_Ctor
/// </code>
/// A class with a single constructor uses no suffix (<c>T_New</c>). With overloads the
/// suffix is the parameter count (<c>T_New_2</c>), plus the position among constructors
/// with the same count when that is ambiguous (<c>T_New_2_1</c>). Positions instead of
/// type names keep the names identical for generic classes after type substitution.
/// Declaration and call sites both derive the name here.
/// </summary>
internal static class CtorNaming
{
    public static List<ConstructorDeclarationSyntax> InstanceCtors(TypeDeclarationSyntax type) =>
        type.Members.OfType<ConstructorDeclarationSyntax>()
            .Where(c => !c.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            .ToList();

    public static string Suffix(IReadOnlyList<ConstructorDeclarationSyntax> ctors,
        ConstructorDeclarationSyntax? ctor)
    {
        if (ctor == null || ctors.Count <= 1) return "";
        int n = ctor.ParameterList.Parameters.Count;
        var sameCount = ctors.Where(c => c.ParameterList.Parameters.Count == n).ToList();
        return sameCount.Count == 1 ? "_" + n : "_" + n + "_" + sameCount.IndexOf(ctor);
    }

    /// Suffix for a resolved constructor symbol; "" for implicit constructors and
    /// constructors outside the project's source.
    public static string SuffixFor(IMethodSymbol? ctor)
    {
        if (ctor == null) return "";
        var syntax = ctor.OriginalDefinition.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()
            as ConstructorDeclarationSyntax;
        if (syntax?.Parent is not TypeDeclarationSyntax type) return "";
        return Suffix(InstanceCtors(type), syntax);
    }

    /// The constructor C# calls implicitly for `class D : B` without `: base(...)`:
    /// B's parameterless one (or one whose parameters are all optional).
    public static IMethodSymbol? ImplicitBaseCtor(INamedTypeSymbol? baseType) =>
        baseType?.InstanceConstructors
            .Where(c => c.Parameters.All(p => p.HasExplicitDefaultValue || p.IsParams))
            .OrderBy(c => c.Parameters.Length)
            .FirstOrDefault();
}
