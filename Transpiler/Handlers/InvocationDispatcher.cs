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

        var args = inv.ArgumentList.Arguments
            .Select(a => BuildArg(a))
            .ToList();

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

    private string BuildArg(ArgumentSyntax a)
    {
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