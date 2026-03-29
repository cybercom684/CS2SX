using System.Text;

namespace CS2SX.Build;

public static class EntryPointGenerator
{
    public static string Write(
        string buildDir,
        string appClass,
        string appHeaderFile,
        IReadOnlyList<string> allHeaders)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generiert von CS2SX -- nicht manuell bearbeiten");
        sb.AppendLine("#include <stdlib.h>");
        sb.AppendLine("#include <string.h>");
        sb.AppendLine("#include \"_forward.h\"");

        foreach (var h in allHeaders)
            sb.AppendLine($"#include \"{h}\"");

        sb.AppendLine();
        sb.AppendLine("int main(int argc, char* argv[])");
        sb.AppendLine("{");
        sb.AppendLine("    (void)argc;");
        sb.AppendLine("    (void)argv;");
        sb.AppendLine();
        sb.AppendLine("    // Heap-Allokation statt Stack — sicherer für große App-Structs");
        sb.AppendLine($"    {appClass}* app = ({appClass}*)calloc(1, sizeof({appClass}));");
        sb.AppendLine("    if (!app) return 1;");
        sb.AppendLine();
        sb.AppendLine($"    {appClass}_Init(app);");
        sb.AppendLine($"    SwitchApp_Run((SwitchApp*)app);");
        sb.AppendLine();
        sb.AppendLine("    free(app);");
        sb.AppendLine("    return 0;");
        sb.AppendLine("}");

        var path = Path.Combine(buildDir, "main.c");
        File.WriteAllText(path, sb.ToString(), System.Text.Encoding.UTF8);
        return path;
    }
}