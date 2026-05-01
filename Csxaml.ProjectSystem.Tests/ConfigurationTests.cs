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
        AssertProjectPlatformTarget("samples", "Csxaml.ProjectSystem.Components", "Csxaml.ProjectSystem.Components.csproj");
        AssertProjectPlatformTarget("samples", "Csxaml.ProjectSystem.Consumer", "Csxaml.ProjectSystem.Consumer.csproj");
    }

    [TestMethod]
    public void SampleLaunches_UseDefaultMachineArchitecture()
    {
        using var launchJson = JsonDocument.Parse(File.ReadAllText(GetRepoPath(".vscode", "launch.json")));
        using var tasksJson = JsonDocument.Parse(File.ReadAllText(GetRepoPath(".vscode", "tasks.json")));

        AssertLaunchUsesDefaultMachineArchitecture(
            launchJson.RootElement.GetProperty("configurations"),
            tasksJson.RootElement.GetProperty("tasks"),
            "Sample: Existing WinUI + CSXAML",
            "build-sample-existing-winui");
        AssertLaunchUsesDefaultMachineArchitecture(
            launchJson.RootElement.GetProperty("configurations"),
            tasksJson.RootElement.GetProperty("tasks"),
            "Sample: Hello World (Generated App)",
            "build-sample-hello-world");
        AssertLaunchUsesDefaultMachineArchitecture(
            launchJson.RootElement.GetProperty("configurations"),
            tasksJson.RootElement.GetProperty("tasks"),
            "Sample: Todo App (Generated App)",
            "build-sample-todo-app");
        AssertLaunchUsesDefaultMachineArchitecture(
            launchJson.RootElement.GetProperty("configurations"),
            tasksJson.RootElement.GetProperty("tasks"),
            "Sample: Feature Gallery (Generated App)",
            "build-sample-feature-gallery");
    }

    [TestMethod]
    public void GeneratedApplicationMode_UsesHiddenGeneratedXamlCompanions()
    {
        var targets = XDocument.Load(GetRepoPath("build", "Csxaml.targets"));

        var constants = targets.Root?
            .Elements("PropertyGroup")
            .Elements("DefineConstants")
            .SingleOrDefault();
        Assert.IsNotNull(constants);
        StringAssert.Contains((string?)constants.Attribute("Condition"), "CsxamlApplicationMode");
        StringAssert.Contains(constants.Value, "DISABLE_XAML_GENERATED_MAIN");

        var application = targets.Root?
            .Elements("ItemGroup")
            .Elements("ApplicationDefinition")
            .SingleOrDefault();
        Assert.IsNotNull(application);
        Assert.AreEqual(@"$(CsxamlGeneratedDirectory)\App.xaml", (string?)application.Attribute("Include"));
        Assert.AreEqual("true", (string?)application.Attribute("CsxamlGenerated"));
        Assert.AreEqual("false", (string?)application.Attribute("Visible"));

        Assert.IsTrue(
            targets.Descendants("FileWrites").Any(
                element => string.Equals(
                    (string?)element.Attribute("Include"),
                    @"$(CsxamlGeneratedDirectory)\**\*.xaml",
                    StringComparison.Ordinal)));

        var generatedPage = targets.Root?
            .Elements("Target")
            .Where(target => string.Equals(
                (string?)target.Attribute("Name"),
                "IncludeCsxamlGeneratedCompileItems",
                StringComparison.Ordinal))
            .Elements("ItemGroup")
            .Elements("Page")
            .SingleOrDefault();

        Assert.IsNotNull(generatedPage);
        Assert.AreEqual(@"$(CsxamlGeneratedDirectory)\**\*.xaml", (string?)generatedPage.Attribute("Include"));
        Assert.AreEqual(@"$(CsxamlGeneratedDirectory)\App.xaml", (string?)generatedPage.Attribute("Exclude"));
        Assert.AreEqual("true", (string?)generatedPage.Attribute("CsxamlGenerated"));
        Assert.AreEqual(
            "WinUI",
            generatedPage.Elements("XamlRuntime").SingleOrDefault()?.Value);
    }

    private static void AssertLaunchUsesDefaultMachineArchitecture(
        JsonElement launchItems,
        JsonElement taskItems,
        string launchName,
        string taskLabel)
    {
        var launch = FindObject(launchItems, "name", launchName);
        var task = FindObject(taskItems, "label", taskLabel);

        Assert.AreEqual(taskLabel, launch.GetProperty("preLaunchTask").GetString());
        AssertPathDoesNotContainX64(launch.GetProperty("program").GetString());
        AssertPathDoesNotContainX64(launch.GetProperty("cwd").GetString());

        var args = task.GetProperty("args").EnumerateArray()
            .Select(argument => argument.GetString())
            .ToList();
        CollectionAssert.DoesNotContain(args, "-p:Platform=x64");
    }

    private static void AssertProjectPlatformTarget(params string[] pathParts)
    {
        var project = XDocument.Load(GetRepoPath(pathParts));
        var platformTarget = project.Root?
            .Elements("PropertyGroup")
            .Elements("PlatformTarget")
            .SingleOrDefault();

        Assert.IsNotNull(platformTarget, $"{string.Join("\\", pathParts)} must declare PlatformTarget.");
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
