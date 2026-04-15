using System.Text;

namespace Csxaml.Generator;

internal static class OutputWriter
{
    public static void WriteAll(string outputDirectory, IReadOnlyList<GeneratedFile> generatedFiles)
    {
        var fullOutputDirectory = Path.GetFullPath(outputDirectory);
        var managedRoot = Directory.GetParent(fullOutputDirectory)?.FullName ?? fullOutputDirectory;
        Directory.CreateDirectory(fullOutputDirectory);

        var comparer = GetPathComparer();
        var expectedPathsByManagedDirectory = generatedFiles
            .GroupBy(
                file => GetValidatedPath(managedRoot, file.ManagedDirectory ?? Path.GetDirectoryName(file.OutputPath)!),
                comparer)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(file => GetValidatedPath(managedRoot, file.OutputPath))
                    .ToHashSet(comparer),
                comparer);

        foreach (var file in generatedFiles)
        {
            WriteFile(managedRoot, file);
        }

        foreach (var pair in expectedPathsByManagedDirectory)
        {
            Directory.CreateDirectory(pair.Key);
            DeleteStaleFiles(pair.Key, pair.Value, comparer);
        }

        foreach (var managedDirectory in expectedPathsByManagedDirectory.Keys.OrderByDescending(path => path.Length))
        {
            DeleteEmptyDirectories(managedDirectory);
        }
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
        string managedDirectory,
        IReadOnlySet<string> expectedPaths,
        StringComparer comparer)
    {
        foreach (var existingPath in Directory.GetFiles(managedDirectory, "*", SearchOption.AllDirectories))
        {
            var normalizedPath = Path.GetFullPath(existingPath);
            if (!expectedPaths.Contains(normalizedPath))
            {
                File.Delete(existingPath);
            }
        }
    }

    private static string GetValidatedPath(string managedRoot, string path)
    {
        var fullOutputPath = Path.GetFullPath(path);
        var relativePath = Path.GetRelativePath(managedRoot, fullOutputPath);
        if (relativePath.StartsWith("..", StringComparison.Ordinal) ||
            Path.IsPathRooted(relativePath))
        {
            throw new InvalidOperationException(
                $"Generated output path '{path}' must stay under '{managedRoot}'.");
        }

        return fullOutputPath;
    }

    private static StringComparer GetPathComparer()
    {
        return OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
    }

    private static void WriteFile(string managedRoot, GeneratedFile file)
    {
        try
        {
            var outputPath = GetValidatedPath(managedRoot, file.OutputPath);
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
