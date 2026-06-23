using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Routes calls to libnx functions (the generated Stubs/LibNX/* surface) straight
/// through to the bare C name. Two recognition paths:
///   1. Fully-qualified text: `LibNX.Services.Hid.hidInitialize()`.
///   2. Idiomatic via `using LibNX.Services;` → `Hid.hidInitialize()` — resolved
///      through the semantic model (the stubs are part of the compilation).
/// Must run before StaticClassHandler, which would otherwise mangle the call to
/// the nonexistent `Hid_hidInitialize`.
/// </summary>
public sealed class LibNxHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem)
            return NotHandled(out result);

        // 1. Explicit fully-qualified LibNX.* call.
        if (calleeStr.StartsWith("LibNX.", StringComparison.Ordinal))
        {
            result = mem.Name.Identifier.Text + "(" + JoinArgs(args) + ")";
            return true;
        }

        // 2. Idiomatic call resolved to a method in the LibNX namespace.
        if (ctx.SemanticModel != null)
        {
            try
            {
                var sym = ctx.SemanticModel.GetSymbolInfo(inv).Symbol as IMethodSymbol;
                var ns = sym?.ContainingType?.ContainingNamespace?.ToDisplayString();
                if (ns != null
                    && (ns == "LibNX" || ns.StartsWith("LibNX.", StringComparison.Ordinal)))
                {
                    result = mem.Name.Identifier.Text + "(" + JoinArgs(args) + ")";
                    return true;
                }
            }
            catch { }
        }

        return NotHandled(out result);
    }
}
