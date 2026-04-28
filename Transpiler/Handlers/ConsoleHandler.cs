using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class ConsoleHandler : InvocationHandlerBase
{
    public override bool TryHandle(
        InvocationExpressionSyntax inv,
        string calleeStr,
        List<string> args,
        TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr,
        out string result)
    {
        if (calleeStr is not ("Console.Write" or "Console.WriteLine"))
            return NotHandled(out result);

        var newline = calleeStr == "Console.WriteLine";
        var nl = newline ? "\\n" : "";

        // Console.WriteLine() ohne Argument
        if (inv.ArgumentList.Arguments.Count == 0)
        {
            result = newline ? "printf(\"\\n\")" : "/* Console.Write() */";
            return true;
        }

        var firstArg = inv.ArgumentList.Arguments[0].Expression;

        // Konstantes String-Literal → direkt einbetten (kein Argument nötig)
        if (firstArg is LiteralExpressionSyntax lit
            && lit.Token.IsKind(SyntaxKind.StringLiteralToken))
        {
            var escaped = StringEscaper.EscapeFormat(lit.Token.ValueText);
            result = $"printf(\"{escaped}{nl}\")";
            return true;
        }

        // Interpolierter String → snprintf + printf
        if (firstArg is InterpolatedStringExpressionSyntax interp)
        {
            result = FormatStringBuilder.BuildPrintf(interp, newline, ctx, writeExpr);
            return true;
        }

        // Mehrere Argumente → string.Format-ähnlich
        // Console.WriteLine("{0} + {1} = {2}", a, b, c)
        if (firstArg is LiteralExpressionSyntax fmtLit
            && fmtLit.Token.IsKind(SyntaxKind.StringLiteralToken)
            && inv.ArgumentList.Arguments.Count > 1)
        {
            result = FormatStringBuilder.BuildPrintf(
                // Baue synthetisch einen InterpolatedString
                // Fallback: alles als %s ausgeben
                null!, newline, ctx, writeExpr);

            // Echter Fallback: printf mit %s-Format für jedes Argument
            var fmtArgs = inv.ArgumentList.Arguments
                .Skip(1)
                .Select(a => writeExpr(a.Expression))
                .ToList();
            var fmtStr = StringEscaper.EscapeFormat(fmtLit.Token.ValueText);
            // Ersetze {0} {1} etc. durch die korrekten Format-Specifier
            fmtStr = ReplaceDotNetPlaceholders(fmtStr, fmtArgs, inv, ctx);
            var argStr = string.Join(", ", fmtArgs);
            result = $"printf(\"{fmtStr}{nl}\", {argStr})";
            return true;
        }

        // FIX: Variable oder Ausdruck als Argument.
        //      Typ bestimmen und korrekten Format-Specifier wählen.
        //      NIEMALS printf(text) — das ist ein Format-String-Injection-Bug.
        var argExpr = writeExpr(firstArg);
        var csType = TypeInferrer.InferCSharpType(firstArg, ctx);
        var cType = TypeRegistry.MapType(csType);
        var specifier = GetFormatSpecifier(csType, cType);

        if (specifier == "%s")
        {
            // String: sicherstellen dass kein NULL übergeben wird
            result = $"printf(\"{specifier}{nl}\", ({argExpr}) ? ({argExpr}) : \"\")";
        }
        else
        {
            result = $"printf(\"{specifier}{nl}\", {argExpr})";
        }
        return true;
    }

    // ── Hilfsmethoden ──────────────────────────────────────────────────────────

    /// <summary>
    /// Ersetzt .NET-Platzhalter {0}, {1} etc. durch C-Format-Specifier.
    /// </summary>
    private static string ReplaceDotNetPlaceholders(
        string fmt,
        List<string> argExprs,
        InvocationExpressionSyntax inv,
        TranspilerContext ctx)
    {
        var sb = new System.Text.StringBuilder();
        int i = 0;
        while (i < fmt.Length)
        {
            if (fmt[i] == '{' && i + 1 < fmt.Length && fmt[i + 1] != '{')
            {
                int close = fmt.IndexOf('}', i + 1);
                if (close > i)
                {
                    var inner = fmt[(i + 1)..close];
                    var colon = inner.IndexOf(':');
                    var idxStr = colon >= 0 ? inner[..colon] : inner;

                    if (int.TryParse(idxStr.Trim(), out var idx)
                        && idx + 1 < inv.ArgumentList.Arguments.Count)
                    {
                        var argNode = inv.ArgumentList.Arguments[idx + 1].Expression;
                        var csT = TypeInferrer.InferCSharpType(argNode, ctx);
                        var cT = TypeRegistry.MapType(csT);
                        sb.Append(GetFormatSpecifier(csT, cT));
                    }
                    else sb.Append("%s");
                    i = close + 1;
                    continue;
                }
            }
            else if (fmt[i] == '{' && i + 1 < fmt.Length && fmt[i + 1] == '{')
            {
                sb.Append('{'); i += 2; continue;
            }
            else if (fmt[i] == '}' && i + 1 < fmt.Length && fmt[i + 1] == '}')
            {
                sb.Append('}'); i += 2; continue;
            }
            sb.Append(fmt[i++]);
        }
        return sb.ToString();
    }

    private static string GetFormatSpecifier(string csType, string cType)
    {
        return csType switch
        {
            "string" => "%s",
            "bool" => "%d",
            "char" => "%c",
            "float" => "%f",
            "double" => "%lf",
            "long" => "%lld",
            "ulong" => "%llu",
            "uint" => "%u",
            "short" => "%d",
            "ushort" => "%u",
            "byte" => "%u",
            "sbyte" => "%d",
            "u8" => "%u",
            "u16" => "%u",
            "u32" => "%u",
            "u64" => "%llu",
            "s8" => "%d",
            "s16" => "%d",
            "s32" => "%d",
            "s64" => "%lld",
            "int" => "%d",
            _ => TypeRegistry.FormatSpecifier(cType),
        };
    }
}