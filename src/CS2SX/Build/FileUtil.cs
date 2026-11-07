namespace CS2SX.Build;

internal static class FileUtil
{
    /// <summary>
    /// Writes <paramref name="content"/> only when it differs from what is on disk, so
    /// unchanged generated files keep their timestamp and dependent object files are
    /// not recompiled. Returns true when the file was (re)written.
    /// </summary>
    public static bool WriteIfChanged(string path, string content)
    {
        try
        {
            if (File.Exists(path) && File.ReadAllText(path) == content)
                return false;
        }
        catch { /* unreadable → just write */ }
        File.WriteAllText(path, content);
        return true;
    }
}
