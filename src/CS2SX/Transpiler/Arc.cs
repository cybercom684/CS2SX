using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler;

/// Reference counting for user classes.
///
/// Every heap object of a user class hierarchy carries `_rc` and `_dtor` in its root
/// struct. `T_Free` releases one reference; at zero it runs the dynamic type's
/// `_Dispose` (which releases the object's own fields, then its base's) and frees the
/// memory. Fields and collections own one reference each: storing a value that is not
/// freshly created retains it (see <see cref="RetainIfShared"/>).
///
/// Classes that derive from runtime types (SwitchApp, controls, exceptions), generic
/// classes and structs keep their own lifetime handling and are not part of this.
internal static class Arc
{
    /// Root class name of the reference-counted hierarchy of <paramref name="t"/>,
    /// or null when <paramref name="t"/> is not such a class.
    public static string? Root(ITypeSymbol? t)
    {
        if (t is not INamedTypeSymbol n || n.TypeKind != TypeKind.Class || n.IsStatic
            || n.IsGenericType || !TranspilerContext.IsUserCode(n))
            return null;
        // Registered per project from syntax before transpiling (ProjectTypeScanner),
        // so headers, _generics.h and the C files agree.
        return TypeRegistry.ArcRoot(n.Name);
    }

    /// True when the class takes part in the `_Dispose`/`_dtor` scheme (records keep
    /// their own self-contained Free).
    public static bool UsesDispose(INamedTypeSymbol? sym) =>
        sym != null && !sym.IsRecord && Root(sym) != null;

    /// A value that already owns its reference (+1): `new T(..)`, null, default.
    public static bool IsFresh(ExpressionSyntax e)
    {
        while (true)
        {
            switch (e)
            {
                case ParenthesizedExpressionSyntax p: e = p.Expression; continue;
                case CastExpressionSyntax c: e = c.Expression; continue;
                case PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.SuppressNullableWarningExpression } s:
                    e = s.Operand; continue;
            }
            return e is BaseObjectCreationExpressionSyntax
                || e.IsKind(SyntaxKind.NullLiteralExpression)
                || e.IsKind(SyntaxKind.DefaultLiteralExpression)
                || e is DefaultExpressionSyntax;
        }
    }

    /// Wraps <paramref name="code"/> (the C for <paramref name="value"/>) so that the
    /// store takes its own reference. Returns <paramref name="code"/> unchanged when the
    /// value is fresh or not a reference-counted class. The result keeps the C type.
    public static string RetainIfShared(ExpressionSyntax value, string code, TranspilerContext ctx)
    {
        if (IsFresh(value) || ctx.SemanticModel == null) return code;
        ITypeSymbol? t;
        try { t = ctx.SemanticModel.GetTypeInfo(value).Type; }
        catch { return code; }
        var root = Root(t);
        return root == null ? code : $"CS2SX_RETAIN({root}, {code})";
    }
}
