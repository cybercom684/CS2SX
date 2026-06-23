// ============================================================================
// CS2SX — Transpiler/Handlers/InvocationDispatcher.cs
//
// FIX: Dispatch-Miss erzeugt jetzt eine Warning statt still durchzufallen.
// FIX: async/await-Aufrufe werden erkannt und mit sinnvollem Fallback behandelt.
// ============================================================================

using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Orchestriert alle IInvocationHandler in Prioritäts-Reihenfolge.
/// </summary>
public sealed class InvocationDispatcher
{
    private readonly IReadOnlyList<IInvocationHandler> _handlers;
    private readonly TranspilerContext _ctx;
    private readonly Func<SyntaxNode?, string> _writeExpr;
    private GenericMethodExpander? _genericMethodExpander;

    // LINQ methods whose lambda arguments must NOT be pre-evaluated by BuildArg.
    // LinqHandler accesses the lambda directly from raw syntax and lifts it with the
    // correct element type hint. Pre-evaluating here generates a stale _lambda_N with
    // fallback type 'int' that compiles (causing GCC errors) but is never called.
    private static readonly HashSet<string> s_linqMethodNames = new(StringComparer.Ordinal)
    {
        "Where", "Select", "First", "FirstOrDefault", "Last", "LastOrDefault",
        "Any", "All", "Count", "Sum", "Min", "Max", "Average", "Aggregate",
        "ToList", "ToArray", "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending",
        "Contains", "Distinct", "Skip", "Take", "Concat", "Reverse",
        "Single", "SingleOrDefault", "ElementAt", "ElementAtOrDefault",
        "ToDictionary", "ToHashSet", "GroupBy", "Zip",
        "TakeWhile", "SkipWhile", "SelectMany",
        "Except", "Intersect", "Union", "Join", "OfType", "Cast", "DefaultIfEmpty",
    };

    // Bekannte C-Builtins die NICHT gewarnt werden sollen
    private static readonly HashSet<string> s_silentPassthrough = new(StringComparer.Ordinal)
    {
        "printf", "sprintf", "snprintf", "fprintf", "puts",
        "malloc", "calloc", "realloc", "free",
        "memset", "memcpy", "memmove",
        "strlen", "strcmp", "strncmp", "strcpy", "strncpy",
        "strstr", "strchr", "strcat",
        "abs", "sqrtf", "sinf", "cosf", "powf", "floorf", "ceilf",
        "rand", "srand", "exit",
        "padUpdate", "padGetButtonsDown", "padGetButtons",
        "framebufferBegin", "framebufferEnd", "appletMainLoop",
        "consoleInit", "consoleUpdate", "consoleClear", "consoleExit",
        "setjmp", "longjmp",
        "atoi", "atof", "strtol",
    };

    private static List<IInvocationHandler> BuildHandlers(
        ExtensionMethodHandler? extensionHandler)
    {
        return new List<IInvocationHandler>
        {
            new LibNxHandler(),
            new AsyncHandler(),        // FIX: async/await Fallback
            new EnvironmentHandler(),
            new InputHandler(),
            new FormHandler(),
            new ConsoleHandler(),
            new MathHandler(),
            new CharHandler(),
            new RandomHandler(),
            new FileHandler(),
            new ParseHandler(),
            new ColorHandler(),
            new StringBuilderHandler(),
            new ListHandler(),
            new LinqHandler(),
            new DictionaryHandler(),
            new StackQueueHandler(),
            new ConvertHandler(),
            new DateTimeHandler(),
            new ArrayHandler(),
            new EnumHandler(),
            new BitConverterHandler(),
            new RegexHandler(),
            new StringMethodHandler(),
            new FieldMethodHandler(),
            new GraphicsHandler(),
            new GraphicsExtHandler(),
            new InputExtHandler(),
            new DirectoryExtHandler(),
            new PathHandler(),
            new SystemExtHandler(),
            new VibrationHandler(),
            new MotionHandler(),
            new KeyboardHandler(),
            new SaveDataHandler(),
            new HttpHandler(),
            new AudioHandler(),
            extensionHandler ?? new ExtensionMethodHandler(),
            new StaticClassHandler(),
            new OwnMethodHandler(),
        };
    }

    public InvocationDispatcher(
     TranspilerContext ctx,
     Func<SyntaxNode?, string> writeExpr,
     ExtensionMethodHandler extensionHandler,
     GenericMethodExpander? genericMethodExpander = null)
    {
        _ctx = ctx;
        _writeExpr = writeExpr;
        _handlers = BuildHandlers(extensionHandler);
        _genericMethodExpander = genericMethodExpander;
    }


    public InvocationDispatcher(
        TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr,
        ExtensionMethodHandler extensionHandler)
    {
        _ctx = ctx;
        _writeExpr = writeExpr;
        _handlers = BuildHandlers(extensionHandler);
    }

    public string? Dispatch(InvocationExpressionSyntax inv)
    {
        var calleeStr = inv.Expression.ToString();

        var rawArgs = inv.ArgumentList.Arguments
            .Select(a => BuildArg(a))
            .ToList();

        var args = ResolveNamedAndOptionalArgs(inv, rawArgs);

        if (inv.Expression is GenericNameSyntax genName && _genericMethodExpander != null)
        {
            var typeArgs = genName.TypeArgumentList.Arguments
                .Select(a => a.ToString().Trim())
                .ToList();
            var parts = calleeStr.Split('.');
            var cn = parts.Length > 1 ? string.Join(".", parts[..^1]) : _ctx.CurrentClass;
            var mn = parts[^1].Contains('<') ? parts[^1][..parts[^1].IndexOf('<')] : parts[^1];

            var resolved = _genericMethodExpander.TryResolve(cn, mn, typeArgs,
                out var specCode);
            if (resolved != null)
            {
                if (!string.IsNullOrEmpty(specCode))
                    _ctx.PendingLambdaPreludes.Add(specCode);
                var argStr = string.Join(", ", args);
                return resolved + "(" + argStr + ")";
            }
        }

        foreach (var handler in _handlers)
        {
            if (handler.TryHandle(inv, calleeStr, args, _ctx, _writeExpr, out var result))
                return result;
        }

        // Return null — WriteInvocation will try TryWriteDirectUserClassCall next,
        // and only warn if it truly falls through to the raw-passthrough fallback.
        return null;
    }

    /// <summary>
    /// Reorders named arguments to positional order and injects optional parameter defaults.
    /// Falls back to the original arg list if SemanticModel is unavailable or resolution fails.
    /// </summary>
    private List<string> ResolveNamedAndOptionalArgs(
        InvocationExpressionSyntax inv, List<string> rawArgs)
    {
        if (_ctx.SemanticModel == null) return rawArgs;

        bool hasNamed = inv.ArgumentList.Arguments.Any(a => a.NameColon != null);

        Microsoft.CodeAnalysis.IMethodSymbol? sym = null;
        try
        {
            sym = _ctx.SemanticModel.GetSymbolInfo(inv).Symbol
                  as Microsoft.CodeAnalysis.IMethodSymbol;
        }
        catch { }

        if (sym == null) return rawArgs;

        var parameters = sym.Parameters;

        // Apply interface upcasts even when no reordering is needed:
        // if a param expects IFace* but arg provides ConcreteClass*, wrap with ConcreteClass_as_IFace()
        var needsUpcast = false;
        if (_ctx.SemanticModel != null)
        {
            try
            {
                for (int i = 0; i < Math.Min(parameters.Length, inv.ArgumentList.Arguments.Count); i++)
                {
                    var paramType = TranspilerContext.FormatTypeSymbol(parameters[i].Type);
                    if (!TypeRegistry.IsRegisteredInterface(paramType)) continue;
                    var argSyn = inv.ArgumentList.Arguments[i].Expression;
                    var argTypeSym = _ctx.SemanticModel.GetTypeInfo(argSyn).Type;
                    if (argTypeSym == null || argTypeSym is Microsoft.CodeAnalysis.IErrorTypeSymbol) continue;
                    var argType = TranspilerContext.FormatTypeSymbol(argTypeSym);
                    if (TypeRegistry.IsRegisteredInterface(argType)) continue;
                    // Concrete class passed to interface param — needs upcast
                    needsUpcast = true;
                    break;
                }
            }
            catch { }
        }

        // No reordering needed if no named args and all params are positional (and no upcasts)
        if (!hasNamed && rawArgs.Count >= parameters.Length && !needsUpcast) return rawArgs;
        // No optional params and no named args and no upcasts → nothing to do
        if (!hasNamed && !parameters.Any(p => p.HasExplicitDefaultValue) && !needsUpcast) return rawArgs;

        try
        {
            // Build result array pre-filled with defaults
            var result = new string[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                result[i] = parameters[i].HasExplicitDefaultValue
                    ? FormatDefaultValue(parameters[i].ExplicitDefaultValue, parameters[i].Type)
                    : (rawArgs.Count > i ? rawArgs[i] : "0 /* missing arg */");
            }

            // Fill actual args (respecting NameColon)
            for (int i = 0; i < inv.ArgumentList.Arguments.Count; i++)
            {
                var arg = inv.ArgumentList.Arguments[i];
                if (arg.NameColon != null)
                {
                    var paramName = arg.NameColon.Name.Identifier.Text;
                    for (int j = 0; j < parameters.Length; j++)
                    {
                        if (parameters[j].Name == paramName)
                        {
                            result[j] = rawArgs[i];
                            break;
                        }
                    }
                }
                else if (i < result.Length)
                {
                    result[i] = rawArgs[i];
                }
            }

            // Apply interface upcasts: ConcreteClass* → IFace* conversion via as_IFace()
            if (needsUpcast && _ctx.SemanticModel != null)
            {
                for (int i = 0; i < Math.Min(parameters.Length, inv.ArgumentList.Arguments.Count); i++)
                {
                    var paramType = TranspilerContext.FormatTypeSymbol(parameters[i].Type);
                    if (!TypeRegistry.IsRegisteredInterface(paramType)) continue;
                    var argSyn = inv.ArgumentList.Arguments[i].Expression;
                    var argTypeSym = _ctx.SemanticModel.GetTypeInfo(argSyn).Type;
                    if (argTypeSym == null || argTypeSym is Microsoft.CodeAnalysis.IErrorTypeSymbol) continue;
                    var argType = TranspilerContext.FormatTypeSymbol(argTypeSym);
                    if (TypeRegistry.IsRegisteredInterface(argType)) continue;
                    result[i] = argType + "_as_" + paramType + "(" + result[i] + ")";
                }
            }

            return result.ToList();
        }
        catch { return rawArgs; }
    }

    private static string FormatDefaultValue(object? value, Microsoft.CodeAnalysis.ITypeSymbol type)
    {
        if (value == null) return "NULL";
        return value switch
        {
            bool b   => b ? "1" : "0",
            string s => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
            float f  => f.ToString("G", System.Globalization.CultureInfo.InvariantCulture) + "f",
            double d => d.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
            _        => value.ToString() ?? "0",
        };
    }

    private string BuildArg(ArgumentSyntax a)
    {
        // Skip lifting lambdas for LINQ methods — LinqHandler accesses raw syntax directly
        // and re-lifts with the correct element type. Lifting here generates a stale
        // _lambda_N with fallback type 'int' that causes GCC errors but is never called.
        if (a.Expression is LambdaExpressionSyntax
            && a.Parent?.Parent is InvocationExpressionSyntax outerInv
            && outerInv.Expression is MemberAccessExpressionSyntax outerMem
            && s_linqMethodNames.Contains(outerMem.Name.Identifier.Text))
            return "";

        // out var x → Deklaration + Adresse
        if (a.RefKindKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OutKeyword)
            && a.Expression is DeclarationExpressionSyntax declExpr
            && declExpr.Designation is SingleVariableDesignationSyntax singleDesig)
        {
            var typeName = declExpr.Type.ToString().Trim();

            // "var" → Typ aus SemanticModel ableiten; Fallback: int
            if (typeName == "var")
            {
                if (_ctx.SemanticModel != null)
                {
                    try
                    {
                        var typeInfo = _ctx.SemanticModel.GetTypeInfo(declExpr.Type);
                        var sym = typeInfo.ConvertedType ?? typeInfo.Type;
                        if (sym != null && sym is not Microsoft.CodeAnalysis.IErrorTypeSymbol)
                            typeName = sym.ToDisplayString();
                    }
                    catch { }
                }
                if (typeName == "var") typeName = "int";
            }

            var cTypeName = TypeRegistry.MapType(typeName);
            var needsPtr = !cTypeName.EndsWith("*") && TypeRegistry.NeedsPointerSuffix(typeName);
            var ptr = needsPtr ? "*" : "";

            _ctx.LocalTypes[singleDesig.Identifier.Text] = typeName;
            _ctx.WriteLine($"{cTypeName}{ptr} {singleDesig.Identifier.Text} = {(needsPtr ? "NULL" : "0")};");
            return "&" + singleDesig.Identifier.Text;
        }

        var expr = _writeExpr(a.Expression);
        var isRef = a.RefKindKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.RefKeyword);
        var isOut = a.RefKindKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OutKeyword);

        if (!isRef && !isOut) return expr;

        var argName = a.Expression.ToString();

        // Single lookup — covers all three cases below
        _ctx.LocalTypes.TryGetValue(argName, out var lt);

        // String-Puffer → kein & (char[] ist bereits Pointer)
        if (lt == "char[]") return expr;

        // LibNX-Structs → mit & (Wert-Typ, muss per Pointer übergeben werden)
        if (lt != null && TypeRegistry.IsLibNxStruct(lt)) return "&" + expr;

        // String-Felder → kein & (const char* ist bereits Pointer)
        var fieldKey = argName.TrimStart('_');
        if (_ctx.FieldTypes.TryGetValue(fieldKey, out var ft) && ft == "string")
            return expr;

        // Für out/ref-Parameter: & nur wenn der Typ kein Pointer ist
        var resolvedType = lt ?? (_ctx.FieldTypes.TryGetValue(fieldKey, out var ft2) ? ft2 : null);
        if (resolvedType != null && TypeRegistry.NeedsPointerSuffix(resolvedType))
            return expr;  // Ist bereits Pointer → direkt übergeben

        return "&" + expr;
    }
}