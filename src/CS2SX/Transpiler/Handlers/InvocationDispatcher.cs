// ============================================================================
// CS2SX — Transpiler/Handlers/InvocationDispatcher.cs
//
// FIX: Dispatch-Miss erzeugt jetzt eine Warning statt still durchzufallen.
// FIX: async/await-Aufrufe werden erkannt und mit sinnvollem Fallback behandelt.
// ============================================================================

using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        ExtensionMethodHandler extensionHandler)
    {
        _ctx = ctx;
        _writeExpr = writeExpr;
        _handlers = BuildHandlers(extensionHandler);
    }

    public string? Dispatch(InvocationExpressionSyntax inv)
    {
        var calleeStr = inv.Expression.ToString();
        // System.Console.WriteLine → Console.WriteLine: handlers match the type name,
        // a namespace in front of it is only qualification.
        if (inv.Expression is MemberAccessExpressionSyntax { Expression: MemberAccessExpressionSyntax qualified } nsCall
            && _ctx.SemanticModel != null)
        {
            try
            {
                if (_ctx.SemanticModel.GetSymbolInfo(qualified).Symbol is INamedTypeSymbol qualifiedType
                    && qualified.Expression is var nsPart
                    && _ctx.SemanticModel.GetSymbolInfo(nsPart).Symbol is INamespaceSymbol)
                    calleeStr = qualifiedType.Name + "." + nsCall.Name;
            }
            catch { }
        }

        var rawArgs = inv.ArgumentList.Arguments
            .Select(a => BuildArg(a))
            .ToList();

        var args = ResolveNamedAndOptionalArgs(inv, PackParamsArgs(inv, rawArgs));

        // Calling a delegate value — `d(x)` or `d.Invoke(x)` — goes through the typed
        // invoker of its delegate type (all entries of a multicast list; null throws).
        var delegateCall = TryWriteDelegateInvoke(inv, args);
        if (delegateCall != null) return delegateCall;

        // GC: memory is reference-counted; GetTotalMemory reports the heap in use,
        // Collect has nothing to do (releases happen at the next frame anyway).
        if (calleeStr is "GC.GetTotalMemory") return "CS2SX_HeapUsed()";
        if (calleeStr is "GC.Collect" or "GC.WaitForPendingFinalizers") return "((void)0)";

        var qualifiedStatic = TryWriteQualifiedStaticCall(inv, args);
        if (qualifiedStatic != null) return qualifiedStatic;

        // a.CompareTo(b) on numbers/char/enums → -1 / 0 / 1 (each side evaluated once)
        if (inv.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: "CompareTo" } ct
            && args.Count == 1 && _ctx.SemanticModel != null)
        {
            try
            {
                var t = _ctx.SemanticModel.GetTypeInfo(ct.Expression).Type;
                if (t != null && (t.TypeKind == TypeKind.Enum || t.SpecialType is >= SpecialType.System_Char and <= SpecialType.System_Double))
                    return "({ __auto_type _cmp_a = (" + _writeExpr(ct.Expression) + "); __auto_type _cmp_b = ("
                         + args[0] + "); (_cmp_a > _cmp_b) - (_cmp_a < _cmp_b); })";
            }
            catch { }
        }

        // flags.HasFlag(F.B) → ((flags & F_B) == F_B)
        if (inv.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: "HasFlag" } hf
            && args.Count == 1 && _ctx.SemanticModel != null)
        {
            try
            {
                if (_ctx.SemanticModel.GetTypeInfo(hf.Expression).Type?.TypeKind == TypeKind.Enum)
                {
                    var self = _writeExpr(hf.Expression);
                    return "(((" + self + ") & (" + args[0] + ")) == (" + args[0] + "))";
                }
            }
            catch { }
        }

        foreach (var handler in _handlers)
        {
            if (handler.TryHandle(inv, calleeStr, args, _ctx, _writeExpr, out var result))
                return AdaptStringArrayResult(inv, result);
        }

        // Return null — WriteInvocation will try TryWriteDirectUserClassCall next,
        // and only warn if it truly falls through to the raw-passthrough fallback.
        return null;
    }

    /// `Game.Helper(x)` — a static method of a (non-static) user class called through
    /// the class name: a plain C function, the type name is not an argument.
    /// (Static classes are handled by the regular handlers.)
    private string? TryWriteQualifiedStaticCall(InvocationExpressionSyntax inv, List<string> args)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax ma || _ctx.SemanticModel == null)
            return null;
        try
        {
            if (_ctx.SemanticModel.GetSymbolInfo(inv).Symbol is not IMethodSymbol
                {
                    IsStatic: true, IsExtensionMethod: false, MethodKind: MethodKind.Ordinary,
                    ContainingType: { IsStatic: false, IsGenericType: false, TypeKind: TypeKind.Class } owner,
                } m
                || !TranspilerContext.IsUserCode(m)
                || _ctx.SemanticModel.GetSymbolInfo(ma.Expression).Symbol is not INamedTypeSymbol
                || inv.ArgumentList.Arguments.Any(a => !a.RefKindKeyword.IsKind(SyntaxKind.None)))
                return null;
            var cName = CSharpToC.BuildCMethodName(owner.Name, m.Name, m.Parameters.Length, _ctx.OverloadedMethods);
            return cName + "(" + string.Join(", ", args) + ")";
        }
        catch { return null; }
    }

    private string? TryWriteDelegateInvoke(InvocationExpressionSyntax inv, List<string> args)
    {
        if (_ctx.SemanticModel == null) return null;
        try
        {
            if (_ctx.SemanticModel.GetSymbolInfo(inv).Symbol is not IMethodSymbol { MethodKind: MethodKind.DelegateInvoke })
                return null;
            var target = inv.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: "Invoke" } ma
                && DelegateModel.IsDelegate(_ctx.SemanticModel.GetTypeInfo(ma.Expression).Type)
                ? ma.Expression
                : inv.Expression;
            var type = _ctx.SemanticModel.GetTypeInfo(target).Type;
            if (!DelegateModel.IsDelegate(type)) return null;
            var callArgs = new List<string> { _writeExpr(target) };
            callArgs.AddRange(args);
            return DelegateModel.InvokeName(type!) + "(" + string.Join(", ", callArgs) + ")";
        }
        catch { return null; }
    }

    /// <summary>
    /// A call in expanded <c>params</c> form — <c>Count(7, 8)</c> or <c>Count()</c> — gets
    /// its trailing arguments packed into one array, exactly as C# does. The array is a
    /// stack array with length header declared just before the statement, so the callee
    /// sees an ordinary T[] (<c>values.Length</c> works). The normal form, an array passed
    /// as a whole (<c>Count(nums)</c>), is left as is. User methods only — framework
    /// params methods (string.Format, Console.WriteLine) have dedicated handlers.
    /// </summary>
    private List<string> PackParamsArgs(InvocationExpressionSyntax inv, List<string> rawArgs)
    {
        if (_ctx.SemanticModel == null) return rawArgs;
        try
        {
            if (_ctx.SemanticModel.GetSymbolInfo(inv).Symbol is not IMethodSymbol m
                || m.Parameters.Length == 0
                || !m.Parameters[^1].IsParams
                || m.Parameters[^1].Type is not IArrayTypeSymbol paramsType
                || !TranspilerContext.IsUserCode(m.ContainingType))
                return rawArgs;

            var pIdx = m.Parameters.Length - 1;
            var invArgs = inv.ArgumentList.Arguments;
            if (invArgs.Count == m.Parameters.Length)
            {
                var last = invArgs[pIdx].Expression;
                var lastType = _ctx.SemanticModel.GetTypeInfo(last).Type;
                if (lastType is IArrayTypeSymbol || last.IsKind(SyntaxKind.NullLiteralExpression))
                    return rawArgs; // normal form
            }

            var elems = rawArgs.Skip(pIdx).ToList();
            var elemC = ArrayModel.ElemCType(TranspilerContext.FormatTypeSymbol(paramsType.ElementType));
            var tmp = _ctx.NextTmp("pa");
            _ctx.WriteLine(ArrayModel.LocalDecl(elemC, tmp, elems.Count.ToString(), elems));
            return rawArgs.Take(pIdx).Append(tmp).ToList();
        }
        catch { return rawArgs; }
    }

    // Runtime helpers that build a List_str for APIs whose C# return type is string[].
    private static readonly string[] s_listStrBuilders =
    {
        "String_Split(", "String_Split_RemoveEmpty(", "String_Split_Whitespace(",
        "CS2SX_File_ReadAllLines(", "CS2SX_Dir_GetFiles(", "CS2SX_Dir_GetDirectories(",
    };

    /// <summary>
    /// Where C# says the call returns <c>string[]</c> but the runtime helper returns a
    /// List_str, convert at the boundary so the value follows the uniform array model
    /// (<c>.Length</c>, indexing, passing to string[] parameters, foreach).
    /// </summary>
    private string AdaptStringArrayResult(InvocationExpressionSyntax inv, string result)
    {
        if (!s_listStrBuilders.Any(b => result.StartsWith(b, StringComparison.Ordinal)))
            return result;
        // The call's own return type — not the converted type, which in e.g. a foreach
        // collection position is the enumerable interface rather than string[].
        ITypeSymbol? returnType = null;
        try { returnType = _ctx.SemanticModel?.GetTypeInfo(inv).Type; }
        catch { }
        return returnType is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_String }
            ? "CS2SX_StrArr_FromList(" + result + ")"
            : result;
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

    /// List&lt;T&gt; / Array methods whose callback the List/Array handlers lift inline
    /// (a loop calling a static function) — pre-writing the lambda would create a
    /// delegate object that is never used.
    private bool IsInlineCollectionCallback(InvocationExpressionSyntax inv)
    {
        if (_ctx.SemanticModel == null) return false;
        try
        {
            if (_ctx.SemanticModel.GetSymbolInfo(inv).Symbol is not IMethodSymbol m) return false;
            var owner = m.ContainingType?.OriginalDefinition.ToDisplayString();
            return owner is "System.Collections.Generic.List<T>" or "System.Array"
                && m.Name is "ForEach" or "Sort" or "Find" or "FindAll" or "Exists"
                    or "TrueForAll" or "ConvertAll" or "FindIndex";
        }
        catch { return false; }
    }

    private string BuildArg(ArgumentSyntax a)
    {
        // Skip lifting lambdas for LINQ methods — LinqHandler accesses raw syntax directly
        // and re-lifts with the correct element type. Lifting here generates a stale
        // _lambda_N with fallback type 'int' that causes GCC errors but is never called.
        if (a.Expression is LambdaExpressionSyntax
            && a.Parent?.Parent is InvocationExpressionSyntax outerInv
            && outerInv.Expression is MemberAccessExpressionSyntax outerMem
            && (s_linqMethodNames.Contains(outerMem.Name.Identifier.Text)
                || IsInlineCollectionCallback(outerInv)))
            return "";

        // Console.Write[Line]($"...") is turned into a single printf straight from the
        // syntax by ConsoleHandler; pre-writing it here would emit a dead snprintf and
        // evaluate every interpolated expression twice.
        if (a.Expression is InterpolatedStringExpressionSyntax
            && a.Parent?.Parent is InvocationExpressionSyntax conInv
            && conInv.Expression.ToString() is "Console.WriteLine" or "Console.Write"
            && conInv.ArgumentList.Arguments.FirstOrDefault() == a)
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