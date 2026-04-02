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
        var virtuals = GetVirtualMethods(node);
        if (virtuals.Count == 0) return;

        output.WriteLine("typedef struct " + className + "_vtable");
        output.WriteLine("{");
        foreach (var method in virtuals)
        {
            var retC    = TypeRegistry.MapType(method.ReturnType.ToString().Trim());
            var parms   = new List<string> { "void* self" };
            foreach (var p in method.ParameterList.Parameters)
            {
                var pt = TypeRegistry.MapType(p.Type?.ToString().Trim() ?? "int");
                parms.Add(pt + " " + p.Identifier.Text);
            }
            output.WriteLine("    " + retC + " (*" + method.Identifier.Text
                           + ")(" + string.Join(", ", parms) + ");");
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
        System.IO.TextWriter output)
    {
        var overrides  = GetOverrideMethods(node);
        if (overrides.Count == 0 && !HasInheritance(node)) return new();

        var instanceName = className + "_vtable_instance";

        output.WriteLine("static " + baseClassName + "_vtable " + instanceName + " =");
        output.WriteLine("{");

        var methodNames = new List<string>();
        foreach (var method in overrides)
        {
            var mName = method.Identifier.Text;
            methodNames.Add(mName);
            output.WriteLine("    ." + mName + " = " + className + "_" + mName + ",");
        }

        output.WriteLine("};");
        output.WriteLine();

        return methodNames;
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
        => GetVirtualMethods(node).Count > 0;

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

    private static List<MethodDeclarationSyntax> GetOverrideMethods(ClassDeclarationSyntax node)
        => node.Members.OfType<MethodDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.OverrideKeyword)))
            .ToList();
}
