using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;

namespace CS2SX.Transpiler;

public static class PropertyWriter
{
    /// <summary>
    /// True wenn die Property auto-implementiert ist (kein expliziter Body).
    /// Auto-Properties werden zu struct-Feldern — keine Funktionen nötig.
    ///
    /// FIX: Expression-Body Properties (=> expr) sind KEINE Auto-Properties —
    ///      sie haben keinen AccessorList sondern einen ExpressionBody und brauchen
    ///      eine Getter-Funktion.
    /// </summary>
    public static bool IsAutoProperty(PropertyDeclarationSyntax prop)
    {
        // Expression-body: public int X => _x;  → KEIN Auto-Property
        if (prop.ExpressionBody != null) return false;

        // Kein AccessorList → weder auto noch expression body → ignorieren
        if (prop.AccessorList == null) return false;

        // Alle Accessoren ohne Body → Auto-Property
        return prop.AccessorList.Accessors.All(a =>
            a.Body == null && a.ExpressionBody == null);
    }

    /// <summary>
    /// Schreibt Getter/Setter-Signaturen in den Header.
    /// Für Properties mit explizitem Body oder Expression-Body aufrufen.
    /// </summary>
    public static void WriteSignatures(
        PropertyDeclarationSyntax prop,
        string className,
        System.IO.TextWriter output)
    {
        if (IsAutoProperty(prop)) return;

        var csType = prop.Type.ToString().Trim();
        var cType = TypeRegistry.MapType(csType);
        var name = prop.Identifier.Text;
        var ptr = NeedsPtr(csType) ? "*" : "";
        var self = className + "* self";

        // Expression-body Property → nur Getter
        if (prop.ExpressionBody != null)
        {
            output.WriteLine($"{cType}{ptr} {className}_get_{name}({self});");
            return;
        }

        var hasGet = HasAccessor(prop, SyntaxKind.GetAccessorDeclaration);
        var hasSet = HasAccessor(prop, SyntaxKind.SetAccessorDeclaration);

        if (hasGet) output.WriteLine($"{cType}{ptr} {className}_get_{name}({self});");
        if (hasSet) output.WriteLine($"void {className}_set_{name}({self}, {cType}{ptr} value);");
    }

    /// <summary>
    /// Schreibt Getter/Setter-Implementierungen.
    /// </summary>
    public static void WriteImplementations(
        PropertyDeclarationSyntax prop,
        string className,
        TranspilerContext ctx,
        ExpressionWriter expr,
        StatementWriter stmt)
    {
        if (IsAutoProperty(prop)) return;

        var csType = prop.Type.ToString().Trim();
        var cType = TypeRegistry.MapType(csType);
        var name = prop.Identifier.Text;
        var ptr = NeedsPtr(csType) ? "*" : "";
        var self = className + "* self";

        ctx.CurrentClass = className;

        // FIX: Expression-body Property → Getter generieren
        // public int Speed => _speed * 2;
        if (prop.ExpressionBody != null)
        {
            ctx.Out.WriteLine($"{cType}{ptr} {className}_get_{name}({self})");
            ctx.Out.WriteLine("{");
            ctx.Indent();
            var val = expr.Write(prop.ExpressionBody.Expression);
            ctx.WriteLine($"return {val};");
            ctx.Dedent();
            ctx.Out.WriteLine("}");
            ctx.Out.WriteLine();
            return;
        }

        // Getter
        var getter = GetAccessor(prop, SyntaxKind.GetAccessorDeclaration);
        if (getter != null)
        {
            ctx.Out.WriteLine($"{cType}{ptr} {className}_get_{name}({self})");
            ctx.Out.WriteLine("{");
            ctx.Indent();
            WriteAccessorBody(getter, ctx, expr, stmt, isVoid: false);
            ctx.Dedent();
            ctx.Out.WriteLine("}");
            ctx.Out.WriteLine();
        }

        // Setter
        var setter = GetAccessor(prop, SyntaxKind.SetAccessorDeclaration);
        if (setter != null)
        {
            ctx.Out.WriteLine($"void {className}_set_{name}({self}, {cType}{ptr} value)");
            ctx.Out.WriteLine("{");
            ctx.Indent();
            ctx.LocalTypes["value"] = csType;
            WriteAccessorBody(setter, ctx, expr, stmt, isVoid: true);
            ctx.Dedent();
            ctx.Out.WriteLine("}");
            ctx.Out.WriteLine();
        }
    }

    // ── ExpressionWriter-Unterstützung ───────────────────────────────────────

    public static string RewriteGet(string receiver, string className, string propName)
        => $"{className}_get_{propName}({receiver})";

    public static string RewriteSet(string receiver, string className, string propName, string value)
        => $"{className}_set_{propName}({receiver}, {value})";

    // ── Private Hilfsmethoden ────────────────────────────────────────────────

    private static void WriteAccessorBody(
        AccessorDeclarationSyntax accessor,
        TranspilerContext ctx,
        ExpressionWriter exprWriter,
        StatementWriter stmtWriter,
        bool isVoid)
    {
        if (accessor.ExpressionBody != null)
        {
            var val = exprWriter.Write(accessor.ExpressionBody.Expression);
            ctx.WriteLine(isVoid ? $"{val};" : $"return {val};");
        }
        else if (accessor.Body != null)
        {
            foreach (var s in accessor.Body.Statements)
                stmtWriter.Write(s);
        }
    }

    private static bool HasAccessor(PropertyDeclarationSyntax prop, SyntaxKind kind)
        => prop.AccessorList?.Accessors.Any(a => a.IsKind(kind)) ?? false;

    private static AccessorDeclarationSyntax? GetAccessor(PropertyDeclarationSyntax prop, SyntaxKind kind)
        => prop.AccessorList?.Accessors.FirstOrDefault(a => a.IsKind(kind));

    private static bool NeedsPtr(string csType)
        => TypeRegistry.NeedsPointerSuffix(csType)
        || TypeRegistry.IsStringBuilder(csType)
        || TypeRegistry.IsList(csType)
        || TypeRegistry.IsDictionary(csType);
}