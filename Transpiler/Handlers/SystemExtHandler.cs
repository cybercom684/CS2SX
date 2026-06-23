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
        if (calleeStr == "System.GetBattery" || calleeStr == "CS2SX.GetBattery" || calleeStr == "NX.GetBattery")
        {
            result = "CS2SX_GetBattery()";
            return true;
        }

        if (calleeStr == "System.GetTime" || calleeStr == "CS2SX.GetTime" || calleeStr == "NX.GetTime")
        {
            result = "CS2SX_GetTime()";
            return true;
        }

        if (calleeStr == "Graphics.LoadTexture")
        {
            var path = args.Count > 0 ? args[0] : "\"\"";
            result = "CS2SX_Texture_LoadBMP(" + path + ")";
            return true;
        }

        // Sys.FreeStr(s) — explicitly free a heap-allocated string (from
        // _cs2sx_heap_strdup). For manual memory management of owned string fields
        // in a finalizer. NULL-safe; must NOT be called on string literals.
        if (calleeStr == "Sys.FreeStr")
        {
            var s = args.Count > 0 ? args[0] : "NULL";
            result = "CS2SX_FreeStr(" + s + ")";
            return true;
        }

        if (calleeStr == "Graphics.DrawTextureCentered")
        {
            result = "Graphics_DrawTextureCentered(" + string.Join(", ", args) + ")";
            return true;
        }

        if (calleeStr == "Graphics.DrawTextureScaled")
        {
            result = "Graphics_DrawTextureScaled(" + string.Join(", ", args) + ")";
            return true;
        }

        if (calleeStr == "Graphics.DrawTextureCenteredScaled")
        {
            result = "Graphics_DrawTextureCenteredScaled(" + string.Join(", ", args) + ")";
            return true;
        }

        return NotHandled(out result);
    }
}