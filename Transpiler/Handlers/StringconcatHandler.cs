using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt String-Konkatenation mit + Operator.
///
/// "Score: " + score.ToString()  → snprintf(_sb0, sizeof(_sb0), "Score: %d", score)
/// "Hi " + name + "!"            → snprintf(_sb0, sizeof(_sb0), "Hi %s!", name)
///
/// Wird von ExpressionWriter aufgerufen wenn ein BinaryExpression mit +
/// einen String-Operanden hat.
/// </summary>
public static class StringConcatFixer
{
    /// <summary>
    /// Versucht eine String-Konkatenation zu einem snprintf-Puffer aufzulösen.
    /// Gibt null zurück wenn der Ausdruck keine String-Konkat ist.
    /// </summary>
    public static string? TryBuildConcat(
        BinaryExpressionSyntax bin,
        TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr)
    {
        if (!bin.IsKind(SyntaxKind.AddExpression))
            return null;

        // Flatten: sammle alle + Operanden
        var parts = new List<SyntaxNode>();
        CollectAddParts(bin, parts);

        // Prüfen ob mindestens ein Operand ein String ist
        bool hasString = parts.Any(p => IsStringPart(p, ctx));
        if (!hasString)
            return null;

        // Format-String und Argumente aufbauen
        var fmt = new System.Text.StringBuilder();
        var args = new List<string>();

        foreach (var part in parts)
        {
            if (part is LiteralExpressionSyntax lit
                && lit.Token.IsKind(SyntaxKind.StringLiteralToken))
            {
                fmt.Append(StringEscaper.EscapeFormat(lit.Token.ValueText));
            }
            else if (part is InterpolatedStringExpressionSyntax interp)
            {
                var (interpFmt, interpArgs) = FormatStringBuilder.Build(interp, ctx, writeExpr);
                fmt.Append(interpFmt);
                args.AddRange(interpArgs);
            }
            else
            {
                var csType = TypeInferrer.InferCSharpType(part, ctx);
                var cType = TypeRegistry.MapType(csType);
                fmt.Append(TypeRegistry.FormatSpecifier(cType));
                args.Add(writeExpr(part));
            }
        }

        if (args.Count == 0)
            return "\"" + fmt + "\"";

        var buf = ctx.NextStringBuf();
        ctx.Out.WriteLine(ctx.Tab
            + "snprintf(" + buf + ", sizeof(" + buf + "), \""
            + fmt + "\", " + string.Join(", ", args) + ");");
        return buf;
    }

    private static void CollectAddParts(SyntaxNode node, List<SyntaxNode> result)
    {
        if (node is BinaryExpressionSyntax bin
            && bin.IsKind(SyntaxKind.AddExpression))
        {
            CollectAddParts(bin.Left, result);
            CollectAddParts(bin.Right, result);
        }
        else
        {
            result.Add(node);
        }
    }

    private static bool IsStringPart(SyntaxNode node, TranspilerContext ctx)
    {
        if (node is LiteralExpressionSyntax lit
            && lit.Token.IsKind(SyntaxKind.StringLiteralToken))
            return true;

        if (node is InterpolatedStringExpressionSyntax)
            return true;

        var t = TypeInferrer.InferCSharpType(node, ctx);
        return t == "string";
    }
}