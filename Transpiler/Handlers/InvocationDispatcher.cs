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

        // FIX: async/await Erkennung — await Foo() → Foo() mit Warning
        // (Roslyn parst await als PrefixUnary, nicht als Invocation,
        //  aber manche Aufrufe wie Task.Run kommen hier an)
        if (calleeStr is "Task.Run" or "Task.Delay" or "Task.WhenAll" or "Task.WhenAny")
        {
            _ctx.Warn($"async/Task call '{calleeStr}' — executed synchronously (no threading on Switch)",
                calleeStr);
            // Task.Run(action) → einfach action() ausführen
            if (inv.ArgumentList.Arguments.Count > 0)
                return _writeExpr(inv.ArgumentList.Arguments[0].Expression) + "()";
            return "/* async not supported */";
        }

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

        // FIX: Warning bei unbekanntem Call ausgeben (außer bekannte C-Builtins)
        if (!s_silentPassthrough.Contains(calleeStr)
            && !calleeStr.StartsWith("CS2SX_", StringComparison.Ordinal)
            && !calleeStr.StartsWith("_cs2sx_", StringComparison.Ordinal))
        {
            _ctx.Warn($"unknown call '{calleeStr}' — passed through as-is, verify generated C",
                calleeStr);
        }

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
            if (typeName == "var") typeName = "int";

            var cTypeName = TypeRegistry.MapType(typeName);
            var needsPtr = TypeRegistry.NeedsPointerSuffix(typeName);
            var ptr = needsPtr ? "*" : "";

            _ctx.LocalTypes[singleDesig.Identifier.Text] = typeName;

            // Variable deklarieren falls noch nicht bekannt
            _ctx.WriteLine($"{cTypeName}{ptr} {singleDesig.Identifier.Text} = {(needsPtr ? "NULL" : "0")};");
            return "&" + singleDesig.Identifier.Text;
        }

        var expr = _writeExpr(a.Expression);
        var isRef = a.RefKindKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.RefKeyword);
        var isOut = a.RefKindKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OutKeyword);

        if (!isRef && !isOut) return expr;

        var argName = a.Expression.ToString();

        // String-Puffer → kein & (bereits Pointer)
        if (_ctx.LocalTypes.TryGetValue(argName, out var lt) && lt == "char[]")
            return expr;

        // LibNX-Structs → mit &
        if (_ctx.LocalTypes.TryGetValue(argName, out var lst)
            && TypeRegistry.IsLibNxStruct(lst))
            return "&" + expr;

        // String-Felder → kein & (const char* ist bereits Pointer)
        var fieldKey = argName.TrimStart('_');
        if (_ctx.FieldTypes.TryGetValue(fieldKey, out var ft) && ft == "string")
            return expr;

        // FIX: Für out-Parameter bei eigenen Methoden: & nur wenn nicht schon Pointer
        // Prüfe ob der Ausdruck bereits ein Pointer-Typ ist
        var resolvedType = lt ?? (_ctx.FieldTypes.TryGetValue(fieldKey, out var ft2) ? ft2 : null);
        if (resolvedType != null && TypeRegistry.NeedsPointerSuffix(resolvedType))
        {
            // Ist bereits Pointer → direkt übergeben (kein &)
            return expr;
        }

        return "&" + expr;
    }
}