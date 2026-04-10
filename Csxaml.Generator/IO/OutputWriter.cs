using System.Text;

namespace Csxaml.Generator;

internal static class OutputWriter
{
    public static void WriteAll(IReadOnlyList<GeneratedFile> generatedFiles)
    {
        foreach (var file in generatedFiles)
        {
            WriteFile(file);
        }
    }

    private static void WriteFile(GeneratedFile file)
    {
        try
        {
            var directory = Path.GetDirectoryName(file.OutputPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(
                file.OutputPath,
                file.Content,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"output file write failure: {file.OutputPath} ({exception.Message})");
        }
    }
}
