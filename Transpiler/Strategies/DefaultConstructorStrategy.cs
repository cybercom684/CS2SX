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

        // Multi-level inheritance: _rc and the vtable pointer live in the absolute
        // user root. `baseChain` is "base." repeated to reach it; `vtType` is the
        // ancestor that declares the vtable struct (overrides reuse the root's type).
        int rootHops = ctx.RootHops.TryGetValue(name, out var rh) && rh > 0 ? rh : 1;
        var baseChain = string.Concat(System.Linq.Enumerable.Repeat("base.", rootHops));
        var vtType = ctx.VTableRoot.TryGetValue(name, out var vr) ? vr : baseType;

        // Forward-declare the vtable instance before _New() so the constructor can
        // reference it even though the full definition appears after all method bodies.
        if (!string.IsNullOrEmpty(baseType) && baseType != "SwitchApp"
            && !CSharpToC.IsControlSubclass(baseType)
            && ctx.VTableTypes.Contains(baseType))
        {
            ctx.Out.WriteLine($"static {vtType}_vtable {name}_vtable_instance;");
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
        else if (baseType != "SwitchApp" && !CSharpToC.IsControlSubclass(baseType)
                 && !ctx.InterfaceTypes.Contains(baseType))  // interface base has no _rc
            ctx.WriteLine("self->" + baseChain + "_rc = 1;");

        // VTable-Zeiger setzen: vtable lives in the embedded base struct, not in self directly.
        // self->base[.base]*.vtable points to the correct vtable for this subclass so that
        // (BaseType*)self casts give correct vtable dispatch.
        if (!string.IsNullOrEmpty(baseType) && baseType != "SwitchApp"
            && !CSharpToC.IsControlSubclass(baseType)
            && ctx.VTableTypes.Contains(baseType))
        {
            ctx.WriteLine("self->" + baseChain + "vtable = &" + name + "_vtable_instance;");
        }

        // Constructor initializer: `: base(args)` / `: this(args)`.
        var ctorInit = explicitCtor?.Initializer;
        bool isUserBase = !string.IsNullOrEmpty(baseType) && baseType != "SwitchApp"
            && !CSharpToC.IsControlSubclass(baseType) && !ctx.InterfaceTypes.Contains(baseType);
        bool hasBaseCall = ctorInit != null
            && ctorInit.ThisOrBaseKeyword.IsKind(SyntaxKind.BaseKeyword);

        if (isUserBase && hasBaseCall)
        {
            // Run the base constructor with the supplied arguments and copy the
            // result into the embedded base struct (the base is a value member).
            // This is what was previously dropped, leaving base fields at 0.
            var baseArgs = ctorInit!.ArgumentList.Arguments
                .Select(a => exprWriter.Write(a.Expression));
            var bTmp = ctx.NextTmp("base");
            ctx.WriteLine($"{baseType}* {bTmp} = {baseType}_New({string.Join(", ", baseArgs)});");
            ctx.WriteLine($"if ({bTmp}) {{ self->base = *{bTmp}; free({bTmp}); }}");
            // The copy clobbers vtable/_rc — restore them at the correct depth.
            if (ctx.VTableTypes.Contains(baseType))
                ctx.WriteLine($"self->{baseChain}vtable = &{name}_vtable_instance;");
            ctx.WriteLine($"self->{baseChain}_rc = 1;");
        }
        else if (ctorInit != null && ctorInit.ThisOrBaseKeyword.IsKind(SyntaxKind.ThisKeyword))
        {
            ctx.Warn(explicitCtor!, "constructor `: this(...)` delegation is not supported "
                + "(only one constructor per class is emitted); ignored.");
            if (isUserBase) transpiler.WriteInstanceFieldInitializersForBaseClass(baseType);
        }
        // Base-class property/field initializers (e.g. Visible = true from UIControl).
        // Without this, memset zeros the embedded base struct and subclass controls
        // are invisible by default. Skipped when a base(...) call already ran the
        // base constructor (which performs those initializers itself).
        else if (!string.IsNullOrEmpty(baseType) && baseType != "SwitchApp"
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