using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using CS2SX.Core;
using CS2SX.Transpiler.Writers;

namespace CS2SX.Transpiler.Handlers;

public sealed class StringBuilderHandler : InvocationHandlerBase
{
    private static readonly HashSet<string> s_methods = new(StringComparer.Ordinal)
    {
        "Append", "AppendLine", "AppendFormat", "Clear", "ToString",
        "Insert", "Replace", "IndexOf",
    };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem
            || !s_methods.Contains(mem.Name.Identifier.Text))
            return NotHandled(out result);

        var objStr = mem.Expression.ToString();

        string sbExpr;
        // Fluent chain: sb.Append(a).Append(b) — the receiver is itself a call that
        // returns the builder (all Append* runtime functions return StringBuilder*).
        // Also any other receiver typed StringBuilder (holder.Log.Append(…)).
        if (mem.Expression is InvocationExpressionSyntax or MemberAccessExpressionSyntax or ElementAccessExpressionSyntax
            && ctx.GetSemanticType(mem.Expression) is "StringBuilder" or "System.Text.StringBuilder")
            sbExpr = writeExpr(mem.Expression);
        else if (!TryResolveStringBuilder(objStr, ctx, out sbExpr))
            return NotHandled(out result);

        var method = mem.Name.Identifier.Text;

        result = method switch
        {
            "Clear" => "StringBuilder_Clear(" + sbExpr + ")",
            "ToString" => "StringBuilder_ToString(" + sbExpr + ")",
            "IndexOf" => "StringBuilder_IndexOf(" + sbExpr + ", " + ArgAt(args, 0) + ")",
            "Insert" => "StringBuilder_Insert(" + sbExpr + ", " + ArgAt(args, 0) + ", " + AsText(inv, 1, args, ctx) + ")",
            "Replace" => "StringBuilder_Replace(" + sbExpr + ", " + AsText(inv, 0, args, ctx) + ", " + AsText(inv, 1, args, ctx) + ")",
            "Append" => BuildAppend(sbExpr, inv, args, ctx),
            "AppendLine" => BuildAppendLine(sbExpr, inv, args, ctx),
            "AppendFormat" => BuildAppendFormat(sbExpr, inv, args, ctx),
            _ => sbExpr + "->" + method + "(" + JoinArgs(args) + ")",
        };
        return true;
    }

    /// Argument i as text: sb.Insert(0, 5), sb.Replace('a', 'b') take numbers and chars too.
    private static string AsText(InvocationExpressionSyntax inv, int i, List<string> args, TranspilerContext ctx)
    {
        var code = ArgAt(args, i);
        if (i >= inv.ArgumentList.Arguments.Count) return code;
        var cs = TypeInferrer.InferCSharpType(inv.ArgumentList.Arguments[i].Expression, ctx);
        if (cs is "string" or "string?") return code;
        var (spec, c) = FormatArg.Adapt(cs, TypeRegistry.FormatSpecifier(TypeRegistry.MapType(cs)), code);
        return spec == "%s" ? c : "(const char*)CS2SX_Format(_cs2sx_next_buf(), \"" + spec + "\", " + c + ")";
    }

    private static string BuildAppend(string sbExpr, InvocationExpressionSyntax inv,
        List<string> args, TranspilerContext ctx)
    {
        if (args.Count == 0) return "";
        var argExpr = inv.ArgumentList.Arguments[0].Expression;
        var argType = TypeInferrer.InferCSharpType(argExpr, ctx);
        return argType switch
        {
            "int" or "s32" or "s16" or "s8" => "StringBuilder_AppendInt(" + sbExpr + ", " + args[0] + ")",
            "uint" or "u32" or "u16" or "u8" => "StringBuilder_AppendUInt(" + sbExpr + ", " + args[0] + ")",
            "long" or "s64" => "StringBuilder_AppendLong(" + sbExpr + ", " + args[0] + ")",
            "ulong" or "u64" => "StringBuilder_AppendULong(" + sbExpr + ", " + args[0] + ")",
            "float" => "StringBuilder_AppendFloat(" + sbExpr + ", " + args[0] + ")",
            "double" => "StringBuilder_AppendDouble(" + sbExpr + ", " + args[0] + ")",
            "char" => "StringBuilder_AppendChar(" + sbExpr + ", " + args[0] + ")",
            _ => "StringBuilder_AppendStr(" + sbExpr + ", " + args[0] + ")",
        };
    }

    // AppendFormat("{0:F2}", x, ...) → snprintf into tmp buf, then StringBuilder_AppendStr
    private static string BuildAppendFormat(string sbExpr, InvocationExpressionSyntax inv,
        List<string> args, TranspilerContext ctx)
    {
        if (inv.ArgumentList.Arguments.Count == 0)
            return "StringBuilder_AppendStr(" + sbExpr + ", \"\")";

        var firstArg = inv.ArgumentList.Arguments[0].Expression;
        if (firstArg is LiteralExpressionSyntax lit
            && lit.Token.RawKind == (int)SyntaxKind.StringLiteralToken)
        {
            var template = lit.Token.ValueText;
            var formatArgs = inv.ArgumentList.Arguments.Skip(1).ToList();
            var (fmt, fmtArgs) = StringMethodHandler.BuildFormatString(template, formatArgs, args.Skip(1).ToList(), ctx);
            var buf = ctx.NextStringBuf();
            if (formatArgs.Count > 0)
            {
                ctx.Out.WriteLine(ctx.Tab
                    + $"{buf} = CS2SX_Format({buf}, \"{fmt}\"" + (fmtArgs.Count > 0 ? ", " + string.Join(", ", fmtArgs) : "") + ");");
            }
            return "StringBuilder_AppendStr(" + sbExpr + ", " + buf + ")";
        }

        // Fallback: treat first arg as string
        return "StringBuilder_AppendStr(" + sbExpr + ", " + ArgAt(args, 0) + ")";
    }

    private static string BuildAppendLine(string sbExpr, InvocationExpressionSyntax inv,
        List<string> args, TranspilerContext ctx)
    {
        if (args.Count == 0)
            return "StringBuilder_AppendChar(" + sbExpr + ", '\\n')";
        var argExpr = inv.ArgumentList.Arguments[0].Expression;
        var argType = TypeInferrer.InferCSharpType(argExpr, ctx);
        return argType switch
        {
            "int" or "s32" or "s16" or "s8" => "StringBuilder_AppendLineInt(" + sbExpr + ", " + args[0] + ")",
            "uint" or "u32" or "u16" or "u8" => "StringBuilder_AppendLineUInt(" + sbExpr + ", " + args[0] + ")",
            "long" or "s64" => "StringBuilder_AppendLineLong(" + sbExpr + ", " + args[0] + ")",
            "ulong" or "u64" => "StringBuilder_AppendLineULong(" + sbExpr + ", " + args[0] + ")",
            "float" => "StringBuilder_AppendLineFloat(" + sbExpr + ", " + args[0] + ")",
            "double" => "StringBuilder_AppendLineDouble(" + sbExpr + ", " + args[0] + ")",
            "char" => "StringBuilder_AppendLineChar(" + sbExpr + ", " + args[0] + ")",
            _ => "StringBuilder_AppendLine(" + sbExpr + ", " + args[0] + ")",
        };
    }
}