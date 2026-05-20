using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Handles BitConverter and basic Encoding/Convert.ToBase64 calls.
///
/// BitConverter.GetBytes(int)     → emits memcpy into a byte buffer
/// BitConverter.ToInt32(arr, 0)   → emits memcpy read
/// BitConverter.ToFloat(arr, 0)   → same for float
/// BitConverter.IsLittleEndian    → 1 (Switch is little-endian ARM)
/// BitConverter.ToString(arr)     → hex dump string
///
/// Encoding.UTF8.GetBytes(str)    → (uint8_t*)str  (strings are already UTF-8 on Switch)
/// Encoding.UTF8.GetString(arr)   → (const char*)arr
/// Encoding.ASCII.GetBytes/String → same
/// </summary>
public sealed class BitConverterHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        switch (calleeStr)
        {
            // ── BitConverter.GetBytes ──────────────────────────────────────────
            case "BitConverter.GetBytes":
            {
                if (args.Count == 0) { result = "NULL"; return true; }
                var val = args[0];
                var buf = ctx.NextTmp("bc_bytes");
                // Infer value type for sizeof
                var argNode = inv.ArgumentList.Arguments[0].Expression;
                var csType = Writers.TypeInferrer.InferCSharpType(argNode, ctx);
                var cType = TypeRegistry.MapType(csType);
                ctx.WriteLine($"uint8_t {buf}[sizeof({cType})];");
                ctx.WriteLine($"memcpy({buf}, &({val}), sizeof({cType}));");
                ctx.ArrayLengths[buf] = $"sizeof({cType})";
                result = buf;
                return true;
            }

            // ── BitConverter.ToInt32/ToInt16/ToFloat etc. ──────────────────────
            case "BitConverter.ToInt32":
            case "BitConverter.ToUInt32":
            case "BitConverter.ToInt16":
            case "BitConverter.ToUInt16":
            case "BitConverter.ToInt64":
            case "BitConverter.ToUInt64":
            case "BitConverter.ToSingle":
            case "BitConverter.ToDouble":
            case "BitConverter.ToChar":
            case "BitConverter.ToBoolean":
            {
                var targetCType = calleeStr switch
                {
                    "BitConverter.ToInt32"   => "int",
                    "BitConverter.ToUInt32"  => "unsigned int",
                    "BitConverter.ToInt16"   => "short",
                    "BitConverter.ToUInt16"  => "unsigned short",
                    "BitConverter.ToInt64"   => "long long",
                    "BitConverter.ToUInt64"  => "unsigned long long",
                    "BitConverter.ToSingle"  => "float",
                    "BitConverter.ToDouble"  => "double",
                    "BitConverter.ToChar"    => "char",
                    "BitConverter.ToBoolean" => "int",
                    _                        => "int"
                };
                if (args.Count < 1) { result = "0"; return true; }
                var arr = args[0];
                var offset = args.Count >= 2 ? args[1] : "0";
                var tmp = ctx.NextTmp("bc_val");
                ctx.WriteLine($"{targetCType} {tmp};");
                ctx.WriteLine($"memcpy(&{tmp}, {arr} + ({offset}), sizeof({targetCType}));");
                result = tmp;
                return true;
            }

            // ── BitConverter.IsLittleEndian ────────────────────────────────────
            case "BitConverter.IsLittleEndian":
                result = "1 /* Switch is little-endian */";
                return true;

            // ── BitConverter.ToString(bytes) → hex string ──────────────────────
            case "BitConverter.ToString":
            {
                if (args.Count == 0) { result = "\"\""; return true; }
                var arr = args[0];
                var arrRaw = inv.ArgumentList.Arguments[0].Expression.ToString();
                var lenExpr = ctx.ArrayLengths.TryGetValue(arrRaw, out var blen) ? blen : "1";
                var buf = ctx.NextStringBuf();
                var iVar = ctx.NextTmp("bci");
                var offVar = ctx.NextTmp("bco");
                ctx.WriteLine($"int {offVar} = 0;");
                ctx.WriteLine($"for (int {iVar} = 0; {iVar} < (int)({lenExpr}) && {offVar} < (int)sizeof({buf}) - 3; {iVar}++)");
                ctx.WriteLine($"    {offVar} += snprintf({buf} + {offVar}, sizeof({buf}) - {offVar}, \"{(args.Count > 1 ? "%02X-" : "%02X")}\", {arr}[{iVar}]);");
                // Remove trailing dash if format has dashes
                ctx.WriteLine($"if ({offVar} > 0 && {buf}[{offVar}-1] == '-') {buf}[{offVar}-1] = '\\0';");
                result = buf;
                return true;
            }

            // ── Encoding.UTF8.GetBytes(str) ────────────────────────────────────
            case "Encoding.UTF8.GetBytes":
            case "Encoding.ASCII.GetBytes":
            case "Encoding.Default.GetBytes":
                if (args.Count == 0) { result = "NULL"; return true; }
                result = "(uint8_t*)(" + args[0] + ")";
                return true;

            // ── Encoding.UTF8.GetString(arr, offset, count) ────────────────────
            case "Encoding.UTF8.GetString":
            case "Encoding.ASCII.GetString":
            case "Encoding.Default.GetString":
                if (args.Count == 0) { result = "\"\""; return true; }
                if (args.Count >= 3)
                    result = "(const char*)(" + args[0] + " + " + args[1] + ")";
                else
                    result = "(const char*)(" + args[0] + ")";
                return true;

            // ── Encoding.UTF8.GetByteCount(str) ────────────────────────────────
            case "Encoding.UTF8.GetByteCount":
            case "Encoding.ASCII.GetByteCount":
                if (args.Count == 0) { result = "0"; return true; }
                result = "(int)strlen(" + args[0] + ")";
                return true;
        }

        return NotHandled(out result);
    }
}
