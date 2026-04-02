using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;

namespace CS2SX.Transpiler;

/// <summary>
/// Transpiliert C# Properties zu C Getter/Setter-Funktionen.
///
/// C#                             → C
/// ───────────────────────────────────────────────────────────────────
/// public int Speed { get; set; } → int f_Speed;          (einfaches Backing-Feld)
///
/// public int Speed               → int Player_get_Speed(Player* self);
/// {                                 void Player_set_Speed(Player* self, int value);
///     get => f_speed * 2;
///     set => f_speed = value / 2;
/// }
///
/// public string Name             → const char* Player_get_Name(Player* self);
/// {                                 (kein Setter → const)
///     get => f_name;
/// }
///
/// Auto-Properties (get; set; ohne Body) werden als einfaches struct-Feld behandelt.
/// Properties mit Body bekommen explizite Getter/Setter-Funktionen.
/// </summary>
public static class PropertyWriter
{
    /// <summary>
    /// True wenn die Property auto-implementiert ist (kein Body).
    /// Auto-Properties werden zu struct-Feldern — keine Funktionen nötig.
    /// </summary>
    public static bool IsAutoProperty(PropertyDeclarationSyntax prop)
    {
        if (prop.AccessorList == null) return false;

        return prop.AccessorList.Accessors.All(a =>
            a.Body == null && a.ExpressionBody == null);
    }

    /// <summary>
    /// Schreibt Getter/Setter-Signaturen in den Header.
    /// Nur für Properties mit explizitem Body aufrufen.
    /// </summary>
    public static void WriteSignatures(
        PropertyDeclarationSyntax prop,
        string className,
        System.IO.TextWriter output)
    {
        if (IsAutoProperty(prop)) return;

        var cType  = TypeRegistry.MapType(prop.Type.ToString().Trim());
        var name   = prop.Identifier.Text;
        var hasGet = HasAccessor(prop, SyntaxKind.GetAccessorDeclaration);
        var hasSet = HasAccessor(prop, SyntaxKind.SetAccessorDeclaration);

        var ptr  = NeedsPtr(prop.Type.ToString().Trim()) ? "*" : "";
        var self = className + "* self";

        if (hasGet) output.WriteLine(cType + ptr + " " + className + "_get_" + name + "(" + self + ");");
        if (hasSet) output.WriteLine("void " + className + "_set_" + name + "(" + self + ", " + cType + ptr + " value);");
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
        var cType  = TypeRegistry.MapType(csType);
        var name   = prop.Identifier.Text;
        var ptr    = NeedsPtr(csType) ? "*" : "";
        var self   = className + "* self";

        ctx.CurrentClass = className;

        // Getter
        var getter = GetAccessor(prop, SyntaxKind.GetAccessorDeclaration);
        if (getter != null)
        {
            ctx.Out.WriteLine(cType + ptr + " " + className + "_get_" + name + "(" + self + ")");
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
            ctx.Out.WriteLine("void " + className + "_set_" + name + "(" + self + ", " + cType + ptr + " value)");
            ctx.Out.WriteLine("{");
            ctx.Indent();
            // "value" als lokale Variable registrieren
            ctx.LocalTypes["value"] = csType;
            WriteAccessorBody(setter, ctx, expr, stmt, isVoid: true);
            ctx.Dedent();
            ctx.Out.WriteLine("}");
            ctx.Out.WriteLine();
        }
    }

    // ── ExpressionWriter-Unterstützung ───────────────────────────────────

    /// <summary>
    /// Rewrites einen Property-Zugriff zu einem Getter-Aufruf.
    /// myObj.Speed = 5 → Player_set_Speed(myObj, 5)
    /// x = myObj.Speed → Player_get_Speed(myObj)
    ///
    /// Wird von ExpressionWriter.WriteMemberAccess aufgerufen wenn der
    /// Member als Property im Context registriert ist.
    /// </summary>
    public static string RewriteGet(string receiver, string className, string propName)
        => className + "_get_" + propName + "(" + receiver + ")";

    public static string RewriteSet(string receiver, string className, string propName, string value)
        => className + "_set_" + propName + "(" + receiver + ", " + value + ")";

    // ── Private Hilfsmethoden ────────────────────────────────────────────

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
            if (isVoid)
                ctx.WriteLine(val + ";");
            else
                ctx.WriteLine("return " + val + ";");
        }
        else if (accessor.Body != null)
        {
            foreach (var s in accessor.Body.Statements)
                stmtWriter.Write(s);
        }
        else
        {
            // Leerer Accessor — nichts schreiben
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
