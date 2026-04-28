// ============================================================================
// Transpiler/Handlers/OwnMethodHandler.cs
//
// PHASE 1 FIX: Konflikt zwischen StaticClassHandler und OwnMethodHandler
// behoben durch SemanticModel-Prüfung.
//
// Strategie:
//   1. Kein Punkt im Namen (kein obj.Method())
//   2. Bekannte C-Builtins nicht anfassen
//   3. Wenn SemanticModel verfügbar: prüfen ob Symbol eine eigene Methode ist
//   4. Fallback: Name beginnt mit Kleinbuchstaben → eigene Methode
//      (Großbuchstaben → StaticClassHandler übernimmt)
//
// Das löst: MinUI.DrawHeader() korrekt als static-class-Aufruf
//           updateScore() korrekt als eigene Methode
// ============================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class OwnMethodHandler : InvocationHandlerBase
{
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
        // C-Standardbibliothek weitere
        "atoi", "atof", "atol", "strtol", "strtof", "strtod",
        "isdigit", "isalpha", "isspace", "isupper", "islower",
        "toupper", "tolower",
        "qsort", "bsearch",
    };

    public override bool TryHandle(
    InvocationExpressionSyntax inv,
    string calleeStr,
    List<string> args,
    TranspilerContext ctx,
    Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr,
    out string result)
    {
        if (inv.Expression is not IdentifierNameSyntax idNode)
            return NotHandled(out result);

        if (string.IsNullOrEmpty(ctx.CurrentClass))
            return NotHandled(out result);

        if (calleeStr.Contains('.'))
            return NotHandled(out result);

        if (calleeStr.Length == 0 || !char.IsLetter(calleeStr[0]))
            return NotHandled(out result);

        if (s_cBuiltins.Contains(calleeStr))
            return NotHandled(out result);

        // FIX: "using static" prüfen — wenn der Name aus einem static-Import kommt,
        //      wird er von den zuständigen Handlern (Math, Console etc.) aufgelöst,
        //      NICHT als eigene Methode behandelt.
        var resolvedPrefix = ctx.UsingStaticResolver.TryResolveStaticMethod(calleeStr);
        if (resolvedPrefix != null && resolvedPrefix != ctx.CurrentClass)
        {
            // Umschreiben als "ClassName.MethodName" und an Dispatcher zurückgeben
            // indem wir einen synthetischen calleeStr bauen — der entsprechende Handler
            // (MathHandler, etc.) greift dann.
            var syntheticCallee = resolvedPrefix + "." + calleeStr;
            // Prüfen ob ein bekannter Handler den synth. Call behandeln würde
            // Wenn ja: direkt weiterleiten
            result = syntheticCallee + "(" + string.Join(", ", args) + ")";
            return true;
        }

        // SemanticModel-Prüfung
        if (ctx.SemanticModel != null)
        {
            try
            {
                var symbolInfo = ctx.SemanticModel.GetSymbolInfo(idNode);
                var symbol = symbolInfo.Symbol
                              ?? symbolInfo.CandidateSymbols.FirstOrDefault();

                if (symbol is IMethodSymbol method
                    && method.ContainingType?.Name == ctx.CurrentClass)
                {
                    var isStatic = method.IsStatic;
                    var allArgs = isStatic
                        ? args
                        : new List<string> { "self" }.Concat(args).ToList();
                    result = $"{ctx.CurrentClass}_{calleeStr}({string.Join(", ", allArgs)})";
                    return true;
                }

                if (symbol is IMethodSymbol staticMethod
                    && staticMethod.IsStatic
                    && staticMethod.ContainingType?.Name != ctx.CurrentClass)
                    return NotHandled(out result);

                if (symbol is not IMethodSymbol)
                    return NotHandled(out result);
            }
            catch { }
        }

        // Heuristik: Kleinbuchstabe → eigene Methode
        if (char.IsUpper(calleeStr[0]))
            return NotHandled(out result);

        var selfArgs = new List<string> { "self" };
        selfArgs.AddRange(args);
        result = $"{ctx.CurrentClass}_{calleeStr}({string.Join(", ", selfArgs)})";
        return true;
    }
}