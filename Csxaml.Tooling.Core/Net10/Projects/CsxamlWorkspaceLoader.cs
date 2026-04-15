using System.Reflection;
using Csxaml.ControlMetadata;
using Csxaml.Generator;
using Csxaml.Tooling.Core.Markup;
using ControlMetadataModel = Csxaml.ControlMetadata.ControlMetadata;

namespace Csxaml.Tooling.Core.Projects;

public sealed class CsxamlWorkspaceLoader
{
    private readonly Parser _parser = new();
    private readonly ExternalControlMetadataBuilder _externalControlMetadataBuilder = new();

    public CsxamlWorkspaceSnapshot Load(string filePath, string currentText)
    {
        var projectFile = CsxamlProjectLocator.FindOwningProjectFile(filePath)
            ?? throw new InvalidOperationException($"No project file was found for '{filePath}'.");
        var project = CsxamlProjectFileReader.Read(projectFile);
        var referencedProjects = CsxamlProjectReferenceResolver.ResolveTransitive(project);

        var componentSymbols = new List<CsxamlWorkspaceComponentSymbol>();
        componentSymbols.AddRange(ReadProjectComponents(project, filePath, currentText));
        foreach (var referencedProject in referencedProjects)
        {
            componentSymbols.AddRange(ReadProjectComponents(referencedProject, currentFilePath: null, currentText: null));
        }

        var externalControls = LoadExternalControls(ResolveExternalAssemblyPaths(project, referencedProjects));
        return new CsxamlWorkspaceSnapshot(project, componentSymbols, externalControls);
    }

    private IReadOnlyList<CsxamlWorkspaceComponentSymbol> ReadProjectComponents(
        CsxamlProjectInfo project,
        string? currentFilePath,
        string? currentText)
    {
        var symbols = new List<CsxamlWorkspaceComponentSymbol>();
        foreach (var filePath in Directory.EnumerateFiles(project.ProjectDirectory, "*.csxaml", SearchOption.AllDirectories))
        {
            if (IsIgnoredPath(filePath))
            {
                continue;
            }

            var sourceText = string.Equals(filePath, currentFilePath, StringComparison.OrdinalIgnoreCase)
                ? currentText ?? File.ReadAllText(filePath)
                : File.ReadAllText(filePath);
            if (TryParseComponent(project, filePath, sourceText, out var symbol))
            {
                symbols.Add(symbol!);
            }
        }

        return symbols;
    }

    private bool TryParseComponent(
        CsxamlProjectInfo project,
        string filePath,
        string text,
        out CsxamlWorkspaceComponentSymbol? symbol)
    {
        try
        {
            var source = new SourceDocument(filePath, text);
            var file = _parser.Parse(source);
            var definition = file.Component;
            symbol = CreateComponentSymbol(
                project,
                filePath,
                file.Namespace?.NamespaceName ?? project.DefaultNamespace,
                definition.Name,
                definition.Parameters
                    .Select(parameter => new ComponentParameterMetadata(parameter.Name, parameter.TypeName))
                    .ToList(),
                definition.SupportsDefaultSlot,
                definition.Span.Start,
                definition.Name.Length);
            return true;
        }
        catch (DiagnosticException)
        {
            var declaration = CsxamlComponentSignatureScanner.Scan(text).FirstOrDefault();
            if (declaration is null)
            {
                symbol = null;
                return false;
            }

            symbol = CreateComponentSymbol(
                project,
                filePath,
                CsxamlNamespaceDirectiveScanner.Scan(text)?.NamespaceName ?? project.DefaultNamespace,
                declaration.Name,
                declaration.Parameters
                    .Select(parameter => new ComponentParameterMetadata(parameter.Name, parameter.TypeName))
                    .ToList(),
                supportsDefaultSlot: false,
                declaration.NameStart,
                declaration.NameLength);
            return true;
        }
    }

    private CsxamlWorkspaceComponentSymbol CreateComponentSymbol(
        CsxamlProjectInfo project,
        string filePath,
        string namespaceName,
        string name,
        IReadOnlyList<ComponentParameterMetadata> parameters,
        bool supportsDefaultSlot,
        int nameStart,
        int nameLength)
    {
        var metadata = new ComponentMetadata(
            name,
            namespaceName,
            project.AssemblyName,
            $"{namespaceName}.{name}Component",
            parameters.Count == 0 ? null : $"{namespaceName}.{name}Props",
            parameters,
            supportsDefaultSlot);
        return new CsxamlWorkspaceComponentSymbol(metadata, filePath, nameStart, nameLength);
    }

    private IReadOnlyList<ControlMetadataModel> LoadExternalControls(IReadOnlyList<string> referencePaths)
    {
        var controls = new List<ControlMetadataModel>();
        using var resolver = new ReferenceAssemblyTypeResolver(referencePaths);
        foreach (var assembly in resolver.LoadedAssemblies)
        {
            foreach (var type in GetLoadableTypes(assembly))
            {
                if (!TryBuildExternalControl(type, out var metadata))
                {
                    continue;
                }

                controls.Add(metadata);
            }
        }

        return controls
            .DistinctBy(control => control.ClrTypeName, StringComparer.Ordinal)
            .OrderBy(control => control.ClrTypeName, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<string> ResolveExternalAssemblyPaths(
        CsxamlProjectInfo project,
        IReadOnlyList<CsxamlProjectInfo> referencedProjects)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var primaryAssemblies = CsxamlProjectOutputResolver.ResolveAssemblyPaths(
            new[] { project }.Concat(referencedProjects));
        foreach (var assemblyPath in primaryAssemblies)
        {
            paths.Add(assemblyPath);

            var outputDirectory = Path.GetDirectoryName(assemblyPath);
            if (outputDirectory is null || !Directory.Exists(outputDirectory))
            {
                continue;
            }

            foreach (var dependencyPath in Directory.EnumerateFiles(outputDirectory, "*.dll", SearchOption.TopDirectoryOnly))
            {
                paths.Add(dependencyPath);
            }
        }

        return paths.ToList();
    }

    private bool TryBuildExternalControl(Type type, out ControlMetadataModel metadata)
    {
        try
        {
            if (type.FullName is null || type.Namespace is null)
            {
                metadata = null!;
                return false;
            }

            if (ControlMetadataRegistry.Controls.Any(control => control.ClrTypeName == type.FullName))
            {
                metadata = null!;
                return false;
            }

            if (_externalControlMetadataBuilder.TryBuild(type, out var builtMetadata, out _))
            {
                metadata = builtMetadata!;
                return true;
            }
        }
        catch (Exception)
        {
            // Tooling should degrade gracefully when a referenced assembly cannot fully load in the editor host.
        }

        metadata = null!;
        return false;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes().Where(type => type is not null)!;
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
    }

    private static bool IsIgnoredPath(string filePath)
    {
        return filePath.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            || filePath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
    }
}
