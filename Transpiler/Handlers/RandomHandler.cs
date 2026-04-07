using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt System.Random Aufrufe.
///
/// System.Random.Shared.Next(min, max)  → CS2SX_Rand_Next(min, max)
/// System.Random.Shared.Next(max)       → CS2SX_Rand_NextMax(max)
/// System.Random.Shared.Next()          → CS2SX_Rand_Next(0, INT_MAX)
/// System.Random.Shared.NextFloat()     → CS2SX_Rand_Float()
/// new Random()                         → wird in ObjectCreation behandelt
/// rand.Next(...)                       → CS2SX_Rand_Next(...)
/// </summary>
public sealed class RandomHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        // System.Random.Shared.Next / System.Random.Shared.NextDouble etc.
        if (calleeStr.StartsWith("System.Random.Shared.", StringComparison.Ordinal))
        {
            var method = calleeStr["System.Random.Shared.".Length..];
            result = BuildRandomCall(method, args);
            return true;
        }

        // Random.Shared.Next(...)
        if (calleeStr.StartsWith("Random.Shared.", StringComparison.Ordinal))
        {
            var method = calleeStr["Random.Shared.".Length..];
            result = BuildRandomCall(method, args);
            return true;
        }

        // Instanz-Aufrufe: _rng.Next(...) oder rng.Next(...)
        if (inv.Expression is MemberAccessExpressionSyntax mem)
        {
            var methodName = mem.Name.Identifier.Text;
            var objRaw = mem.Expression.ToString();
            var objKey = objRaw.TrimStart('_');

            bool isRandom = ctx.LocalTypes.TryGetValue(objRaw, out var lt) && lt == "Random"
                         || ctx.FieldTypes.TryGetValue(objKey, out var ft) && ft == "Random";

            if (isRandom)
            {
                result = BuildRandomCall(methodName, args);
                return true;
            }
        }

        return NotHandled(out result);
    }

    private static string BuildRandomCall(string method, List<string> args) =>
        method switch
        {
            "Next" when args.Count == 2 => "CS2SX_Rand_Next(" + args[0] + ", " + args[1] + ")",
            "Next" when args.Count == 1 => "CS2SX_Rand_NextMax(" + args[0] + ")",
            "Next" => "CS2SX_Rand_Next(0, 32767)",
            "NextInt64" => "(long long)CS2SX_Rand_Next(0, 32767)",
            "NextDouble" => "(double)CS2SX_Rand_Float()",
            "NextSingle" => "CS2SX_Rand_Float()",
            "NextFloat" => "CS2SX_Rand_Float()",
            _ => "CS2SX_Rand_Next(0, 32767)",
        };
}