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

        // Classes that get an interface from a base class (class Square : Shape, where
        // Shape : IShape): Square_as_IShape uses Shape's vtable instance, whose entries
        // dispatch virtually.
        foreach (var cls in _classBase.Keys)
        {
            var seen = new HashSet<string>();
            for (var b = _classBase.GetValueOrDefault(cls); b != null && seen.Add(b); b = _classBase.GetValueOrDefault(b))
            {
                if (!ClassInterfaces.TryGetValue(b, out var baseIfaces)) continue;
                foreach (var iface in baseIfaces)
                {
                    if (ClassInterfaces.TryGetValue(cls, out var own) && own.Contains(iface)) continue;
                    if (!InheritedInterfaces.TryGetValue(cls, out var list)) InheritedInterfaces[cls] = list = new();
                    if (list.All(x => x.iface != iface)) list.Add((iface, b));
                }
            }
        }
    }

    private readonly Dictionary<string, string> _classBase = new(StringComparer.Ordinal);

    /// Interfaces a class has through a base class: (interface, the base that implements it).
    public Dictionary<string, List<(string iface, string implementer)>> InheritedInterfaces { get; } = new(StringComparer.Ordinal);

    private void AnalyzeFile(Microsoft.CodeAnalysis.SyntaxTree tree)
    {
        foreach (var cls in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            if (cls.BaseList == null) continue;
            var firstBase = cls.BaseList.Types[0].Type.ToString().Trim();
            if (!_collector.Interfaces.ContainsKey(firstBase))
                _classBase[cls.Identifier.Text] = firstBase;

            var implementedInterfaces = new List<string>();
            foreach (var baseType in cls.BaseList.Types)
            {
                var typeName = baseType.Type.ToString().Trim();
                if (_collector.Interfaces.ContainsKey(typeName))
                {
                    implementedInterfaces.Add(typeName);
                    // Also collect all transitively inherited interfaces
                    foreach (var transitiveIface in CollectTransitiveInterfaces(typeName))
                        if (!implementedInterfaces.Contains(transitiveIface))
                            implementedInterfaces.Add(transitiveIface);
                    Log.Debug($"InterfaceExpander: '{cls.Identifier.Text}' implementiert '{typeName}'");
                }
            }

            if (implementedInterfaces.Count > 0)
                ClassInterfaces[cls.Identifier.Text] = implementedInterfaces;
        }
    }

    private IEnumerable<string> CollectTransitiveInterfaces(string ifaceName)
    {
        if (!_collector.Interfaces.TryGetValue(ifaceName, out var iface)) yield break;
        if (iface.BaseList == null) yield break;
        foreach (var baseType in iface.BaseList.Types)
        {
            var parentName = baseType.Type.ToString().Trim();
            if (_collector.Interfaces.ContainsKey(parentName))
            {
                yield return parentName;
                foreach (var grandparent in CollectTransitiveInterfaces(parentName))
                    yield return grandparent;
            }
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
            var retCsType = method.ReturnType.ToString().Trim();
            var retType = TypeRegistry.MapType(retCsType);
            if (!retType.EndsWith("*") && TypeRegistry.NeedsPointerSuffix(retCsType))
                retType += "*";

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
        // Same layout as CS2SX_IfaceHdr: the value is reference-counted and owns obj.
        sb.AppendLine($"    int _rc;");
        sb.AppendLine($"    CS2SX_FreeFn _release;");
        sb.AppendLine($"    const struct CS2SX_GcKind* _kind;");
        sb.AppendLine($"    const CS2SX_ClassInfo* _cls;");
        sb.AppendLine($"}} {ifaceName};");
        sb.AppendLine($"static inline {ifaceName}* {ifaceName}_Retain({ifaceName}* i) {{ return ({ifaceName}*)CS2SX_Iface_Retain(i); }}");
        sb.AppendLine($"static inline void {ifaceName}_Free({ifaceName}* i) {{ CS2SX_Iface_Free(i); }}");
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

            var retTypeCsH = method.ReturnType.ToString().Trim();
            var retType = TypeRegistry.MapType(retTypeCsH);
            if (!retType.EndsWith("*") && TypeRegistry.NeedsPointerSuffix(retTypeCsH))
                retType += "*";
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

            // Pass 1: Stub-Funktionen für nicht-implementierte Methoden vorab generieren.
            // NULL-Vtable-Slots würden beim Aufruf zu einem Segfault führen.
            foreach (var method in iface.Members.OfType<MethodDeclarationSyntax>())
            {
                var mName = method.Identifier.Text;
                bool implemented = cls.Members.OfType<MethodDeclarationSyntax>()
                    .Any(m => m.Identifier.Text == mName
                           || (m.ExplicitInterfaceSpecifier != null
                               && m.Identifier.Text == mName
                               && m.ExplicitInterfaceSpecifier.Name.ToString().TrimEnd() == ifaceName));

                if (!implemented)
                {
                    var retType = TypeRegistry.MapType(method.ReturnType.ToString().Trim());
                    var stubName = $"_cs2sx_{className}_{ifaceName}_{mName}_not_impl";
                    var parms = new List<string> { "void* self" };
                    foreach (var p in method.ParameterList.Parameters)
                    {
                        var pt = TypeRegistry.MapType(p.Type?.ToString().Trim() ?? "int");
                        var isRef = p.Modifiers.Any(m =>
                            m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword));
                        parms.Add(pt + (isRef ? "*" : "") + " " + p.Identifier.Text);
                    }
                    sb.AppendLine($"static {retType} {stubName}({string.Join(", ", parms)})");
                    sb.AppendLine("{");
                    sb.AppendLine($"    /* Interface method '{mName}' not implemented by '{className}' */");
                    if (retType != "void")
                        sb.AppendLine($"    return ({retType})0;");
                    sb.AppendLine("}");
                    sb.AppendLine();
                    Log.Warning($"InterfaceExpander: '{className}' implementiert '{ifaceName}', aber Methode '{mName}' fehlt — Stub generiert");
                }
            }

            // Pass 1b: Stub-Funktionen für nicht-implementierte Properties
            foreach (var prop in iface.Members.OfType<PropertyDeclarationSyntax>())
            {
                var pName = prop.Identifier.Text;
                var propRetType = TypeRegistry.MapType(prop.Type.ToString().Trim());
                var hasGet = prop.AccessorList?.Accessors
                    .Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) ?? false;
                var hasSet = prop.AccessorList?.Accessors
                    .Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) ?? false;
                bool propImpl = cls.Members.OfType<PropertyDeclarationSyntax>()
                    .Any(p => p.Identifier.Text == pName);

                if (hasGet && !propImpl)
                {
                    var getStub = $"_cs2sx_{className}_{ifaceName}_get_{pName}_not_impl";
                    sb.AppendLine($"static {propRetType} {getStub}(void* self)");
                    sb.AppendLine("{");
                    sb.AppendLine($"    /* Interface property getter '{pName}' not implemented by '{className}' */");
                    sb.AppendLine($"    return ({propRetType})0;");
                    sb.AppendLine("}");
                    sb.AppendLine();
                    Log.Warning($"InterfaceExpander: '{className}' implementiert '{ifaceName}', aber Property-Getter '{pName}' fehlt — Stub generiert");
                }
                if (hasSet && !propImpl)
                {
                    var setStub = $"_cs2sx_{className}_{ifaceName}_set_{pName}_not_impl";
                    sb.AppendLine($"static void {setStub}(void* self, {propRetType} value)");
                    sb.AppendLine("{");
                    sb.AppendLine($"    /* Interface property setter '{pName}' not implemented by '{className}' */");
                    sb.AppendLine("}");
                    sb.AppendLine();
                    Log.Warning($"InterfaceExpander: '{className}' implementiert '{ifaceName}', aber Property-Setter '{pName}' fehlt — Stub generiert");
                }
            }

            // Pass 2: vtable-Initialisierer — NOT static (header declares it extern, needs external linkage)
            sb.AppendLine($"{ifaceName}_vtable {instanceName} =");
            sb.AppendLine("{");

            foreach (var method in iface.Members.OfType<MethodDeclarationSyntax>())
            {
                var mName = method.Identifier.Text;
                // Match both regular and explicit interface implementations (IFoo.Bar)
                bool implemented = cls.Members.OfType<MethodDeclarationSyntax>()
                    .Any(m => m.Identifier.Text == mName
                           || (m.ExplicitInterfaceSpecifier != null
                               && m.Identifier.Text == mName
                               && m.ExplicitInterfaceSpecifier.Name.ToString().TrimEnd() == ifaceName));

                var implMethod = cls.Members.OfType<MethodDeclarationSyntax>().FirstOrDefault(m => m.Identifier.Text == mName);
                bool isVirtual = implMethod != null && implMethod.ExplicitInterfaceSpecifier == null
                    && implMethod.Modifiers.Any(x => x.IsKind(SyntaxKind.VirtualKeyword) || x.IsKind(SyntaxKind.AbstractKeyword) || x.IsKind(SyntaxKind.OverrideKeyword));
                if (implemented && isVirtual)
                {
                    // Virtual / abstract: the entry calls through the object's class vtable,
                    // so an interface value of a derived class runs the override.
                    var ifm = iface.Members.OfType<MethodDeclarationSyntax>().First(m => m.Identifier.Text == mName);
                    var retCs = ifm.ReturnType.ToString().Trim();
                    var ret = TypeRegistry.MapType(retCs);
                    if (!ret.EndsWith("*") && TypeRegistry.NeedsPointerSuffix(retCs)) ret += "*";
                    var ps = ifm.ParameterList.Parameters.Select(p =>
                    {
                        var pt = TypeRegistry.MapType(p.Type?.ToString().Trim() ?? "int");
                        if (!pt.EndsWith("*") && TypeRegistry.NeedsPointerSuffix(p.Type?.ToString().Trim() ?? "int")) pt += "*";
                        return pt + " " + p.Identifier.Text;
                    }).ToList();
                    var root = TypeRegistry.ArcRoot(className) ?? className;
                    var thunk = $"_cs2sx_{className}_{ifaceName}_{mName}_virt";
                    var callArgs = string.Concat(ifm.ParameterList.Parameters.Select(p => ", " + p.Identifier.Text));
                    sb.Insert(0, $"static {ret} {thunk}({string.Join(", ", new[] { "void* self" }.Concat(ps))}) " +
                        $"{{ {(ret == "void" ? "" : "return ")}(({root}*)self)->vtable->{mName}(({root}*)self{callArgs}); }}" + Environment.NewLine);
                    sb.AppendLine($"    .{mName} = {thunk},");
                }
                else if (implemented)
                {
                    // Cast to the vtable's void*-based function pointer type to avoid
                    // "incompatible pointer type" errors (impl uses ClassName*, vtable uses void*).
                    var retCsM = iface.Members.OfType<MethodDeclarationSyntax>()
                        .First(m => m.Identifier.Text == mName).ReturnType.ToString().Trim();
                    var retTypeM = TypeRegistry.MapType(retCsM);
                    if (!retTypeM.EndsWith("*") && TypeRegistry.NeedsPointerSuffix(retCsM))
                        retTypeM += "*";
                    var paramsList = new List<string> { "void*" };
                    paramsList.AddRange(iface.Members.OfType<MethodDeclarationSyntax>()
                        .First(m => m.Identifier.Text == mName).ParameterList.Parameters
                        .Select(p => TypeRegistry.MapType(p.Type?.ToString().Trim() ?? "int")));
                    var castType = $"{retTypeM} (*)({string.Join(", ", paramsList)})";
                    sb.AppendLine($"    .{mName} = ({castType}){className}_{mName},");
                }
                else
                {
                    var stubName = $"_cs2sx_{className}_{ifaceName}_{mName}_not_impl";
                    sb.AppendLine($"    .{mName} = {stubName},");
                }
            }

            foreach (var prop in iface.Members.OfType<PropertyDeclarationSyntax>())
            {
                var pName = prop.Identifier.Text;
                var hasGet = prop.AccessorList?.Accessors
                    .Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) ?? false;
                var hasSet = prop.AccessorList?.Accessors
                    .Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) ?? false;
                bool propImpl = cls.Members.OfType<PropertyDeclarationSyntax>()
                    .Any(p => p.Identifier.Text == pName);

                if (hasGet)
                    sb.AppendLine($"    .get_{pName} = {(propImpl ? $"{className}_get_{pName}" : $"_cs2sx_{className}_{ifaceName}_get_{pName}_not_impl")},");
                if (hasSet)
                    sb.AppendLine($"    .set_{pName} = {(propImpl ? $"{className}_set_{pName}" : $"_cs2sx_{className}_{ifaceName}_set_{pName}_not_impl")},");
            }

            sb.AppendLine("};");
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
            interfaces = new List<string>();
        if (interfaces.Count == 0 && !InheritedInterfaces.ContainsKey(className))
            return "";

        var sb = new System.Text.StringBuilder();
        var pairs = interfaces.Select(i => (iface: i, implementer: className)).ToList();
        if (InheritedInterfaces.TryGetValue(className, out var inherited)) pairs.AddRange(inherited);
        foreach (var (ifaceName, implementer) in pairs)
        {
            if (!_collector.Interfaces.ContainsKey(ifaceName)) continue;
            var instanceName = $"{implementer}_{ifaceName}_vtable_instance";
            // vtable instance forward declaration
            sb.AppendLine($"extern {ifaceName}_vtable {instanceName};");
            // _as_IFace helper: heap-allocate a new IFace struct each call so multiple pushes
            // onto the Navigator stack don't share the same struct (which would cause wrong obj).
            // The value belongs to the current frame and keeps the object alive
            // (reference-counted classes): CS2SX_Iface_Wrap.
            var root = TypeRegistry.ArcRoot(className);
            if (root != null)
            {
                sb.AppendLine($"{root}* {root}_Retain({root}* self);");
                sb.AppendLine($"void {root}_Free({root}* self);");
                sb.AppendLine($"extern const CS2SX_GcKind {root}_GcKind;");
                sb.AppendLine($"extern const CS2SX_ClassInfo {className}_ClassInfo;");
            }
            sb.AppendLine($"static inline {ifaceName}* {className}_as_{ifaceName}({className}* self)");
            sb.AppendLine("{");
            sb.AppendLine(root != null
                ? $"    return ({ifaceName}*)CS2SX_Iface_Wrap(sizeof({ifaceName}), &{instanceName}, self, (void* (*)(void*)){root}_Retain, (CS2SX_FreeFn){root}_Free, &{root}_GcKind, &{className}_ClassInfo);"
                : $"    return ({ifaceName}*)CS2SX_Iface_Wrap(sizeof({ifaceName}), &{instanceName}, self, NULL, NULL, NULL, NULL);");
            sb.AppendLine("}");
        }
        return sb.ToString();
    }

    /// <summary>
    /// Schreibt die Interface-Header in eine Datei.
    /// </summary>
    public string WriteInterfaceHeader(string buildDir)
    {
        if (_collector.Interfaces.Count == 0) return string.Empty;

        var interfaceContent = ExpandInterfaceHeaders();
        // After all interface structs are defined, generate Stack/List variants for each interface
        // so Stack<IScreen> etc. are available without additional macro calls in user code.
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("#pragma once");
        sb.AppendLine("#include \"_forward.h\"");
        sb.AppendLine();
        sb.Append(interfaceContent);
        sb.AppendLine();
        // Stack variants for each interface type (pointer-based).
        // The type is named Stack_IFace_ptr but functions use the short Stack_IFace_* names
        // so generated code like Stack_IScreen_Push() matches without requiring name mapping.
        sb.AppendLine("// Stack variants for each interface type (pointer-based)");
        foreach (var ifaceName in _collector.Interfaces.Keys)
        {
            sb.AppendLine($"#ifndef CS2SX_STACK_{ifaceName}_DEFINED");
            sb.AppendLine($"#define CS2SX_STACK_{ifaceName}_DEFINED");
            sb.AppendLine($"typedef struct {{ {ifaceName}** data; int count; int capacity; int _rc; }} Stack_{ifaceName}_ptr;");
            sb.AppendLine($"static inline void Stack_{ifaceName}_Free(Stack_{ifaceName}_ptr* s);");
            sb.AppendLine($"static inline Stack_{ifaceName}_ptr* Stack_{ifaceName}_Retain(Stack_{ifaceName}_ptr* s) {{ if (s) s->_rc++; return s; }}");
            sb.AppendLine($"static inline Stack_{ifaceName}_ptr* Stack_{ifaceName}_New(void) {{");
            sb.AppendLine($"    Stack_{ifaceName}_ptr* s = (Stack_{ifaceName}_ptr*)malloc(sizeof(Stack_{ifaceName}_ptr));");
            sb.AppendLine($"    if (!s) return NULL;");
            sb.AppendLine($"    s->data = ({ifaceName}**)malloc(8 * sizeof({ifaceName}*)); s->count = 0; s->capacity = 8; s->_rc = 1; CS2SX_ReleaseLater(s, (CS2SX_FreeFn)Stack_{ifaceName}_Free); return s; }}");
            sb.AppendLine($"static inline void Stack_{ifaceName}_Push(Stack_{ifaceName}_ptr* s, {ifaceName}* val) {{");
            sb.AppendLine($"    if (!s) return;");
            sb.AppendLine($"    if (s->count >= s->capacity) {{ s->capacity *= 2; s->data = ({ifaceName}**)realloc(s->data, s->capacity * sizeof({ifaceName}*)); }}");
            sb.AppendLine($"    s->data[s->count++] = ({ifaceName}*)CS2SX_Iface_Retain(val); }}");
            sb.AppendLine($"static inline {ifaceName}* Stack_{ifaceName}_Pop(Stack_{ifaceName}_ptr* s) {{");
            sb.AppendLine($"    if (!s || s->count == 0) return NULL;");
            sb.AppendLine($"    {ifaceName}* _v = s->data[--s->count]; CS2SX_Iface_Drop(_v); return _v; }}");
            sb.AppendLine($"static inline {ifaceName}* Stack_{ifaceName}_Peek(Stack_{ifaceName}_ptr* s) {{");
            sb.AppendLine($"    if (!s || s->count == 0) return NULL;");
            sb.AppendLine($"    return s->data[s->count - 1]; }}");
            sb.AppendLine($"static inline void Stack_{ifaceName}_Free(Stack_{ifaceName}_ptr* s) {{ if (!s || --s->_rc > 0) return; for (int _i = 0; _i < s->count; _i++) CS2SX_Iface_Drop(s->data[_i]); free(s->data); free(s); }}");
            sb.AppendLine($"#endif");
        }
        var content = sb.ToString();
        var path = Path.Combine(buildDir, "_interfaces.h");
        File.WriteAllText(path, content);
        Log.Info($"InterfaceExpander: {_collector.Interfaces.Count} Interface(s) → _interfaces.h");
        return path;
    }
}