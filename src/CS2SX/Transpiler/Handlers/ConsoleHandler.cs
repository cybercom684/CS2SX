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
            && lit.Token.IsKind(SyntaxKind.StringLiteralToken)
            && inv.ArgumentList.Arguments.Count == 1)
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
            // Use the dispatcher's already-written args: writing the syntax again
            // would emit (and evaluate) every argument's prelude a second time.
            var fmtArgs = args.Skip(1).ToList();
            var fmtStr = StringEscaper.EscapeFormat(fmtLit.Token.ValueText);
            var (printfFmt, printfArgs) = ReplaceDotNetPlaceholders(fmtStr, fmtArgs, inv, ctx);
            result = printfArgs.Count > 0
                ? $"printf(\"{printfFmt}{nl}\", {string.Join(", ", printfArgs)})"
                : $"printf(\"{printfFmt}{nl}\")";
            return true;
        }

        // FIX: Variable oder Ausdruck als Argument.
        //      Typ bestimmen und korrekten Format-Specifier wählen.
        //      NIEMALS printf(text) — das ist ein Format-String-Injection-Bug.
        var argExpr = args.Count > 0 && args[0].Length > 0 ? args[0] : writeExpr(firstArg);
        var csType = TypeInferrer.InferCSharpType(firstArg, ctx);
        var cType = TypeRegistry.MapType(csType);
        var specifier = GetFormatSpecifier(csType, cType);
        (specifier, argExpr) = FormatArg.Adapt(csType, specifier, argExpr);
        if (csType is "bool" or "double" or "float")
        {
            result = $"printf(\"%s{nl}\", {argExpr})";
            return true;
        }

        if (specifier == "%s")
        {
            // String: NULL-sicher ausgeben. argExpr kann ein Funktionsaufruf sein
            // (z.B. MyClass_GetName(self)) — daher in eine temp-Variable speichern
            // statt den Ausdruck zweimal auszuwerten (Seiteneffekte / Overhead).
            var tmpVar = ctx.NextTmp("ps");
            ctx.WriteLine($"const char* {tmpVar} = {argExpr};");
            result = $"printf(\"{specifier}{nl}\", {tmpVar} ? {tmpVar} : \"\")";
        }
        else
        {
            result = $"printf(\"{specifier}{nl}\", {argExpr})";
        }
        return true;
    }

    // ── Hilfsmethoden ──────────────────────────────────────────────────────────

    /// <summary>
    /// .NET placeholders {0}, {1:F2} … → printf specifiers, plus the printf argument
    /// list in placeholder order ({1} {0} and repeated {0} work).
    /// </summary>
    private static (string Fmt, List<string> Args) ReplaceDotNetPlaceholders(
        string fmt,
        List<string> argExprs,
        InvocationExpressionSyntax inv,
        TranspilerContext ctx)
    {
        var sb = new System.Text.StringBuilder();
        var outArgs = new List<string>();
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
                    var fmtSpec = colon >= 0 ? inner[(colon + 1)..] : null;

                    if (int.TryParse(idxStr.Trim(), out var idx)
                        && idx + 1 < inv.ArgumentList.Arguments.Count && idx < argExprs.Count)
                    {
                        var argNode = inv.ArgumentList.Arguments[idx + 1].Expression;
                        var csT = TypeInferrer.InferCSharpType(argNode, ctx);
                        var cT = TypeRegistry.MapType(csT);
                        var spec = GetFormatSpecifier(csT, cT);
                        var code = argExprs[idx];
                        if (fmtSpec != null)
                            spec = StringMethodHandler.MapFormatSpecifier(fmtSpec, spec);
                        else
                            (spec, code) = FormatArg.Adapt(csT, spec, code);
                        sb.Append(spec);
                        outArgs.Add(code);
                    }
                    else sb.Append(inner);
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
        return (sb.ToString(), outArgs);
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
            "u64" => "%lu",
            "s8" => "%d",
            "s16" => "%d",
            "s32" => "%d",
            "s64" => "%ld",
            "int" => "%d",
            _ => TypeRegistry.FormatSpecifier(cType),
        };
    }
}