using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class DictionaryHandler : InvocationHandlerBase
{
    private static readonly HashSet<string> s_methods = new(StringComparer.Ordinal)
    {
        "Add", "Remove", "ContainsKey", "TryGetValue", "Clear",
    };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem
            || !s_methods.Contains(mem.Name.Identifier.Text))
            return NotHandled(out result);

        var objStr = mem.Expression.ToString();

        if (!TryResolveDict(objStr, ctx, out var dictType, out var dictExpr))
            return NotHandled(out result);

        var cDict = DictFuncPrefix(dictType);
        var method = mem.Name.Identifier.Text;

        if (method == "TryGetValue" && args.Count >= 2)
        {
            var outPtr = args[1].StartsWith("&") ? args[1] : "&" + args[1];
            result = cDict + "_TryGetValue(" + dictExpr + ", " + args[0] + ", " + outPtr + ")";
            return true;
        }

        result = method switch
        {
            "Add" => cDict + "_Add(" + dictExpr + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",
            "Remove" => cDict + "_Remove(" + dictExpr + ", " + ArgAt(args, 0) + ")",
            "ContainsKey" => cDict + "_ContainsKey(" + dictExpr + ", " + ArgAt(args, 0) + ")",
            "Clear" => cDict + "_Clear(" + dictExpr + ")",
            _ => dictExpr + "->" + method + "(" + JoinArgs(args) + ")",
        };
        return true;
    }
}