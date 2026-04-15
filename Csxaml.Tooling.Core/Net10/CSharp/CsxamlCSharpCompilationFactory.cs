using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Csxaml.Tooling.Core.Projects;

namespace Csxaml.Tooling.Core.CSharp;

internal sealed class CsxamlCSharpCompilationFactory
{
    public CSharpCompilation Create(string filePath, CsxamlProjectedDocument document)
    {
        return Create(filePath, document, includeProjectSources: false);
    }

    public CSharpCompilation CreateWithProjectSources(string filePath, CsxamlProjectedDocument document)
    {
        return Create(filePath, document, includeProjectSources: true);
    }

    private CSharpCompilation Create(
        string filePath,
        CsxamlProjectedDocument document,
        bool includeProjectSources)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var projects = ResolveProjects(filePath);
        var references = ResolveMetadataReferences(projects, includePrimaryProjectAssemblies: !includeProjectSources);
        var syntaxTrees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(
                document.Text,
                parseOptions,
                path: filePath + ".projection.cs")
        };

        if (includeProjectSources)
        {
            syntaxTrees.AddRange(LoadProjectSourceTrees(projects, parseOptions));
        }

        return CSharpCompilation.Create(
            Path.GetFileNameWithoutExtension(filePath) + ".CsxamlProjection",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static IReadOnlyList<MetadataReference> ResolveMetadataReferences(
        IReadOnlyList<CsxamlProjectInfo> projects,
        bool includePrimaryProjectAssemblies)
    {
        var references = new List<MetadataReference>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddTrustedPlatformReferences(references, seenPaths);

        foreach (var assemblyPath in ResolveProjectAssemblyPaths(projects, includePrimaryProjectAssemblies))
        {
            AddReference(references, seenPaths, assemblyPath);
        }

        return references;
    }

    private static void AddTrustedPlatformReferences(
        ICollection<MetadataReference> references,
        ISet<string> seenPaths)
    {
        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is not string trustedPlatformAssemblies)
        {
            return;
        }

        foreach (var path in trustedPlatformAssemblies.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            AddReference(references, seenPaths, path);
        }
    }

    private static void AddReference(
        ICollection<MetadataReference> references,
        ISet<string> seenPaths,
        string path)
    {
        if (!File.Exists(path) || !seenPaths.Add(path))
        {
            return;
        }

        try
        {
            references.Add(MetadataReference.CreateFromFile(path));
        }
        catch (Exception)
        {
            // Tooling should continue even if one dependency assembly is not reference-loadable.
        }
    }

    private static IReadOnlyList<CsxamlProjectInfo> ResolveProjects(string filePath)
    {
        var projectFile = CsxamlProjectLocator.FindOwningProjectFile(filePath);
        if (projectFile is null)
        {
            return Array.Empty<CsxamlProjectInfo>();
        }

        var project = CsxamlProjectFileReader.Read(projectFile);
        return new[] { project }
            .Concat(CsxamlProjectReferenceResolver.ResolveTransitive(project))
            .ToList();
    }

    private static IEnumerable<SyntaxTree> LoadProjectSourceTrees(
        IReadOnlyList<CsxamlProjectInfo> projects,
        CSharpParseOptions parseOptions)
    {
        foreach (var sourcePath in CsxamlProjectSourceResolver.ResolveSourcePaths(projects))
        {
            yield return CSharpSyntaxTree.ParseText(
                File.ReadAllText(sourcePath),
                parseOptions,
                path: sourcePath);
        }
    }

    private static IEnumerable<string> ResolveProjectAssemblyPaths(
        IEnumerable<CsxamlProjectInfo> projects,
        bool includePrimaryProjectAssemblies)
    {
        var primaryAssemblies = CsxamlProjectOutputResolver.ResolveAssemblyPaths(projects);
        foreach (var assemblyPath in primaryAssemblies)
        {
            var outputDirectory = Path.GetDirectoryName(assemblyPath);
            if (includePrimaryProjectAssemblies)
            {
                yield return assemblyPath;
            }

            if (outputDirectory is null || !Directory.Exists(outputDirectory))
            {
                continue;
            }

            foreach (var dependencyPath in Directory.EnumerateFiles(outputDirectory, "*.dll", SearchOption.TopDirectoryOnly))
            {
                if (!includePrimaryProjectAssemblies &&
                    string.Equals(dependencyPath, assemblyPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return dependencyPath;
            }
        }
    }
}
