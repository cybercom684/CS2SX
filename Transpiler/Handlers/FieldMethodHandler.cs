using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class FieldMethodHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem)
            return NotHandled(out result);

        var objStr = mem.Expression.ToString();
        if (!objStr.StartsWith('_'))
            return NotHandled(out result);

        var fieldKey = objStr.TrimStart('_');
        if (!ctx.FieldTypes.TryGetValue(fieldKey, out var fieldType))
            return NotHandled(out result);

        var methodName = mem.Name.Identifier.Text;
        var allArgs = new List<string> { "self->f_" + fieldKey };
        allArgs.AddRange(args);

        result = fieldType + "_" + methodName + "(" + string.Join(", ", allArgs) + ")";
        return true;
    }
}