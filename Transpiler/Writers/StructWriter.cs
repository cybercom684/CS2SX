// ============================================================================
// CS2SX — Transpiler/Writers/StructWriter.cs  (NEU — FIX 4)
//
// Behandelt C# value-type structs als C stack structs.
//
// Verhalten:
//   • C# struct Vec2 { public float X, Y; }
//     →  typedef struct Vec2 { float X; float Y; } Vec2;
//
//   • new Vec2(1f, 2f)            →  (Vec2){ 1.0f, 2.0f }
//   • new Vec2 { X=1, Y=2 }      →  (Vec2){ .X = 1, .Y = 2 }
//   • new Vec2()                  →  (Vec2){0}
//   • Vec2 v = new Vec2(1, 2)     →  Vec2 v = { 1, 2 };     (Stack — kein malloc!)
//   • Vec2[] arr = new Vec2[N]    →  Vec2 arr[N] = {0};      (Stack-Array!)
//
// Unterstützte Features:
//   • Felder (Fields) mit optionalem Default-Wert
//   • Auto-Properties (als Felder emittiert)
//   • Readonly Felder (const in C)
//   • Struct-Methoden als freie Funktionen: Vec2_Add(Vec2* self, ...)
//   • IEquatable<T>-Implementierung → Vec2_Equals(Vec2 a, Vec2 b)
//   • Implizite Konvertierungen → Cast-Makros
//
// Was nicht unterstützt wird (Switch-Homebrew braucht das nicht):
//   • Interface-Implementierungen (außer IEquatable)
//   • Vererbung (C# structs können nicht erben)
//   • Boxing
// ============================================================================

using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Writers;

public sealed class StructWriter
{
    private readonly TranspilerContext _ctx;
    private readonly ExpressionWriter _expr;
    private readonly StatementWriter _stmt;

    public StructWriter(TranspilerContext ctx, ExpressionWriter expr, StatementWriter stmt)
    {
        _ctx = ctx;
        _expr = expr;
        _stmt = stmt;
    }

    // ── Header-Modus: typedef + forward declaration ───────────────────────────

    public void WriteHeaderDecl(StructDeclarationSyntax node)
    {
        var name = node.Identifier.Text;

        // Struct-Namen registrieren (wichtig für ExpressionWriter)
        _ctx.ValueTypeStructs.Add(name);

        _ctx.WriteLine("// C# value-type struct: " + name);
        _ctx.WriteLine("typedef struct " + name + " " + name + ";");

        // Methoden-Signaturen
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            WriteStructMethodSignature(method, name, forwardDecl: true);
        }

        _ctx.WriteLine("");
    }

    // ── Implementierungs-Modus: volle Definition ──────────────────────────────

    public void WriteImpl(StructDeclarationSyntax node)
    {
        var name = node.Identifier.Text;

        // Sicherstellen dass registriert
        _ctx.ValueTypeStructs.Add(name);

        _ctx.WriteLine("// ── Struct: " + name + " ────────────────────────────────────────");
        WriteStructDef(node, name);
        WriteStructMethods(node, name);
        _ctx.WriteLine("");
    }

    // ── Struct-Definition ─────────────────────────────────────────────────────

    private void WriteStructDef(StructDeclarationSyntax node, string name)
    {
        _ctx.WriteLine("typedef struct " + name + " {");
        _ctx.Indent();

        // Felder
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            WriteStructField(field, name);
        }

        // Auto-Properties
        foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (IsAutoProperty(prop))
                WriteStructAutoProperty(prop, name);
        }

        _ctx.Dedent();
        _ctx.WriteLine("} " + name + ";");
        _ctx.WriteLine("");
    }

    private void WriteStructField(FieldDeclarationSyntax field, string structName)
    {
        var csType = field.Declaration.Type.ToString().Trim();
        var cType = TypeRegistry.MapType(csType);
        bool isConst = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)
                                             || m.IsKind(SyntaxKind.ConstKeyword));
        var constKw = isConst ? "const " : "";
        var ptr = TypeRegistry.NeedsPointerSuffix(csType) ? "*" : "";

        foreach (var v in field.Declaration.Variables)
        {
            var defaultComment = v.Initializer != null
                ? " /* = " + _expr.Write(v.Initializer.Value) + " */"
                : "";

            _ctx.WriteLine(constKw + cType + ptr + " " + v.Identifier + ";" + defaultComment);
            _ctx.FieldTypes[v.Identifier.Text] = csType;
        }
    }

    private void WriteStructAutoProperty(PropertyDeclarationSyntax prop, string structName)
    {
        var csType = prop.Type.ToString().Trim();
        var cType = TypeRegistry.MapType(csType);
        var ptr = TypeRegistry.NeedsPointerSuffix(csType) ? "*" : "";
        _ctx.WriteLine(cType + ptr + " " + prop.Identifier + ";");
        _ctx.FieldTypes[prop.Identifier.Text] = csType;
    }

    // ── Struct-Methoden als freie C-Funktionen ────────────────────────────────

    private void WriteStructMethods(StructDeclarationSyntax node, string name)
    {
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            _ctx.ClearMethodContext();
            // Feld-Typen wieder laden (ClearMethodContext löscht LocalTypes, nicht FieldTypes)

            WriteStructMethodSignature(method, name, forwardDecl: false);
            WriteStructMethodBody(method, name);
        }

        // IEquatable<T> → Equals-Funktion mit by-value Vergleich
        WriteStructEquals(node, name);
    }

    private void WriteStructMethodSignature(
        MethodDeclarationSyntax method, string structName, bool forwardDecl)
    {
        var retType = method.ReturnType.ToString().Trim();
        var cRetType = MapReturnType(retType);
        var mName = structName + "_" + method.Identifier.Text;

        // Structs: self als Pointer (für Mutation) oder by-value (für read-only)
        // Wir nutzen Pointer für Einfachheit (kompatibel mit beiden)
        bool isMutating = method.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword))
                       || MethodMutatesStruct(method);

        var selfPart = isMutating
            ? structName + "* self"
            : structName + "* self"; // Immer Pointer für Konsistenz

        var paramList = BuildParamList(method.ParameterList, prependComma: true);
        var allParams = selfPart + paramList;

        if (forwardDecl)
            _ctx.WriteLine(cRetType + " " + mName + "(" + allParams + ");");
        else
            _ctx.WriteLine(cRetType + " " + mName + "(" + allParams + ")");
    }

    private void WriteStructMethodBody(MethodDeclarationSyntax method, string structName)
    {
        // self->field Zugriffe in Methodenrumpf korrekt mappen
        _ctx.LocalTypes["self"] = structName + "*";

        _ctx.WriteLine("{");
        _ctx.Indent();

        if (method.Body != null)
            foreach (var s in method.Body.Statements)
                _stmt.Write(s);
        else if (method.ExpressionBody != null)
        {
            var ret = _expr.Write(method.ExpressionBody.Expression);
            if (method.ReturnType.ToString().Trim() != "void")
                _ctx.WriteLine("return " + ret + ";");
            else
                _ctx.WriteLine(ret + ";");
        }

        _ctx.Dedent();
        _ctx.WriteLine("}");
        _ctx.WriteLine("");
    }

    // ── IEquatable<T> → by-value Equals ──────────────────────────────────────

    private void WriteStructEquals(StructDeclarationSyntax node, string name)
    {
        // Prüfe ob IEquatable<T> implementiert
        bool hasEquatable = node.BaseList?.Types
            .Any(t => t.Type.ToString().Contains("IEquatable")) == true;

        // Auch ohne explizites IEquatable: Structs sollten vergleichbar sein
        // Emit: int Vec2_Equals(Vec2 a, Vec2 b) { return memcmp(&a, &b, sizeof(Vec2)) == 0; }

        _ctx.WriteLine("int " + name + "_Equals(" + name + " a, " + name + " b)");
        _ctx.WriteLine("{");
        _ctx.Indent();
        _ctx.WriteLine("return memcmp(&a, &b, sizeof(" + name + ")) == 0;");
        _ctx.Dedent();
        _ctx.WriteLine("}");
        _ctx.WriteLine("");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool IsAutoProperty(PropertyDeclarationSyntax prop) =>
        prop.AccessorList != null
        && prop.AccessorList.Accessors.All(a =>
               a.Body == null && a.ExpressionBody == null)
        && prop.ExpressionBody == null;

    private static bool MethodMutatesStruct(MethodDeclarationSyntax method)
    {
        // Heuristik: Methode mutiert den Struct wenn sie Assignments auf this.X hat
        if (method.Body == null) return false;
        return method.Body.Statements
            .OfType<ExpressionStatementSyntax>()
            .Any(s => s.Expression is AssignmentExpressionSyntax asgn
                   && (asgn.Left.ToString().StartsWith("this.")
                    || !asgn.Left.ToString().Contains('.')));
    }

    private string BuildParamList(ParameterListSyntax paramList, bool prependComma)
    {
        var parts = paramList.Parameters.Select(p =>
        {
            var csType = p.Type?.ToString().Trim() ?? "int";
            var cType = TypeRegistry.MapType(csType);
            var ptr = TypeRegistry.NeedsPointerSuffix(csType)
                      || TypeRegistry.IsList(csType) ? "*" : "";

            if (p.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword)
                                  || m.IsKind(SyntaxKind.OutKeyword)))
                ptr += "*";

            _ctx.LocalTypes[p.Identifier.Text] = csType;
            return cType + ptr + " " + p.Identifier.Text;
        }).ToList();

        if (parts.Count == 0) return "";
        return (prependComma ? ", " : "") + string.Join(", ", parts);
    }

    private static string MapReturnType(string csRetType)
    {
        if (csRetType == "void") return "void";
        var cType = TypeRegistry.MapType(csRetType);
        var ptr = TypeRegistry.NeedsPointerSuffix(csRetType)
                 || TypeRegistry.IsList(csRetType) ? "*" : "";
        return cType + ptr;
    }
}