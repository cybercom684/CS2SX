// ============================================================================
// Transpiler/Handlers/AudioHandler.cs — Maps Audio.* C# calls → C functions
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
            // Core
            ["Audio.Init"]          = "CS2SX_Audio_Init",
            ["Audio.Update"]        = "CS2SX_Audio_Update",
            ["Audio.SetVolume"]     = "CS2SX_Audio_SetVolume",
            ["Audio.Stop"]          = "CS2SX_Audio_Stop",
            ["Audio.Exit"]          = "CS2SX_Audio_Exit",

            // Sine synthesizer
            ["Audio.PlayTone"]      = "CS2SX_Audio_PlayTone",

            // WAV playback
            ["Audio.LoadWav"]       = "CS2SX_Audio_LoadWav",
            ["Audio.UnloadSound"]   = "CS2SX_Audio_UnloadSound",
            ["Audio.PlaySound"]     = "CS2SX_Audio_PlaySound",
            ["Audio.StopInstance"]  = "CS2SX_Audio_StopInstance",
            ["Audio.StopSound"]     = "CS2SX_Audio_StopSound",
            ["Audio.StopAllSounds"] = "CS2SX_Audio_StopAllSounds",
            ["Audio.IsPlaying"]     = "CS2SX_Audio_IsPlaying",

            // Effects
            ["Audio.SetLowPass"]    = "CS2SX_Audio_SetLowPass",
            ["Audio.SetEcho"]       = "CS2SX_Audio_SetEcho",
            ["Audio.ClearEffects"]  = "CS2SX_Audio_ClearEffects",

            // Wavetable synth: oscillator config
            ["Audio.SetOscA"]       = "CS2SX_Audio_SetOscA",
            ["Audio.SetOscB"]       = "CS2SX_Audio_SetOscB",
            ["Audio.SetSub"]        = "CS2SX_Audio_SetSub",

            // Wavetable synth: envelope + filter
            ["Audio.SetADSR"]       = "CS2SX_Audio_SetADSR",
            ["Audio.SetFilter"]     = "CS2SX_Audio_SetFilter",
            ["Audio.SetFilterEnv"]  = "CS2SX_Audio_SetFilterEnv",

            // Wavetable synth: LFO + pitch
            ["Audio.SetLFO"]        = "CS2SX_Audio_SetLFO",
            ["Audio.SetPitchBend"]  = "CS2SX_Audio_SetPitchBend",

            // Wavetable synth: note control
            ["Audio.PlayNote"]      = "CS2SX_Audio_PlayNote",
            ["Audio.ReleaseNote"]   = "CS2SX_Audio_ReleaseNote",
            ["Audio.ReleaseAll"]    = "CS2SX_Audio_ReleaseAll",
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
