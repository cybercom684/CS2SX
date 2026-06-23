using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class HttpHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            ["Http.Get"]               = "CS2SX_Http_Get",
            ["Http.Post"]              = "CS2SX_Http_Post",
            ["Http.PostJson"]          = "CS2SX_Http_PostJson",
            ["Http.IsAvailable"]       = "CS2SX_Http_IsAvailable",
            ["Http.GetLastStatusCode"] = "CS2SX_Http_GetLastStatusCode",
            ["Http.JsonInt"]           = "CS2SX_Http_JsonInt",
            ["Http.JsonFloat"]         = "CS2SX_Http_JsonFloat",
            ["Http.JsonStr"]           = "CS2SX_Http_JsonStr",
            ["Http.WeatherTemp"]       = "CS2SX_Http_WeatherTemp",
            ["Http.WeatherWind"]       = "CS2SX_Http_WeatherWind",
            ["Http.WeatherCode"]       = "CS2SX_Http_WeatherCode",
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
