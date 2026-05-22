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
            ["Path.GetFileName"]              = "CS2SX_Path_GetFileName",
            ["Path.GetExtension"]             = "CS2SX_Path_GetExtension",
            ["Path.GetDirectoryName"]         = "CS2SX_Path_GetDirectoryName",
            ["Path.IsDirectory"]              = "CS2SX_Path_IsDirectory",
            ["Path.GetFileNameWithoutExtension"] = "CS2SX_Path_GetFileNameWithoutExt",
            ["Path.IsPathRooted"]             = "CS2SX_Path_IsPathRooted",
        };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        // Path.Combine(a, b[, c, d]) → snprintf-Puffer mit "/" als Trenner
        if (calleeStr == "Path.Combine")
        {
            if (args.Count == 0) { result = "\"\""; return true; }
            if (args.Count == 1) { result = ArgAt(args, 0); return true; }

            var buf = ctx.NextStringBuf(1024);
            if (args.Count == 2)
            {
                ctx.Out.WriteLine(ctx.Tab
                    + $"snprintf({buf}, sizeof({buf}), \"%s/%s\", "
                    + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ");");
            }
            else if (args.Count == 3)
            {
                ctx.Out.WriteLine(ctx.Tab
                    + $"snprintf({buf}, sizeof({buf}), \"%s/%s/%s\", "
                    + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ", " + ArgAt(args, 2) + ");");
            }
            else
            {
                // 4+ Argumente: iterativ zusammenbauen — buf ist der Akkumulator
                var tmp = ctx.NextStringBuf(1024);
                ctx.Out.WriteLine(ctx.Tab + $"snprintf({buf}, sizeof({buf}), \"%s/%s\", "
                    + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ");");
                for (int i = 2; i < args.Count; i++)
                {
                    ctx.Out.WriteLine(ctx.Tab + $"snprintf({tmp}, sizeof({tmp}), \"%s/%s\", {buf}, {ArgAt(args, i)});");
                    ctx.Out.WriteLine(ctx.Tab + $"memcpy({buf}, {tmp}, sizeof({buf}));");
                }
                result = buf;
                return true;
            }
            result = buf;
            return true;
        }

        // Path.ChangeExtension(path, ext) → CS2SX_Path_ChangeExtension
        if (calleeStr == "Path.ChangeExtension")
        {
            result = "CS2SX_Path_ChangeExtension(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")";
            return true;
        }

        if (!s_map.TryGetValue(calleeStr, out var cFunc))
            return NotHandled(out result);

        result = cFunc + "(" + ArgAt(args, 0) + ")";
        return true;
    }
}