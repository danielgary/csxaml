namespace Csxaml.VisualStudio.Tests;

internal static class VisualStudioArtifactPaths
{
    public static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    public static string GetManifestPath()
    {
        return GetExistingPath(
            Path.Combine("Csxaml.VisualStudio", "obj"),
            "net8.0-windows8.0",
            "extension.vsixmanifest");
    }

    public static string GetPublishManifestPath()
    {
        return Path.Combine(
            RepoRoot,
            "Csxaml.VisualStudio",
            "publishManifest.json");
    }

    public static string GetVsixPath()
    {
        return GetExistingPath(
            Path.Combine("Csxaml.VisualStudio", "bin"),
            "net8.0-windows8.0",
            "Csxaml.VisualStudio.vsix");
    }

    private static string GetExistingPath(string projectOutputRoot, string targetFramework, string fileName)
    {
        foreach (var configuration in GetConfigurationCandidates())
        {
            var candidatePath = Path.Combine(
                RepoRoot,
                projectOutputRoot,
                configuration,
                targetFramework,
                fileName);

            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }
        }

        return Path.Combine(
            RepoRoot,
            projectOutputRoot,
            GetBuildConfiguration(),
            targetFramework,
            fileName);
    }

    private static IReadOnlyList<string> GetConfigurationCandidates()
    {
        var candidates = new List<string> { GetBuildConfiguration() };
        if (!candidates.Contains("Release", StringComparer.OrdinalIgnoreCase))
        {
            candidates.Add("Release");
        }

        if (!candidates.Contains("Debug", StringComparer.OrdinalIgnoreCase))
        {
            candidates.Add("Debug");
        }

        return candidates;
    }

    private static string GetBuildConfiguration()
    {
        var frameworkDirectory = Directory.GetParent(AppContext.BaseDirectory);
        var configurationDirectory = frameworkDirectory?.Parent;
        return configurationDirectory?.Name ?? "Debug";
    }
}
