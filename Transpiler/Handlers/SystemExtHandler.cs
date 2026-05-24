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
        if (calleeStr == "System.GetBattery" || calleeStr == "CS2SX.GetBattery")
        {
            result = "CS2SX_GetBattery()";
            return true;
        }

        if (calleeStr == "System.GetTime" || calleeStr == "CS2SX.GetTime")
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