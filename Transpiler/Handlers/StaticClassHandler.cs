using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt Aufrufe auf static-Klassen wie MinUI.DrawHeader(...).
/// MinUI.DrawHeader("title", preset) → MinUI_DrawHeader("title", preset)
///
/// Greift wenn:
///   - Receiver beginnt mit Großbuchstaben
///   - Receiver ist keine bekannte API (Graphics, Input, Console etc.)
///   - Receiver ist keine lokale Variable und kein Feld
/// </summary>
public sealed class StaticClassHandler : InvocationHandlerBase
{
    private static readonly HashSet<string> s_knownApis = new(StringComparer.Ordinal)
    {
        "Graphics", "Input", "Console", "File", "Directory", "Path",
        "System", "Math", "Color", "Form", "LibNX",
        "string", "String", "int", "Int32", "float", "Single",
    };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem)
            return NotHandled(out result);

        var receiverName = mem.Expression.ToString();
        var methodName = mem.Name.Identifier.Text;

        if (receiverName.Contains('.'))
            return NotHandled(out result);

        if (receiverName.Length == 0 || !char.IsUpper(receiverName[0]))
            return NotHandled(out result);

        if (s_knownApis.Contains(receiverName))
            return NotHandled(out result);

        if (ctx.LocalTypes.ContainsKey(receiverName))
            return NotHandled(out result);

        var fieldKey = receiverName.TrimStart('_');
        if (ctx.FieldTypes.ContainsKey(fieldKey))
            return NotHandled(out result);

        result = receiverName + "_" + methodName + "(" + JoinArgs(args) + ")";
        return true;
    }
}