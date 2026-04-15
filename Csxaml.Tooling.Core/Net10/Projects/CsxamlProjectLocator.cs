namespace Csxaml.Tooling.Core.Projects;

public static class CsxamlProjectLocator
{
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
