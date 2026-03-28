using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Strategies;

/// <summary>
/// Konstruktor-Strategie für SwitchApp-Subklassen.
/// Generiert eine _Init-Funktion die Lifecycle-Funktionszeiger verdrahtet.
/// </summary>
public sealed class SwitchAppConstructorStrategy : IConstructorStrategy
{
    private const string SwitchAppBase = "SwitchApp";

    public bool Matches(ClassDeclarationSyntax node, string baseType) =>
        baseType == SwitchAppBase;

    public void Write(ClassDeclarationSyntax node, string name, string baseType,
        TranspilerContext ctx, ExpressionWriter exprWriter, CSharpToC transpiler)
    {
        transpiler.WriteStaticFieldDefinitions(node, name);

        ctx.Out.WriteLine("void " + name + "_Init(" + name + "* self)");
        ctx.Out.WriteLine("{");
        ctx.Indent();

        // Lifecycle-Funktionszeiger verdrahten
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            var methodName = method.Identifier.Text;
            var isOverride = method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));
            var isAbstract = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
            if (!isOverride && !isAbstract) continue;

            if (methodName is "OnInit" or "OnFrame" or "OnExit")
                ctx.WriteLine("((SwitchApp*)self)->" + methodName
                    + " = (void(*)(SwitchApp*))" + name + "_" + methodName + ";");
        }

        transpiler.WriteInstanceFieldInitializers(node);

        ctx.Dedent();
        ctx.Out.WriteLine("}");
        ctx.Out.WriteLine();
    }
}