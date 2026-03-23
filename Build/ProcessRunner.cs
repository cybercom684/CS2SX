using System.Diagnostics;

namespace CS2SX.Build;

internal static class ProcessRunner
{
    public static string ResolveTool(string path)
    {
        if (File.Exists(path)) return path;
        var withExe = path + ".exe";
        if (File.Exists(withExe)) return withExe;
        return path;
    }

    public static void Run(string exe, string args, string toolName)
    {
        exe = ResolveTool(exe);

        if (!File.Exists(exe))
            throw new FileNotFoundException(
                toolName + " nicht gefunden. Ist DEVKITPRO korrekt gesetzt?\nGesuchter Pfad: " + exe, exe);

        var psi = new ProcessStartInfo(exe, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        var proc = Process.Start(psi)
            ?? throw new InvalidOperationException("Konnte " + toolName + " nicht starten.");

        using (proc)
        {
            var stdout = Task.Run(() => proc.StandardOutput.ReadToEnd());
            var stderr = Task.Run(() => proc.StandardError.ReadToEnd());
            proc.WaitForExit();

            var outText = stdout.Result;
            var errText = stderr.Result;

            if (!string.IsNullOrWhiteSpace(outText)) Console.Write(outText);
            if (!string.IsNullOrWhiteSpace(errText)) Console.Error.Write(errText);

            if (proc.ExitCode != 0)
                throw new Exception(toolName + " fehlgeschlagen (Exit-Code " + proc.ExitCode + ").");
        }
    }

    public static string GetDevkitPro()
    {
        return Environment.GetEnvironmentVariable("DEVKITPRO")
            ?? throw new InvalidOperationException(
                "Umgebungsvariable DEVKITPRO ist nicht gesetzt.\n"
                + "Bitte DevkitPro installieren: https://devkitpro.org/wiki/Getting_Started");
    }
}