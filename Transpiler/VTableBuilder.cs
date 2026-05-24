using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using Microsoft.CodeAnalysis;

namespace CS2SX.Transpiler;

/// <summary>
/// Erzeugt vtable-Infrastruktur für C# Vererbung und virtuelle Methoden.
///
/// C#                              → C
/// ──────────────────────────────────────────────────────────────────────
/// abstract class Animal           → typedef struct Animal_vtable {
/// {                                     void (*Speak)(void* self);
///     abstract void Speak();            void (*Update)(void* self);
///     virtual void Update() { }     } Animal_vtable;
/// }
///                                   typedef struct Animal {
///                                       Animal_vtable* vtable;
///                                       /* Felder */
///                                   } Animal;
///
/// class Dog : Animal              → static void Dog_Speak(void* self);
/// {                                 static void Dog_Update(void* self);
///     override void Speak() { }
///     override void Update() { }    static Animal_vtable Dog_vtable_instance = {
/// }                                     .Speak  = Dog_Speak,
///                                       .Update = Dog_Update,
///                                   };
///
///                                   Dog* Dog_New() {
///                                       Dog* self = malloc(sizeof(Dog));
///                                       self->vtable = &Dog_vtable_instance;
///                                       return self;
///                                   }
///
/// Virtueller Aufruf:
///   animal.Speak()  →  animal->vtable->Speak(animal)
/// </summary>
public static class VTableBuilder
{
    // ── Header-Ausgabe ────────────────────────────────────────────────────

    /// <summary>
    /// Schreibt die vtable-Struct-Definition für eine Basisklasse in den Header.
    /// Wird für alle Klassen mit virtual/abstract Methoden aufgerufen.
    /// </summary>
    public static void WriteVTableStruct(
        ClassDeclarationSyntax node,
        string className,
        System.IO.TextWriter output)
    {
        var virtuals  = GetVirtualMethods(node);
        var virtProps = GetVirtualProperties(node);
        if (virtuals.Count == 0 && virtProps.Count == 0) return;

        output.WriteLine("typedef struct " + className + "_vtable");
        output.WriteLine("{");

        foreach (var method in virtuals)
        {
            var retC  = TypeRegistry.MapType(method.ReturnType.ToString().Trim());
            var parms = new List<string> { "void* self" };
            foreach (var p in method.ParameterList.Parameters)
            {
                var pt = TypeRegistry.MapType(p.Type?.ToString().Trim() ?? "int");
                parms.Add(pt + " " + p.Identifier.Text);
            }
            output.WriteLine("    " + retC + " (*" + method.Identifier.Text
                           + ")(" + string.Join(", ", parms) + ");");
        }

        // Virtual/abstract properties → getter and setter function pointers
        foreach (var prop in virtProps)
        {
            var propRetC = TypeRegistry.MapType(prop.Type.ToString().Trim());
            var needsPtr = TypeRegistry.NeedsPointerSuffix(prop.Type.ToString().Trim())
                        && !propRetC.EndsWith("*");
            var retDecl = propRetC + (needsPtr ? "*" : "");
            var propName = prop.Identifier.Text;

            bool hasGetter = prop.AccessorList?.Accessors
                .Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) == true
                || prop.ExpressionBody != null;
            bool hasSetter = prop.AccessorList?.Accessors
                .Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) == true;

            if (hasGetter)
                output.WriteLine($"    {retDecl} (*get_{propName})(void* self);");
            if (hasSetter)
                output.WriteLine($"    void (*set_{propName})(void* self, {retDecl} value);");
        }

        output.WriteLine("} " + className + "_vtable;");
        output.WriteLine();
    }

    /// <summary>
    /// Schreibt den vtable-Zeiger-Eintrag in die Struct-Definition.
    /// Muss als erstes Feld erscheinen damit Casting funktioniert.
    /// </summary>
    public static void WriteVTableFieldDecl(
        string baseClassName,
        System.IO.TextWriter output)
    {
        output.WriteLine("    " + baseClassName + "_vtable* vtable;");
    }

    // ── Implementierungs-Ausgabe ──────────────────────────────────────────

    /// <summary>
    /// Schreibt die statische vtable-Instanz für eine abgeleitete Klasse.
    /// Wird nach allen Methoden der Klasse ausgegeben.
    ///
    /// Gibt eine Liste der Methoden zurück die als override erkannt wurden
    /// (für den Konstruktor der dann self->vtable = &Foo_vtable_instance setzen muss).
    /// </summary>
    public static List<string> WriteVTableInstance(
        ClassDeclarationSyntax node,
        string className,
        string baseClassName,
        System.IO.TextWriter output,
        Microsoft.CodeAnalysis.SemanticModel? semanticModel = null)
    {
        var overrides     = GetOverrideMethods(node);
        var overrideProps = node.Members.OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
            .ToList();

        if (overrides.Count == 0 && overrideProps.Count == 0 && !HasInheritance(node))
            return new();

        var instanceName = className + "_vtable_instance";

        output.WriteLine("static " + baseClassName + "_vtable " + instanceName + " =");
        output.WriteLine("{");

        var overriddenNames = new HashSet<string>(overrides.Select(m => m.Identifier.Text),
            StringComparer.Ordinal);

        var methodNames = new List<string>();
        foreach (var method in overrides)
        {
            var mName = method.Identifier.Text;
            methodNames.Add(mName);
            // Cast to match vtable slot signature (void* self) to avoid -Wincompatible-pointer-types
            var retC = TypeRegistry.MapType(method.ReturnType.ToString().Trim());
            var castParms = new List<string> { "void*" };
            foreach (var p in method.ParameterList.Parameters)
                castParms.Add(TypeRegistry.MapType(p.Type?.ToString().Trim() ?? "int"));
            var castType = retC + "(*)(" + string.Join(", ", castParms) + ")";
            output.WriteLine($"    .{mName} = ({castType}){className}_{mName},");
        }

        // For virtual methods NOT overridden in this class, wire to the base implementation
        // so vtable slots are never NULL (null function pointer → crash on first call).
        if (semanticModel != null)
        {
            try
            {
                var classSym = semanticModel.GetDeclaredSymbol(node);
                var baseSym = classSym?.BaseType;
                if (baseSym != null)
                {
                    foreach (var bm in baseSym.GetMembers().OfType<Microsoft.CodeAnalysis.IMethodSymbol>()
                        .Where(m => (m.IsVirtual || m.IsAbstract) && !m.IsStatic))
                    {
                        if (overriddenNames.Contains(bm.Name)) continue;
                        if (bm.IsAbstract) continue; // no base body to fall back to
                        var retC = TypeRegistry.MapType(
                            TranspilerContext.FormatTypeSymbol(bm.ReturnType));
                        var castParms = new List<string> { "void*" };
                        foreach (var p in bm.Parameters)
                            castParms.Add(TypeRegistry.MapType(
                                TranspilerContext.FormatTypeSymbol(p.Type)));
                        var castType = retC + "(*)(" + string.Join(", ", castParms) + ")";
                        output.WriteLine(
                            $"    .{bm.Name} = ({castType}){baseSym.Name}_{bm.Name},");
                    }
                }
            }
            catch { }
        }

        // Override property getter/setter entries
        foreach (var prop in overrideProps)
        {
            var propName = prop.Identifier.Text;
            bool hasGetter = prop.AccessorList?.Accessors
                .Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) == true
                || prop.ExpressionBody != null;
            bool hasSetter = prop.AccessorList?.Accessors
                .Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) == true;
            if (hasGetter)
                output.WriteLine($"    .get_{propName} = {className}_get_{propName},");
            if (hasSetter)
                output.WriteLine($"    .set_{propName} = {className}_set_{propName},");
        }

        output.WriteLine("};");
        output.WriteLine();

        return methodNames;
    }

    /// <summary>
    /// Validates that all override methods in a derived class have a matching
    /// virtual/abstract method in the declared base class.
    /// Emits warnings for mismatches that would cause runtime crashes.
    /// </summary>
    public static void ValidateOverrides(
        ClassDeclarationSyntax derivedClass,
        string baseClassName,
        TranspilerContext ctx)
    {
        // Collect virtual/abstract methods from bases we know about
        // (We only have the syntax of the derived class here; we warn conservatively.)
        var overrides = derivedClass.Members
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.OverrideKeyword)))
            .ToList();

        if (overrides.Count == 0) return;

        // Try to find the base class definition via SemanticModel
        if (ctx.SemanticModel != null)
        {
            try
            {
                var sym = ctx.SemanticModel.GetDeclaredSymbol(derivedClass);
                var baseType = sym?.BaseType;

                if (baseType != null)
                {
                    var baseVirtualNames = new HashSet<string>(
                        baseType.GetMembers()
                            .OfType<Microsoft.CodeAnalysis.IMethodSymbol>()
                            .Where(m => m.IsVirtual || m.IsAbstract || m.IsOverride)
                            .Select(m => m.Name),
                        StringComparer.Ordinal);

                    foreach (var ov in overrides)
                    {
                        var methodName = ov.Identifier.Text;
                        if (!baseVirtualNames.Contains(methodName))
                        {
                            ctx.Warn($"'{derivedClass.Identifier.Text}.{methodName}' is marked override " +
                                     $"but '{baseClassName}' has no matching virtual/abstract method. " +
                                     $"This will cause a runtime crash if called via vtable.",
                                     methodName);
                        }
                    }
                }
            }
            catch { /* SemanticModel lookup failures are non-fatal */ }
        }
        else
        {
            // Without SemanticModel, warn generically that we can't validate
            if (overrides.Count > 0 && !string.IsNullOrEmpty(baseClassName)
                && baseClassName != "SwitchApp"
                && !CSharpToC.IsControlSubclass(baseClassName))
            {
                ctx.Warn($"Cannot validate vtable overrides for '{derivedClass.Identifier.Text}' " +
                         $"(no SemanticModel). Ensure '{baseClassName}' declares matching virtual methods.",
                         baseClassName);
            }
        }
    }

    /// <summary>
    /// Erzeugt den C-Code für einen virtuellen Methodenaufruf.
    /// obj.Speak() → obj->vtable->Speak(obj)
    /// </summary>
    public static string WriteVirtualCall(
        string receiverExpr,
        string methodName,
        IEnumerable<string> args)
    {
        var argList = new List<string> { receiverExpr };
        argList.AddRange(args);
        return receiverExpr + "->vtable->" + methodName
             + "(" + string.Join(", ", argList) + ")";
    }

    // ── Konstruktor-Ergänzung ─────────────────────────────────────────────

    /// <summary>
    /// Gibt die vtable-Zuweisung zurück die im Konstruktor erscheinen soll.
    /// Nur für Klassen die eine Basisklasse mit virtuellen Methoden haben.
    /// </summary>
    public static string VTableAssignment(string className)
        => "self->vtable = &" + className + "_vtable_instance;";

    // ── Utility ───────────────────────────────────────────────────────────

    public static bool HasVirtualMethods(ClassDeclarationSyntax node)
        => GetVirtualMethods(node).Count > 0 || GetVirtualProperties(node).Count > 0;

    public static bool HasInheritance(ClassDeclarationSyntax node)
        => node.BaseList?.Types.Any() ?? false;

    public static bool NeedsVTable(ClassDeclarationSyntax node)
        => HasVirtualMethods(node) || HasInheritance(node);

    private static List<MethodDeclarationSyntax> GetVirtualMethods(ClassDeclarationSyntax node)
        => node.Members.OfType<MethodDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(mod =>
                mod.IsKind(SyntaxKind.VirtualKeyword) ||
                mod.IsKind(SyntaxKind.AbstractKeyword)))
            .ToList();

    internal static List<PropertyDeclarationSyntax> GetVirtualProperties(ClassDeclarationSyntax node)
        => node.Members.OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Modifiers.Any(mod =>
                mod.IsKind(SyntaxKind.VirtualKeyword) ||
                mod.IsKind(SyntaxKind.AbstractKeyword)))
            .ToList();

    private static List<MethodDeclarationSyntax> GetOverrideMethods(ClassDeclarationSyntax node)
        => node.Members.OfType<MethodDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.OverrideKeyword)))
            .ToList();
}
