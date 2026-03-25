using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class StringMethodHandler : IInvocationHandler
{
    private static readonly HashSet<string> s_staticMethods = new(StringComparer.Ordinal)
    {
        "string.IsNullOrEmpty", "string.IsNullOrWhiteSpace",
        "string.Contains", "string.StartsWith", "string.EndsWith",
        "string.Format", "string.Concat",
        "String.IsNullOrEmpty", "String.IsNullOrWhiteSpace",
        "String.Format", "String.Concat",
    };

    private static readonly HashSet<string> s_instanceMethods = new(StringComparer.Ordinal)
    {
        "Contains", "StartsWith", "EndsWith", "Equals", "ToString",
    };

    public bool CanHandle(InvocationExpressionSyntax inv, string calleeStr, TranspilerContext ctx)
    {
        if (s_staticMethods.Contains(calleeStr)) return true;

        if (inv.Expression is MemberAccessExpressionSyntax mem
            && s_instanceMethods.Contains(mem.Name.Identifier.Text))
        {
            var objStr = mem.Expression.ToString();
            var objKey = objStr.TrimStart('_');
            string? type = null;
            ctx.LocalTypes.TryGetValue(objStr, out type);
            if (type == null) ctx.FieldTypes.TryGetValue(objKey, out type);

            if (TypeRegistry.IsStringBuilder(type ?? "") || TypeRegistry.IsList(type ?? ""))
                return false;

            return true;
        }

        return false;
    }

    public string Handle(InvocationExpressionSyntax inv, string calleeStr, List<string> args,
        TranspilerContext ctx, Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        switch (calleeStr)
        {
            case "string.IsNullOrEmpty":
            case "string.IsNullOrWhiteSpace":
            case "String.IsNullOrEmpty":
            case "String.IsNullOrWhiteSpace":
                return "String_IsNullOrEmpty(" + (args.Count > 0 ? args[0] : "\"\"") + ")";

            case "string.Contains":
                return "String_Contains("
                     + args[0] + ", " + (args.Count > 1 ? args[1] : "\"\"") + ")";

            case "string.StartsWith":
                return "String_StartsWith("
                     + args[0] + ", " + (args.Count > 1 ? args[1] : "\"\"") + ")";

            case "string.EndsWith":
                return "String_EndsWith("
                     + args[0] + ", " + (args.Count > 1 ? args[1] : "\"\"") + ")";

            case "string.Format":
            case "String.Format":
                return HandleStringFormat(inv, ctx, writeExpr);

            case "string.Concat":
            case "String.Concat":
                return HandleStringConcat(args);
        }

        // Instanz-Methoden
        if (inv.Expression is MemberAccessExpressionSyntax mem)
        {
            var receiver = writeExpr(mem.Expression);
            var methodName = mem.Name.Identifier.Text;
            var receiverType = TypeInferrer.InferCSharpType(mem.Expression, ctx);

            switch (methodName)
            {
                case "ToString":
                    return receiverType switch
                    {
                        "uint" or "u32" => "UInt_ToString((unsigned int)" + receiver + ")",
                        "float" => "Float_ToString(" + receiver + ")",
                        _ => "Int_ToString((int)" + receiver + ")",
                    };

                case "Contains":
                    return "String_Contains("
                         + receiver + ", " + (args.Count > 0 ? args[0] : "\"\"") + ")";

                case "StartsWith":
                    return "String_StartsWith("
                         + receiver + ", " + (args.Count > 0 ? args[0] : "\"\"") + ")";

                case "EndsWith":
                    return "String_EndsWith("
                         + receiver + ", " + (args.Count > 0 ? args[0] : "\"\"") + ")";

                case "Equals":
                    return "strcmp("
                         + receiver + ", " + (args.Count > 0 ? args[0] : "\"\"") + ") == 0";
            }
        }

        return args.Count > 0 ? args[0] : "\"\"";
    }

    // string.Format("template {0} {1}", a, b)
    // Gibt einen snprintf-Aufruf zurück der _cs2sx_strbuf befüllt.
    // Verwendet KEIN Komma-Operator — stattdessen wird ein separates
    // snprintf-Statement via ctx.Out emittiert und der Ausdruck selbst
    // ist nur _cs2sx_strbuf. Das funktioniert nur wenn der Kontext
    // ein Statement ist (Zuweisung, Argument, Label_SetText etc.).
    // Für Zuweisungen wie: const char* x = string.Format(...)
    // emittieren wir das snprintf davor und geben _cs2sx_strbuf zurück.
    private static string HandleStringFormat(
    InvocationExpressionSyntax inv,
    TranspilerContext ctx,
    Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        if (inv.ArgumentList.Arguments.Count == 0) return "\"\"";

        var firstArgExpr = inv.ArgumentList.Arguments[0].Expression;

        if (firstArgExpr is not LiteralExpressionSyntax lit
            || !lit.Token.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralToken))
            return writeExpr(firstArgExpr);

        var template = lit.Token.ValueText;
        var formatArgs = inv.ArgumentList.Arguments.Skip(1).ToList();

        if (formatArgs.Count == 0)
            return "\"" + StringEscaper.EscapeRaw(template) + "\"";

        var fmt = BuildFormatString(template, formatArgs, ctx);
        var argStr = string.Join(", ", formatArgs.Select(a => writeExpr(a.Expression)));

        // snprintf als eigenes Statement VORHER ausgeben
        ctx.Out.WriteLine(ctx.Tab
            + "snprintf(_cs2sx_strbuf, sizeof(_cs2sx_strbuf), \""
            + fmt + "\", " + argStr + ");");

        // nur _cs2sx_strbuf als Ausdruck zurückgeben — kein Komma-Operator
        return "_cs2sx_strbuf";
    }

    // string.Concat(a, b, c) → snprintf mit mehreren %s
    private static string HandleStringConcat(List<string> args)
    {
        if (args.Count == 0) return "\"\"";
        if (args.Count == 1) return args[0];

        var fmt = string.Concat(Enumerable.Repeat("%s", args.Count));
        return "snprintf(_cs2sx_strbuf, sizeof(_cs2sx_strbuf), \""
             + fmt + "\", " + string.Join(", ", args) + "), _cs2sx_strbuf";
    }

    private static string BuildFormatString(
        string template,
        List<ArgumentSyntax> formatArgs,
        TranspilerContext ctx)
    {
        var sb = new System.Text.StringBuilder();
        int i = 0;
        while (i < template.Length)
        {
            char c = template[i];

            if (c == '{' && i + 1 < template.Length)
            {
                if (template[i + 1] == '{') { sb.Append('{'); i += 2; continue; }

                int close = template.IndexOf('}', i);
                if (close > i)
                {
                    var inner = template.Substring(i + 1, close - i - 1);
                    var colonIdx = inner.IndexOf(':');
                    var idxStr = colonIdx >= 0 ? inner[..colonIdx] : inner;
                    if (int.TryParse(idxStr, out var argIdx) && argIdx < formatArgs.Count)
                        sb.Append(TypeInferrer.FormatSpecifier(formatArgs[argIdx].Expression, ctx));
                    else
                        sb.Append("%s");
                    i = close + 1;
                    continue;
                }
            }
            else if (c == '}' && i + 1 < template.Length && template[i + 1] == '}')
            {
                sb.Append('}'); i += 2; continue;
            }
            else if (c == '%')
            {
                sb.Append("%%"); i++; continue;
            }
            else if (c == '\\')
            {
                sb.Append("\\\\"); i++; continue;
            }
            else if (c == '"')
            {
                sb.Append("\\\""); i++; continue;
            }

            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }
}