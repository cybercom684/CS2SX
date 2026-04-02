using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Writers;

/// <summary>
/// Baut printf/snprintf Format-Strings aus C# interpolierten Strings.
///
/// Änderungen:
///   • BuildPrintf und BuildLabelSetText nutzen NextStringBuf() statt
///     des globalen _cs2sx_strbuf — verhindert Buffer-Kollisionen bei
///     verschachtelten Aufrufen wie $"{a.ToString()} {b.ToString()}"
///   • Build() gibt jetzt auch die Puffer-Größe zurück (für Caller)
/// </summary>
public static class FormatStringBuilder
{
    /// <summary>
    /// Baut einen printf-Aufruf aus einem interpolierten String.
    /// Bei reinen Text-Strings (keine Holes) wird kein Puffer allokiert.
    /// </summary>
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
    /// Baut einen snprintf-Aufruf der in einen Label-Puffer schreibt.
    /// Nutzt einen lokalen Puffer statt des globalen _cs2sx_strbuf.
    /// </summary>
    public static string BuildLabelSetText(
        string labelExpr,
        InterpolatedStringExpressionSyntax interp,
        TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        var (fmt, args) = Build(interp, ctx, writeExpr);

        if (args.Count == 0)
            return "Label_SetText(" + labelExpr + ", \"" + fmt + "\")";

        // Lokalen Puffer allokieren statt globalem _cs2sx_strbuf
        var buf = ctx.NextStringBuf();
        return "snprintf(" + buf + ", sizeof(" + buf + "), \""
             + fmt + "\", " + string.Join(", ", args) + ");\n"
             + new string(' ', 4) + "Label_SetText(" + labelExpr + ", " + buf + ")";
    }

    /// <summary>
    /// Baut einen snprintf-Aufruf in einen explizit gegebenen Puffer.
    /// Wird von StringMethodHandler.HandleStringFormat verwendet.
    /// </summary>
    public static string BuildSnprintf(
        string bufName,
        string fmt,
        IReadOnlyList<string> args)
    {
        if (args.Count == 0)
            return "\"" + fmt + "\"";

        return "snprintf(" + bufName + ", sizeof(" + bufName + "), \""
             + fmt + "\", " + string.Join(", ", args) + ")";
    }

    /// <summary>
    /// Kernlogik: baut Format-String und Argument-Liste.
    /// </summary>
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
            {
                fmt.Append(StringEscaper.EscapeFormat(text.TextToken.ValueText));
            }
            else if (part is InterpolationSyntax hole)
            {
                fmt.Append(TypeInferrer.FormatSpecifier(hole.Expression, ctx));
                args.Add(writeExpr(hole.Expression));
            }
        }

        return (fmt.ToString(), args);
    }
}
