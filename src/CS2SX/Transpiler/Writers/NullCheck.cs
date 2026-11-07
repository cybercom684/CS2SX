using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Writers;

/// <summary>
/// `x.Member` on a null reference throws NullReferenceException in C#; in C it would
/// dereference NULL and crash the console. Receivers of member access, method calls
/// and string operations are therefore written as <c>CS2SX_NN(x)</c> (cs2sx_exc.h:
/// evaluates x once, throws when it is NULL, otherwise yields x).
///
/// Skipped where the receiver cannot be null, so hot code pays nothing:
/// `this`/`base`, `new …`, literals and interpolated strings, and locals whose every
/// assignment is one of those. Value types never need it.
/// </summary>
internal static class NullCheck
{
    public static string Wrap(ExpressionSyntax receiver, string code, TranspilerContext ctx) =>
        Needed(receiver, ctx) ? "CS2SX_NN(" + code + ")" : code;

    public static bool Needed(ExpressionSyntax e, TranspilerContext ctx)
    {
        var model = ctx.SemanticModel;
        if (model == null) return false;
        while (true)
        {
            if (e is ParenthesizedExpressionSyntax p) e = p.Expression;
            else if (e is PostfixUnaryExpressionSyntax f && f.IsKind(SyntaxKind.SuppressNullableWarningExpression))
                e = f.Operand;   // x! asserts non-null at compile time only — still checked
            else break;
        }
        if (IsNeverNull(e)) return false;
        try
        {
            var sym = model.GetSymbolInfo(e).Symbol;
            if (sym is ITypeSymbol or INamespaceSymbol) return false;   // Type.StaticMember
            var type = model.GetTypeInfo(e).Type;
            if (type == null || type is IErrorTypeSymbol || !type.IsReferenceType) return false;
            if (type.TypeKind == TypeKind.TypeParameter) return false;
            if (sym is ILocalSymbol local && LocalIsNeverNull(local, ctx)) return false;
            return true;
        }
        catch { return false; }
    }

    private static bool IsNeverNull(ExpressionSyntax e) => e
        is ThisExpressionSyntax or BaseExpressionSyntax
        or ObjectCreationExpressionSyntax or ImplicitObjectCreationExpressionSyntax
        or ArrayCreationExpressionSyntax or ImplicitArrayCreationExpressionSyntax
        or InterpolatedStringExpressionSyntax
        or LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression };

    /// A local that only ever receives non-null values (`var p = new P();` and no
    /// other assignment, or only such assignments).
    private static bool LocalIsNeverNull(ILocalSymbol local, TranspilerContext ctx)
    {
        if (ctx.NeverNullLocals.TryGetValue(local, out var cached)) return cached;
        bool result = false;
        var decl = local.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as VariableDeclaratorSyntax;
        var body = decl?.Ancestors().FirstOrDefault(a => a is BaseMethodDeclarationSyntax
            or AccessorDeclarationSyntax or LocalFunctionStatementSyntax or AnonymousFunctionExpressionSyntax);
        if (decl?.Initializer != null && body != null && decl.Parent?.Parent is LocalDeclarationStatementSyntax
            && IsNeverNull(decl.Initializer.Value))
        {
            result = true;
            foreach (var id in body.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (id.Identifier.Text != local.Name) continue;
                bool written = id.Parent is AssignmentExpressionSyntax a && a.Left == id
                    || id.Parent is ArgumentSyntax arg && !arg.RefKindKeyword.IsKind(SyntaxKind.None);
                if (!written) continue;
                if (!SymbolEqualityComparer.Default.Equals(ctx.SemanticModel!.GetSymbolInfo(id).Symbol, local))
                    continue;
                if (id.Parent is AssignmentExpressionSyntax asg
                    && asg.IsKind(SyntaxKind.SimpleAssignmentExpression) && IsNeverNull(asg.Right))
                    continue;
                if (id.Parent is AssignmentExpressionSyntax add && add.IsKind(SyntaxKind.AddAssignmentExpression)
                    && local.Type.SpecialType == SpecialType.System_String)
                    continue;   // s += x stays a non-null string
                result = false;
                break;
            }
        }
        ctx.NeverNullLocals[local] = result;
        return result;
    }
}
