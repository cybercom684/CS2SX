using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Strategies;

/// <summary>
/// Konstruktor-Strategie für normale Klassen (keine SwitchApp, kein Control).
/// Generiert eine _New-Funktion mit malloc + Feld-Initialisierung.
/// </summary>
public sealed class DefaultConstructorStrategy : IConstructorStrategy
{
    public bool Matches(ClassDeclarationSyntax node, string baseType) => true;

    public void Write(ClassDeclarationSyntax node, string name, string baseType,
        TranspilerContext ctx, ExpressionWriter exprWriter, CSharpToC transpiler)
    {
        transpiler.WriteStaticFieldDefinitions(node, name);

        ctx.Out.WriteLine(name + "* " + name + "_New()");
        ctx.Out.WriteLine("{");
        ctx.Indent();

        ctx.WriteLine(name + "* self = (" + name + "*)malloc(sizeof(" + name + "));");
        ctx.WriteLine("memset(self, 0, sizeof(" + name + "));");

        transpiler.WriteInstanceFieldInitializers(node);

        // Virtual/Override Funktionszeiger verdrahten
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            var isOverride = method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));
            var isVirtual = method.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword));
            if (!isOverride && !isVirtual) continue;

            var returnType = TypeRegistry.MapType(method.ReturnType.ToString().Trim());
            var paramTypes = new List<string> { name + "*" };
            foreach (var p in method.ParameterList.Parameters)
                paramTypes.Add(TypeRegistry.MapType(p.Type!.ToString().Trim()));

            var castSig = returnType + "(*)(" + string.Join(", ", paramTypes) + ")";
            ctx.WriteLine("self->base." + method.Identifier.Text
                + " = (" + castSig + ")" + name + "_" + method.Identifier.Text + ";");
        }

        ctx.WriteLine("return self;");
        ctx.Dedent();
        ctx.Out.WriteLine("}");
        ctx.Out.WriteLine();
    }
}