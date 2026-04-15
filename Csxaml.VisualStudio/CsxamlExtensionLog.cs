using System;
using System.IO;

namespace Csxaml.VisualStudio;

internal static class CsxamlExtensionLog
{
    public static string LogFilePath => Path.Combine(Path.GetTempPath(), "csxaml-visualstudio.log");

    public static void Write(string message)
    {
        try
        {
            File.AppendAllText(LogFilePath, $"{DateTime.UtcNow:O} {message}{Environment.NewLine}");
        }
        catch
        {
        }
    }
}
