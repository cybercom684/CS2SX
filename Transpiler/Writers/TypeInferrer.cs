using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Writers;

/// <summary>
/// Leitet den C#-Typ eines Ausdrucks her ohne Roslyn-Semantic-Model.
/// Wird für Append-Dispatch, Format-Specifier und Pointer-Entscheidungen genutzt.
/// </summary>
public static class TypeInferrer
{
    /// <summary>
    /// Gibt den C#-Typ des Ausdrucks zurück (best-effort ohne Semantic-Model).
    /// </summary>
    public static string InferCSharpType(Microsoft.CodeAnalysis.SyntaxNode? expr, TranspilerContext ctx)
    {
        if (expr == null) return "int";

        switch (expr)
        {
            case LiteralExpressionSyntax lit:
                if (lit.Token.Value is int) return "int";
                if (lit.Token.Value is uint) return "uint";
                if (lit.Token.Value is long) return "long";
                if (lit.Token.Value is ulong) return "ulong";
                if (lit.Token.Value is float) return "float";
                if (lit.Token.Value is double) return "double";
                if (lit.Token.Value is char) return "char";
                if (lit.Token.Value is string) return "string";
                if (lit.Token.Value is bool) return "bool";
                return "int";

            case IdentifierNameSyntax id:
                {
                    var name = id.Identifier.Text;
                    var trimmed = name.TrimStart('_');
                    if (ctx.LocalTypes.TryGetValue(name, out var lt)) return lt;
                    if (ctx.FieldTypes.TryGetValue(trimmed, out var ft)) return ft;
                    return "int";
                }

            case CastExpressionSyntax cast:
                return cast.Type.ToString().Trim();

            case MemberAccessExpressionSyntax mem:
                {
                    var prop = mem.Name.Identifier.Text;
                    var objKey = mem.Expression.ToString().TrimStart('_');
                    if (ctx.FieldTypes.TryGetValue(objKey, out var ft)) return ft;
                    if (prop is "Count" or "Length") return "int";
                    return "int";
                }

            case InvocationExpressionSyntax inv:
                {
                    var callee = inv.Expression.ToString();
                    if (callee.Contains("Get") || callee.Contains("Count")) return "int";
                    if (callee.Contains("ToString")) return "string";
                    return "int";
                }

            case BinaryExpressionSyntax:
            case PrefixUnaryExpressionSyntax:
            case PostfixUnaryExpressionSyntax:
                return "int";

            default:
                return "int";
        }
    }

    /// <summary>
    /// Gibt den printf Format-Specifier für einen Ausdruck zurück.
    /// </summary>
    public static string FormatSpecifier(Microsoft.CodeAnalysis.SyntaxNode? expr, TranspilerContext ctx)
    {
        // Direkter Check für .Count und .Length → immer %d
        if (expr is MemberAccessExpressionSyntax mem2)
        {
            var prop = mem2.Name.Identifier.Text;
            if (prop is "Count" or "Length" or "count" or "length")
                return "%d";
        }

        var csType = InferCSharpType(expr, ctx);
        var cType = TypeRegistry.MapType(csType);
        return TypeRegistry.FormatSpecifier(cType);
    }
}