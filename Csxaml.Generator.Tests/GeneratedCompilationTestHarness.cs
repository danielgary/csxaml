using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using RoslynDiagnostic = Microsoft.CodeAnalysis.Diagnostic;

namespace Csxaml.Generator.Tests;

internal static class GeneratedCompilationTestHarness
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    private static readonly string BuildConfiguration = GetBuildConfiguration();

    public static IReadOnlyList<RoslynDiagnostic> Compile(string generatedCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            generatedCode,
            path: "GeneratedComponent.g.cs",
            options: new CSharpParseOptions(LanguageVersion.Latest));
        var compilation = CSharpCompilation.Create(
            "GeneratedComponentTests",
            [syntaxTree],
            GetReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        return compilation.GetDiagnostics();
    }

    private static IReadOnlyList<MetadataReference> GetReferences()
    {
        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            ?.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToList()
            ?? [];

        references.Add(MetadataReference.CreateFromFile(GetAssemblyPath(
            "Csxaml.Runtime",
            "net10.0-windows10.0.19041.0")));
        references.Add(MetadataReference.CreateFromFile(GetAssemblyPath(
            "Csxaml.ControlMetadata",
            "net10.0")));
        references.Add(MetadataReference.CreateFromFile(typeof(Button).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(FindWinUiMetadataPath()));
        references.Add(MetadataReference.CreateFromFile(FindWindowsSdkMetadataPath("Windows.Foundation.UniversalApiContract.winmd")));
        references.Add(MetadataReference.CreateFromFile(typeof(ActivatorUtilities).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(IServiceCollection).Assembly.Location));
        return references;
    }

    private static string FindWinUiMetadataPath()
    {
        var packageRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages",
            "microsoft.windowsappsdk.winui");
        var candidate = Directory
            .EnumerateFiles(packageRoot, "Microsoft.UI.Xaml.winmd", SearchOption.AllDirectories)
            .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        return candidate ?? throw new FileNotFoundException(
            "Could not find Microsoft.UI.Xaml.winmd under the NuGet package cache.",
            packageRoot);
    }

    private static string FindWindowsSdkMetadataPath(string fileName)
    {
        var packageRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages",
            "microsoft.windows.sdk.net.ref");
        var candidate = Directory
            .EnumerateFiles(packageRoot, fileName, SearchOption.AllDirectories)
            .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        return candidate ?? throw new FileNotFoundException(
            $"Could not find {fileName} under the NuGet package cache.",
            packageRoot);
    }

    private static string GetAssemblyPath(string projectName, string targetFramework)
    {
        foreach (var configuration in GetConfigurationCandidates())
        {
            var candidatePath = Path.Combine(
                RepoRoot,
                projectName,
                "bin",
                configuration,
                targetFramework,
                $"{projectName}.dll");

            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }
        }

        throw new FileNotFoundException(
            $"Could not find '{projectName}.dll' for target framework '{targetFramework}'.",
            Path.Combine(
                RepoRoot,
                projectName,
                "bin",
                BuildConfiguration,
                targetFramework,
                $"{projectName}.dll"));
    }

    private static IReadOnlyList<string> GetConfigurationCandidates()
    {
        var candidates = new List<string> { BuildConfiguration };
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
