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

    public static IReadOnlyList<string> ResolveAssemblyClosurePaths(
        IEnumerable<CsxamlProjectInfo> projects,
        bool includePrimaryAssemblies = true)
    {
        var paths = new List<string>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var assemblyPath in ResolveAssemblyPaths(projects))
        {
            if (includePrimaryAssemblies)
            {
                AddPath(paths, seenPaths, assemblyPath);
            }

            foreach (var dependencyPath in EnumerateOutputDirectoryAssemblyPaths(assemblyPath))
            {
                AddPath(paths, seenPaths, dependencyPath);
            }
        }

        return paths;
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

    private static IEnumerable<string> EnumerateOutputDirectoryAssemblyPaths(string assemblyPath)
    {
        var outputDirectory = Path.GetDirectoryName(assemblyPath);
        if (outputDirectory is null || !Directory.Exists(outputDirectory))
        {
            return Array.Empty<string>();
        }

        return Directory
            .EnumerateFiles(outputDirectory, "*.dll", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);
    }

    private static void AddPath(ICollection<string> paths, ISet<string> seenPaths, string path)
    {
        if (!seenPaths.Add(path))
        {
            return;
        }

        paths.Add(path);
    }
}
