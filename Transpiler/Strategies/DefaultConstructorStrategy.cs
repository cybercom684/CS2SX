using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Strategies;

/// <summary>
/// Konstruktor-Strategie für normale Klassen (keine SwitchApp, kein Control).
/// Generiert eine _New-Funktion mit malloc + Feld-Initialisierung.
/// Setzt zusätzlich vtable-Zeiger wenn die Basisklasse virtuelle Methoden hat.
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
        ctx.WriteLine("if (!self) return NULL;");
        ctx.WriteLine("memset(self, 0, sizeof(" + name + "));");

        // VTable-Zeiger setzen wenn Basisklasse existiert
        if (!string.IsNullOrEmpty(baseType) && baseType != "SwitchApp"
            && !CSharpToC.IsControlSubclass(baseType))
        {
            ctx.WriteLine("self->vtable = &" + name + "_vtable_instance;");
        }

        transpiler.WriteInstanceFieldInitializers(node);

        // Override-Funktionszeiger für eigene virtual/override Methoden verdrahten
        // (für Klassen ohne explizite Basisklasse, die eigene virtual-Methoden haben)
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            var isOverride = method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));
            var isVirtual = method.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword));
            if (!isOverride && !isVirtual) continue;

            // Nur für Inline-Funktionszeiger im Struct (nicht VTable-basiert)
            if (!string.IsNullOrEmpty(baseType) && !CSharpToC.IsControlSubclass(baseType))
                continue; // VTable übernimmt das

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