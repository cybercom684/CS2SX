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

using CS2SX.Core;
using CS2SX.Transpiler;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        if (calleeStr.Length == 0 || (!char.IsLetter(calleeStr[0]) && calleeStr[0] != '_'))
            return NotHandled(out result);

        if (s_cBuiltins.Contains(calleeStr))
            return NotHandled(out result);

        // SemanticModel check first — it is authoritative over any using-static heuristic.
        // A "using static EditorController" must not shadow methods on the current class.
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
                    var callArgs = BuildArgsWithRefOut(inv, method, args, ctx, writeExpr);
                    var cName = CSharpToC.BuildCMethodName(ctx.CurrentClass, calleeStr,
                        method.Parameters.Length, ctx.OverloadedMethods);

                    if (isStatic)
                        result = $"{cName}({string.Join(", ", callArgs)})";
                    else if (callArgs.Count > 0)
                        result = $"{cName}(self, {string.Join(", ", callArgs)})";
                    else
                        result = $"{cName}(self)";
                    return true;
                }

                // Inherited instance method from a base class — emit BaseType_Method((BaseType*)self, args)
                if (symbol is IMethodSymbol inheritedMethod
                    && !inheritedMethod.IsStatic
                    && inheritedMethod.ContainingType?.Name != ctx.CurrentClass)
                {
                    var baseType = inheritedMethod.ContainingType?.Name ?? ctx.CurrentClass;
                    var callArgs = BuildArgsWithRefOut(inv, inheritedMethod, args, ctx, writeExpr);
                    var selfExpr = $"({baseType}*)self";
                    var cName = CSharpToC.BuildCMethodName(baseType, calleeStr,
                        inheritedMethod.Parameters.Length, ctx.OverloadedMethods);
                    result = callArgs.Count > 0
                        ? $"{cName}({selfExpr}, {string.Join(", ", callArgs)})"
                        : $"{cName}({selfExpr})";
                    return true;
                }

                if (symbol is IMethodSymbol staticMethod
                    && staticMethod.IsStatic
                    && staticMethod.ContainingType?.Name != ctx.CurrentClass)
                    return NotHandled(out result);

                // Delegate field/property invocation: self->field() or self->f_field()
                // Use the stripped name (no leading _) for the field access prefix logic.
                var strippedCallee = calleeStr.TrimStart('_');
                if (symbol is IPropertySymbol propSym)
                {
                    var propTypeName = TranspilerContext.FormatTypeSymbol(propSym.Type);
                    if (TypeRegistry.IsDelegate(propTypeName))
                    {
                        var pfx = TypeRegistry.HasNoPrefix(strippedCallee) ? "" : "f_";
                        var callArgStr = args.Count > 0 ? string.Join(", ", args) : "";
                        result = $"self->{pfx}{strippedCallee}({callArgStr})";
                        return true;
                    }
                    return NotHandled(out result);
                }
                if (symbol is IFieldSymbol fieldSym)
                {
                    var fieldTypeName = TranspilerContext.FormatTypeSymbol(fieldSym.Type);
                    if (TypeRegistry.IsDelegate(fieldTypeName))
                    {
                        var pfx = TypeRegistry.HasNoPrefix(strippedCallee) ? "" : "f_";
                        var callArgStr = args.Count > 0 ? string.Join(", ", args) : "";
                        result = $"self->{pfx}{strippedCallee}({callArgStr})";
                        return true;
                    }
                    return NotHandled(out result);
                }

                if (symbol is not IMethodSymbol)
                    return NotHandled(out result);
            }
            catch { }
        }

        // No SemanticModel: check FieldTypes for delegate fields (also try trimmed _ prefix)
        var calleeKey = calleeStr.TrimStart('_');
        if ((ctx.FieldTypes.TryGetValue(calleeStr, out var fieldDelegateType)
          || ctx.FieldTypes.TryGetValue(calleeKey, out fieldDelegateType))
            && TypeRegistry.IsDelegate(fieldDelegateType))
        {
            var pfx = TypeRegistry.HasNoPrefix(calleeKey) ? "" : "f_";
            var callArgStr = args.Count > 0 ? string.Join(", ", args) : "";
            result = $"self->{pfx}{calleeKey}({callArgStr})";
            return true;
        }

        // "using static" resolution — fallback when semantic model is unavailable
        var resolvedPrefix = ctx.UsingStaticResolver.TryResolveStaticMethod(calleeStr);
        if (resolvedPrefix != null && resolvedPrefix != ctx.CurrentClass)
        {
            var syntheticCallee = resolvedPrefix + "." + calleeStr;
            result = syntheticCallee + "(" + string.Join(", ", args) + ")";
            return true;
        }

        // Heuristic: lowercase = own method
        if (char.IsUpper(calleeStr[0]))
            return NotHandled(out result);

        var selfArgs = new List<string> { "self" };
        selfArgs.AddRange(args);
        result = $"{ctx.CurrentClass}_{calleeStr}({string.Join(", ", selfArgs)})";
        return true;
    }

    private static List<string> BuildArgsWithRefOut(
        InvocationExpressionSyntax inv,
        IMethodSymbol method,
        List<string> alreadyBuiltArgs,
        TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr)
    {
        // The args list was already built by InvocationDispatcher.BuildArg which handles
        // out/ref keywords. We just need to ensure consistency with method parameter kinds.
        var result = new List<string>();
        var invArgs = inv.ArgumentList.Arguments;

        for (int i = 0; i < invArgs.Count && i < method.Parameters.Length; i++)
        {
            var param = method.Parameters[i];
            var arg = invArgs[i];
            var built = i < alreadyBuiltArgs.Count ? alreadyBuiltArgs[i] : writeExpr(arg.Expression);

            if (param.RefKind is RefKind.Ref or RefKind.Out)
            {
                // Ensure & prefix for value types not already having it
                if (!built.StartsWith("&") && !built.StartsWith("(*"))
                {
                    var csType = TypeInferrer.InferCSharpType(arg.Expression, ctx);
                    if (TypeRegistry.IsPrimitive(csType) && csType != "string")
                        result.Add("&" + built);
                    else
                        result.Add(built);
                }
                else
                    result.Add(built);
            }
            else if (param.IsParams)
            {
                result.Add(built);
                // params array: companion count argument
                int cnt = arg.Expression switch
                {
                    ImplicitArrayCreationExpressionSyntax ia => ia.Initializer?.Expressions.Count ?? 0,
                    ArrayCreationExpressionSyntax ac         => ac.Initializer?.Expressions.Count ?? 0,
                    _ => 0
                };
                result.Add(cnt.ToString());
            }
            else
            {
                result.Add(built);
            }
        }

        // Append default values for optional parameters omitted at the call site
        for (int i = result.Count; i < method.Parameters.Length; i++)
        {
            var param = method.Parameters[i];
            if (!param.IsOptional) break;
            result.Add(FormatDefaultValue(param));
        }

        return result;
    }

    private static string FormatDefaultValue(IParameterSymbol param)
    {
        if (!param.HasExplicitDefaultValue) return "0";
        return param.ExplicitDefaultValue switch
        {
            null => "NULL",
            true => "1",
            false => "0",
            string s => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
            var v => v.ToString() ?? "0"
        };
    }
}