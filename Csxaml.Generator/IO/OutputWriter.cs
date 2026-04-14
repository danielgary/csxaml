using System.Text;

namespace Csxaml.Generator;

internal static class OutputWriter
{
    public static void WriteAll(string outputDirectory, IReadOnlyList<GeneratedFile> generatedFiles)
    {
        var fullOutputDirectory = Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(fullOutputDirectory);

        var comparer = GetPathComparer();
        var expectedPaths = generatedFiles
            .Select(file => GetValidatedOutputPath(fullOutputDirectory, file.OutputPath))
            .ToHashSet(comparer);

        foreach (var file in generatedFiles)
        {
            WriteFile(fullOutputDirectory, file);
        }

        DeleteStaleFiles(fullOutputDirectory, expectedPaths, comparer);
        DeleteEmptyDirectories(fullOutputDirectory);
    }

    private static void DeleteEmptyDirectories(string outputDirectory)
    {
        foreach (var directory in Directory.GetDirectories(outputDirectory, "*", SearchOption.AllDirectories)
                     .OrderByDescending(path => path.Length))
        {
            if (!Directory.EnumerateFileSystemEntries(directory).Any())
            {
                Directory.Delete(directory);
            }
        }
    }

    private static void DeleteStaleFiles(
        string outputDirectory,
        IReadOnlySet<string> expectedPaths,
        StringComparer comparer)
    {
        foreach (var existingPath in Directory.GetFiles(outputDirectory, "*", SearchOption.AllDirectories))
        {
            var normalizedPath = Path.GetFullPath(existingPath);
            if (!expectedPaths.Contains(normalizedPath))
            {
                File.Delete(existingPath);
            }
        }
    }

    private static string GetValidatedOutputPath(string outputDirectory, string outputPath)
    {
        var fullOutputPath = Path.GetFullPath(outputPath);
        var relativePath = Path.GetRelativePath(outputDirectory, fullOutputPath);
        if (relativePath.StartsWith("..", StringComparison.Ordinal) ||
            Path.IsPathRooted(relativePath))
        {
            throw new InvalidOperationException(
                $"Generated output path '{outputPath}' must stay under '{outputDirectory}'.");
        }

        return fullOutputPath;
    }

    private static StringComparer GetPathComparer()
    {
        return OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
    }

    private static void WriteFile(string outputDirectory, GeneratedFile file)
    {
        try
        {
            var outputPath = GetValidatedOutputPath(outputDirectory, file.OutputPath);
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(outputPath))
            {
                var existingContent = File.ReadAllText(outputPath);
                if (string.Equals(existingContent, file.Content, StringComparison.Ordinal))
                {
                    return;
                }
            }

            File.WriteAllText(outputPath, file.Content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"output file write failure: {file.OutputPath} ({exception.Message})");
        }
    }
}
