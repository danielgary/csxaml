namespace Csxaml.Tooling.Core.Projects;

public static class CsxamlProjectOutputResolver
{
    public static IReadOnlyList<string> ResolveAssemblyPaths(IEnumerable<CsxamlProjectInfo> projects)
    {
        return projects
            .Select(ResolveAssemblyPath)
            .Where(path => path is not null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string? ResolveAssemblyPath(CsxamlProjectInfo project)
    {
        var preferred = Path.Combine(
            project.ProjectDirectory,
            "bin",
            "Debug",
            project.TargetFramework,
            $"{project.AssemblyName}.dll");
        if (File.Exists(preferred))
        {
            return preferred;
        }

        var binDirectory = Path.Combine(project.ProjectDirectory, "bin");
        return Directory.Exists(binDirectory)
            ? Directory
                .EnumerateFiles(binDirectory, $"{project.AssemblyName}.dll", SearchOption.AllDirectories)
                .OrderByDescending(path => File.GetLastWriteTimeUtc(path))
                .FirstOrDefault()
            : null;
    }
}
