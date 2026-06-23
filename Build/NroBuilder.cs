
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

    public void Build(string elfPath, string nroPath, string nacpPath,
                      string? iconPath = null, string? romfsDir = null)
    {
        var elf2nro = Path.Combine(_devkitPath, "tools", "bin", "elf2nro");

        string? romfsBin = null;
        if (romfsDir != null && Directory.Exists(romfsDir))
        {
            // elf2nro needs a pre-built romfs binary — build it with build_romfs.
            // Qualify the temp name with the PID so concurrent/watch builds don't collide.
            romfsBin = Path.Combine(Path.GetTempPath(),
                $"cs2sx_romfs_{System.Diagnostics.Process.GetCurrentProcess().Id}.bin");
            var buildRomfs = Path.Combine(_devkitPath, "tools", "bin", "build_romfs");
            ProcessRunner.Run(buildRomfs,
                $"\"{romfsDir}\" \"{romfsBin}\"", "build_romfs");
        }

        var args = "\"" + elfPath + "\" \"" + nroPath + "\" --nacp=\"" + nacpPath + "\"";
        if (iconPath != null && File.Exists(iconPath))
            args += " --icon=\"" + iconPath + "\"";
        if (romfsBin != null && File.Exists(romfsBin))
            args += " --romfs=\"" + romfsBin + "\"";

        ProcessRunner.Run(elf2nro, args, "elf2nro");

        if (romfsBin != null && File.Exists(romfsBin))
            File.Delete(romfsBin);
    }
}