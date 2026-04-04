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
// PathHandler
// Behandelt Path.GetFileName, Path.GetExtension etc.
// ============================================================================

public sealed class PathHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            ["Path.GetFileName"] = "CS2SX_Path_GetFileName",
            ["Path.GetExtension"] = "CS2SX_Path_GetExtension",
            ["Path.GetDirectoryName"] = "CS2SX_Path_GetDirectoryName",
            ["Path.IsDirectory"] = "CS2SX_Path_IsDirectory",
        };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        // Path.Combine(a, b) → snprintf-Puffer
        if (calleeStr == "Path.Combine")
        {
            var buf = ctx.NextStringBuf(1024);
            ctx.Out.WriteLine(ctx.Tab
                + $"snprintf({buf}, sizeof({buf}), \"%s/%s\", "
                + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ");");
            result = buf;
            return true;
        }

        if (!s_map.TryGetValue(calleeStr, out var cFunc))
            return NotHandled(out result);

        result = cFunc + "(" + ArgAt(args, 0) + ")";
        return true;
    }
}