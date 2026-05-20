using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Handles DateTime static helpers and Stopwatch method calls.
///
/// DateTime.Now.Ticks → CS2SX_DateTime_Now_Ticks()   (as long long)
/// Individual DateTime.Now.* properties are handled in ExpressionWriter.WriteMemberAccess.
///
/// Stopwatch.StartNew()   → CS2SX_Stopwatch_StartNew()
/// sw.Start()             → CS2SX_Stopwatch_Start(sw)
/// sw.Stop()              → CS2SX_Stopwatch_Stop(sw)
/// sw.Reset()             → CS2SX_Stopwatch_Reset(sw)
/// sw.Restart()           → CS2SX_Stopwatch_Restart(sw)
/// </summary>
public sealed class DateTimeHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        // ── DateTime static ───────────────────────────────────────────────────
        if (calleeStr == "DateTime.Now.ToString" || calleeStr == "DateTimeOffset.Now.ToString")
        {
            // DateTime.Now.ToString("HH:mm:ss") → formatted via strftime
            if (args.Count > 0)
            {
                // Map common .NET format strings to strftime patterns
                var fmtArg = inv.ArgumentList.Arguments[0].Expression;
                var fmtStr = fmtArg is LiteralExpressionSyntax lit ? lit.Token.ValueText : null;
                var strftimeFmt = fmtStr != null ? MapDateFormat(fmtStr) : "%Y-%m-%d %H:%M:%S";
                var buf = ctx.NextStringBuf();
                ctx.Out.WriteLine(ctx.Tab
                    + $"strftime({buf}, sizeof({buf}), \"{strftimeFmt}\", _cs2sx_now());");
                result = buf;
                return true;
            }
            else
            {
                var buf = ctx.NextStringBuf();
                ctx.Out.WriteLine(ctx.Tab
                    + $"strftime({buf}, sizeof({buf}), \"%Y-%m-%d %H:%M:%S\", _cs2sx_now());");
                result = buf;
                return true;
            }
        }

        // ── TimeSpan static factories ─────────────────────────────────────────
        switch (calleeStr)
        {
            case "TimeSpan.FromMilliseconds":
                result = "CS2SX_TimeSpan_FromMs(" + (args.Count > 0 ? args[0] : "0") + ")";
                return true;
            case "TimeSpan.FromSeconds":
                result = "CS2SX_TimeSpan_FromSec(" + (args.Count > 0 ? args[0] : "0") + ")";
                return true;
            case "TimeSpan.FromMinutes":
                result = "CS2SX_TimeSpan_FromSec((" + (args.Count > 0 ? args[0] : "0") + ") * 60.0)";
                return true;
            case "TimeSpan.FromHours":
                result = "CS2SX_TimeSpan_FromSec((" + (args.Count > 0 ? args[0] : "0") + ") * 3600.0)";
                return true;
            case "TimeSpan.FromTicks":
                result = "CS2SX_TimeSpan_FromTicks(" + (args.Count > 0 ? args[0] : "0") + ")";
                return true;
            case "TimeSpan.Zero":
                result = "CS2SX_TimeSpan_FromTicks(0)";
                return true;
        }

        // ── Stopwatch static factory ──────────────────────────────────────────
        if (calleeStr == "Stopwatch.StartNew")
        {
            result = "CS2SX_Stopwatch_StartNew()";
            return true;
        }

        // ── Stopwatch instance methods ─────────────────────────────────────────
        if (inv.Expression is not MemberAccessExpressionSyntax mem)
            return NotHandled(out result);

        var methodName = mem.Name.Identifier.Text;
        if (methodName is not ("Start" or "Stop" or "Reset" or "Restart"))
            return NotHandled(out result);

        // Verify receiver is a Stopwatch type
        var rawReceiver = mem.Expression.ToString();
        var receiverKey = rawReceiver.TrimStart('_');
        string? receiverType = null;
        ctx.LocalTypes.TryGetValue(rawReceiver, out receiverType);
        if (receiverType == null) ctx.FieldTypes.TryGetValue(receiverKey, out receiverType);
        if (receiverType == null) receiverType = ctx.GetSemanticType(mem.Expression);

        if (receiverType != "Stopwatch")
            return NotHandled(out result);

        var swObj = writeExpr(mem.Expression);
        result = methodName switch
        {
            "Start"   => "CS2SX_Stopwatch_Start("   + swObj + ")",
            "Stop"    => "CS2SX_Stopwatch_Stop("    + swObj + ")",
            "Reset"   => "CS2SX_Stopwatch_Reset("   + swObj + ")",
            "Restart" => "CS2SX_Stopwatch_Restart(" + swObj + ")",
            _         => "/* Stopwatch." + methodName + " not implemented */"
        };
        return true;
    }

    private static string MapDateFormat(string dotNetFmt) => dotNetFmt
        .Replace("yyyy", "%Y").Replace("yy", "%y")
        .Replace("MM", "%m").Replace("dd", "%d")
        .Replace("HH", "%H").Replace("hh", "%I")
        .Replace("mm", "%M").Replace("ss", "%S")
        .Replace("tt", "%p");
}
