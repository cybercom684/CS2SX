// ============================================================================
// Transpiler/Handlers/OwnMethodHandler.cs
//
// Behandelt Aufrufe eigener Methoden innerhalb der selben Klasse.
// z.B. buildHeader("title") → TouchDemoApp_buildHeader(self, "title")
//
// FIX: Vorher wurde geprüft !char.IsUpper(calleeStr[0]) — das schloss
// alle lowercase Methoden (buildHeader, updateLogic, drawUI etc.) aus.
// Der Check war gedacht um Konflikte mit C-Stdlib-Funktionen zu vermeiden,
// aber er war zu aggressiv. Korrekte Bedingung: der Name darf keine
// Punkte enthalten (kein Member-Access) und muss in MethodReturnTypes
// oder als Methode der aktuellen Klasse bekannt sein.
//
// Neue Strategie:
//   1. Kein Punkt im Namen (kein obj.Method())
//   2. Name ist kein bekanntes C-Keyword oder Stdlib-Funktion
//   3. CurrentClass ist gesetzt (wir sind in einer Klasse)
//   4. Name beginnt mit Buchstabe (kein _ oder Zahl)
// ============================================================================

using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class OwnMethodHandler : InvocationHandlerBase
{
    // Bekannte C-Stdlib und libnx Funktionen die NICHT als eigene Methoden
    // behandelt werden dürfen — verhindert false positives.
    private static readonly HashSet<string> s_cBuiltins = new(StringComparer.Ordinal)
    {
        "printf", "sprintf", "snprintf", "fprintf", "puts", "putchar",
        "malloc", "calloc", "realloc", "free",
        "memset", "memcpy", "memmove", "strlen", "strcmp", "strncmp",
        "strcpy", "strncpy", "strcat", "strncat", "strstr", "strchr",
        "abs", "sqrtf", "sinf", "cosf", "powf", "floorf", "ceilf",
        "rand", "srand", "exit", "abort",
        "fopen", "fclose", "fread", "fwrite", "fseek", "ftell",
        "setjmp", "longjmp",
        // libnx
        "padUpdate", "padGetButtonsDown", "padGetButtons",
        "framebufferBegin", "framebufferEnd", "appletMainLoop",
        "consoleInit", "consoleUpdate", "consoleClear", "consoleExit",
        "hidGetTouchScreenStates", "padGetStickPos",
        "psmGetBatteryChargePercentage", "psmGetChargerType",
        "psmInitialize", "psmExit",
        "fsOpenSdCardFileSystem", "fsFsClose",
    };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        // Muss ein einfacher Identifier sein (kein obj.Method())
        if (inv.Expression is not IdentifierNameSyntax)
            return NotHandled(out result);

        // Muss in einer Klasse sein
        if (string.IsNullOrEmpty(ctx.CurrentClass))
            return NotHandled(out result);

        // Kein Punkt → kein Member-Access
        if (calleeStr.Contains('.'))
            return NotHandled(out result);

        // Muss mit Buchstabe beginnen
        if (calleeStr.Length == 0 || !char.IsLetter(calleeStr[0]))
            return NotHandled(out result);

        // Bekannte C-Builtins nicht anfassen
        if (s_cBuiltins.Contains(calleeStr))
            return NotHandled(out result);

        // Alles andere → eigene Methode der Klasse
        var allArgs = new List<string> { "self" };
        allArgs.AddRange(args);
        result = ctx.CurrentClass + "_" + calleeStr + "(" + string.Join(", ", allArgs) + ")";
        return true;
    }
}