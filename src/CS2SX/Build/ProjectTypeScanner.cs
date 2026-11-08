using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Build;

/// <summary>
/// Registers project-wide type facts BEFORE any file is transpiled, so every file
/// sees the same picture regardless of transpile order:
///   • user enums and structs (incl. <c>record struct</c>) are value types,
///   • operator overloads exist for a type (so <c>a + b</c> becomes <c>T_op_add(a, b)</c>
///     even in files transpiled before the declaring file).
/// Used by build, check and dump alike.
/// </summary>
public static class ProjectTypeScanner
{
    public static void RegisterUserTypes(IEnumerable<string> sourceFiles)
    {
        var options = new CSharpParseOptions(LanguageVersion.Latest);
        var classes = new Dictionary<string, (string? baseName, bool excluded)>(StringComparer.Ordinal);
        var interfaces = new HashSet<string>(StringComparer.Ordinal);

        foreach (var file in sourceFiles)
        {
            SyntaxNode root;
            try { root = CSharpSyntaxTree.ParseText(File.ReadAllText(file), options).GetRoot(); }
            catch { continue; }

            foreach (var node in root.DescendantNodes())
            {
                switch (node)
                {
                    case EnumDeclarationSyntax en:
                        TypeRegistry.RegisterUserEnum(en.Identifier.Text);
                        break;

                    case StructDeclarationSyntax st:
                        TypeRegistry.RegisterUserStruct(st.Identifier.Text, HasInstanceCtor(st));
                        RegisterOperators(st);
                        break;

                    case RecordDeclarationSyntax rec
                        when rec.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword):
                        TypeRegistry.RegisterUserStruct(rec.Identifier.Text, HasInstanceCtor(rec));
                        RegisterOperators(rec);
                        break;

                    case ClassDeclarationSyntax cls:
                        RegisterOperators(cls);
                        classes[cls.Identifier.Text] = (cls.BaseList?.Types.FirstOrDefault()?.Type.ToString(),
                            cls.TypeParameterList != null || cls.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)));
                        break;

                    case RecordDeclarationSyntax recCls:
                        classes[recCls.Identifier.Text] = (recCls.BaseList?.Types.FirstOrDefault()?.Type.ToString(),
                            recCls.TypeParameterList != null);
                        break;

                    case InterfaceDeclarationSyntax iface:
                        interfaces.Add(iface.Identifier.Text);
                        break;
                }
            }
        }

        RegisterArcRoots(classes, interfaces);
    }

    /// Reference-counted class hierarchies (see Transpiler.Arc): a user class whose
    /// base chain consists of user classes only. Its root is the class without a base
    /// class (interfaces do not count). Mirrors Arc.Root, but from syntax, because
    /// _generics.h is written before the files are transpiled.
    private static void RegisterArcRoots(
        Dictionary<string, (string? baseName, bool excluded)> classes, HashSet<string> interfaces)
    {
        bool IsInterfaceName(string n) =>
            interfaces.Contains(n) || (n.Length > 1 && n[0] == 'I' && char.IsUpper(n[1]) && !classes.ContainsKey(n));

        var roots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var name in classes.Keys)
        {
            string cur = name;
            string? root = null;
            for (int hops = 0; hops < 64; hops++)
            {
                if (!classes.TryGetValue(cur, out var info) || info.excluded) break;
                var b = info.baseName?.Split('<')[0].Trim();
                if (b == null || b == "object" || IsInterfaceName(b)) { root = cur; break; }
                cur = b;   // a non-user base (SwitchApp, Exception, …) ends the loop above
            }
            if (root != null) roots[name] = root;
        }
        TypeRegistry.SetArcRoots(roots);
    }

    private static bool HasInstanceCtor(TypeDeclarationSyntax type) =>
        type.Members.OfType<ConstructorDeclarationSyntax>()
            .Any(c => !c.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)));

    private static void RegisterOperators(TypeDeclarationSyntax type)
    {
        foreach (var op in type.Members.OfType<OperatorDeclarationSyntax>())
            OperatorOverloadWriter.Register(type.Identifier.Text, op.OperatorToken.Text);
    }
}
