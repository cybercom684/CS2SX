using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Writers;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt StringBuilder-Methoden: Append, AppendLine, Clear, ToString.
///
/// Erweiterung: Neuen case in Handle() ergänzen.
/// </summary>
public sealed class StringBuilderHandler : IInvocationHandler
{
    private static readonly HashSet<string> s_methods = new(StringComparer.Ordinal)
    {
        "Append", "AppendLine", "AppendFormat", "Clear", "ToString",
    };

    public bool CanHandle(InvocationExpressionSyntax inv, string calleeStr, TranspilerContext ctx)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem) return false;
        if (!s_methods.Contains(mem.Name.Identifier.Text)) return false;

        var objStr = mem.Expression.ToString();
        var objKey = objStr.TrimStart('_');
        return ctx.LocalTypes.TryGetValue(objStr, out var lt) && TypeRegistry.IsStringBuilder(lt)
            || ctx.FieldTypes.TryGetValue(objKey, out var ft) && TypeRegistry.IsStringBuilder(ft);
    }

    public string Handle(InvocationExpressionSyntax inv, string calleeStr, List<string> args,
        TranspilerContext ctx, Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        var mem        = (MemberAccessExpressionSyntax)inv.Expression;
        var objStr     = mem.Expression.ToString();
        var objKey     = objStr.TrimStart('_');
        var methodName = mem.Name.Identifier.Text;

        // C-Ausdruck des StringBuilder-Objekts ermitteln
        string sbExpr;
        if (ctx.LocalTypes.TryGetValue(objStr, out var lt) && TypeRegistry.IsStringBuilder(lt))
            sbExpr = objStr;
        else
            sbExpr = "self->f_" + objKey;

        switch (methodName)
        {
            case "Clear":
                return "StringBuilder_Clear(" + sbExpr + ")";

            case "ToString":
                return "StringBuilder_ToString(" + sbExpr + ")";

            case "Append":
            {
                if (args.Count == 0) return "";
                var argExpr = inv.ArgumentList.Arguments[0].Expression;
                var argType = TypeInferrer.InferCSharpType(argExpr, ctx);
                return argType switch
                {
                    "int"   or "s32" or "s16" or "s8"   => "StringBuilder_AppendInt(" + sbExpr + ", " + args[0] + ")",
                    "uint"  or "u32" or "u16" or "u8"   => "StringBuilder_AppendUInt(" + sbExpr + ", " + args[0] + ")",
                    "float"                              => "StringBuilder_AppendFloat(" + sbExpr + ", " + args[0] + ")",
                    "char"                               => "StringBuilder_AppendChar(" + sbExpr + ", " + args[0] + ")",
                    _                                    => "StringBuilder_AppendStr(" + sbExpr + ", " + args[0] + ")",
                };
            }

            case "AppendLine":
            {
                if (args.Count == 0)
                    return "StringBuilder_AppendChar(" + sbExpr + ", '\\n')";
                var argExpr = inv.ArgumentList.Arguments[0].Expression;
                var argType = TypeInferrer.InferCSharpType(argExpr, ctx);
                return argType switch
                {
                    "int"  => "StringBuilder_AppendLineInt(" + sbExpr + ", " + args[0] + ")",
                    "uint" => "StringBuilder_AppendUInt(" + sbExpr + ", " + args[0] + "); StringBuilder_AppendChar(" + sbExpr + ", '\\n')",
                    _      => "StringBuilder_AppendLine(" + sbExpr + ", " + args[0] + ")",
                };
            }

            default:
                return sbExpr + "->" + methodName + "(" + string.Join(", ", args) + ")";
        }
    }
}
