using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Writers;

/// <summary>
/// Baut printf/snprintf Format-Strings aus C# interpolierten Strings.
///
/// Änderungen:
///   • BuildToBuffer() neu: für interpolierte Strings in Argument-Position.
///     Schreibt snprintf in lokalen Puffer und gibt Puffernamen zurück.
///     Verhindert den Bug "printf() return value (int) als const char* Argument".
///
///   • BuildPrintf() bleibt für Console.WriteLine — dort ist printf() korrekt
///     weil es das eigentliche Statement ist, nicht ein Argument.
///
/// Bug-Fix:
///   $"Aktive Berührungen: {touch.count}" als Argument zu Graphics_DrawText
///   wurde zu printf(...) transpiliert → GCC: "makes pointer from integer".
///   Fix: InterpolatedStringExpressionSyntax in ExpressionWriter.Write()
///   ruft BuildToBuffer() auf, nicht BuildPrintf().
/// </summary>
public static class FormatStringBuilder
{
    /// <summary>
    /// Interpolierten String für printf (Console.WriteLine).
    /// Gibt "printf(...)" zurück — nur für Statement-Position verwenden.
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
    /// Interpolierten String in snprintf-Puffer schreiben, Puffernamen zurückgeben.
    ///
    /// FIX: Für interpolierte Strings in Argument-Position (z.B. als Parameter
    /// von Graphics_DrawText, Label.SetText etc.) muss ein const char* zurückgegeben
    /// werden — nicht der int-Rückgabewert von printf().
    ///
    /// Ohne Interpolationsausdrücke (reines Literal) wird das Literal direkt
    /// als C-String zurückgegeben, kein Puffer nötig.
    /// </summary>
    public static string BuildToBuffer(
        InterpolatedStringExpressionSyntax interp,
        TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        var (fmt, args) = Build(interp, ctx, writeExpr);

        // Kein Interpolations-Argument → reines String-Literal
        if (args.Count == 0)
            return "\"" + fmt + "\"";

        var buf = ctx.NextStringBuf();
        ctx.Out.WriteLine(ctx.Tab
            + "snprintf(" + buf + ", sizeof(" + buf + "), \""
            + fmt + "\", " + string.Join(", ", args) + ");");
        return buf;
    }

    /// <summary>
    /// Baut einen snprintf-Aufruf der in einen Label-Puffer schreibt.
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

        var buf = ctx.NextStringBuf();
        return "snprintf(" + buf + ", sizeof(" + buf + "), \""
             + fmt + "\", " + string.Join(", ", args) + ");\n"
             + new string(' ', 4) + "Label_SetText(" + labelExpr + ", " + buf + ")";
    }

    /// <summary>
    /// Baut einen snprintf-Aufruf in einen explizit gegebenen Puffer.
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