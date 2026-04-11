// ============================================================================
// Transpiler/Handlers/AudioHandler.cs
//
// PHASE 3: Behandelt Audio-Aufrufe.
//
// Audio.Init(sampleRate)             → CS2SX_Audio_Init(sampleRate)
// Audio.PlayTone(freq, amp, ms)      → CS2SX_Audio_PlayTone(freq, amp, ms)
// Audio.SetVolume(vol)               → CS2SX_Audio_SetVolume(vol)
// Audio.Stop()                       → CS2SX_Audio_Stop()
// Audio.Exit()                       → CS2SX_Audio_Exit()
// ============================================================================

using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class AudioHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            ["Audio.Init"] = "CS2SX_Audio_Init",
            ["Audio.PlayTone"] = "CS2SX_Audio_PlayTone",
            ["Audio.SetVolume"] = "CS2SX_Audio_SetVolume",
            ["Audio.Stop"] = "CS2SX_Audio_Stop",
            ["Audio.Exit"] = "CS2SX_Audio_Exit",
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