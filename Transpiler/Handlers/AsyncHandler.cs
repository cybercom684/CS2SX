// ============================================================================
// Transpiler/Handlers/AsyncHandler.cs  (NEU)
//
// FIX: async/await Fallback-Behandlung.
// await someTask → someTask (synchron, mit Warning)
// await Task.Run(() => Foo()) → Foo() (direkter Aufruf)
//
// Vollständiges async/await mit Scheduling ist auf der Switch nicht möglich
// da kein OS-Threading-Support. Der Transpiler warnt und führt synchron aus.
// ============================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class AsyncHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        // Task.Run(lambda) → lambda() direkt aufrufen
        if (calleeStr == "Task.Run" && inv.ArgumentList.Arguments.Count > 0)
        {
            ctx.Warn($"Task.Run — executed synchronously (no threading on Switch)", calleeStr);
            var lambdaArg = inv.ArgumentList.Arguments[0].Expression;
            if (lambdaArg is LambdaExpressionSyntax)
            {
                result = writeExpr(lambdaArg) + "()";
                return true;
            }
            result = args[0] + "()";
            return true;
        }

        // Task.Delay → kein op (busy-wait wäre schlecht, einfach ignorieren mit Warning)
        if (calleeStr == "Task.Delay")
        {
            ctx.Warn("Task.Delay — ignored on Switch (use svcSleepThread for delays)", calleeStr);
            result = "/* Task.Delay ignored */";
            return true;
        }

        // Task.WhenAll / Task.WhenAny → alle args aufrufen
        if (calleeStr is "Task.WhenAll" or "Task.WhenAny")
        {
            ctx.Warn($"{calleeStr} — executed sequentially (no threading on Switch)", calleeStr);
            result = "(" + string.Join(", ", args.Select(a => a + "()")) + ")";
            return true;
        }

        // ConfigureAwait → passthrough
        if (calleeStr.EndsWith(".ConfigureAwait"))
        {
            result = args.Count > 0 ? args[0] : "NULL";
            return true;
        }

        return NotHandled(out result);
    }
}