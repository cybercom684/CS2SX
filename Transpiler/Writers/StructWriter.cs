// ============================================================================
// CS2SX — Transpiler/Writers/StructWriter.cs
//
// C# value-type structs (incl. `record struct`) → C value structs.
//
//   C#                                   C
//   struct Vec2 { public float X, Y; }   typedef struct Vec2 { float X; float Y; } Vec2;   (header)
//   public Vec2(float x, float y) {..}   Vec2 Vec2_New(float x, float y)                   (returns by value)
//   new Vec2(1, 2)                       Vec2_New(1, 2)
//   new Vec2 { X = 1 }                   (Vec2){ .X = 1 }
//   new Vec2() / default                 (Vec2){0}
//   v.Length()                           Vec2_Length(&v)       (self is always Vec2*)
//   static Vec2 Lerp(..)                 Vec2 Vec2_Lerp(..)    (no self)
//   operator +(Vec2 a, Vec2 b)           Vec2 Vec2_op_add(Vec2 a, Vec2 b)
//
// Field names are kept as written in C# (no f_ prefix), like libnx structs.
// The full definition lives in the HEADER so other files and classes can embed
// the struct by value. Structs are registered project-wide up front
// (ProjectTypeScanner → TypeRegistry.IsUserStruct).
//
// Not supported: computed properties, multiple constructors (first one wins, with
// a warning), interfaces other than IEquatable, boxing.
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

    // ── Header: full definition + prototypes ─────────────────────────────────

    public void WriteHeaderDecl(TypeDeclarationSyntax node)
    {
        var name = node.Identifier.Text;
        _ctx.ValueTypeStructs.Add(name);

        _ctx.WriteLine("// C# value-type struct: " + name);
        WriteStructDef(node, name);

        foreach (var ctor in Constructors(node).Take(1))
            _ctx.WriteLine(CtorSignature(ctor, name) + ";");
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
            _ctx.WriteLine(MethodSignature(method, name) + ";");
        foreach (var op in node.Members.OfType<OperatorDeclarationSyntax>())
            _ctx.WriteLine(OperatorSignature(op, name) + ";");
        _ctx.WriteLine("int " + name + "_Equals(" + name + " a, " + name + " b);");
        _ctx.WriteLine("");
    }

    // ── Implementation: function bodies ──────────────────────────────────────

    public void WriteImpl(TypeDeclarationSyntax node)
    {
        var name = node.Identifier.Text;
        _ctx.ValueTypeStructs.Add(name);

        _ctx.WriteLine("// ── Struct: " + name + " ────────────────────────────────────────");

        var ctors = Constructors(node).ToList();
        if (ctors.Count > 1)
            _ctx.Warn(ctors[1], $"Struct '{name}' hat {ctors.Count} Konstruktoren — nur der erste wird transpiliert.");
        if (ctors.Count > 0)
            WriteCtor(ctors[0], name);

        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            _ctx.ClearMethodContext();
            bool isStatic = IsStatic(method.Modifiers);
            _ctx.WriteLine(MethodSignature(method, name));
            _ctx.IsStaticMethod = isStatic;
            if (!isStatic) _ctx.LocalTypes["self"] = name + "*";
            WriteBody(method.Body, method.ExpressionBody, method.ReturnType.ToString().Trim() != "void");
        }

        foreach (var op in node.Members.OfType<OperatorDeclarationSyntax>())
        {
            _ctx.ClearMethodContext();
            _ctx.WriteLine(OperatorSignature(op, name));
            _ctx.IsStaticMethod = true;
            WriteBody(op.Body, op.ExpressionBody, hasReturn: true);
        }

        _ctx.WriteLine("int " + name + "_Equals(" + name + " a, " + name + " b)");
        _ctx.WriteLine("{");
        _ctx.Indent();
        _ctx.WriteLine("return memcmp(&a, &b, sizeof(" + name + ")) == 0;");
        _ctx.Dedent();
        _ctx.WriteLine("}");
        _ctx.WriteLine("");
    }

    // ── Definition ───────────────────────────────────────────────────────────

    private void WriteStructDef(TypeDeclarationSyntax node, string name)
    {
        _ctx.WriteLine("typedef struct " + name + " {");
        _ctx.Indent();

        int members = 0;

        // record struct Score(string Player, int Points) → positional members first
        if (node is RecordDeclarationSyntax rec && rec.ParameterList != null)
            foreach (var p in rec.ParameterList.Parameters)
            {
                var csType = p.Type?.ToString().Trim() ?? "int";
                _ctx.WriteLine(MapStorageType(csType) + " " + p.Identifier.Text + ";");
                _ctx.FieldTypes[p.Identifier.Text] = csType;
                members++;
            }

        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (IsStatic(field.Modifiers) || field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword)))
                continue;
            var csType = field.Declaration.Type.ToString().Trim();
            foreach (var v in field.Declaration.Variables)
            {
                _ctx.WriteLine(MapStorageType(csType) + " " + v.Identifier.Text + ";");
                _ctx.FieldTypes[v.Identifier.Text] = csType;
                members++;
            }
        }

        foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (!IsAutoProperty(prop) || IsStatic(prop.Modifiers)) continue;
            var csType = prop.Type.ToString().Trim();
            _ctx.WriteLine(MapStorageType(csType) + " " + prop.Identifier.Text + ";");
            _ctx.FieldTypes[prop.Identifier.Text] = csType;
            members++;
        }

        // An empty struct is not valid ISO C.
        if (members == 0) _ctx.WriteLine("char _empty;");

        _ctx.Dedent();
        _ctx.WriteLine("} " + name + ";");
    }

    // ── Constructor → T T_New(params) returning by value ─────────────────────

    private void WriteCtor(ConstructorDeclarationSyntax ctor, string name)
    {
        _ctx.ClearMethodContext();
        _ctx.WriteLine(CtorSignature(ctor, name));
        _ctx.LocalTypes["self"] = name + "*";
        _ctx.WriteLine("{");
        _ctx.Indent();
        _ctx.WriteLine(name + " _self_value = {0};");
        _ctx.WriteLine(name + "* self = &_self_value;");
        if (ctor.Body != null)
            foreach (var s in ctor.Body.Statements) _stmt.Write(s);
        else if (ctor.ExpressionBody != null)
            _ctx.WriteLine(_expr.Write(ctor.ExpressionBody.Expression) + ";");
        _ctx.WriteLine("return _self_value;");
        _ctx.Dedent();
        _ctx.WriteLine("}");
        _ctx.WriteLine("");
    }

    private void WriteBody(BlockSyntax? body, ArrowExpressionClauseSyntax? exprBody, bool hasReturn)
    {
        _ctx.WriteLine("{");
        _ctx.Indent();
        if (body != null)
            foreach (var s in body.Statements) _stmt.Write(s);
        else if (exprBody != null)
        {
            var e = _expr.Write(exprBody.Expression);
            _ctx.WriteLine(hasReturn ? "return " + e + ";" : e + ";");
        }
        _ctx.Dedent();
        _ctx.WriteLine("}");
        _ctx.WriteLine("");
    }

    // ── Signatures ───────────────────────────────────────────────────────────

    private string CtorSignature(ConstructorDeclarationSyntax ctor, string name) =>
        name + " " + name + "_New(" + ParamList(ctor.ParameterList, "void") + ")";

    private string MethodSignature(MethodDeclarationSyntax method, string name)
    {
        var ret = MapReturnType(method.ReturnType.ToString().Trim());
        var fn = name + "_" + method.Identifier.Text;
        if (IsStatic(method.Modifiers))
            return ret + " " + fn + "(" + ParamList(method.ParameterList, "void") + ")";
        var rest = ParamList(method.ParameterList, "");
        return ret + " " + fn + "(" + name + "* self" + (rest.Length > 0 ? ", " + rest : "") + ")";
    }

    private string OperatorSignature(OperatorDeclarationSyntax op, string name)
    {
        var ret = MapReturnType(op.ReturnType.ToString().Trim());
        return ret + " " + OperatorOverloadWriter.GetFunctionName(name, op.OperatorToken.Text)
             + "(" + ParamList(op.ParameterList, "void") + ")";
    }

    private string ParamList(ParameterListSyntax paramList, string empty)
    {
        var parts = paramList.Parameters.Select(p =>
        {
            var csType = p.Type?.ToString().Trim() ?? "int";
            var decl = MapStorageType(csType);
            if (p.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword)))
                decl += "*";
            _ctx.LocalTypes[p.Identifier.Text] = csType;
            return decl + " " + p.Identifier.Text;
        }).ToList();
        return parts.Count == 0 ? empty : string.Join(", ", parts);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static IEnumerable<ConstructorDeclarationSyntax> Constructors(TypeDeclarationSyntax node) =>
        node.Members.OfType<ConstructorDeclarationSyntax>().Where(c => !IsStatic(c.Modifiers));

    private static bool IsStatic(SyntaxTokenList mods) => mods.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

    private static bool IsAutoProperty(PropertyDeclarationSyntax prop) =>
        prop.AccessorList != null
        && prop.AccessorList.Accessors.All(a => a.Body == null && a.ExpressionBody == null)
        && prop.ExpressionBody == null;

    private static string MapStorageType(string csType)
    {
        var cType = TypeRegistry.MapType(csType);
        var ptr = !cType.EndsWith("*") && (TypeRegistry.NeedsPointerSuffix(csType)
                  || TypeRegistry.IsList(csType)) ? "*" : "";
        return cType + ptr;
    }

    private static string MapReturnType(string csRetType) =>
        csRetType == "void" ? "void" : MapStorageType(csRetType);
}
