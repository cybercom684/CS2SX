using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Strategies;

/// <summary>
/// Konstruktor-Strategie für normale Klassen (keine SwitchApp, kein Control).
/// Generiert eine _New()-Funktion mit malloc + Feld-Initialisierung.
///
/// FIX: Wenn die Klasse einen expliziten Konstruktor mit Parametern hat,
/// werden diese als Parameter an _New() weitergegeben und der Konstruktor-Body
/// wird transpiliert.
///
/// Beispiel:
///   C#:  new MinUiColorPreset(Color.Gray, Color.White, Color.Cyan)
///   Alt: MinUiColorPreset_New()  → alle Felder 0 (falsch)
///   Neu: MinUiColorPreset_New(COLOR_GRAY, COLOR_WHITE, COLOR_CYAN)
///        → führt Konstruktor-Body aus, setzt f_Background etc.
/// </summary>
public sealed class DefaultConstructorStrategy : IConstructorStrategy
{
    public bool Matches(ClassDeclarationSyntax node, string baseType) => true;

    public void Write(ClassDeclarationSyntax node, string name, string baseType,
        TranspilerContext ctx, ExpressionWriter exprWriter, CSharpToC transpiler)
    {
        transpiler.WriteStaticFieldDefinitions(node, name);

        // Expliziten Konstruktor der Klasse suchen
        var explicitCtor = node.Members
            .OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault();

        // Parameter-Liste für _New() aufbauen
        var paramDecls = new List<string>();
        var paramNames = new List<string>();

        if (explicitCtor != null)
        {
            foreach (var p in explicitCtor.ParameterList.Parameters)
            {
                var decl = transpiler.BuildParamDecl(p);
                paramDecls.Add(decl);
                paramNames.Add(p.Identifier.Text);

                // Parameter in LocalTypes registrieren damit der Body-Writer
                // sie korrekt als lokale Variablen behandelt
                var csType = p.Type?.ToString().Trim() ?? "int";
                ctx.LocalTypes[p.Identifier.Text] = csType;
            }
        }

        // Signatur: ClassName* ClassName_New(params...)
        var paramStr = paramDecls.Count > 0
            ? string.Join(", ", paramDecls)
            : "void";

        // Record position before vtable forward-declare + constructor so we can
        // insert any lambda preludes (from property initializers) in front of them.
        var preSigPos = ctx.Out.GetStringBuilder().Length;

        // Forward-declare the vtable instance before _New() so the constructor can
        // reference it even though the full definition appears after all method bodies.
        if (!string.IsNullOrEmpty(baseType) && baseType != "SwitchApp"
            && !CSharpToC.IsControlSubclass(baseType)
            && ctx.VTableTypes.Contains(baseType))
        {
            ctx.Out.WriteLine($"static {baseType}_vtable {name}_vtable_instance;");
            ctx.Out.WriteLine();
        }

        ctx.Out.WriteLine(name + "* " + name + "_New(" + paramStr + ")");
        ctx.Out.WriteLine("{");
        ctx.Indent();

        ctx.WriteLine(name + "* self = (" + name + "*)malloc(sizeof(" + name + "));");
        ctx.WriteLine("if (!self) return NULL;");
        ctx.WriteLine("memset(self, 0, sizeof(" + name + "));");
        if (string.IsNullOrEmpty(baseType))
            ctx.WriteLine("self->_rc = 1;");
        else if (baseType != "SwitchApp" && !CSharpToC.IsControlSubclass(baseType))
            ctx.WriteLine("self->base._rc = 1;");

        // VTable-Zeiger setzen: vtable lives in the embedded base struct, not in self directly.
        // self->base.vtable points to the correct vtable for this subclass so that
        // (BaseType*)self casts give correct vtable dispatch.
        if (!string.IsNullOrEmpty(baseType) && baseType != "SwitchApp"
            && !CSharpToC.IsControlSubclass(baseType)
            && ctx.VTableTypes.Contains(baseType))
        {
            ctx.WriteLine("self->base.vtable = &" + name + "_vtable_instance;");
        }

        // Base-class property/field initializers (e.g. Visible = true from UIControl).
        // Without this, memset zeros the embedded base struct and subclass controls
        // are invisible by default.
        if (!string.IsNullOrEmpty(baseType) && baseType != "SwitchApp"
            && !CSharpToC.IsControlSubclass(baseType))
        {
            transpiler.WriteInstanceFieldInitializersForBaseClass(baseType);
        }

        // Feld-Initializer aus Feld-Deklarationen
        transpiler.WriteInstanceFieldInitializers(node);

        // Expliziten Konstruktor-Body transpilieren
        if (explicitCtor?.Body != null)
        {
            var stmtWriter = new StatementWriter(ctx, exprWriter);
            foreach (var stmt in explicitCtor.Body.Statements)
                stmtWriter.Write(stmt);
        }
        else if (explicitCtor?.ExpressionBody != null)
        {
            ctx.WriteLine(exprWriter.Write(explicitCtor.ExpressionBody.Expression) + ";");
        }

        // Override-Funktionszeiger für virtuelle Methoden (nur für built-in Control-Subklassen)
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            var isOverride = method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));
            var isVirtual = method.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword));
            if (!isOverride && !isVirtual) continue;

            // Nur für Control-Subklassen: dort sind direkte Funktionszeiger in base
            if (!CSharpToC.IsControlSubclass(baseType)) continue;

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

        // Flush lambda preludes generated by property initializers before the constructor
        if (ctx.PendingLambdaPreludes.Count > 0)
        {
            var sb2 = ctx.Out.GetStringBuilder();
            var ctorText = sb2.ToString(preSigPos, sb2.Length - preSigPos);
            sb2.Remove(preSigPos, sb2.Length - preSigPos);
            ctx.FlushLambdaPreludes();
            ctx.Out.Write(ctorText);
        }

        // LocalTypes aufräumen
        foreach (var p in paramNames)
            ctx.LocalTypes.Remove(p);
    }
}