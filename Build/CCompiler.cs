namespace CS2SX.Build;

public sealed class CCompiler
{
    private readonly string _devkitPath;

    public CCompiler()
    {
        _devkitPath = ProcessRunner.GetDevkitPro();
    }

    public void Compile(IEnumerable<string> cFiles, string outputElf, string includeDir, string? projectDir = null)
    {
        var gcc = Path.Combine(_devkitPath, "devkitA64", "bin", "aarch64-none-elf-gcc");
        var libnxInc = Path.Combine(_devkitPath, "libnx", "include");
        var libnxLib = Path.Combine(_devkitPath, "libnx", "lib");
        var switchSpecs = Path.Combine(_devkitPath, "libnx", "switch.specs");

        gcc = ProcessRunner.ResolveTool(gcc);

        var fileArgs = string.Join(" ", cFiles.Select(f => "\"" + f + "\""));

        var args = fileArgs
                 + " -o \"" + outputElf + "\""
                 + " -I\"" + includeDir + "\""
                 + (projectDir != null ? " -I\"" + projectDir + "\"" : "")
                 + " -I\"" + libnxInc + "\""
                 + " -march=armv8-a+crc+crypto -mtune=cortex-a57 -mtp=soft -fPIE"
                 + " -ffunction-sections -fdata-sections"
                 + " -std=c11"
                 + " -O2 -Wall -Wextra -Wno-unused-parameter"
                 + " -specs=\"" + switchSpecs + "\""
                 + " -L\"" + libnxLib + "\" -lnx"
                 + " -Wl,--gc-sections";

        ProcessRunner.Run(gcc, args, "GCC");
    }
}