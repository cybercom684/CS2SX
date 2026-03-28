using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        if (!TryResolveStringBuilder(objStr, ctx, out var sbExpr))
            return NotHandled(out result);

        var method = mem.Name.Identifier.Text;

        result = method switch
        {
            "Clear" => "StringBuilder_Clear(" + sbExpr + ")",
            "ToString" => "StringBuilder_ToString(" + sbExpr + ")",
            "IndexOf" => "StringBuilder_IndexOf(" + sbExpr + ", " + ArgAt(args, 0) + ")",
            "Insert" => "StringBuilder_Insert(" + sbExpr + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",
            "Replace" => "StringBuilder_Replace(" + sbExpr + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",
            "Append" => BuildAppend(sbExpr, inv, args, ctx),
            "AppendLine" => BuildAppendLine(sbExpr, inv, args, ctx),
            _ => sbExpr + "->" + method + "(" + JoinArgs(args) + ")",
        };
        return true;
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
            "float" => "StringBuilder_AppendFloat(" + sbExpr + ", " + args[0] + ")",
            "char" => "StringBuilder_AppendChar(" + sbExpr + ", " + args[0] + ")",
            _ => "StringBuilder_AppendStr(" + sbExpr + ", " + args[0] + ")",
        };
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
            "int" => "StringBuilder_AppendLineInt(" + sbExpr + ", " + args[0] + ")",
            "uint" => "StringBuilder_AppendUInt(" + sbExpr + ", " + args[0]
                    + "); StringBuilder_AppendChar(" + sbExpr + ", '\\n')",
            _ => "StringBuilder_AppendLine(" + sbExpr + ", " + args[0] + ")",
        };
    }
}