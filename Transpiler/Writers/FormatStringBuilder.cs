using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Writers;

public static class FormatStringBuilder
{
    public static string BuildPrintf(
        InterpolatedStringExpressionSyntax interp,
        bool newline,
        TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        var (fmt, args) = Build(interp, ctx, writeExpr);
        var nl = newline ? "\\n" : "";
        if (args.Count == 0)
            return "printf(\"" + fmt + nl + "\")";
        return "printf(\"" + fmt + nl + "\", " + string.Join(", ", args) + ")";
    }

    /// <summary>
    /// FIX: BuildToBuffer nutzt CurrentReturnBuffer wenn gesetzt.
    /// Damit wird bei return $"..." ein statischer Puffer verwendet statt
    /// eines stack-lokalen snprintf-Puffers (verhindert -Wreturn-local-addr).
    /// </summary>
    public static string BuildToBuffer(
        InterpolatedStringExpressionSyntax interp,
        TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        var (fmt, args) = Build(interp, ctx, writeExpr);

        if (args.Count == 0)
            return "\"" + fmt + "\"";

        // FIX: statischen Return-Buffer nutzen wenn vorhanden
        string buf;
        if (!string.IsNullOrEmpty(ctx.CurrentReturnBuffer))
        {
            buf = ctx.CurrentReturnBuffer;
            // Kein WriteLine für den Buffer-Decl nötig — bereits deklariert
            ctx.Out.WriteLine(ctx.Tab
                + "snprintf(" + buf + ", CS2SX_RETURN_BUF_SIZE, \""
                + fmt + "\", " + string.Join(", ", args) + ");");
        }
        else
        {
            buf = ctx.NextStringBuf();
            ctx.Out.WriteLine(ctx.Tab
                + "snprintf(" + buf + ", sizeof(" + buf + "), \""
                + fmt + "\", " + string.Join(", ", args) + ");");
        }

        return buf;
    }

    public static string BuildLabelSetText(
        string labelExpr,
        InterpolatedStringExpressionSyntax interp,
        TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        var (fmt, args) = Build(interp, ctx, writeExpr);
        if (args.Count == 0)
            return "Label_SetText(" + labelExpr + ", \"" + fmt + "\")";
        var buf = ctx.NextStringBuf();
        return "snprintf(" + buf + ", sizeof(" + buf + "), \""
             + fmt + "\", " + string.Join(", ", args) + ");\n"
             + new string(' ', 4) + "Label_SetText(" + labelExpr + ", " + buf + ")";
    }

    public static string BuildSnprintf(string bufName, string fmt, IReadOnlyList<string> args)
    {
        if (args.Count == 0) return "\"" + fmt + "\"";
        return "snprintf(" + bufName + ", sizeof(" + bufName + "), \""
             + fmt + "\", " + string.Join(", ", args) + ")";
    }

    public static (string fmt, List<string> args) Build(
        InterpolatedStringExpressionSyntax interp,
        TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        var fmt = new System.Text.StringBuilder();
        var args = new List<string>();

        foreach (var part in interp.Contents)
        {
            if (part is InterpolatedStringTextSyntax text)
                fmt.Append(StringEscaper.EscapeFormat(text.TextToken.ValueText));
            else if (part is InterpolationSyntax hole)
            {
                fmt.Append(TypeInferrer.FormatSpecifier(hole.Expression, ctx));
                args.Add(writeExpr(hole.Expression));
            }
        }
        return (fmt.ToString(), args);
    }
}