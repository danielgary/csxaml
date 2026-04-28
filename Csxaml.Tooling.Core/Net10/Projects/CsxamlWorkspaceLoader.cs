using Csxaml.Generator;

namespace Csxaml.Tooling.Core.Projects;

/// <summary>
/// Loads project, component, and external-control metadata for editor services.
/// </summary>
public sealed class CsxamlWorkspaceLoader
{
    private readonly CsxamlWorkspaceComponentCache _componentCache = new(new Parser());
    private readonly CsxamlExternalControlCache _externalControlCache = new(new ExternalControlMetadataBuilder());

    /// <summary>
    /// Loads a workspace snapshot for a CSXAML document.
    /// </summary>
    /// <param name="filePath">The CSXAML file path.</param>
    /// <param name="currentText">The current unsaved source text for the file.</param>
    /// <returns>The workspace snapshot used by tooling services.</returns>
    public CsxamlWorkspaceSnapshot Load(string filePath, string currentText)
    {
        var projectFile = CsxamlProjectLocator.FindOwningProjectFile(filePath)
            ?? throw new InvalidOperationException($"No project file was found for '{filePath}'.");
        var project = CsxamlProjectFileReader.Read(projectFile);
        var referencedProjects = CsxamlProjectReferenceResolver.ResolveTransitive(project);
        var componentSymbols = LoadComponentSymbols(project, referencedProjects, filePath, currentText);
        var externalControls = _externalControlCache.Load(ResolveExternalAssemblyPaths(project, referencedProjects));
        return new CsxamlWorkspaceSnapshot(project, componentSymbols, externalControls);
    }

    private IReadOnlyList<CsxamlWorkspaceComponentSymbol> LoadComponentSymbols(
        CsxamlProjectInfo project,
        IReadOnlyList<CsxamlProjectInfo> referencedProjects,
        string currentFilePath,
        string currentText)
    {
        var symbols = new List<CsxamlWorkspaceComponentSymbol>();
        symbols.AddRange(_componentCache.GetProjectComponents(project, currentFilePath, currentText));
        foreach (var referencedProject in referencedProjects)
        {
            symbols.AddRange(_componentCache.GetProjectComponents(referencedProject, currentFilePath: null, currentText: null));
        }

        return symbols;
    }

    private static IReadOnlyList<string> ResolveExternalAssemblyPaths(
        CsxamlProjectInfo project,
        IReadOnlyList<CsxamlProjectInfo> referencedProjects)
    {
        return CsxamlProjectOutputResolver.ResolveAssemblyClosurePaths(
            new[] { project }.Concat(referencedProjects));
    }
}
