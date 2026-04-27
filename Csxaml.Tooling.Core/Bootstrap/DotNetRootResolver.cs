using System.Text.Json;

namespace Csxaml.Tooling.Core.Bootstrap;

/// <summary>
/// Resolves the .NET root that can run a tooling executable.
/// </summary>
public static class DotNetRootResolver
{
    /// <summary>
    /// Selects a configured or default .NET root compatible with the executable runtime configuration.
    /// </summary>
    /// <param name="executablePath">The tooling executable path whose runtime configuration should be inspected.</param>
    /// <param name="configuredRoot">The user-configured .NET root candidate.</param>
    /// <param name="defaultRoot">The editor or process default .NET root candidate.</param>
    /// <returns>The compatible .NET root, or <see langword="null"/> when no candidate exists.</returns>
    public static string? Resolve(string executablePath, string? configuredRoot, string? defaultRoot)
    {
        var runtimeConfigPath = Path.ChangeExtension(executablePath, ".runtimeconfig.json");
        var requirement = ReadFrameworkRequirement(runtimeConfigPath);

        if (requirement is not null)
        {
            if (IsCompatibleRoot(configuredRoot, requirement.Value))
            {
                return configuredRoot;
            }

            if (IsCompatibleRoot(defaultRoot, requirement.Value))
            {
                return defaultRoot;
            }
        }

        if (!string.IsNullOrWhiteSpace(configuredRoot) && Directory.Exists(configuredRoot))
        {
            return configuredRoot;
        }

        if (!string.IsNullOrWhiteSpace(defaultRoot) && Directory.Exists(defaultRoot))
        {
            return defaultRoot;
        }

        return null;
    }

    private static FrameworkRequirement? ReadFrameworkRequirement(string runtimeConfigPath)
    {
        if (!File.Exists(runtimeConfigPath))
        {
            return null;
        }

        using var stream = File.OpenRead(runtimeConfigPath);
        using var document = JsonDocument.Parse(stream);
        if (!document.RootElement.TryGetProperty("runtimeOptions", out var runtimeOptions))
        {
            return null;
        }

        if (TryReadFramework(runtimeOptions, out var requirement))
        {
            return requirement;
        }

        if (!runtimeOptions.TryGetProperty("frameworks", out var frameworks))
        {
            return null;
        }

        foreach (var frameworkElement in frameworks.EnumerateArray())
        {
            if (TryParseFramework(frameworkElement, out requirement))
            {
                return requirement;
            }
        }

        return null;
    }

    private static bool TryReadFramework(JsonElement runtimeOptions, out FrameworkRequirement requirement)
    {
        if (runtimeOptions.TryGetProperty("framework", out var framework)
            && TryParseFramework(framework, out requirement))
        {
            return true;
        }

        requirement = default;
        return false;
    }

    private static bool TryParseFramework(JsonElement frameworkElement, out FrameworkRequirement requirement)
    {
        requirement = default;
        if (!frameworkElement.TryGetProperty("name", out var nameElement)
            || !frameworkElement.TryGetProperty("version", out var versionElement))
        {
            return false;
        }

        var name = nameElement.GetString();
        var versionText = versionElement.GetString();
        if (string.IsNullOrWhiteSpace(name)
            || string.IsNullOrWhiteSpace(versionText)
            || !Version.TryParse(versionText, out var version))
        {
            return false;
        }

        requirement = new FrameworkRequirement(name, version);
        return true;
    }

    private static bool IsCompatibleRoot(string? root, FrameworkRequirement requirement)
    {
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
        {
            return false;
        }

        var sharedFrameworkRoot = Path.Combine(root, "shared", requirement.Name);
        if (!Directory.Exists(sharedFrameworkRoot))
        {
            return false;
        }

        foreach (var candidateDirectory in Directory.EnumerateDirectories(sharedFrameworkRoot))
        {
            var candidateVersionText = Path.GetFileName(candidateDirectory);
            if (Version.TryParse(candidateVersionText, out var candidateVersion)
                && candidateVersion.Major == requirement.Version.Major
                && candidateVersion.Minor == requirement.Version.Minor
                && candidateVersion >= requirement.Version)
            {
                return true;
            }
        }

        return false;
    }

    private readonly record struct FrameworkRequirement(string Name, Version Version);
}
