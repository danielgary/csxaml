using System.Xml.Linq;
using System.Text.Json;

namespace Csxaml.ProjectSystem.Tests;

[TestClass]
public sealed class ConfigurationTests
{
    [TestMethod]
    public void NuGetConfig_UsesOnlyNuGetOrgPackageSource()
    {
        var config = XDocument.Load(GetRepoPath("NuGet.Config"));
        var sources = config.Root?
            .Element("packageSources")?
            .Elements("add")
            .Select(element => new
            {
                Key = (string?)element.Attribute("key"),
                Value = (string?)element.Attribute("value")
            })
            .ToList();

        Assert.IsNotNull(sources);
        Assert.HasCount(1, sources);
        Assert.AreEqual("NuGet.org", sources[0].Key);
        Assert.AreEqual("https://api.nuget.org/v3/index.json", sources[0].Value);
    }

    [TestMethod]
    public void WinUiLibraries_StayArchitectureNeutralForGeneratorReflection()
    {
        AssertProjectPlatformTarget("Csxaml.Runtime", "Csxaml.Runtime.csproj");
        AssertProjectPlatformTarget("Csxaml.ExternalControls", "Csxaml.ExternalControls.csproj");
        AssertProjectPlatformTarget("Csxaml.ProjectSystem.Components", "Csxaml.ProjectSystem.Components.csproj");
        AssertProjectPlatformTarget("Csxaml.ProjectSystem.Consumer", "Csxaml.ProjectSystem.Consumer.csproj");
    }

    [TestMethod]
    public void DemoLaunch_UsesDefaultMachineArchitecture()
    {
        using var launchJson = JsonDocument.Parse(File.ReadAllText(GetRepoPath(".vscode", "launch.json")));
        using var tasksJson = JsonDocument.Parse(File.ReadAllText(GetRepoPath(".vscode", "tasks.json")));

        var launch = FindObject(
            launchJson.RootElement.GetProperty("configurations"),
            "name",
            "Csxaml.Demo");
        var task = FindObject(
            tasksJson.RootElement.GetProperty("tasks"),
            "label",
            "build-demo");

        Assert.AreEqual("build-demo", launch.GetProperty("preLaunchTask").GetString());
        AssertPathDoesNotContainX64(launch.GetProperty("program").GetString());
        AssertPathDoesNotContainX64(launch.GetProperty("cwd").GetString());

        var args = task.GetProperty("args").EnumerateArray()
            .Select(argument => argument.GetString())
            .ToList();
        CollectionAssert.DoesNotContain(args, "-p:Platform=x64");
    }

    private static void AssertProjectPlatformTarget(string projectDirectory, string projectFileName)
    {
        var project = XDocument.Load(GetRepoPath(projectDirectory, projectFileName));
        var platformTarget = project.Root?
            .Elements("PropertyGroup")
            .Elements("PlatformTarget")
            .SingleOrDefault();

        Assert.IsNotNull(platformTarget, $"{projectDirectory} must declare PlatformTarget.");
        Assert.AreEqual("AnyCPU", platformTarget.Value);
    }

    private static void AssertPathDoesNotContainX64(string? path)
    {
        Assert.IsNotNull(path);
        Assert.IsFalse(path.Contains("\\x64\\", StringComparison.OrdinalIgnoreCase), path);
    }

    private static JsonElement FindObject(JsonElement items, string propertyName, string value)
    {
        foreach (var item in items.EnumerateArray())
        {
            if (item.TryGetProperty(propertyName, out var property) &&
                string.Equals(property.GetString(), value, StringComparison.Ordinal))
            {
                return item;
            }
        }

        Assert.Fail($"Could not find JSON object with {propertyName}='{value}'.");
        return default;
    }

    private static string GetRepoPath(params string[] parts)
    {
        var pathParts = new[]
        {
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            ".."
        }.Concat(parts).ToArray();

        return Path.GetFullPath(Path.Combine(pathParts));
    }
}
