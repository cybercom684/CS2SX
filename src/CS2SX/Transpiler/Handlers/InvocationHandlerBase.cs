using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public abstract class InvocationHandlerBase : IInvocationHandler
{
    public abstract bool TryHandle(
        InvocationExpressionSyntax inv,
        string calleeStr,
        List<string> args,
        TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr,
        out string result);

    // ── Argument-Hilfsmethoden ────────────────────────────────────────────

    protected static string ArgAt(List<string> args, int index) =>
        index < args.Count ? args[index] : "\"\"";

    protected static string JoinArgs(List<string> args) =>
        string.Join(", ", args);

    protected static string JoinArgsTail(List<string> args) =>
        string.Join(", ", args.Skip(1));

    // ── Typ-Lookup ────────────────────────────────────────────────────────

    protected static string? LookupType(string objStr, TranspilerContext ctx)
    {
        var objKey = objStr.TrimStart('_');
        if (ctx.LocalTypes.TryGetValue(objStr, out var lt)) return lt;
        if (ctx.FieldTypes.TryGetValue(objKey, out var ft)) return ft;
        return null;
    }

    protected static string BuildObjectExpr(string objStr, TranspilerContext ctx)
    {
        if (ctx.LocalTypes.ContainsKey(objStr)) return objStr;
        return "self->f_" + objStr.TrimStart('_');
    }

    // ── Collection-Lookup ─────────────────────────────────────────────────

    protected static bool TryResolveList(
        string objStr, TranspilerContext ctx,
        out string listType, out string listExpr)
    {
        var objKey = objStr.TrimStart('_');

        if (ctx.LocalTypes.TryGetValue(objStr, out var lt) && TypeRegistry.IsList(lt))
        {
            listType = lt;
            listExpr = objStr;
            return true;
        }

        if (ctx.FieldTypes.TryGetValue(objKey, out var ft) && TypeRegistry.IsList(ft))
        {
            listType = ft;
            listExpr = "self->f_" + objKey;
            return true;
        }

        listType = null!;
        listExpr = null!;
        return false;
    }

    protected static bool TryResolveDict(
        string objStr, TranspilerContext ctx,
        out string dictType, out string dictExpr)
    {
        var objKey = objStr.TrimStart('_');

        if (ctx.LocalTypes.TryGetValue(objStr, out var lt) && TypeRegistry.IsDictionary(lt))
        {
            dictType = lt;
            dictExpr = objStr;
            return true;
        }

        if (ctx.FieldTypes.TryGetValue(objKey, out var ft) && TypeRegistry.IsDictionary(ft))
        {
            dictType = ft;
            dictExpr = "self->f_" + objKey;
            return true;
        }

        dictType = null!;
        dictExpr = null!;
        return false;
    }

    protected static bool TryResolveStringBuilder(
        string objStr, TranspilerContext ctx,
        out string sbExpr)
    {
        var objKey = objStr.TrimStart('_');

        if (ctx.LocalTypes.TryGetValue(objStr, out var lt) && TypeRegistry.IsStringBuilder(lt))
        {
            sbExpr = objStr;
            return true;
        }

        if (ctx.FieldTypes.TryGetValue(objKey, out var ft) && TypeRegistry.IsStringBuilder(ft))
        {
            sbExpr = "self->f_" + objKey;
            return true;
        }

        sbExpr = null!;
        return false;
    }

    // ── C-Funktionsnamen ──────────────────────────────────────────────────

    /// <summary>
    /// Gibt den C-Funktionspräfix für ein List-Makro zurück.
    /// List&lt;string&gt; → "List_str", List&lt;int&gt; → "List_int"
    /// </summary>
    protected static string ListFuncPrefix(string listType)
    {
        var inner = TypeRegistry.GetListInnerType(listType)!;
        // string → "str" (nicht "char") damit List_str_Add/Get/etc. korrekt ist
        var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
        return "List_" + cInner;
    }

    /// <summary>
    /// Gibt den C-Funktionspräfix für ein Dict-Makro zurück.
    /// Dictionary&lt;string,int&gt; → "Dict_str_int"
    /// </summary>
    protected static string DictFuncPrefix(string dictType)
    {
        var types = TypeRegistry.GetDictionaryTypes(dictType)!.Value;
        var cKey = types.key == "string" ? "str" : TypeRegistry.MapType(types.key);
        var cVal = types.val == "string" ? "str" : TypeRegistry.MapType(types.val);
        return "Dict_" + cKey + "_" + cVal;
    }

    // ── Fail-Hilfsmethode ─────────────────────────────────────────────────

    protected static bool NotHandled(out string result)
    {
        result = null!;
        return false;
    }
}