using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Strategies;

/// <summary>
/// Konstruktor-Strategie für Control-Subklassen (eigene Controls).
/// Generiert eine _New-Funktion die Draw/Update Funktionszeiger verdrahtet.
/// </summary>
public sealed class ControlSubclassConstructorStrategy : IConstructorStrategy
{
    public bool Matches(ClassDeclarationSyntax node, string baseType) =>
        CSharpToC.IsControlSubclass(baseType);

    public void Write(ClassDeclarationSyntax node, string name, string baseType,
        TranspilerContext ctx, ExpressionWriter exprWriter, CSharpToC transpiler)
    {
        transpiler.WriteStaticFieldDefinitions(node, name);

        ctx.Out.WriteLine(name + "* " + name + "_New()");
        ctx.Out.WriteLine("{");
        ctx.Indent();

        ctx.WriteLine(name + "* self = (" + name + "*)malloc(sizeof(" + name + "));");
        ctx.WriteLine("if (!self) return NULL;");
        ctx.WriteLine("memset(self, 0, sizeof(" + name + "));");
        ctx.WriteLine("((Control*)self)->visible = 1;");
        ctx.WriteLine("((Control*)self)->focusable = 0;");

        // Draw/Update Funktionszeiger verdrahten
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            var methodName = method.Identifier.Text;
            var isOverride = method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));
            if (!isOverride) continue;

            if (methodName == "Draw")
                ctx.WriteLine("((Control*)self)->Draw = (void(*)(Control*))"
                    + name + "_Draw;");

            if (methodName == "Update")
            {
                ctx.WriteLine("((Control*)self)->Update = (void(*)(Control*, u64, u64))"
                    + name + "_Update;");
                ctx.WriteLine("((Control*)self)->focusable = 1;");
            }
        }

        transpiler.WriteInstanceFieldInitializers(node);

        ctx.WriteLine("return self;");
        ctx.Dedent();
        ctx.Out.WriteLine("}");
        ctx.Out.WriteLine();
    }
}