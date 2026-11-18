using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler;

/// Reference counting for user classes.
///
/// Every heap object of a user class hierarchy carries `_rc` and `_dtor` in its root
/// struct. `T_Free` releases one reference; at zero it runs the dynamic type's
/// `_Dispose` (which releases the object's own fields, then its base's) and frees the
/// memory. Fields and collections own one reference each: storing a value that is not
/// freshly created retains it (see <see cref="RetainIfShared"/>).
///
/// Classes that derive from runtime types (SwitchApp, controls, exceptions), generic
/// classes and structs keep their own lifetime handling and are not part of this.
internal static class Arc
{
    /// Root class name of the reference-counted hierarchy of <paramref name="t"/>,
    /// or null when <paramref name="t"/> is not such a class.
    public static string? Root(ITypeSymbol? t)
    {
        if (t is not INamedTypeSymbol n || n.TypeKind != TypeKind.Class || n.IsStatic
            || n.IsGenericType || !TranspilerContext.IsUserCode(n))
            return null;
        // Registered per project from syntax before transpiling (ProjectTypeScanner),
        // so headers, _generics.h and the C files agree.
        return TypeRegistry.ArcRoot(n.Name);
    }

    /// True when the class takes part in the `_Dispose`/`_dtor` scheme (records keep
    /// their own self-contained Free).
    public static bool UsesDispose(INamedTypeSymbol? sym) =>
        sym != null && !sym.IsRecord && Root(sym) != null;

    /// C prefix of a reference-counted collection type (List_int, Dict_str_Node,
    /// Stack_float, HashSet_Item, StringBuilder), or null for any other type.
    public static string? CollectionPrefix(ITypeSymbol? t)
    {
        if (t == null || t is IErrorTypeSymbol) return null;
        var cs = TranspilerContext.FormatTypeSymbol(t).TrimEnd('?');
        return CollectionPrefix(cs);
    }

    public static string? CollectionPrefix(string cs)
    {
        cs = cs.Trim().TrimEnd('?');
        if (TypeRegistry.IsStringBuilder(cs)) return "StringBuilder";
        if (TypeRegistry.IsList(cs))
        {
            var inner = TypeRegistry.GetListInnerType(cs) ?? "int";
            return "List_" + (inner == "string" ? "str" : TypeRegistry.MapType(inner).TrimEnd('*').Replace(" ", "_"));
        }
        if (TypeRegistry.IsDictionary(cs) && TypeRegistry.GetDictionaryTypes(cs) is { } kv)
        {
            string N(string x) => x == "string" ? "str" : TypeRegistry.MapType(x).TrimEnd('*').Replace(" ", "_");
            return "Dict_" + N(kv.key) + "_" + N(kv.val);
        }
        if (TypeRegistry.IsStack(cs) || TypeRegistry.IsQueue(cs) || TypeRegistry.IsHashSet(cs))
            return TypeRegistry.MapType(cs).TrimEnd('*').Trim();
        return null;
    }

    /// <paramref name="code"/> with a reference taken, for a store that keeps the
    /// object or collection (fields, records, closures, …). Null/default need none.
    /// A class value stored as an interface is wrapped first (Class_as_IFace).
    public static string RetainAny(ExpressionSyntax value, string code, TranspilerContext ctx)
    {
        if (IsFresh(value) || code == "NULL" || ctx.SemanticModel == null) return code;
        try
        {
            var ti = ctx.SemanticModel.GetTypeInfo(value);
            if (IsDelegate(ti.ConvertedType ?? ti.Type)) return "CS2SX_Delegate_Retain(" + code + ")";
            if (IsInterface(ti.ConvertedType))
                return "CS2SX_IFACE_KEEP(" + WrapInterface(value, code, ctx) + ")";
            if (CollectionPrefix(ti.Type) is { } p) return p + "_Retain(" + code + ")";
        }
        catch { }
        return RetainIfShared(value, code, ctx);
    }

    public static bool IsDelegate(ITypeSymbol? t) => t?.TypeKind == TypeKind.Delegate;

    /// An interface of the project (an IFace* value made by Class_as_IFace).
    public static bool IsInterface(ITypeSymbol? t) =>
        t is INamedTypeSymbol { TypeKind: TypeKind.Interface, IsGenericType: false } n
        && TranspilerContext.IsUserCode(n) && TypeRegistry.IsRegisteredInterface(n.Name);

    /// Delegates and interface values: (retain, release) of CS2SX_RC_STORE, the
    /// KEEP/DROP hooks of collections and the cycle collector's kind; null for
    /// any other type.
    public static (string retain, string free, string keep, string drop, string kind)? RcValue(ITypeSymbol? t) =>
        IsDelegate(t) ? ("CS2SX_Delegate_Retain", "CS2SX_Delegate_Release", "CS2SX_DLG_KEEP", "CS2SX_DLG_DROP", "CS2SX_Delegate_GcKind")
        : IsInterface(t) ? ("CS2SX_Iface_Retain", "CS2SX_Iface_Free", "CS2SX_IFACE_KEEP", "CS2SX_IFACE_DROP", "CS2SX_Iface_GcKind")
        : null;

    /// The same for a C# type name (generated code that has no symbols).
    public static (string retain, string free, string keep, string drop, string kind)? RcValue(string csType)
    {
        csType = csType.Trim().TrimEnd('?');
        if (TypeRegistry.IsDelegate(csType) || csType == "Action" || csType.StartsWith("Func<") || csType.StartsWith("Action<"))
            return ("CS2SX_Delegate_Retain", "CS2SX_Delegate_Release", "CS2SX_DLG_KEEP", "CS2SX_DLG_DROP", "CS2SX_Delegate_GcKind");
        if (TypeRegistry.IsRegisteredInterface(csType))
            return ("CS2SX_Iface_Retain", "CS2SX_Iface_Free", "CS2SX_IFACE_KEEP", "CS2SX_IFACE_DROP", "CS2SX_Iface_GcKind");
        return null;
    }

    // ── Run-time type tests (x is T) ─────────────────────────────────────────────

    /// C condition for `code is target`, where code has the static type
    /// <paramref name="input"/>. Classes compare their run-time type (T_ClassInfo),
    /// exceptions their type info; a test the compiler already knows is a null check.
    public static string TypeTest(string code, ITypeSymbol? input, ITypeSymbol? target)
    {
        if (target == null || input == null) return "(" + code + " != NULL)";
        if (target.IsValueType) return "1";
        if (DerivesFrom(input, target)) return "(" + code + " != NULL)";
        if (IsException(target))
        {
            var ti = TypeRegistry.ExceptionTypeInfo(target.Name);
            return "CS2SX_Exc_Is((Exception*)(" + code + "), &" + ti + ")";
        }
        if (target is INamedTypeSymbol { TypeKind: TypeKind.Class } cls && Root(cls) is { } root && UsesDispose(cls))
        {
            if (IsInterface(input))
                return "({ CS2SX_IfaceHdr* _cs2sx_ti = (CS2SX_IfaceHdr*)(" + code + "); _cs2sx_ti && CS2SX_IsA(_cs2sx_ti->cls, &" + cls.Name + "_ClassInfo); })";
            if (input is INamedTypeSymbol { TypeKind: TypeKind.Class } inCls && Root(inCls) == root)
                return "({ " + root + "* _cs2sx_to = (" + root + "*)(" + code + "); _cs2sx_to && CS2SX_IsA(_cs2sx_to->_class, &" + cls.Name + "_ClassInfo); })";
        }
        return "(" + code + " != NULL)";
    }

    /// C expression for the matched value as <paramref name="target"/> (the variable of
    /// `x is T t`): a cast; an interface value gives up its object.
    public static string TypeBinding(string code, ITypeSymbol? input, ITypeSymbol? target, string cType)
    {
        if (IsInterface(input) && target is INamedTypeSymbol { TypeKind: TypeKind.Class })
            return "(" + cType + "*)((CS2SX_IfaceHdr*)(" + code + "))->obj";
        return "(" + cType + "*)" + code;
    }

    private static bool DerivesFrom(ITypeSymbol t, ITypeSymbol target)
    {
        for (var b = t; b != null; b = b.BaseType)
            if (SymbolEqualityComparer.Default.Equals(b, target)) return true;
        return target.TypeKind == TypeKind.Interface && t.AllInterfaces.Contains((INamedTypeSymbol)target, SymbolEqualityComparer.Default);
    }

    private static bool IsException(ITypeSymbol t)
    {
        for (var b = t; b != null; b = b.BaseType)
            if (b.Name == "Exception" && b.ContainingNamespace?.ToDisplayString() == "System") return true;
        return false;
    }

    // ── Value structs that hold references ──────────────────────────────────────
    //
    // A struct (or tuple) member that refers to an object, a collection, a delegate,
    // an interface value or a string holds no reference while the struct lives in a
    // local — like a local variable. Storage that keeps the struct (a class field, a
    // list) keeps its members too: S_Keep takes a reference to each one (strings: an
    // own copy), S_Drop gives them up at the next frame, S_Gc lets the cycle
    // collector follow them.

    /// True when the struct, or a struct inside it, has a member that holds a reference.
    public static bool StructHasRefs(ITypeSymbol? t, int depth = 0)
    {
        if (t is not INamedTypeSymbol { TypeKind: TypeKind.Struct } s || depth > 8
            || s.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            return false;
        // Tuples: their functions are generated from the type text (TupleFunctions),
        // which knows no nested structs.
        if (s.IsTupleType)
            return s.TupleElements.Any(e => MemberKind(e.Type, depth) is { } k && k != "struct");
        if (!TranspilerContext.IsUserCode(s)) return false;
        return StructMembers(s).Any(m => MemberKind(m.type, depth) != null);
    }

    /// C name of a value struct: the struct's name, or _Tuple2_Item_int for a tuple.
    public static string ValueCName(ITypeSymbol t) =>
        t is INamedTypeSymbol { IsTupleType: true }
            ? TypeRegistry.GetTupleStructName(TranspilerContext.FormatTypeSymbol(t))
            : TranspilerContext.FormatTypeSymbol(t);

    /// S_Keep / S_Drop / S_Gc of a tuple type, from its C# text — static inline, for
    /// _generics.h (after the collections, before the classes); null without references.
    public static string? TupleFunctions(string tupleCs)
    {
        if (!TypeRegistry.IsTuple(tupleCs)) return null;
        var name = TypeRegistry.GetTupleStructName(tupleCs);
        var parts = SplitTop(tupleCs.Trim()[1..^1]);
        var keep = new System.Text.StringBuilder();
        var drop = new System.Text.StringBuilder();
        var gc = new System.Text.StringBuilder();
        var protos = new System.Text.StringBuilder();
        bool any = false;
        for (int i = 0; i < parts.Count; i++)
        {
            var e = parts[i].Trim();
            var sp = e.LastIndexOf(' ');
            var type = (sp >= 0 && !e[..sp].TrimEnd().EndsWith(",") ? e[..sp] : e).Trim().TrimEnd('?');
            var m = "s->item" + (i + 1);
            if (type == "string")
            {
                keep.Append($"    {m} = CS2SX_StrOwn({m});\n"); drop.Append($"    CS2SX_StrDrop({m});\n"); any = true;
            }
            else if (TypeRegistry.ArcRoot(type) is { } r)
            {
                protos.Append($"{r}* {r}_Retain({r}* self);\nvoid {r}_Free({r}* self);\nextern const CS2SX_GcKind {r}_GcKind;\n");
                keep.Append($"    if ({m}) {r}_Retain(({r}*){m});\n");
                drop.Append($"    CS2SX_ReleaseLater((void*){m}, (CS2SX_FreeFn){r}_Free);\n");
                gc.Append($"    if (visit) visit((void*){m}, &{r}_GcKind); else if ({m}) {{ void* _o = (void*){m}; {m} = NULL; {r}_Free(({r}*)_o); }}\n");
                any = true;
            }
            else if (CollectionPrefix(type) is { } p)
            {
                keep.Append($"    {p}_Retain({m});\n"); drop.Append($"    CS2SX_ReleaseLater((void*){m}, (CS2SX_FreeFn){p}_Free);\n"); any = true;
            }
            else if (RcValue(type) is { } rc)
            {
                keep.Append($"    {rc.retain}({m});\n"); drop.Append($"    CS2SX_ReleaseLater((void*){m}, (CS2SX_FreeFn){rc.free});\n");
                gc.Append($"    if (visit) visit((void*){m}, &{rc.kind}); else if ({m}) {{ void* _o = (void*){m}; {m} = NULL; {rc.free}(_o); }}\n");
                any = true;
            }
        }
        if (!any) return null;
        return $"#ifndef CS2SX_TUPLE_RC_{name}\n#define CS2SX_TUPLE_RC_{name}\n{protos}"
             + $"static inline void {name}_Keep({name}* s)\n{{\n{keep}}}\n"
             + $"static inline void {name}_Drop({name}* s)\n{{\n{drop}}}\n"
             + $"static inline void {name}_Gc({name}* s, CS2SX_GcVisit visit)\n{{\n{gc}    (void)s; (void)visit;\n}}\n"
             + $"static inline {name} {name}_KeepV({name} v) {{ {name}_Keep(&v); return v; }}\n"
             + $"static inline void {name}_DropV({name} v) {{ {name}_Drop(&v); }}\n#endif\n";
    }

    private static List<string> SplitTop(string s)
    {
        var result = new List<string>();
        int depth = 0, start = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] is '<' or '(') depth++;
            else if (s[i] is '>' or ')') depth--;
            else if (s[i] == ',' && depth == 0) { result.Add(s[start..i]); start = i + 1; }
        }
        result.Add(s[start..]);
        return result;
    }

    /// Instance members of a user struct as the C struct has them (auto-properties
    /// by their own name).
    public static IEnumerable<(string name, ITypeSymbol type)> StructMembers(INamedTypeSymbol s)
    {
        foreach (var f in s.GetMembers().OfType<IFieldSymbol>())
        {
            if (f.IsStatic || f.IsConst) continue;
            var name = f.AssociatedSymbol is IPropertySymbol p ? p.Name : f.Name;
            yield return (name, f.Type);
        }
    }

    private static string? MemberKind(ITypeSymbol t, int depth)
    {
        if (t.SpecialType == SpecialType.System_String) return "str";
        if (Root(t) != null) return "arc";
        if (CollectionPrefix(t) != null) return "coll";
        if (RcValue(t) != null) return "rc";
        if (StructHasRefs(t, depth + 1)) return "struct";
        return null;
    }

    /// Bodies of S_Keep / S_Drop / S_Gc for the struct (C), or null when it holds no references.
    public static string? StructFunctions(INamedTypeSymbol s, string cName)
    {
        if (!StructHasRefs(s)) return null;
        var keep = new System.Text.StringBuilder();
        var drop = new System.Text.StringBuilder();
        var gc = new System.Text.StringBuilder();
        foreach (var (name, type) in StructMembers(s))
        {
            var m = "s->" + name;
            switch (MemberKind(type, 0))
            {
                case "str":
                    keep.Append($"    {m} = CS2SX_StrOwn({m});\n");
                    drop.Append($"    CS2SX_StrDrop({m});\n");
                    break;
                case "arc":
                {
                    var r = Root(type)!;
                    keep.Append($"    if ({m}) {r}_Retain(({r}*){m});\n");
                    drop.Append($"    CS2SX_ReleaseLater((void*){m}, (CS2SX_FreeFn){r}_Free);\n");
                    gc.Append($"    if (visit) visit((void*){m}, &{r}_GcKind); else if ({m}) {{ void* _o = (void*){m}; {m} = NULL; {r}_Free(({r}*)_o); }}\n");
                    break;
                }
                case "coll":
                {
                    var p = CollectionPrefix(type)!;
                    keep.Append($"    {p}_Retain({m});\n");
                    drop.Append($"    CS2SX_ReleaseLater((void*){m}, (CS2SX_FreeFn){p}_Free);\n");
                    if (type is INamedTypeSymbol { TypeArguments: [var elem] } && (Root(elem) != null || RcValue(elem) != null)
                        && TypeRegistry.IsList(TranspilerContext.FormatTypeSymbol(type)))
                        gc.Append($"    if (visit) visit((void*){m}, &{p}_GcKind);\n");
                    break;
                }
                case "rc":
                {
                    var rc = RcValue(type)!.Value;
                    keep.Append($"    {rc.retain}({m});\n");
                    drop.Append($"    CS2SX_ReleaseLater((void*){m}, (CS2SX_FreeFn){rc.free});\n");
                    gc.Append($"    if (visit) visit((void*){m}, &{rc.kind}); else if ({m}) {{ void* _o = (void*){m}; {m} = NULL; {rc.free}(_o); }}\n");
                    break;
                }
                case "struct":
                {
                    var n = TranspilerContext.FormatTypeSymbol(type);
                    keep.Append($"    {n}_Keep(&{m});\n");
                    drop.Append($"    {n}_Drop(&{m});\n");
                    gc.Append($"    {n}_Gc(&{m}, visit);\n");
                    break;
                }
            }
        }
        return $"void {cName}_Keep({cName}* s)\n{{\n{keep}}}\n\n"
             + $"void {cName}_Drop({cName}* s)\n{{\n{drop}}}\n\n"
             + $"void {cName}_Gc({cName}* s, CS2SX_GcVisit visit)\n{{\n{gc}    (void)s; (void)visit;\n}}\n\n";
    }

    /// Declarations of what StructFunctions defines, and what it calls.
    public static string? StructPrototypes(INamedTypeSymbol s, string cName)
    {
        if (!StructHasRefs(s)) return null;
        var sb = new System.Text.StringBuilder();
        foreach (var (_, type) in StructMembers(s))
            if (Root(type) is { } r)
                sb.Append($"{r}* {r}_Retain({r}* self);\nvoid {r}_Free({r}* self);\nextern const CS2SX_GcKind {r}_GcKind;\n");
        sb.Append($"void {cName}_Keep({cName}* s);\nvoid {cName}_Drop({cName}* s);\n");
        sb.Append($"void {cName}_Gc({cName}* s, CS2SX_GcVisit visit);\n");
        sb.Append($"static inline {cName} {cName}_KeepV({cName} v) {{ {cName}_Keep(&v); return v; }}\n");
        sb.Append($"static inline void {cName}_DropV({cName} v) {{ {cName}_Drop(&v); }}\n");
        return sb.ToString();
    }

    /// <paramref name="code"/> of a class-typed value converted to an interface
    /// (`IThing t = new Thing()`): Thing_as_IThing(code). Unchanged otherwise.
    public static string WrapInterface(ExpressionSyntax value, string code, TranspilerContext ctx)
    {
        if (ctx.SemanticModel == null || code.Contains("_as_")) return code;
        try
        {
            var ti = ctx.SemanticModel.GetTypeInfo(value);
            if (!IsInterface(ti.ConvertedType) || ti.Type is not { TypeKind: TypeKind.Class or TypeKind.Struct } cls)
                return code;
            return TranspilerContext.FormatTypeSymbol(cls) + "_as_" + ti.ConvertedType!.Name + "(" + code + ")";
        }
        catch { return code; }
    }

    /// A value that needs no reference: null / default. (A new object's initial
    /// reference belongs to the current frame — see T_New — so a store retains it
    /// like any other object; whatever ends up nowhere is freed at the next frame.)
    public static bool IsFresh(ExpressionSyntax e)
    {
        while (true)
        {
            switch (e)
            {
                case ParenthesizedExpressionSyntax p: e = p.Expression; continue;
                case CastExpressionSyntax c: e = c.Expression; continue;
                case PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.SuppressNullableWarningExpression } s:
                    e = s.Operand; continue;
            }
            return e.IsKind(SyntaxKind.NullLiteralExpression)
                || e.IsKind(SyntaxKind.DefaultLiteralExpression)
                || e is DefaultExpressionSyntax;
        }
    }

    /// Wraps <paramref name="code"/> (the C for <paramref name="value"/>) so that the
    /// store takes its own reference. Returns <paramref name="code"/> unchanged when the
    /// value is fresh or not a reference-counted class. The result keeps the C type.
    public static string RetainIfShared(ExpressionSyntax value, string code, TranspilerContext ctx)
    {
        if (IsFresh(value) || ctx.SemanticModel == null) return code;
        ITypeSymbol? t;
        try { t = ctx.SemanticModel.GetTypeInfo(value).Type; }
        catch { return code; }
        var root = Root(t);
        return root == null ? code : $"CS2SX_RETAIN({root}, {code})";
    }
}
