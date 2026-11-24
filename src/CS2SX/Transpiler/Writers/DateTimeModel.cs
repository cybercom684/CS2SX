// ============================================================================
// CS2SX — Transpiler/Writers/DateTimeModel.cs
//
// DateTime and TimeSpan in C (Runtime/switchforms.h, src/rt_datetime.c):
//
//   DateTime  → CS2SX_DateTime (long long ticks since 0001-01-01, like .NET)
//   TimeSpan  → CS2SX_TimeSpan { long long ticks; }
//
//   new DateTime(2024, 2, 28)       CS2SX_DT_New(2024, 2, 28, 0, 0, 0, 0)
//   DateTime.Now / UtcNow / Today   CS2SX_DT_Now() / …
//   dt.Year, dt.DayOfWeek, …        CS2SX_DT_Part(dt, CS2SX_DT_YEAR)
//   dt.AddDays(2), dt.AddMonths(1)  CS2SX_DT_Add(dt, 2, CS2SX_TICKS_PER_DAY), …
//   dt.ToString("yyyy-MM-dd")       CS2SX_DT_ToString(dt, "yyyy-MM-dd")
//   a - b (DateTime)                CS2SX_TimeSpan_FromTicks(a - b)
//   dt + span, dt - span            CS2SX_DT_AddTicks(dt, ±span.ticks)
//   span1 < span2                   (span1).ticks < (span2).ticks
// ============================================================================

using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Writers;

public static class DateTimeModel
{
    public static bool IsDateTime(ITypeSymbol? t) =>
        t is INamedTypeSymbol { Name: "DateTime", ContainingNamespace.Name: "System" };
    public static bool IsTimeSpan(ITypeSymbol? t) =>
        t is INamedTypeSymbol { Name: "TimeSpan", ContainingNamespace.Name: "System" };

    private static string? Part(string name) => name switch
    {
        "Year" => "CS2SX_DT_YEAR", "Month" => "CS2SX_DT_MONTH", "Day" => "CS2SX_DT_DAY",
        "Hour" => "CS2SX_DT_HOUR", "Minute" => "CS2SX_DT_MINUTE", "Second" => "CS2SX_DT_SECOND",
        "Millisecond" => "CS2SX_DT_MS", "DayOfWeek" => "CS2SX_DT_DOW", "DayOfYear" => "CS2SX_DT_DOY",
        _ => null,
    };

    /// DateTime.Now, dt.Year, DayOfWeek.Friday, ts.Ticks, … — null when not one of these.
    public static string? Member(MemberAccessExpressionSyntax mem, SemanticModel? model, Func<SyntaxNode, string> write)
    {
        if (model == null) return null;
        try
        {
            var sym = model.GetSymbolInfo(mem).Symbol;
            // DayOfWeek.Friday: the enum's value.
            if (sym is IFieldSymbol { HasConstantValue: true, ContainingType: { Name: "DayOfWeek", ContainingNamespace.Name: "System" } } f)
                return Convert.ToInt32(f.ConstantValue).ToString();
            if (sym is not IPropertySymbol and not IFieldSymbol) return null;
            var owner = sym.ContainingType;
            var name = sym.Name;
            if (IsDateTime(owner))
            {
                if (sym.IsStatic)
                    return name switch
                    {
                        "Now" => "CS2SX_DT_Now()", "UtcNow" => "CS2SX_DT_UtcNow()", "Today" => "CS2SX_DT_Today()",
                        "MinValue" => "((CS2SX_DateTime)0)", "MaxValue" => "((CS2SX_DateTime)3155378975999999999LL)",
                        _ => null,
                    };
                var recv = write(mem.Expression);
                if (Part(name) is { } part) return $"CS2SX_DT_Part({recv}, {part})";
                return name switch
                {
                    "Ticks" => "((long long)(" + recv + "))",
                    "Date" => $"({{ CS2SX_DateTime _dd = ({recv}); _dd - _dd % CS2SX_TICKS_PER_DAY; }})",
                    "TimeOfDay" => $"CS2SX_TimeSpan_FromTicks(({recv}) % CS2SX_TICKS_PER_DAY)",
                    _ => null,
                };
            }
            if (IsTimeSpan(owner))
            {
                if (sym.IsStatic)
                    return name switch
                    {
                        "Zero" => "CS2SX_TimeSpan_FromTicks(0)",
                        "MaxValue" => "CS2SX_TimeSpan_FromTicks(9223372036854775807LL)",
                        "MinValue" => "CS2SX_TimeSpan_FromTicks((-9223372036854775807LL - 1))",
                        _ => null,
                    };
                if (name == "Ticks") return "(" + write(mem.Expression) + ").ticks";
            }
        }
        catch { }
        return null;
    }

    /// dt.AddDays(1), dt.ToString("…"), DateTime.IsLeapYear(y), ts.ToString(), … — null when not one of these.
    public static string? Invocation(InvocationExpressionSyntax inv, List<string> args, SemanticModel? model,
        Func<SyntaxNode, string> write)
    {
        if (model == null || inv.Expression is not MemberAccessExpressionSyntax mem) return null;
        try
        {
            if (model.GetSymbolInfo(inv).Symbol is not IMethodSymbol m) return null;
            var a0 = args.Count > 0 ? args[0] : "0";
            var a1 = args.Count > 1 ? args[1] : "0";
            if (IsDateTime(m.ContainingType))
            {
                if (m.IsStatic)
                    return m.Name switch
                    {
                        "IsLeapYear" => $"CS2SX_DT_IsLeapYear({a0})",
                        "DaysInMonth" => $"CS2SX_DT_DaysInMonth({a0}, {a1})",
                        "Compare" => $"({{ CS2SX_DateTime _ca = ({a0}), _cb = ({a1}); (_ca > _cb) - (_ca < _cb); }})",
                        _ => null,
                    };
                var recv = write(mem.Expression);
                return m.Name switch
                {
                    "AddDays" => $"CS2SX_DT_Add({recv}, {a0}, CS2SX_TICKS_PER_DAY)",
                    "AddHours" => $"CS2SX_DT_Add({recv}, {a0}, CS2SX_TICKS_PER_HOUR)",
                    "AddMinutes" => $"CS2SX_DT_Add({recv}, {a0}, CS2SX_TICKS_PER_MIN)",
                    "AddSeconds" => $"CS2SX_DT_Add({recv}, {a0}, CS2SX_TICKS_PER_SEC)",
                    "AddMilliseconds" => $"CS2SX_DT_Add({recv}, {a0}, CS2SX_TICKS_PER_MS)",
                    "AddTicks" => $"CS2SX_DT_AddTicks({recv}, {a0})",
                    "AddMonths" => $"CS2SX_DT_AddMonths({recv}, {a0})",
                    "AddYears" => $"CS2SX_DT_AddMonths({recv}, ({a0}) * 12)",
                    "Add" => $"CS2SX_DT_AddTicks({recv}, ({a0}).ticks)",
                    "Subtract" when IsDateTime(m.Parameters[0].Type) => $"CS2SX_TimeSpan_FromTicks(({recv}) - ({a0}))",
                    "Subtract" => $"CS2SX_DT_AddTicks({recv}, -({a0}).ticks)",
                    "ToString" when args.Count == 0 => $"CS2SX_DT_ToString({recv}, NULL)",
                    "ToString" when m.Parameters[0].Type.SpecialType == SpecialType.System_String => $"CS2SX_DT_ToString({recv}, {a0})",
                    "CompareTo" => $"({{ CS2SX_DateTime _ca = ({recv}), _cb = ({a0}); (_ca > _cb) - (_ca < _cb); }})",
                    "Equals" => $"(({recv}) == ({a0}))",
                    _ => null,
                };
            }
            if (IsTimeSpan(m.ContainingType))
            {
                if (m.IsStatic)
                    return m.Name switch
                    {
                        "FromDays" => $"CS2SX_TimeSpan_FromSec(({a0}) * 86400.0)",
                        _ => null,
                    };
                var recv = write(mem.Expression);
                return m.Name switch
                {
                    "ToString" when args.Count == 0 => $"CS2SX_TS_ToString({recv})",
                    "Add" => $"CS2SX_TimeSpan_Add({recv}, {a0})",
                    "Subtract" => $"CS2SX_TimeSpan_Sub({recv}, {a0})",
                    "Negate" => $"CS2SX_TimeSpan_FromTicks(-({recv}).ticks)",
                    "Duration" => $"({{ long long _tt = ({recv}).ticks; CS2SX_TimeSpan_FromTicks(_tt < 0 ? -_tt : _tt); }})",
                    "CompareTo" => $"({{ long long _ca = ({recv}).ticks, _cb = ({a0}).ticks; (_ca > _cb) - (_ca < _cb); }})",
                    "Equals" => $"(({recv}).ticks == ({a0}).ticks)",
                    _ => null,
                };
            }
        }
        catch { }
        return null;
    }

    /// new DateTime(y, m, d[, h, mi, s[, ms]]), new DateTime(ticks), new TimeSpan(…).
    public static string? Creation(ITypeSymbol? type, List<string> args)
    {
        if (IsDateTime(type))
        {
            if (args.Count == 0) return "((CS2SX_DateTime)0)";
            if (args.Count == 1) return $"((CS2SX_DateTime)({args[0]}))";
            if (args.Count is 3 or 6 or 7)
            {
                var all = args.Concat(Enumerable.Repeat("0", 7 - args.Count));
                return $"CS2SX_DT_New({string.Join(", ", all)})";
            }
            return null;
        }
        if (IsTimeSpan(type))
            return args.Count switch
            {
                0 => "CS2SX_TimeSpan_FromTicks(0)",
                1 => $"CS2SX_TimeSpan_FromTicks({args[0]})",
                3 => $"CS2SX_TimeSpan_New(0, {args[0]}, {args[1]}, {args[2]}, 0)",
                4 => $"CS2SX_TimeSpan_New({args[0]}, {args[1]}, {args[2]}, {args[3]}, 0)",
                5 => $"CS2SX_TimeSpan_New({args[0]}, {args[1]}, {args[2]}, {args[3]}, {args[4]})",
                _ => null,
            };
        return null;
    }

    /// a - b, dt + span, span < span — null for other operands.
    public static string? Binary(BinaryExpressionSyntax bin, string left, string right, SemanticModel? model)
    {
        if (model == null) return null;
        try
        {
            var lt = model.GetTypeInfo(bin.Left).Type;
            var rt = model.GetTypeInfo(bin.Right).Type;
            var op = bin.OperatorToken.Text;
            if (IsDateTime(lt) && IsDateTime(rt) && op == "-")
                return $"CS2SX_TimeSpan_FromTicks(({left}) - ({right}))";
            if (IsDateTime(lt) && IsTimeSpan(rt) && op is "+" or "-")
                return $"CS2SX_DT_AddTicks({left}, {(op == "-" ? "-" : "")}({right}).ticks)";
            if (IsTimeSpan(lt) && IsTimeSpan(rt) && op is "==" or "!=" or "<" or ">" or "<=" or ">=")
                return $"(({left}).ticks {op} ({right}).ticks)";
        }
        catch { }
        return null;
    }
}
