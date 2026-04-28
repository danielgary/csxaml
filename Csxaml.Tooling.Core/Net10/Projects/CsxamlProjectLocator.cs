namespace Csxaml.Tooling.Core.Projects;

/// <summary>
/// Locates the owning project file for a CSXAML document.
/// </summary>
public static class CsxamlProjectLocator
{
    /// <summary>
    /// Walks up from a source file to find the nearest project file.
    /// </summary>
    /// <param name="filePath">The source file path.</param>
    /// <returns>The nearest project file path, or <see langword="null"/> when none is found.</returns>
    public static string? FindOwningProjectFile(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        while (!string.IsNullOrWhiteSpace(directory))
        {
            var projectFiles = Directory
                .EnumerateFiles(directory, "*.csproj", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();
            if (projectFiles.Count > 0)
            {
                return projectFiles[0];
            }

            directory = Path.GetDirectoryName(directory);
        }

        return null;
    }
}
