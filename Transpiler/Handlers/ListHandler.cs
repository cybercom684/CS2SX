using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class ListHandler : IInvocationHandler
{
    private static readonly HashSet<string> s_methods = new(StringComparer.Ordinal)
    {
        "Add", "Clear", "RemoveAt", "Remove", "Contains", "Insert",
    };

    public bool CanHandle(InvocationExpressionSyntax inv, string calleeStr, TranspilerContext ctx)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem) return false;
        if (!s_methods.Contains(mem.Name.Identifier.Text)) return false;

        var objStr = mem.Expression.ToString();
        var objKey = objStr.TrimStart('_');
        return ctx.LocalTypes.TryGetValue(objStr, out var lt) && TypeRegistry.IsList(lt)
            || ctx.FieldTypes.TryGetValue(objKey, out var ft) && TypeRegistry.IsList(ft);
    }

    public string Handle(InvocationExpressionSyntax inv, string calleeStr, List<string> args,
        TranspilerContext ctx, Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        var mem = (MemberAccessExpressionSyntax)inv.Expression;
        var objStr = mem.Expression.ToString();
        var objKey = objStr.TrimStart('_');
        var methodName = mem.Name.Identifier.Text;

        string listType;
        string listExpr;

        if (ctx.LocalTypes.TryGetValue(objStr, out var lt) && TypeRegistry.IsList(lt))
        {
            listType = lt;
            listExpr = objStr;
        }
        else
        {
            ctx.FieldTypes.TryGetValue(objKey, out var ft);
            listType = ft!;
            listExpr = "self->f_" + objKey;
        }

        var inner = TypeRegistry.GetListInnerType(listType)!;
        var cInner = inner == "string" ? "char" : TypeRegistry.MapType(inner);
        var cList = "List_" + cInner;

        return methodName switch
        {
            "Add" => cList + "_Add(" + listExpr + ", " + string.Join(", ", args) + ")",
            "Clear" => cList + "_Clear(" + listExpr + ")",
            "RemoveAt" => cList + "_Remove(" + listExpr + ", " + string.Join(", ", args) + ")",
            "Remove" => args.Count > 0 && args[0] == "int"
                          ? cList + "_Remove(" + listExpr + ", " + string.Join(", ", args) + ")"   // index
                          : cList + "_RemoveValue(" + listExpr + ", " + string.Join(", ", args) + ")", // value
            "Contains" => cList + "_Contains(" + listExpr + ", " + string.Join(", ", args) + ")",
            "Insert" => cList + "_Insert(" + listExpr + ", " + string.Join(", ", args) + ")",
            _ => listExpr + "->" + methodName + "(" + string.Join(", ", args) + ")",
        };
    }
}