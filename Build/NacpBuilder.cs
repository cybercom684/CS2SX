
// ============================================================================
// NacpBuilder
// ============================================================================

using CS2SX.Build;

public sealed class NacpBuilder
{
    private readonly string _devkitPath;

    public NacpBuilder()
    {
        _devkitPath = ProcessRunner.GetDevkitPro();
    }

    public void Build(string nacpPath, string appName, string author, string version)
    {
        var nacptool = Path.Combine(_devkitPath, "tools", "bin", "nacptool");
        var args = "--create \"" + appName + "\" \"" + author + "\" \"" + version + "\" \"" + nacpPath + "\"";
        ProcessRunner.Run(nacptool, args, "nacptool");
    }
}
