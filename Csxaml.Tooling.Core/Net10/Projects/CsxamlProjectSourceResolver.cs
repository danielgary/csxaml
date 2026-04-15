namespace Csxaml.Tooling.Core.Projects;

public static class CsxamlProjectSourceResolver
{
    public static IReadOnlyList<string> ResolveSourcePaths(IEnumerable<CsxamlProjectInfo> projects)
    {
        return projects
            .SelectMany(EnumerateSourcePaths)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IEnumerable<string> EnumerateSourcePaths(CsxamlProjectInfo project)
    {
        if (!Directory.Exists(project.ProjectDirectory))
        {
            yield break;
        }

        foreach (var filePath in Directory.EnumerateFiles(project.ProjectDirectory, "*.cs", SearchOption.AllDirectories))
        {
            if (IsIgnoredPath(filePath))
            {
                continue;
            }

            yield return filePath;
        }
    }

    private static bool IsIgnoredPath(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        while (!string.IsNullOrWhiteSpace(directory))
        {
            var name = Path.GetFileName(directory);
            if (string.Equals(name, "bin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "obj", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            directory = Path.GetDirectoryName(directory);
        }

        return false;
    }
}
