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
// InputExtHandler
// Behandelt Stick- und Touch-Aufrufe
// ============================================================================

public sealed class InputExtHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        switch (calleeStr)
        {
            // Sticks — brauchen Zugriff auf den PadState der SwitchAppEx
            // In switchapp_ext.h wird der pad lokal gehalten; der Wrapper
            // liest direkt aus dem SwitchAppEx-Struct.
            // Wir generieren einen direkten Funktionsaufruf mit Dummy-Pad-Arg
            // der durch den Runtime-Wrapper aufgelöst wird.
            case "Input.GetStickLeft":
                result = "CS2SX_Input_GetStickLeft((PadState*)((SwitchAppEx*)self)->f_padPtr)";
                // Einfachere Alternative: globale Pad-Variable
                result = "_cs2sx_get_stick_left()";
                return true;

            case "Input.GetStickRight":
                result = "_cs2sx_get_stick_right()";
                return true;

            case "Input.GetTouch":
                result = "CS2SX_Input_GetTouch()";
                return true;

            // Stick-Norm (als statische Methode)
            case "CS2SX_StickNorm":
                result = "CS2SX_StickNorm(" + ArgAt(args, 0) + ")";
                return true;
        }

        // TouchState.HitRect — Instanzmethode auf einem TouchState-Feld
        if (inv.Expression is MemberAccessExpressionSyntax mem
            && mem.Name.Identifier.Text == "HitRect")
        {
            var objStr = mem.Expression.ToString();
            var objKey = objStr.TrimStart('_');
            bool isField = ctx.FieldTypes.ContainsKey(objKey)
                        || ctx.LocalTypes.ContainsKey(objStr);

            if (isField || objStr.Contains("touch") || objStr.Contains("Touch"))
            {
                var obj = objStr.StartsWith('_')
                    ? "self->f_" + objKey
                    : objStr;
                result = "CS2SX_Touch_HitRect(&" + obj + ", " + JoinArgs(args) + ")";
                return true;
            }
        }

        return NotHandled(out result);
    }
}