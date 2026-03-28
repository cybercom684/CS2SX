using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class StringMethodHandler : InvocationHandlerBase
{
    private static readonly HashSet<string> s_staticMethods = new(StringComparer.Ordinal)
    {
        "string.IsNullOrEmpty", "string.IsNullOrWhiteSpace",
        "string.Contains",      "string.StartsWith",
        "string.EndsWith",      "string.Format",
        "string.Concat",        "string.Join",
        "string.Split",
        "String.IsNullOrEmpty", "String.IsNullOrWhiteSpace",
        "String.Format",        "String.Concat",
        "String.Join",          "String.Split",
    };

    private static readonly HashSet<string> s_instanceMethods = new(StringComparer.Ordinal)
    {
        "Contains", "StartsWith", "EndsWith", "Equals",
        "ToString", "Trim", "TrimStart", "TrimEnd",
        "ToUpper", "ToLower", "Replace", "Substring",
        "IndexOf", "LastIndexOf", "PadLeft", "PadRight",
        "Split", "CompareTo",
    };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (s_staticMethods.Contains(calleeStr))
        {
            result = HandleStatic(inv, calleeStr, args, ctx, writeExpr);
            return true;
        }

        if (inv.Expression is MemberAccessExpressionSyntax mem
            && s_instanceMethods.Contains(mem.Name.Identifier.Text))
        {
            var objStr = mem.Expression.ToString();
            var type = LookupType(objStr, ctx);

            if (!TypeRegistry.IsStringBuilder(type ?? "")
             && !TypeRegistry.IsList(type ?? ""))
            {
                result = HandleInstance(inv, mem, args, ctx, writeExpr);
                return true;
            }
        }

        return NotHandled(out result);
    }

    private string HandleStatic(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx, Func<SyntaxNode?, string> writeExpr)
    {
        return calleeStr switch
        {
            "string.IsNullOrEmpty" or "String.IsNullOrEmpty"
          or "string.IsNullOrWhiteSpace" or "String.IsNullOrWhiteSpace"
                => "String_IsNullOrEmpty(" + ArgAt(args, 0) + ")",

            "string.Contains" => "String_Contains(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",
            "string.StartsWith" => "String_StartsWith(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",
            "string.EndsWith" => "String_EndsWith(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "string.Format" or "String.Format"
                => HandleStringFormat(inv, ctx, writeExpr),

            "string.Concat" or "String.Concat"
                => HandleStringConcat(args),

            "string.Join" or "String.Join"
                => "String_Join(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "string.Split" or "String.Split"
                => "String_Split(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            _ => args.Count > 0 ? args[0] : "\"\"",
        };
    }

    private string HandleInstance(InvocationExpressionSyntax inv,
        MemberAccessExpressionSyntax mem, List<string> args,
        TranspilerContext ctx, Func<SyntaxNode?, string> writeExpr)
    {
        var receiver = writeExpr(mem.Expression);
        var methodName = mem.Name.Identifier.Text;
        var recvType = TypeInferrer.InferCSharpType(mem.Expression, ctx);

        return methodName switch
        {
            "ToString" => recvType switch
            {
                "uint" or "u32" => "UInt_ToString((unsigned int)" + receiver + ")",
                "float" => "Float_ToString(" + receiver + ")",
                _ => "Int_ToString((int)" + receiver + ")",
            },
            "Contains" => "String_Contains(" + receiver + ", " + ArgAt(args, 0) + ")",
            "StartsWith" => "String_StartsWith(" + receiver + ", " + ArgAt(args, 0) + ")",
            "EndsWith" => "String_EndsWith(" + receiver + ", " + ArgAt(args, 0) + ")",
            "Equals" => "strcmp(" + receiver + ", " + ArgAt(args, 0) + ") == 0",
            "Trim" => "String_Trim(" + receiver + ")",
            "TrimStart" => "String_TrimStart(" + receiver + ")",
            "TrimEnd" => "String_TrimEnd(" + receiver + ")",
            "ToUpper" => "String_ToUpper(" + receiver + ")",
            "ToLower" => "String_ToLower(" + receiver + ")",
            "Replace" => "String_Replace(" + receiver + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",
            "CompareTo" => "String_CompareTo(" + receiver + ", " + ArgAt(args, 0) + ")",
            "LastIndexOf" => "String_LastIndexOf(" + receiver + ", " + ArgAt(args, 0) + ")",
            "Split" => "String_Split(" + receiver + ", " + ArgAt(args, 0) + ")",

            "Substring" => args.Count == 1
                ? "String_SubstringFrom(" + receiver + ", " + ArgAt(args, 0) + ")"
                : "String_Substring(" + receiver + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "IndexOf" => IsCharLiteral(inv, 0)
                ? "String_IndexOfChar(" + receiver + ", " + ArgAt(args, 0) + ")"
                : "String_IndexOf(" + receiver + ", " + ArgAt(args, 0) + ")",

            "PadLeft" => args.Count == 1
                ? "String_PadLeft(" + receiver + ", " + ArgAt(args, 0) + ", ' ')"
                : "String_PadLeft(" + receiver + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "PadRight" => args.Count == 1
                ? "String_PadRight(" + receiver + ", " + ArgAt(args, 0) + ", ' ')"
                : "String_PadRight(" + receiver + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            _ => args.Count > 0 ? args[0] : "\"\"",
        };
    }

    private static bool IsCharLiteral(InvocationExpressionSyntax inv, int argIndex)
    {
        if (inv.ArgumentList.Arguments.Count <= argIndex) return false;
        return inv.ArgumentList.Arguments[argIndex].Expression
            is LiteralExpressionSyntax lit
            && lit.Token.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.CharacterLiteralToken);
    }

    private static string HandleStringFormat(InvocationExpressionSyntax inv,
        TranspilerContext ctx, Func<SyntaxNode?, string> writeExpr)
    {
        if (inv.ArgumentList.Arguments.Count == 0) return "\"\"";

        var firstArg = inv.ArgumentList.Arguments[0].Expression;
        if (firstArg is not LiteralExpressionSyntax lit
            || !lit.Token.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralToken))
            return writeExpr(firstArg);

        var template = lit.Token.ValueText;
        var formatArgs = inv.ArgumentList.Arguments.Skip(1).ToList();

        if (formatArgs.Count == 0)
            return "\"" + StringEscaper.EscapeRaw(template) + "\"";

        var fmt = BuildFormatString(template, formatArgs, ctx);
        var argStr = string.Join(", ", formatArgs.Select(a => writeExpr(a.Expression)));

        ctx.Out.WriteLine(ctx.Tab
            + "snprintf(_cs2sx_strbuf, sizeof(_cs2sx_strbuf), \""
            + fmt + "\", " + argStr + ");");

        return "_cs2sx_strbuf";
    }

    private static string HandleStringConcat(List<string> args)
    {
        if (args.Count == 0) return "\"\"";
        if (args.Count == 1) return args[0];
        var fmt = string.Concat(Enumerable.Repeat("%s", args.Count));
        return "snprintf(_cs2sx_strbuf, sizeof(_cs2sx_strbuf), \""
             + fmt + "\", " + string.Join(", ", args) + "), _cs2sx_strbuf";
    }

    private static string BuildFormatString(string template,
        List<ArgumentSyntax> formatArgs, TranspilerContext ctx)
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
            else if (c == '%') { sb.Append("%%"); i++; continue; }
            else if (c == '\\') { sb.Append("\\\\"); i++; continue; }
            else if (c == '"') { sb.Append("\\\""); i++; continue; }
            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }
}