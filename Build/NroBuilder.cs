
// ============================================================================
// NroBuilder
// ============================================================================

using CS2SX.Build;

public sealed class NroBuilder
{
    private readonly string _devkitPath;

    public NroBuilder()
    {
        _devkitPath = ProcessRunner.GetDevkitPro();
    }

    public void Build(string elfPath, string nroPath, string nacpPath, string? iconPath = null)
    {
        var elf2nro = Path.Combine(_devkitPath, "tools", "bin", "elf2nro");

        var args = "\"" + elfPath + "\" \"" + nroPath + "\" --nacp=\"" + nacpPath + "\"";
        if (iconPath != null && File.Exists(iconPath))
            args += " --icon=\"" + iconPath + "\"";

        ProcessRunner.Run(elf2nro, args, "elf2nro");
    }
}