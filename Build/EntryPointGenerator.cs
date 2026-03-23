namespace CS2SX.Build;

public static class EntryPointGenerator
{
    public static string Generate(string appClassName, string includeFileName, IEnumerable<string>? allHeaders = null)
    {
        var includes = allHeaders != null
            ? string.Join("\n", allHeaders.Select(h => $"#include \"{h}\""))
            : $"#include \"{includeFileName}\"";

        return "// Auto-generiert von CS2SX -- nicht manuell bearbeiten\n"
             + "#include <stdlib.h>\n"
             + "#include <string.h>\n"
             + "#include \"_forward.h\"\n"
             + includes + "\n"
             + "\n"
             + "int main(int argc, char* argv[])\n"
             + "{\n"
             + "    (void)argc;\n"
             + "    (void)argv;\n"
             + "\n"
             + "    " + appClassName + " app;\n"
             + "    memset(&app, 0, sizeof(" + appClassName + "));\n"
             + "    " + appClassName + "_Init(&app);\n"
             + "    SwitchApp_Run((SwitchApp*)&app);\n"
             + "\n"
             + "    return 0;\n"
             + "}\n";
    }

    public static string Write(string outputDir, string appClassName, string includeFileName, IEnumerable<string>? allHeaders = null)
    {
        var content = Generate(appClassName, includeFileName, allHeaders);
        var path = Path.Combine(outputDir, "main.c");
        File.WriteAllText(path, content);
        return path;
    }
}