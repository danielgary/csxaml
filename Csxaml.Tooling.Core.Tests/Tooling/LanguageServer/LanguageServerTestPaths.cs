namespace Csxaml.Tooling.Core.Tests.Tooling.LanguageServer;

internal static class LanguageServerTestPaths
{
    public static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    public static string GetServerExecutablePath()
    {
        foreach (var configuration in GetConfigurationCandidates())
        {
            foreach (var fileName in GetLaunchFileNames())
            {
                var candidatePath = Path.Combine(
                    RepoRoot,
                    "Csxaml.LanguageServer",
                    "bin",
                    configuration,
                    "net10.0",
                    fileName);

                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }
            }
        }

        return Path.Combine(
            RepoRoot,
            "Csxaml.LanguageServer",
            "bin",
            GetBuildConfiguration(),
            "net10.0",
            GetLaunchFileNames()[0]);
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

    private static IReadOnlyList<string> GetLaunchFileNames()
    {
        return OperatingSystem.IsWindows()
            ? ["Csxaml.LanguageServer.dll", "Csxaml.LanguageServer.exe"]
            : ["Csxaml.LanguageServer.dll", "Csxaml.LanguageServer"];
    }
}
