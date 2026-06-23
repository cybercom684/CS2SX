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
        "ToUpper", "ToUpperInvariant", "ToLower", "ToLowerInvariant", "Replace", "Substring",
        "IndexOf", "LastIndexOf", "PadLeft", "PadRight",
        "Split", "CompareTo", "ToCharArray",
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

            // When type is unknown (null), verify via SemanticModel that the receiver is
            // actually a string. Static class names like "Navigator" resolve to null here,
            // and their methods (e.g. Navigator.Replace) must NOT be treated as string ops.
            // Exception: ToString() is valid on any type — int/float/enum receivers must
            // reach BuildToString() instead of falling through to invalid C `expr->ToString()`.
            if (type == null && ctx.SemanticModel != null)
            {
                try
                {
                    var semInfo = ctx.SemanticModel.GetTypeInfo(mem.Expression);
                    var semType = semInfo.ConvertedType ?? semInfo.Type;
                    if (semType != null)
                    {
                        var dn = semType.ToDisplayString();
                        if (mem.Name.Identifier.Text != "ToString"
                         && !dn.Equals("string", StringComparison.OrdinalIgnoreCase)
                         && dn != "System.String"
                         && !dn.Contains("char"))
                            return NotHandled(out result);
                    }
                    else
                    {
                        // Unresolvable type (e.g. static class receiver) — do not handle
                        return NotHandled(out result);
                    }
                }
                catch { }
            }

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
                => "String_IsNullOrEmpty(" + ArgAt(args, 0) + ")",

            "string.IsNullOrWhiteSpace" or "String.IsNullOrWhiteSpace"
                => "String_IsNullOrWhiteSpace(" + ArgAt(args, 0) + ")",

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
                => HandleSplitStatic(inv, args),

            _ => WarnUnhandledString(inv, calleeStr, ctx, args),
        };
    }

    // Unhandled static/instance string method: warn loudly instead of silently
    // returning the first argument (which compiles but is wrong at runtime).
    private static string WarnUnhandledString(
        InvocationExpressionSyntax inv, string name,
        TranspilerContext ctx, List<string> args)
    {
        ctx.Warn(inv, $"unsupported string method '{name}' — emitted a passthrough; "
                    + "result will be wrong. Verify generated C.");
        return args.Count > 0 ? args[0] : "\"\"";
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
                return "String_CompareIgnoreCase(" + a + ", " + b + ")";
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
            "ToString" => BuildToString(mem.Expression, receiver, recvType, ctx),
            "Contains" => "String_Contains(" + receiver + ", " + ArgAt(args, 0) + ")",
            "StartsWith" => "String_StartsWith(" + receiver + ", " + ArgAt(args, 0) + ")",
            "EndsWith" => "String_EndsWith(" + receiver + ", " + ArgAt(args, 0) + ")",
            "Equals" => "strcmp(" + receiver + ", " + ArgAt(args, 0) + ") == 0",
            "Trim" => "String_Trim(" + receiver + ")",
            "TrimStart" => "String_TrimStart(" + receiver + ")",
            "TrimEnd" => "String_TrimEnd(" + receiver + ")",
            "ToUpper" or "ToUpperInvariant" => "String_ToUpper(" + receiver + ")",
            "ToLower" or "ToLowerInvariant" => "String_ToLower(" + receiver + ")",
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

            "Split" => HandleSplitInstance(inv, receiver, args),

            "Substring" => args.Count == 1
                ? "String_SubstringFrom(" + receiver + ", " + ArgAt(args, 0) + ")"
                : "String_Substring(" + receiver + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "PadLeft" or "PadStart" => args.Count == 1
                ? "String_PadLeft(" + receiver + ", " + ArgAt(args, 0) + ", ' ')"
                : "String_PadLeft(" + receiver + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "PadRight" or "PadEnd" => args.Count == 1
                ? "String_PadRight(" + receiver + ", " + ArgAt(args, 0) + ", ' ')"
                : "String_PadRight(" + receiver + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",

            "ToCharArray" => receiver + " /* ToCharArray — const char* is already a char array in C */",

            _ => WarnUnhandledString(inv, methodName, ctx, args),
        };
    }

    private static string BuildToString(
        Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax receiverExpr,
        string receiver, string recvType, CS2SX.Core.TranspilerContext ctx)
    {
        // Enum.ToString() → ternary name lookup using known members.
        // Check semantic model first (works across files), then EnumDefs (current TU).
        List<string>? members = null;
        if (ctx.SemanticModel != null)
        {
            try
            {
                var typeInfo = ctx.SemanticModel.GetTypeInfo(receiverExpr);
                var sym = typeInfo.ConvertedType ?? typeInfo.Type;
                if (sym?.TypeKind == Microsoft.CodeAnalysis.TypeKind.Enum
                    && ctx.EnumDefs.TryGetValue(sym.Name, out var semMembers))
                    members = semMembers;
            }
            catch { }
        }
        if (members == null && ctx.EnumDefs.TryGetValue(recvType, out var localMembers))
            members = localMembers;

        if (members != null && members.Count > 0)
        {
            var chain = string.Join(" : ", members.Select(m => $"{receiver} == {m} ? \"{m}\""));
            return "(" + chain + " : \"0\")";
        }

        return recvType switch
        {
            "uint" or "u32" or "uint32_t" => "UInt_ToString((unsigned int)" + receiver + ")",
            "float" => "Float_ToString(" + receiver + ")",
            "double" => "Double_ToString(" + receiver + ")",
            "long" or "i64" or "int64_t" => "Long_ToString((long long)" + receiver + ")",
            "ulong" or "u64" or "uint64_t" => "ULong_ToString((unsigned long long)" + receiver + ")",
            _ => "Int_ToString((int)" + receiver + ")",
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

    // Split instance: s.Split(',')  / s.Split(new char[]{','}) / s.Split(',', StringSplitOptions.*)
    private static string HandleSplitInstance(InvocationExpressionSyntax inv,
        string receiver, List<string> args)
    {
        if (args.Count == 0) return "String_Split_Whitespace(" + receiver + ")";
        var sep = ExtractSplitSeparator(inv, 0, args[0]);
        if (sep == "\"\"") return "String_Split_Whitespace(" + receiver + ")";
        var removeEmpty = HasRemoveEmptyEntries(inv);
        var fn = removeEmpty ? "String_Split_RemoveEmpty" : "String_Split";
        return $"{fn}({receiver}, {sep})";
    }

    // Split static: string.Split(str, ',') / string.Split(str, new char[]{})
    private static string HandleSplitStatic(InvocationExpressionSyntax inv, List<string> args)
    {
        if (args.Count == 0) return "NULL";
        if (args.Count == 1) return "String_Split_Whitespace(" + args[0] + ")";
        var sep = ExtractSplitSeparator(inv, 1, args[1]);
        if (sep == "\"\"") return "String_Split_Whitespace(" + args[0] + ")";
        var removeEmpty = HasRemoveEmptyEntries(inv);
        var fn = removeEmpty ? "String_Split_RemoveEmpty" : "String_Split";
        return $"{fn}({args[0]}, {sep})";
    }

    private static bool HasRemoveEmptyEntries(InvocationExpressionSyntax inv)
    {
        foreach (var arg in inv.ArgumentList.Arguments)
        {
            var raw = arg.Expression.ToString();
            if (raw.Contains("RemoveEmptyEntries")) return true;
        }
        return false;
    }

    // Extracts the split separator from a C# Split() argument.
    // Handles: char literal, string literal, new char[]{'/',...} initializers.
    // StringSplitOptions and count overloads are silently ignored (extra args).
    private static string ExtractSplitSeparator(
        InvocationExpressionSyntax inv, int argIndex, string fallback)
    {
        if (inv.ArgumentList.Arguments.Count <= argIndex) return fallback;
        var expr = inv.ArgumentList.Arguments[argIndex].Expression;

        // StringSplitOptions enum — skip, return plain separator as is
        if (expr.ToString().Contains("StringSplitOptions")) return fallback;

        // char literal → wrap as one-char string  ','  →  ","
        if (expr is LiteralExpressionSyntax charLit
            && charLit.Token.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.CharacterLiteralToken))
        {
            var ch = charLit.Token.ValueText;
            if (ch == "\\") return "\"\\\\\"";
            if (ch == "\"") return "\"\\\"\"";
            return "\"" + ch + "\"";
        }

        // new char[] { '/', '\\' } — take first element
        if (expr is ArrayCreationExpressionSyntax arrCreate
            && arrCreate.Initializer?.Expressions.Count > 0)
        {
            var first = arrCreate.Initializer.Expressions[0];
            if (first is LiteralExpressionSyntax flit
                && flit.Token.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.CharacterLiteralToken))
            {
                var ch = flit.Token.ValueText;
                return "\"" + ch + "\"";
            }
        }

        // new[] { '/' } implicit array
        if (expr is ImplicitArrayCreationExpressionSyntax implArr
            && implArr.Initializer.Expressions.Count > 0)
        {
            var first = implArr.Initializer.Expressions[0];
            if (first is LiteralExpressionSyntax flit
                && flit.Token.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.CharacterLiteralToken))
                return "\"" + flit.Token.ValueText + "\"";
        }

        return fallback;
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

    // Public overload for use by StringBuilderHandler
    public static string BuildFormatStringPublic(string template,
        List<ArgumentSyntax> formatArgs, TranspilerContext ctx)
        => BuildFormatString(template, formatArgs, ctx);

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
                    var fmtSpec = colonIdx >= 0 ? inner[(colonIdx + 1)..] : null;
                    if (int.TryParse(idxStr.Trim(), out var argIdx) && argIdx < formatArgs.Count)
                    {
                        var baseSpec = TypeInferrer.FormatSpecifier(formatArgs[argIdx].Expression, ctx);
                        sb.Append(fmtSpec != null ? MapFormatSpecifier(fmtSpec, baseSpec) : baseSpec);
                    }
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

    // Maps .NET format specifiers to printf format strings.
    // fmtSpec is the text after the colon in {0:F2}, baseSpec is the inferred %d/%f/%s etc.
    internal static string MapFormatSpecifier(string fmtSpec, string baseSpec)
    {
        if (string.IsNullOrEmpty(fmtSpec)) return baseSpec;

        var upper = fmtSpec.TrimStart().ToUpperInvariant();
        char letter = upper.Length > 0 ? upper[0] : '\0';
        var numStr = upper.Length > 1 ? upper[1..] : "";
        int.TryParse(numStr, out var precision);

        return letter switch
        {
            'F' => numStr.Length > 0 ? $"%.{precision}f" : "%f",
            'E' => numStr.Length > 0 ? $"%.{precision}e" : "%e",
            'G' => numStr.Length > 0 ? $"%.{precision}g" : "%g",
            'N' => numStr.Length > 0 ? $"%.{precision}f" : "%.2f",
            'D' => numStr.Length > 0 ? $"%0{precision}d"  : "%d",
            'X' => numStr.Length > 0 ? $"%0{precision}X"  : "%X",
            'x' => numStr.Length > 0 ? $"%0{precision}x"  : "%x",
            'P' => numStr.Length > 0 ? $"%.{precision}f%%"  : "%.2f%%",
            'C' => "%.2f",
            _ => baseSpec,
        };
    }
}