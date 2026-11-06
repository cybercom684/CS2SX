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
                        break;
                }
            }
        }
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
