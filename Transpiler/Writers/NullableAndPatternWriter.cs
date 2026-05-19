using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Writers;

/// <summary>
/// Behandelt Nullable-Typen (T?) und Pattern-Matching-Ausdrücke.
///
/// Nullable-Typen:
/// ───────────────────────────────────────────────────────────────────
/// int? x = null;          → int* x = NULL;
/// int? x = 5;             → int _x_val = 5; int* x = &_x_val;
/// x.HasValue              → (x != NULL)
/// x.Value                 → (*x)
/// x ?? 0                  → (x != NULL ? *x : 0)
/// x?.ToString()           → (x != NULL ? Int_ToString(*x) : NULL)
///
/// Pattern Matching:
/// ───────────────────────────────────────────────────────────────────
/// switch (x)              → if (x == 1) { ... }
/// {                          else if (x == 2) { ... }
///   case 1: ...              else { ... }
///   case 2: ...
///   default: ...
/// }
///
/// switch expression:
/// x switch {              → (x == 1 ? a : x == 2 ? b : c)
///   1 => a,
///   2 => b,
///   _ => c,
/// }
///
/// Type-Pattern:
/// if (obj is Dog d) { }   → if (Dog_Is(obj)) { Dog* d = (Dog*)obj;  ... }
/// obj is null             → obj == NULL
/// obj is not null         → obj != NULL
/// </summary>
public static class NullableHandler
{
    /// <summary>
    /// True wenn der C#-Typ nullable ist (T? oder Nullable<T>).
    /// </summary>
    public static bool IsNullable(string csType)
        => csType.EndsWith("?") || csType.StartsWith("Nullable<");

    /// <summary>
    /// Gibt den inneren Typ zurück: "int?" → "int", "Nullable<float>" → "float"
    /// </summary>
    public static string GetInnerType(string csType)
    {
        if (csType.EndsWith("?")) return csType[..^1].Trim();
        if (csType.StartsWith("Nullable<") && csType.EndsWith(">"))
            return csType[9..^1].Trim();
        return csType;
    }

    /// <summary>
    /// Mappt einen Nullable-C#-Typ auf C: "int?" → "int*"
    /// </summary>
    public static string MapNullableType(string csType)
    {
        var inner = GetInnerType(csType);
        return TypeRegistry.MapType(inner) + "*";
    }

    /// <summary>
    /// Transpiliert den null-coalescing Operator: x ?? defaultVal
    /// → (x != NULL ? *x : defaultVal)
    /// </summary>
    public static string WriteNullCoalescing(
        string nullableExpr,
        string defaultExpr,
        bool innerIsValueType = true)
    {
        var deref = innerIsValueType ? "*" : "";
        return "(" + nullableExpr + " != NULL ? "
             + deref + nullableExpr + " : "
             + defaultExpr + ")";
    }

    /// <summary>
    /// Transpiliert den null-conditional Operator: x?.Method()
    /// → (x != NULL ? x->Method(x) : NULL)
    /// </summary>
    public static string WriteNullConditional(
        string nullableExpr,
        string accessExpr)
    {
        return "(" + nullableExpr + " != NULL ? " + accessExpr + " : NULL)";
    }

    /// <summary>
    /// Transpiliert x.HasValue → (x != NULL)
    /// </summary>
    public static string WriteHasValue(string expr)
        => "(" + expr + " != NULL)";

    /// <summary>
    /// Transpiliert x.Value → (*x)
    /// </summary>
    public static string WriteGetValue(string expr)
        => "(*" + expr + ")";
}

/// <summary>
/// Transpiliert switch-Ausdrücke und Pattern-Matching.
/// </summary>
public static class PatternMatchingWriter
{
    /// <summary>
    /// Transpiliert einen switch-Ausdruck (C# 8+) zu einem verschachtelten ternären Ausdruck.
    ///
    /// x switch { 1 => "one", 2 => "two", _ => "other" }
    /// → (x == 1 ? "one" : (x == 2 ? "two" : "other"))
    /// </summary>
    public static string WriteSwitchExpression(
        SwitchExpressionSyntax switchExpr,
        TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr)
    {
        var governed = writeExpr(switchExpr.GoverningExpression);
        var arms = switchExpr.Arms.ToList();

        return BuildTernaryChain(governed, arms, 0, ctx, writeExpr);
    }

    private static string BuildTernaryChain(
        string governed,
        List<SwitchExpressionArmSyntax> arms,
        int index,
        TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr)
    {
        if (index >= arms.Count) return "0 /* unreachable */";

        var arm = arms[index];
        var val = writeExpr(arm.Expression);

        // Discard pattern "_" → else-Zweig
        if (arm.Pattern is DiscardPatternSyntax)
            return val;

        var cond = WritePattern(arm.Pattern, governed, ctx, writeExpr);

        // When-Klausel
        if (arm.WhenClause != null)
            cond = "(" + cond + " && " + writeExpr(arm.WhenClause.Condition) + ")";

        var rest = BuildTernaryChain(governed, arms, index + 1, ctx, writeExpr);
        return "(" + cond + " ? " + val + " : " + rest + ")";
    }

    /// <summary>
    /// Schreibt eine Pattern-Bedingung.
    ///
    /// ConstantPattern    1       → x == 1
    /// NullPattern        null    → x == NULL
    /// NotPattern         not null→ x != NULL
    /// TypePattern        Dog d   → Dog_Is(x) (mit Binding-Variable)
    /// RelationalPattern  > 5     → x > 5
    /// BinaryPattern      and/or  → (a && b) / (a || b)
    /// </summary>
    public static string WritePattern(
    PatternSyntax pattern,
    string subject,
    TranspilerContext ctx,
    Func<SyntaxNode?, string> writeExpr)
    {
        return pattern switch
        {
            ConstantPatternSyntax cp =>
                subject + " == " + writeExpr(cp.Expression),

            UnaryPatternSyntax up when up.IsKind(SyntaxKind.NotPattern) =>
                "!(" + WritePattern(up.Pattern, subject, ctx, writeExpr) + ")",

            BinaryPatternSyntax bp when bp.IsKind(SyntaxKind.AndPattern) =>
                WritePattern(bp.Left, subject, ctx, writeExpr)
                + " && "
                + WritePattern(bp.Right, subject, ctx, writeExpr),

            BinaryPatternSyntax bp when bp.IsKind(SyntaxKind.OrPattern) =>
                WritePattern(bp.Left, subject, ctx, writeExpr)
                + " || "
                + WritePattern(bp.Right, subject, ctx, writeExpr),

            RelationalPatternSyntax rp =>
                subject + " " + rp.OperatorToken.Text + " " + writeExpr(rp.Expression),

            DeclarationPatternSyntax dp =>
                WriteTypePattern(dp, subject, ctx),

            TypePatternSyntax =>
                "(" + subject + " != NULL)",

            DiscardPatternSyntax =>
                "1 /* _ */",

            ListPatternSyntax lp =>
                WriteListPattern(lp, subject, ctx, writeExpr),

            RecursivePatternSyntax rp when rp.PropertyPatternClause != null =>
                WritePropertyPattern(rp, subject, ctx, writeExpr),

            _ => FallbackPattern(pattern, subject, ctx),
        };
    }

    // Roslyn represents { Prop: Pattern } as RecursivePatternSyntax with PropertyPatternClause
    private static string WritePropertyPattern(
        RecursivePatternSyntax rp,
        string subject,
        TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr)
    {
        // Dog { Name: "Rex", Age: > 2 }  →  (subject != NULL && subject->f_Name == "Rex" && subject->Age > 2)
        var conditions = new List<string>();

        // Optional type check before property access
        if (rp.Type != null)
        {
            var typeName = rp.Type.ToString().Trim();
            if (!TypeRegistry.IsPrimitive(typeName))
                conditions.Add($"({subject} != NULL)");
        }
        else
        {
            conditions.Add($"({subject} != NULL)");
        }

        if (rp.PropertyPatternClause != null)
        {
            foreach (var sub in rp.PropertyPatternClause.Subpatterns)
            {
                var propName = sub.NameColon?.Name.Identifier.Text
                            ?? sub.ExpressionColon?.Expression.ToString().Trim();
                if (propName == null) continue;

                var cProp = TypeRegistry.HasNoPrefix(propName) ? propName : "f_" + propName;
                var access = $"{subject}->{cProp}";
                conditions.Add(WritePattern(sub.Pattern, access, ctx, writeExpr));
            }
        }

        return "(" + string.Join(" && ", conditions) + ")";
    }

    private static string FallbackPattern(PatternSyntax pattern, string subject, TranspilerContext ctx)
    {
        ctx.Warn($"Unsupported pattern type: {pattern.GetType().Name} — falling back to null check",
            pattern.ToString());
        return "(" + subject + " != NULL) /* unsupported pattern: " + pattern.GetType().Name + " */";
    }

    private static string WriteTypePattern(
        DeclarationPatternSyntax dp,
        string subject,
        TranspilerContext ctx)
    {
        var typeName = dp.Type.ToString().Trim();
        var varDesig = dp.Designation as SingleVariableDesignationSyntax;
        var bindingVar = varDesig?.Identifier.Text;

        // Binding-Variable als gecasteten Pointer registrieren
        if (bindingVar != null)
            ctx.LocalTypes[bindingVar] = typeName;

        // Ohne RTTI-System: Typ-Check wird als Null-Check approximiert.
        // Für Primitive (int, float, ...) ist die Bedingung immer wahr.
        if (TypeRegistry.IsPrimitive(typeName) && typeName != "string")
            return "1";
        return "(" + subject + " != NULL)";
    }


    private static string WriteListPattern(
        ListPatternSyntax lp,
        string subject,
        TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr)
    {
        // C has no built-in list patterns; we emit a length + element check.
        // Works for arrays and List<T>* where we know the count.
        // Determine whether subject is a list or array via LocalTypes/FieldTypes.
        var subjectKey = subject.TrimStart('_').Replace("self->f_", "").Replace("self->", "");
        string? colType = null;
        ctx.LocalTypes.TryGetValue(subject, out colType);
        if (colType == null) ctx.FieldTypes.TryGetValue(subjectKey, out colType);

        var patterns = lp.Patterns.ToList();
        var hasRest = lp.Patterns.OfType<SlicePatternSyntax>().Any();
        var fixedPatterns = patterns.Where(p => p is not SlicePatternSyntax).ToList();

        var conditions = new List<string>();

        // Length check (only when there's no slice pattern, or slice is at end)
        if (!hasRest)
        {
            var countExpr = TypeRegistry.IsList(colType ?? "")
                ? subject + "->count"
                : "(sizeof(" + subject + ")/sizeof(" + subject + "[0]))";
            conditions.Add(countExpr + " == " + patterns.Count);
        }
        else
        {
            var countExpr = TypeRegistry.IsList(colType ?? "")
                ? subject + "->count"
                : "(sizeof(" + subject + ")/sizeof(" + subject + "[0]))";
            // At least fixedPatterns.Count elements needed
            if (fixedPatterns.Count > 0)
                conditions.Add(countExpr + " >= " + fixedPatterns.Count);
        }

        // Element checks (skip discard and slice patterns)
        int elemIdx = 0;
        foreach (var p in patterns)
        {
            if (p is SlicePatternSyntax or DiscardPatternSyntax) { elemIdx++; continue; }

            var elemAccess = TypeRegistry.IsList(colType ?? "")
                ? "List_" + GetListInnerCType(colType!) + "_Get(" + subject + ", " + elemIdx + ")"
                : subject + "[" + elemIdx + "]";

            conditions.Add(WritePattern(p, elemAccess, ctx, writeExpr));
            elemIdx++;
        }

        return conditions.Count == 0
            ? "1 /* empty list pattern */"
            : "(" + string.Join(" && ", conditions) + ")";
    }

    /// <summary>
    /// Gibt C-Code aus der eine is-Pattern-Prüfung + Binding-Variable deklariert.
    /// Wird von StatementWriter für `if (obj is Dog d)` Statements aufgerufen.
    /// </summary>
    public static string WriteIsPattern(
        IsPatternExpressionSyntax isExpr,
        TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr)
    {
        var subject = writeExpr(isExpr.Expression);
        return WritePattern(isExpr.Pattern, subject, ctx, writeExpr);
    }

    private static string GetListInnerCType(string listType)
    {
        var inner = TypeRegistry.GetListInnerType(listType) ?? "int";
        return inner == "string" ? "str" : TypeRegistry.MapType(inner);
    }
}
