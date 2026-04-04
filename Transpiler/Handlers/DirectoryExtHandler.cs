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
// DirectoryExtHandler
// Behandelt GetDirectories und GetEntries
// ============================================================================

public sealed class DirectoryExtHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            ["Directory.GetDirectories"] = "CS2SX_Dir_GetDirectories",
            ["Directory.GetEntries"] = "CS2SX_Dir_GetEntries",
        };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (!s_map.TryGetValue(calleeStr, out var cFunc))
            return NotHandled(out result);

        result = cFunc + "(" + ArgAt(args, 0) + ")";
        return true;
    }
}