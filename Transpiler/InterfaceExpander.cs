// ============================================================================
// CS2SX — Transpiler/InterfaceExpander.cs
//
// Expandiert C# Interfaces zu C vtable-Structs.
//
// C# Interface:                    C Output:
// ──────────────────────────────────────────────────────────────────────
// interface IRenderable {          typedef struct IRenderable_vtable {
//     void Draw();                     void (*Draw)(void* self);
//     int GetWidth();              } IRenderable_vtable;
// }
//                                  typedef struct IRenderable {
//                                      IRenderable_vtable* vtable;
//                                  } IRenderable;
//
// class Button : IRenderable {     static IRenderable_vtable Button_IRenderable_vtable = {
//     public void Draw() { ... }       .Draw  = Button_Draw,
//     public int GetWidth() { ... }    .GetWidth = Button_GetWidth,
//                                  };
// }
//
// Aufruf:
//   IRenderable* r = ...           IRenderable* r = ...
//   r.Draw()                       r->vtable->Draw(r)
//
// Interface-Implementierung im Konstruktor:
//   self->vtable = &Button_IRenderable_vtable;
// ============================================================================

using CS2SX.Core;
using CS2SX.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler;

public sealed class InterfaceExpander
{
    private readonly GenericInstantiationCollector _collector;

    // Welche Klasse implementiert welche Interfaces
    // ClassName → Liste von InterfaceNames
    public Dictionary<string, List<string>> ClassInterfaces
    {
        get;
    } =
        new(StringComparer.Ordinal);

    public InterfaceExpander(GenericInstantiationCollector collector)
    {
        _collector = collector;
    }

    /// <summary>
    /// Analysiert alle Klassen und registriert welche Interfaces sie implementieren.
    /// </summary>
    public void AnalyzeImplementations(IReadOnlyList<string> sourceFiles)
    {
        foreach (var file in sourceFiles)
        {
            try
            {
                var source = File.ReadAllText(file);
                var tree = CSharpSyntaxTree.ParseText(source,
                    new CSharpParseOptions(LanguageVersion.CSharp12));
                AnalyzeFile(tree);
            }
            catch { }
        }
    }

    private void AnalyzeFile(Microsoft.CodeAnalysis.SyntaxTree tree)
    {
        foreach (var cls in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            if (cls.BaseList == null) continue;

            var implementedInterfaces = new List<string>();
            foreach (var baseType in cls.BaseList.Types)
            {
                var typeName = baseType.Type.ToString().Trim();
                if (_collector.Interfaces.ContainsKey(typeName))
                {
                    implementedInterfaces.Add(typeName);
                    Log.Debug($"InterfaceExpander: '{cls.Identifier.Text}' implementiert '{typeName}'");
                }
            }

            if (implementedInterfaces.Count > 0)
                ClassInterfaces[cls.Identifier.Text] = implementedInterfaces;
        }
    }

    // ── Header-Generierung ────────────────────────────────────────────────────

    /// <summary>
    /// Erzeugt vtable-Structs und Interface-Typen für alle bekannten Interfaces.
    /// Gibt den Header-Code zurück.
    /// </summary>
    public string ExpandInterfaceHeaders()
    {
        if (_collector.Interfaces.Count == 0) return "";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("// ── Interface vtable definitions (auto-generated) ────────────────────");
        sb.AppendLine();

        foreach (var (ifaceName, iface) in _collector.Interfaces)
        {
            sb.AppendLine(BuildInterfaceVTableStruct(ifaceName, iface));
        }

        return sb.ToString();
    }

    private string BuildInterfaceVTableStruct(string ifaceName,
        InterfaceDeclarationSyntax iface)
    {
        var sb = new System.Text.StringBuilder();

        // vtable struct
        sb.AppendLine($"typedef struct {ifaceName}_vtable");
        sb.AppendLine("{");

        foreach (var method in iface.Members.OfType<MethodDeclarationSyntax>())
        {
            var retType = TypeRegistry.MapType(method.ReturnType.ToString().Trim());
            var parms = new List<string> { "void* self" };

            foreach (var p in method.ParameterList.Parameters)
            {
                var pt = TypeRegistry.MapType(p.Type?.ToString().Trim() ?? "int");
                var isRef = p.Modifiers.Any(m =>
                    m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword));
                parms.Add(pt + (isRef ? "*" : "") + " " + p.Identifier.Text);
            }

            sb.AppendLine($"    {retType} (*{method.Identifier.Text})({string.Join(", ", parms)});");
        }

        // Properties als Getter/Setter Funktionszeiger
        foreach (var prop in iface.Members.OfType<PropertyDeclarationSyntax>())
        {
            var retType = TypeRegistry.MapType(prop.Type.ToString().Trim());
            var hasGet = prop.AccessorList?.Accessors
                .Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) ?? false;
            var hasSet = prop.AccessorList?.Accessors
                .Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) ?? false;

            if (hasGet)
                sb.AppendLine($"    {retType} (*get_{prop.Identifier})(void* self);");
            if (hasSet)
                sb.AppendLine($"    void (*set_{prop.Identifier})(void* self, {retType} value);");
        }

        sb.AppendLine($"}} {ifaceName}_vtable;");
        sb.AppendLine();

        // Interface-Wrapper-Struct
        sb.AppendLine($"typedef struct {ifaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    {ifaceName}_vtable* vtable;");
        sb.AppendLine($"    void* obj;");
        sb.AppendLine($"}} {ifaceName};");
        sb.AppendLine();

        // Interface-Aufruf-Makros (komfortabler Aufruf ohne ->vtable->)
        foreach (var method in iface.Members.OfType<MethodDeclarationSyntax>())
        {
            var mName = method.Identifier.Text;
            var paramNames = method.ParameterList.Parameters
                .Select(p => p.Identifier.Text)
                .ToList();

            var paramDecls = new List<string> { $"{ifaceName}* _iface" };
            paramDecls.AddRange(method.ParameterList.Parameters.Select(p =>
            {
                var pt = TypeRegistry.MapType(p.Type?.ToString().Trim() ?? "int");
                return $"{pt} {p.Identifier.Text}";
            }));

            var callArgs = new List<string> { "_iface->obj" };
            callArgs.AddRange(paramNames);

            var retType = TypeRegistry.MapType(method.ReturnType.ToString().Trim());
            var retKw = retType == "void" ? "" : "return ";

            sb.AppendLine($"static inline {retType} {ifaceName}_{mName}({string.Join(", ", paramDecls)})");
            sb.AppendLine("{");
            sb.AppendLine($"    {retKw}_iface->vtable->{mName}({string.Join(", ", callArgs)});");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    // ── vtable-Instanz für implementierende Klassen ────────────────────────────

    /// <summary>
    /// Generiert für eine Klasse die vtable-Instanzen für alle implementierten Interfaces.
    /// Gibt den C-Code zurück der nach den Methoden der Klasse eingefügt wird.
    /// </summary>
    public string ExpandClassVTableInstances(
        string className,
        ClassDeclarationSyntax cls)
    {
        if (!ClassInterfaces.TryGetValue(className, out var interfaces))
            return "";

        var sb = new System.Text.StringBuilder();

        foreach (var ifaceName in interfaces)
        {
            if (!_collector.Interfaces.TryGetValue(ifaceName, out var iface)) continue;

            var instanceName = $"{className}_{ifaceName}_vtable_instance";
            sb.AppendLine($"static {ifaceName}_vtable {instanceName} =");
            sb.AppendLine("{");

            foreach (var method in iface.Members.OfType<MethodDeclarationSyntax>())
            {
                var mName = method.Identifier.Text;
                // Prüfen ob die Klasse diese Methode implementiert
                bool implemented = cls.Members.OfType<MethodDeclarationSyntax>()
                    .Any(m => m.Identifier.Text == mName);

                if (implemented)
                    sb.AppendLine($"    .{mName} = {className}_{mName},");
                else
                    sb.AppendLine($"    .{mName} = NULL, /* not implemented */");
            }

            foreach (var prop in iface.Members.OfType<PropertyDeclarationSyntax>())
            {
                var pName = prop.Identifier.Text;
                var hasGet = prop.AccessorList?.Accessors
                    .Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) ?? false;
                var hasSet = prop.AccessorList?.Accessors
                    .Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) ?? false;

                if (hasGet) sb.AppendLine($"    .get_{pName} = {className}_get_{pName},");
                if (hasSet) sb.AppendLine($"    .set_{pName} = {className}_set_{pName},");
            }

            sb.AppendLine("};");
            sb.AppendLine();

            // Helper-Funktion: Klasse als Interface verpacken
            sb.AppendLine($"static inline {ifaceName} {className}_as_{ifaceName}({className}* self)");
            sb.AppendLine("{");
            sb.AppendLine($"    {ifaceName} _iface;");
            sb.AppendLine($"    _iface.vtable = &{instanceName};");
            sb.AppendLine($"    _iface.obj = self;");
            sb.AppendLine($"    return _iface;");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Forward-Declarations der Interface-vtable-Instanzen für den Header.
    /// </summary>
    public string ExpandClassVTableDeclarations(string className)
    {
        if (!ClassInterfaces.TryGetValue(className, out var interfaces))
            return "";

        var sb = new System.Text.StringBuilder();
        foreach (var ifaceName in interfaces)
        {
            if (!_collector.Interfaces.ContainsKey(ifaceName)) continue;
            var instanceName = $"{className}_{ifaceName}_vtable_instance";
            sb.AppendLine($"extern {ifaceName}_vtable {instanceName};");
            sb.AppendLine($"{ifaceName} {className}_as_{ifaceName}({className}* self);");
        }
        return sb.ToString();
    }

    /// <summary>
    /// Schreibt die Interface-Header in eine Datei.
    /// </summary>
    public string WriteInterfaceHeader(string buildDir)
    {
        if (_collector.Interfaces.Count == 0) return string.Empty;

        var content = "#pragma once\n#include \"_forward.h\"\n\n"
                    + ExpandInterfaceHeaders();
        var path = Path.Combine(buildDir, "_interfaces.h");
        File.WriteAllText(path, content);
        Log.Info($"InterfaceExpander: {_collector.Interfaces.Count} Interface(s) → _interfaces.h");
        return path;
    }
}