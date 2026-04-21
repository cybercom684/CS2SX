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
        "string.Split",         "string.Compare",
        "String.IsNullOrEmpty", "String.IsNullOrWhiteSpace",
        "String.Format",        "String.Concat",
        "String.Join",          "String.Split",
        "String.Compare",
        "String.LastIndexOf",   "string.LastIndexOf",
        "String.IndexOf",       "string.IndexOf",
        "String.Substring",     "string.Substring",
        "String.Replace",       "string.Replace",
        "String.ToUpper",       "string.ToUpper",
        "String.ToLower",       "string.ToLower",
        "String.Trim",          "string.Trim",
        "String.TrimStart",     "string.TrimStart",
        "String.TrimEnd",       "string.TrimEnd",
        "String.Contains",      "string.Contains",
        "String.StartsWith",    "string.StartsWith",
        "String.EndsWith",      "string.EndsWith",
        "String.Length",        "string.Length",
        "String.PadLeft",       "string.PadLeft",
        "String.PadRight",      "string.PadRight",
        "String.CompareTo",     "string.CompareTo",
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
            // FIX: inv wird jetzt durchgereicht
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

    // Datei: Transpiler/Handlers/StringMethodHandler.cs
    // NUR DIESE METHODE ERSETZEN (HandleStatic - Signatur ändert sich)

    private string HandleStatic(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx, Func<SyntaxNode?, string> writeExpr)
    {
        return calleeStr switch
        {
            "string.IsNullOrEmpty" or "String.IsNullOrEmpty"
          or "string.IsNullOrWhiteSpace" or "String.IsNullOrWhiteSpace"
                => "String_IsNullOrEmpty(" + ArgAt(args, 0) + ")",

            // FIX: inv wird jetzt durchgereicht damit wir den Roh-Text des 3. Args lesen können
            "string.Compare" or "String.Compare"
                => HandleStringCompare(inv, args),

            "string.LastIndexOf" or "String.LastIndexOf"
                => args.Count >= 2
                    ? "String_LastIndexOf(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")"
                    : "String_LastIndexOf(" + ArgAt(args, 0) + ", \"\")",

            "string.IndexOf" or "String.IndexOf"
                => args.Count >= 2
                    ? "String_IndexOf(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")"
                    : "String_IndexOf(" + ArgAt(args, 0) + ", \"\")",

            "string.Substring" or "String.Substring"
                => args.Count >= 3
                    ? "String_Substring(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ", " + ArgAt(args, 2) + ")"
                    : "String_SubstringFrom(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "string.ToUpper" or "String.ToUpper"
                => "String_ToUpper(" + ArgAt(args, 0) + ")",

            "string.ToLower" or "String.ToLower"
                => "String_ToLower(" + ArgAt(args, 0) + ")",

            "string.Trim" or "String.Trim"
                => "String_Trim(" + ArgAt(args, 0) + ")",

            "string.TrimStart" or "String.TrimStart"
                => "String_TrimStart(" + ArgAt(args, 0) + ")",

            "string.TrimEnd" or "String.TrimEnd"
                => "String_TrimEnd(" + ArgAt(args, 0) + ")",

            "string.Replace" or "String.Replace"
                => "String_Replace(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ", " + ArgAt(args, 2) + ")",

            "string.Contains" or "String.Contains"
                => args.Count >= 2
                    ? "String_Contains(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")"
                    : "String_Contains(" + ArgAt(args, 0) + ", \"\")",

            "string.StartsWith" or "String.StartsWith"
                => "String_StartsWith(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "string.EndsWith" or "String.EndsWith"
                => "String_EndsWith(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "string.PadLeft" or "String.PadLeft"
                => args.Count >= 3
                    ? "String_PadLeft(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ", " + ArgAt(args, 2) + ")"
                    : "String_PadLeft(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ", ' ')",

            "string.PadRight" or "String.PadRight"
                => args.Count >= 3
                    ? "String_PadRight(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ", " + ArgAt(args, 2) + ")"
                    : "String_PadRight(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ", ' ')",

            "string.CompareTo" or "String.CompareTo"
                => "String_CompareTo(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "string.Format" or "String.Format"
                => HandleStringFormat(inv, ctx, writeExpr),

            "string.Concat" or "String.Concat"
                => HandleStringConcat(args, ctx),

            "string.Join" or "String.Join"
                => "String_Join(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "string.Split" or "String.Split"
                => "String_Split(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            _ => args.Count > 0 ? args[0] : "\"\"",
        };
    }

    /// <summary>
    /// FIX: string.Compare Overload-Handling
    ///
    /// Das dritte Argument (StringComparison-Enum) wird vom Transpiler bereits als
    /// int-Ausdruck übersetzt bevor der Handler es sieht. Deshalb darf args[2] NICHT
    /// auf ".Contains(IgnoreCase)" geprüft werden — das war die alte fehlerhafte Logik.
    ///
    /// Korrekte Lösung: Das dritte Argument direkt vom originalen Syntax-Tree lesen
    /// (InvocationExpressionSyntax), BEVOR es transpiliert wurde.
    /// </summary>
    private static string HandleStringCompare(
        InvocationExpressionSyntax inv, List<string> args)
    {
        if (args.Count < 2) return "0";

        var a = args[0];
        var b = args[1];

        // FIX: Drittes Argument direkt vom Syntax-Tree lesen (nicht aus args[2]),
        // weil args[2] bereits ein transpilierter C-Ausdruck (z.B. eine Zahl) ist.
        if (inv.ArgumentList.Arguments.Count >= 3)
        {
            // Den originalen C#-Text des dritten Arguments holen
            var thirdArgRaw = inv.ArgumentList.Arguments[2].Expression.ToString();

            if (thirdArgRaw.Contains("IgnoreCase")
                || thirdArgRaw.Contains("ignoreCase")
                || thirdArgRaw.Contains("CurrentCultureIgnoreCase")
                || thirdArgRaw.Contains("InvariantCultureIgnoreCase")
                || thirdArgRaw.Contains("OrdinalIgnoreCase"))
            {
                return "String_EqualsIgnoreCase(" + a + ", " + b + ") ? 0 : strcmp(" + a + ", " + b + ")";
            }
        }

        return "String_CompareTo(" + a + ", " + b + ")";
    }

    // FIX: LastIndexOf und IndexOf mit char-Literal korrekt auf *Char-Variante routen.
    // Das Problem war dass IsCharLiteral(inv, 0) auf den falschen Index prüfte —
    // bei Instanzmethoden ist Argument 0 das erste *nach* dem Receiver, also korrekt.
    // Der eigentliche Bug: args[0] enthielt '/' (char als int), aber String_LastIndexOf
    // erwartet const char*. Die *Char-Varianten nehmen int/char direkt.

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

            // FIX: char-Literal explizit prüfen und auf *Char-Variante routen.
            // IsCharLiteral liest den Syntax-Node bevor er transpiliert wurde,
            // daher ist die Prüfung zuverlässig — auch wenn args[0] bereits '/' (int) ist.
            "LastIndexOf" => IsCharLiteralArg(inv, 0)
                ? "String_LastIndexOfChar(" + receiver + ", " + ArgAt(args, 0) + ")"
                : "String_LastIndexOf(" + receiver + ", " + ArgAt(args, 0) + ")",

            "IndexOf" => IsCharLiteralArg(inv, 0)
                ? "String_IndexOfChar(" + receiver + ", " + ArgAt(args, 0) + ")"
                : "String_IndexOf(" + receiver + ", " + ArgAt(args, 0) + ")",

            "Split" => "String_Split(" + receiver + ", " + ArgAt(args, 0) + ")",

            "Substring" => args.Count == 1
                ? "String_SubstringFrom(" + receiver + ", " + ArgAt(args, 0) + ")"
                : "String_Substring(" + receiver + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "PadLeft" => args.Count == 1
                ? "String_PadLeft(" + receiver + ", " + ArgAt(args, 0) + ", ' ')"
                : "String_PadLeft(" + receiver + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "PadRight" => args.Count == 1
                ? "String_PadRight(" + receiver + ", " + ArgAt(args, 0) + ", ' ')"
                : "String_PadRight(" + receiver + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            _ => args.Count > 0 ? args[0] : "\"\"",
        };
    }

    // FIX: Umbenannt von IsCharLiteral → IsCharLiteralArg um Verwechslungen zu vermeiden.
    // argIndex ist 0-basiert relativ zu den Argumenten der Instanzmethode
    // (d.h. Argument 0 = erstes Argument nach dem Receiver).
    private static bool IsCharLiteralArg(InvocationExpressionSyntax inv, int argIndex)
    {
        if (inv.ArgumentList.Arguments.Count <= argIndex) return false;
        return inv.ArgumentList.Arguments[argIndex].Expression
            is Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax lit
            && lit.Token.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.CharacterLiteralToken);
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
        var buf = ctx.NextStringBuf();
        ctx.Out.WriteLine(ctx.Tab
            + "snprintf(" + buf + ", sizeof(" + buf + "), \""
            + fmt + "\", " + argStr + ");");
        return buf;
    }

    private static string HandleStringConcat(List<string> args, TranspilerContext ctx)
    {
        if (args.Count == 0) return "\"\"";
        if (args.Count == 1) return args[0];
        var fmt = string.Concat(Enumerable.Repeat("%s", args.Count));
        var buf = ctx.NextStringBuf();
        ctx.Out.WriteLine(ctx.Tab
            + "snprintf(" + buf + ", sizeof(" + buf + "), \""
            + fmt + "\", " + string.Join(", ", args) + ");");
        return buf;
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
                    var commaIdx = inner.IndexOf(',');
                    var cutIdx = -1;
                    if (colonIdx >= 0) cutIdx = colonIdx;
                    if (commaIdx >= 0 && (cutIdx < 0 || commaIdx < cutIdx)) cutIdx = commaIdx;
                    var idxStr = cutIdx >= 0 ? inner[..cutIdx] : inner;
                    if (int.TryParse(idxStr.Trim(), out var argIdx) && argIdx < formatArgs.Count)
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