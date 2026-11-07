using Xunit;

namespace CS2SX.Tests.Harness;

/// devkitPro (devkitA64 + libnx) detection for tests that run GCC.
public static class Toolchain
{
    public static string? Root
    {
        get
        {
            var dkp = Environment.GetEnvironmentVariable("DEVKITPRO");
            if (string.IsNullOrEmpty(dkp) && OperatingSystem.IsWindows())
                dkp = @"C:\devkitPro";
            if (string.IsNullOrEmpty(dkp)) return null;
            var gcc = Path.Combine(dkp, "devkitA64", "bin",
                OperatingSystem.IsWindows() ? "aarch64-none-elf-gcc.exe" : "aarch64-none-elf-gcc");
            return File.Exists(gcc) ? dkp : null;
        }
    }

    public static bool Available => Root != null;
}

/// A theory that is skipped when devkitPro is not installed (e.g. on CI without it).
public sealed class ToolchainTheoryAttribute : TheoryAttribute
{
    public ToolchainTheoryAttribute()
    {
        if (!Toolchain.Available)
            Skip = "devkitPro (devkitA64) not installed — set DEVKITPRO to run GCC tests";
    }
}
