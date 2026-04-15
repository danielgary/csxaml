using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using RoslynDiagnostic = Microsoft.CodeAnalysis.Diagnostic;

namespace Csxaml.Generator.Tests;

internal static class GeneratedCompilationTestHarness
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

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
        references.Add(MetadataReference.CreateFromFile(typeof(ActivatorUtilities).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(IServiceCollection).Assembly.Location));
        return references;
    }

    private static string GetAssemblyPath(string projectName, string targetFramework)
    {
        return Path.Combine(
            RepoRoot,
            projectName,
            "bin",
            "Debug",
            targetFramework,
            $"{projectName}.dll");
    }
}
