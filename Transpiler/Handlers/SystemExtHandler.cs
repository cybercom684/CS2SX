using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

// ============================================================================
// SystemHandler
// Behandelt System.GetBattery()
// ============================================================================

public sealed class SystemExtHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (calleeStr != "System.GetBattery" && calleeStr != "CS2SX.GetBattery")
            return NotHandled(out result);

        result = "CS2SX_GetBattery()";
        return true;
    }
}