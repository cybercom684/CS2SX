// ============================================================================
// CS2SX — Transpiler/Handlers/EventDelegateHandler.cs
//
// Handles delegate += / -= (multicast subscribe/unsubscribe) and event invocation.
//
// C# model:                         C model (simplified):
//   event Action<int> OnScore;   →   List_Action_t* OnScore_handlers;
//   OnScore += UpdateHUD;        →   List_Action_t_Add(self->f_OnScore_handlers, UpdateHUD)
//   OnScore?.Invoke(score);      →   for each handler: handler(ctx, score)
//   OnScore -= UpdateHUD;        →   List_Action_t_RemoveValue(...)
//
// Limitations:
//   - Delegate objects are function pointers (void*); no closure capture.
//   - No thread safety.
//   - Only Action and Action<T> delegates are supported (void return).
// ============================================================================

using CS2SX.Core;
using CS2SX.Transpiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class EventDelegateHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        // Handles: someDelegate.Invoke(args) or someDelegate?.Invoke(args) (via MemberAccess)
        if (inv.Expression is MemberAccessExpressionSyntax mem
            && mem.Name.Identifier.Text == "Invoke")
        {
            var delegateExpr = writeExpr(mem.Expression);
            var delegateRaw = mem.Expression.ToString();
            var delegateKey = delegateRaw.TrimStart('_');

            // Is this a field/local of a known delegate type?
            string? delType = null;
            ctx.LocalTypes.TryGetValue(delegateRaw, out delType);
            if (delType == null) ctx.FieldTypes.TryGetValue(delegateKey, out delType);

            if (delType != null && (delType == "Action"
                || delType.StartsWith("Action<")
                || delType.StartsWith("Func<")))
            {
                // Null-checked invocation — lifted lambdas no longer take void* ctx arg
                var argStr = args.Count > 0 ? string.Join(", ", args) : "";
                result = "((void*)" + delegateExpr + " != NULL ? " + delegateExpr + "(" + argStr + "), 0 : 0)";
                return true;
            }

            // Multicast list invocation: e.g. OnScore_handlers (List of function pointers)
            var listKey = delegateKey + "_handlers";
            if (ctx.FieldTypes.TryGetValue(listKey, out var listType)
                && TypeRegistry.IsList(listType))
            {
                var idxVar = ctx.NextTmp("ei");
                var listExpr = "self->f_" + listKey;
                var invokeArgSuffix = args.Count > 0
                    ? string.Join(", ", args)
                    : "";
                ctx.WriteLine($"if ({listExpr}) for (int {idxVar} = 0; {idxVar} < {listExpr}->count; {idxVar}++)");
                ctx.WriteLine("{");
                ctx.Indent();
                // Cast to unspecified-param function pointer (C11 compatible, args passed by register)
                ctx.WriteLine($"void (*_ev_fn)() = (void(*)())List_voidptr_Get({listExpr}, {idxVar});");
                ctx.WriteLine($"if (_ev_fn) _ev_fn({invokeArgSuffix});");
                ctx.Dedent();
                ctx.WriteLine("}");
                result = "/* event invoked */";
                return true;
            }
        }

        return NotHandled(out result);
    }
}