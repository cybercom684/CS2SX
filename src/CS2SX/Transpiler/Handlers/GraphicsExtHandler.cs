using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

// ============================================================================
// GraphicsExtHandler
// Behandelt alle neuen Graphics-Methoden aus switchapp_ext.h
// ============================================================================

public sealed class GraphicsExtHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            // Dreiecke
            ["Graphics.DrawTriangle"] = "Graphics_DrawTriangle",
            ["Graphics.FillTriangle"] = "Graphics_FillTriangle",

            // Ellipsen
            ["Graphics.DrawEllipse"] = "Graphics_DrawEllipse",
            ["Graphics.FillEllipse"] = "Graphics_FillEllipse",

            // Abgerundete Rechtecke
            ["Graphics.DrawRoundedRect"] = "Graphics_DrawRoundedRect",
            ["Graphics.FillRoundedRect"] = "Graphics_FillRoundedRect",

            // Alpha
            ["Graphics.SetPixelAlpha"] = "Graphics_SetPixelAlpha",
            ["Graphics.FillRectAlpha"] = "Graphics_FillRectAlpha",
            ["Graphics.DrawTextAlpha"] = "Graphics_DrawTextAlpha",

            // Hilfsfunktionen
            ["Graphics.DrawTextShadow"] = "Graphics_DrawTextShadow",
            ["Graphics.DrawGrid"] = "Graphics_DrawGrid",
            ["Graphics.DrawPolygon"] = "Graphics_DrawPolygon",
        };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (!s_map.TryGetValue(calleeStr, out var cFunc))
            return NotHandled(out result);

        result = cFunc + "(" + JoinArgs(args) + ")";
        return true;
    }
}