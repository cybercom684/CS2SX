using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt Dictionary&lt;K,V&gt;-Methoden: Add, Remove, ContainsKey, TryGetValue, Clear.
///
/// Erweiterung: Neuen case in Handle() ergänzen.
/// </summary>
public sealed class DictionaryHandler : IInvocationHandler
{
    private static readonly HashSet<string> s_methods = new(StringComparer.Ordinal)
    {
        "Add", "Remove", "ContainsKey", "TryGetValue", "Clear",
    };

    public bool CanHandle(InvocationExpressionSyntax inv, string calleeStr, TranspilerContext ctx)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem) return false;
        if (!s_methods.Contains(mem.Name.Identifier.Text)) return false;

        var objStr = mem.Expression.ToString();
        var objKey = objStr.TrimStart('_');
        return (ctx.LocalTypes.TryGetValue(objStr, out var lt) && TypeRegistry.IsDictionary(lt))
            || (ctx.FieldTypes.TryGetValue(objKey, out var ft) && TypeRegistry.IsDictionary(ft));
    }

    public string Handle(InvocationExpressionSyntax inv, string calleeStr, List<string> args,
        TranspilerContext ctx, Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        var mem = (MemberAccessExpressionSyntax)inv.Expression;
        var objStr = mem.Expression.ToString();
        var objKey = objStr.TrimStart('_');
        var methodName = mem.Name.Identifier.Text;

        string dictType;
        string dictExpr;

        if (ctx.LocalTypes.TryGetValue(objStr, out var lt) && TypeRegistry.IsDictionary(lt))
        {
            dictType = lt;
            dictExpr = objStr;
        }
        else
        {
            ctx.FieldTypes.TryGetValue(objKey, out var ft);
            dictType = ft!;
            dictExpr = "self->f_" + objKey;
        }

        var types = TypeRegistry.GetDictionaryTypes(dictType)!.Value;
        var cKey = types.key == "string" ? "str" : TypeRegistry.MapType(types.key);
        var cVal = types.val == "string" ? "str" : TypeRegistry.MapType(types.val);
        var cDict = "Dict_" + cKey + "_" + cVal;

        return methodName switch
        {
            "Add" => cDict + "_Add(" + dictExpr + ", " + args[0] + ", " + args[1] + ")",
            "Remove" => cDict + "_Remove(" + dictExpr + ", " + args[0] + ")",
            "ContainsKey" => cDict + "_ContainsKey(" + dictExpr + ", " + args[0] + ")",
            "TryGetValue" => cDict + "_TryGetValue(" + dictExpr + ", " + args[0] + ", " + args[1] + ")",
            "Clear" => cDict + "_Clear(" + dictExpr + ")",
            _ => dictExpr + "->" + methodName + "(" + string.Join(", ", args) + ")",
        };
    }
}