using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Abstrakte Basisklasse für alle IInvocationHandler.
///
/// Stellt gemeinsame Hilfsmethoden bereit die in mehreren Handlern
/// identisch verwendet werden — eliminiert Copy-paste zwischen
/// DictionaryHandler, ListHandler, StringBuilderHandler, FieldMethodHandler.
///
/// Erweiterung: Neuen Handler von InvocationHandlerBase ableiten
/// und TryHandle implementieren.
/// </summary>
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

    /// <summary>
    /// Gibt args[index] zurück, oder "" wenn index außerhalb liegt.
    /// </summary>
    protected static string ArgAt(List<string> args, int index) =>
        index < args.Count ? args[index] : "\"\"";

    /// <summary>
    /// Gibt alle args als kommaseparierte Liste zurück.
    /// </summary>
    protected static string JoinArgs(List<string> args) =>
        string.Join(", ", args);

    /// <summary>
    /// Gibt args[1..] als kommaseparierte Liste zurück (ohne erstes Argument).
    /// </summary>
    protected static string JoinArgsTail(List<string> args) =>
        string.Join(", ", args.Skip(1));

    // ── Typ-Lookup ────────────────────────────────────────────────────────

    /// <summary>
    /// Sucht den C#-Typ eines Bezeichners in LocalTypes und FieldTypes.
    /// Gibt null zurück wenn nicht gefunden.
    /// </summary>
    protected static string? LookupType(string objStr, TranspilerContext ctx)
    {
        var objKey = objStr.TrimStart('_');
        if (ctx.LocalTypes.TryGetValue(objStr, out var lt)) return lt;
        if (ctx.FieldTypes.TryGetValue(objKey, out var ft)) return ft;
        return null;
    }

    /// <summary>
    /// Gibt den C-Ausdruck für den Zugriff auf ein Objekt zurück.
    /// Lokale Variable → direkter Name, Feld → self->f_feldname.
    /// </summary>
    protected static string BuildObjectExpr(string objStr, TranspilerContext ctx)
    {
        var objKey = objStr.TrimStart('_');
        if (ctx.LocalTypes.ContainsKey(objStr)) return objStr;
        return "self->f_" + objKey;
    }

    // ── Collection-Lookup ─────────────────────────────────────────────────

    /// <summary>
    /// Versucht den Objekt-Ausdruck als List&lt;T&gt; aufzulösen.
    /// Gibt true zurück wenn erfolgreich, setzt listType und listExpr.
    /// </summary>
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

    /// <summary>
    /// Versucht den Objekt-Ausdruck als Dictionary&lt;K,V&gt; aufzulösen.
    /// Gibt true zurück wenn erfolgreich, setzt dictType und dictExpr.
    /// </summary>
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

    /// <summary>
    /// Versucht den Objekt-Ausdruck als StringBuilder aufzulösen.
    /// Gibt true zurück wenn erfolgreich, setzt sbExpr.
    /// </summary>
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
    /// Beispiel: List&lt;int&gt; → "List_int"
    /// </summary>
    protected static string ListFuncPrefix(string listType)
    {
        var inner = TypeRegistry.GetListInnerType(listType)!;
        var cInner = inner == "string" ? "char" : TypeRegistry.MapType(inner);
        return "List_" + cInner;
    }

    /// <summary>
    /// Gibt den C-Funktionspräfix für ein Dict-Makro zurück.
    /// Beispiel: Dictionary&lt;string,int&gt; → "Dict_str_int"
    /// </summary>
    protected static string DictFuncPrefix(string dictType)
    {
        var types = TypeRegistry.GetDictionaryTypes(dictType)!.Value;
        var cKey = types.key == "string" ? "str" : TypeRegistry.MapType(types.key);
        var cVal = types.val == "string" ? "str" : TypeRegistry.MapType(types.val);
        return "Dict_" + cKey + "_" + cVal;
    }

    // ── Fail-Hilfsmethode ─────────────────────────────────────────────────

    /// <summary>
    /// Setzt result auf null und gibt false zurück — für nicht-zuständige Handler.
    /// </summary>
    protected static bool NotHandled(out string result)
    {
        result = null!;
        return false;
    }
}