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

