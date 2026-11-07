using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class GraphicsHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            ["Graphics.Init"] = "Graphics_Init",
            ["Graphics.LoadImage"] = "Graphics_LoadImage",
            ["Graphics.LoadImageThumb"] = "Graphics_LoadImageThumb",
            ["Graphics.ScaleTextureSmooth"] = "Graphics_ScaleTextureSmooth",
            ["Graphics.BlitPhysical"] = "Graphics_BlitPhysical",
            ["Graphics.SetOutputResolution"] = "Graphics_SetOutputResolution",
            ["Graphics.GetScaleNum"] = "Graphics_GetScaleNum",
            ["Graphics.GetScaleDen"] = "Graphics_GetScaleDen",
            ["Graphics.BeginFrame"] = "Graphics_BeginFrame",
            ["Graphics.EndFrame"] = "Graphics_EndFrame",
            ["Graphics.FillScreen"] = "Graphics_FillScreen",
            ["Graphics.SetPixel"] = "Graphics_SetPixel",
            ["Graphics.DrawRect"] = "Graphics_DrawRect",
            ["Graphics.FillRect"] = "Graphics_FillRect",
            ["Graphics.DrawLine"] = "Graphics_DrawLine",
            ["Graphics.DrawCircle"] = "Graphics_DrawCircle",
            ["Graphics.FillCircle"] = "Graphics_FillCircle",
            ["Graphics.DrawText"] = "Graphics_DrawText",
            ["Graphics.DrawChar"] = "Graphics_DrawChar",
            ["Graphics.MeasureTextWidth"] = "Graphics_MeasureTextWidth",
            ["Graphics.MeasureTextHeight"] = "Graphics_MeasureTextHeight",
            ["Graphics.DrawTexture"] = "Graphics_DrawTexture",
            ["Graphics.DrawTriangle"] = "Graphics_DrawTriangle",
            ["Graphics.FillTriangle"] = "Graphics_FillTriangle",
            ["Graphics.DrawEllipse"] = "Graphics_DrawEllipse",
            ["Graphics.FillEllipse"] = "Graphics_FillEllipse",
            ["Graphics.DrawRoundedRect"] = "Graphics_DrawRoundedRect",
            ["Graphics.FillRoundedRect"] = "Graphics_FillRoundedRect",
            ["Graphics.SetPixelAlpha"] = "Graphics_SetPixelAlpha",
            ["Graphics.FillRectAlpha"] = "Graphics_FillRectAlpha",
            ["Graphics.DrawTextAlpha"] = "Graphics_DrawTextAlpha",
            ["Graphics.DrawTextShadow"] = "Graphics_DrawTextShadow",
            ["Graphics.DrawGrid"] = "Graphics_DrawGrid",
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