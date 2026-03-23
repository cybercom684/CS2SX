/* class CCompiler */
void Compile(IEnumerable<string> cFiles, const char* outputElf, const char* includeDir)
{
    var gcc = Path.Combine(_devkitPath, "devkitA64", "bin", "aarch64-none-elf-gcc");
    var libnxInclude = Path.Combine(_devkitPath, "libnx", "include");
    var libnxLib = Path.Combine(_devkitPath, "libnx", "lib");
    var specs = Path.Combine(_devkitPath, "libnx", "switch.specs");
    var args = string.Join(" ", cFiles.Select(f => $"\"{f}\"")) + $" -o \"{outputElf}\"" + $" -I\"{includeDir}\"" + $" -I\"{libnxInclude}\"" + $" -L\"{libnxLib}\" -lnx" + $" -specs=\"{specs}\"";
    Run(gcc, args);
}

void Run(const char* exe, const char* args)
{
    var psi = new ProcessStartInfo(exe, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
    var proc = Process.Start(psi)!;
    var stdout = Task.Run(() => proc.StandardOutput.ReadToEnd());
    var stderr = Task.Run(() => proc.StandardError.ReadToEnd());
    proc.WaitForExit();
    Console.Write(stdout.Result);
    Console.Write(stderr.Result);
    if (proc.ExitCode != 0)
    {
        /* unsupported: ThrowStatementSyntax */
    }
}

/* class NroBuilder */
void Build(const char* elfPath, const char* nroPath)
{
    var elf2nro = Path.Combine(_devkitPath, "tools", "bin", "elf2nro");
    var psi = new ProcessStartInfo(elf2nro, $"\"{elfPath}\" \"{nroPath}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
    var proc = Process.Start(psi)!;
    Console.Write(proc.StandardOutput.ReadToEnd());
    Console.Write(proc.StandardError.ReadToEnd());
    proc.WaitForExit();
    if (proc.ExitCode != 0)
    {
        /* unsupported: ThrowStatementSyntax */
    }
}

/* class ProjectReader */
void Load(const char* csprojPath)
{
    csprojPath = Path.GetFullPath(csprojPath);
    ProjectDirectory = Path.GetDirectoryName(csprojPath)!;
    var xml = XDocument.Load(csprojPath);
    var includes = xml.Descendants("Compile").Select(e => e.Attribute("Include")?.Value).Where(v => v != null).Select(v => Path.GetFullPath(Path.Combine(ProjectDirectory, v!)));
    SourceFiles = includes.ToList();
    if (SourceFiles.Count == 0)
    {
        SourceFiles = Directory.GetFiles(ProjectDirectory, "*.cs", SearchOption.AllDirectories).Where(f => !f.Contains("obj") && !f.Contains("bin")).ToList();
    }
}

/* class CSharpToC */
const char* Transpile(const char* csharpSource)
{
    var tree = CSharpSyntaxTree.ParseText(csharpSource);
    Visit(tree.GetRoot());
    return _out.ToString();
}

void VisitClassDeclaration(ClassDeclarationSyntax node)
{
    _out.WriteLine($"/* class {node.Identifier} */");
    base.VisitClassDeclaration(node);
}

void VisitMethodDeclaration(MethodDeclarationSyntax node)
{
    var returnType = TypeMapper.Map(node.ReturnType.ToString().Trim());
    var name = node.Identifier.Text;
    var parameters = string.Join(", ", node.ParameterList.Parameters.Select(p => $"{TypeMapper.Map(p.Type!.ToString().Trim())} {p.Identifier}"));
    _out.WriteLine($"{returnType} {name}({parameters})");
    _out.WriteLine("{");
    _indent++;
    if (node.Body != null)
    {
        /* unsupported: ForEachStatementSyntax */
    }
    _indent--;
    _out.WriteLine("}");
    _out.WriteLine();
}

/* class MethodTranspiler */
void WriteStatement(StatementSyntax stmt)
{
    /* unsupported: SwitchStatementSyntax */
}

const char* WriteExpression(SyntaxNode expr)
{
    return expr switch
        {
            BinaryExpressionSyntax bin =>
                $"{WriteExpression(bin.Left)} {bin.OperatorToken} {WriteExpression(bin.Right)}",
            LiteralExpressionSyntax lit =>
                lit.Token.Text,
            IdentifierNameSyntax id =>
                id.Identifier.Text,
            PrefixUnaryExpressionSyntax pre =>
                $"{pre.OperatorToken}{WriteExpression(pre.Operand)}",
            PostfixUnaryExpressionSyntax post =>
                $"{WriteExpression(post.Operand)}{post.OperatorToken}",
            AssignmentExpressionSyntax assign =>
                $"{WriteExpression(assign.Left)} {assign.OperatorToken} {WriteExpression(assign.Right)}",
            MemberAccessExpressionSyntax mem =>
                $"{WriteExpression(mem.Expression)}.{mem.Name}",
            InvocationExpressionSyntax inv =>
                $"{WriteExpression(inv.Expression)}({string.Join(", ", inv.ArgumentList.Arguments.Select(a => WriteExpression(a.Expression)))})",
            ParenthesizedExpressionSyntax par =>
                $"({WriteExpression(par.Expression)})",
            _ => expr.ToString()
        };
}

/* class TypeMapper */
const char* Map(const char* csType)
{
}

