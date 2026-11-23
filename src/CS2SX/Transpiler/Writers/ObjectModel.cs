using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Writers;

/// `object` values (void* in C) always point to something with the header of a
/// reference-counted class (CS2SX_ObjHdr): an object of a class of the project, or
/// a box (runtime: cs2sx_delegate.h). Converting a string, number, bool, char or
/// enum to object boxes it; the type info of the box (CS2SX_Box_Int32, E_BoxInfo)
/// or of the class (T_ClassInfo) makes `o is T`, casts and ToString exact.
internal static class ObjectModel
{
    public static bool IsObject(ITypeSymbol? t) => t?.SpecialType == SpecialType.System_Object;

    /// The type info a value of type t has as an object, or null when t cannot be
    /// stored in an object (structs, collections, delegates, records, …).
    public static string? TypeInfo(ITypeSymbol? t)
    {
        if (t == null) return null;
        if (t is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } n)
            t = n.TypeArguments[0];
        var box = t.SpecialType switch
        {
            SpecialType.System_String => "CS2SX_Box_String",
            SpecialType.System_Boolean => "CS2SX_Box_Boolean",
            SpecialType.System_Char => "CS2SX_Box_Char",
            SpecialType.System_SByte => "CS2SX_Box_SByte",
            SpecialType.System_Byte => "CS2SX_Box_Byte",
            SpecialType.System_Int16 => "CS2SX_Box_Int16",
            SpecialType.System_UInt16 => "CS2SX_Box_UInt16",
            SpecialType.System_Int32 => "CS2SX_Box_Int32",
            SpecialType.System_UInt32 => "CS2SX_Box_UInt32",
            SpecialType.System_Int64 => "CS2SX_Box_Int64",
            SpecialType.System_UInt64 => "CS2SX_Box_UInt64",
            SpecialType.System_Single => "CS2SX_Box_Single",
            SpecialType.System_Double => "CS2SX_Box_Double",
            _ => null,
        };
        if (box != null) return "&" + box;
        if (t.TypeKind == TypeKind.Enum && TranspilerContext.IsUserCode(t)) return "&" + t.Name + "_BoxInfo";
        if (t is INamedTypeSymbol { TypeKind: TypeKind.Class } cls && Arc.UsesDispose(cls))
            return "&" + cls.Name + "_ClassInfo";
        return null;
    }

    /// Why a value of type t cannot become an object, or null when it can.
    public static string? WhyNot(ITypeSymbol? t)
    {
        if (t == null || IsObject(t) || TypeInfo(t) != null) return null;
        if (Arc.IsInterface(t)) return null;   // its object is stored
        if (t is INamedTypeSymbol { IsRecord: true })
            return $"Ein Record ({t.Name}) kann nicht in object gespeichert werden — eine Klasse verwenden oder den Record-Typ direkt.";
        if (t.IsValueType)
            return $"Der Werttyp {t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} kann nicht in object gespeichert werden (nur Zahlen, bool, char, Enums) — den Typ direkt verwenden oder eine Klasse daraus machen.";
        return $"Ein Wert vom Typ {t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} kann nicht in object gespeichert werden — erlaubt sind Objekte eigener Klassen, Strings, Zahlen, bool, char und Enums.";
    }

    /// C code of value (of type from) as an object.
    public static string Box(ITypeSymbol from, string code)
    {
        if (from is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } n)
        {
            var inner = Box(n.TypeArguments[0], "_cs2sx_nb.value");
            return $"({{ __auto_type _cs2sx_nb = ({code}); _cs2sx_nb.has ? {inner} : NULL; }})";
        }
        if (Arc.IsInterface(from))
            return $"({{ CS2SX_IfaceHdr* _cs2sx_ih = (CS2SX_IfaceHdr*)({code}); _cs2sx_ih ? _cs2sx_ih->obj : NULL; }})";
        var info = TypeInfo(from);
        if (info == null) return code;
        return from.SpecialType switch
        {
            SpecialType.System_String => $"CS2SX_Box_Str({code})",
            SpecialType.System_Single or SpecialType.System_Double => $"CS2SX_Box_D({info}, (double)({code}))",
            SpecialType.System_UInt64 => $"CS2SX_Box_U({info}, (unsigned long long)({code}))",
            _ when from.IsValueType => $"CS2SX_Box_I({info}, (long long)({code}))",
            _ => $"((void*)({code}))",
        };
    }

    /// C code of the object value code as a target (a cast or an unboxing that checks
    /// the type and throws InvalidCastException), or null when target is not supported.
    public static string? Unbox(ITypeSymbol target, string code, string cType)
    {
        var info = TypeInfo(target);
        if (info == null) return null;
        return target.SpecialType switch
        {
            SpecialType.System_String => $"CS2SX_Unbox_Str({code})",
            SpecialType.System_Single => $"((float)CS2SX_Unbox_D({code}, {info}))",
            SpecialType.System_Double => $"CS2SX_Unbox_D({code}, {info})",
            SpecialType.System_UInt64 => $"CS2SX_Unbox_U({code}, {info})",
            _ when target.IsValueType => $"(({cType})CS2SX_Unbox_I({code}, {info}))",
            _ => $"(({cType}*)CS2SX_Obj_Cast({code}, {info}))",
        };
    }

    /// The value of an object known to hold a target (the variable of `o is int n`):
    /// read without another check.
    public static string? Read(ITypeSymbol target, string code, string cType)
    {
        var info = TypeInfo(target);
        if (info == null) return null;
        return target.SpecialType switch
        {
            SpecialType.System_String => $"((CS2SX_Box*)({code}))->v.s",
            SpecialType.System_Single => $"((float)((CS2SX_Box*)({code}))->v.d)",
            SpecialType.System_Double => $"((CS2SX_Box*)({code}))->v.d",
            SpecialType.System_UInt64 => $"((CS2SX_Box*)({code}))->v.u",
            _ when target.IsValueType => $"(({cType})((CS2SX_Box*)({code}))->v.i)",
            _ => $"(({cType}*)({code}))",
        };
    }

    /// `o is T` for an object o.
    public static string? Is(ITypeSymbol target, string code) =>
        TypeInfo(target) is { } info ? $"CS2SX_Obj_Is({code}, {info})" : null;

    /// True where a value converted to object is kept (stored, returned, put into a
    /// collection or passed to a method of the project) and therefore boxed. Elsewhere
    /// — "x" + 5, Console.WriteLine(5), string.Format — C# converts to object too, but
    /// CS2SX formats the value directly.
    public static bool BoxesHere(ExpressionSyntax expr, SemanticModel model)
    {
        SyntaxNode node = expr;
        while (node.Parent is ParenthesizedExpressionSyntax
               || node.Parent is ConditionalExpressionSyntax c && c.Condition != node
               || node.Parent is SwitchExpressionArmSyntax arm && arm.Expression == node
               || node.Parent is SwitchExpressionSyntax)
            node = node.Parent;
        switch (node.Parent)
        {
            case EqualsValueClauseSyntax or ReturnStatementSyntax or ArrowExpressionClauseSyntax
                or InitializerExpressionSyntax or YieldStatementSyntax:
                return true;
            // (s += 5 converts 5 to object too — string concatenation, nothing kept.)
            case AssignmentExpressionSyntax asg:
                return asg.Right == node && asg.IsKind(SyntaxKind.SimpleAssignmentExpression);
            case ArgumentSyntax { Parent.Parent: var call } when call != null:
                try
                {
                    if (model.GetSymbolInfo(call).Symbol is not IMethodSymbol m) return false;
                    if (TranspilerContext.IsUserCode(m.ContainingType)) return true;
                    var ns = m.ContainingType?.ContainingNamespace?.ToDisplayString();
                    return ns == "System.Collections.Generic";
                }
                catch { return false; }
            default:
                return false;
        }
    }
}
