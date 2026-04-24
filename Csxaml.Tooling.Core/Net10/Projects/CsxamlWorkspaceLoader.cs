using Csxaml.Generator;

namespace Csxaml.Tooling.Core.Projects;

public sealed class CsxamlWorkspaceLoader
{
    private readonly CsxamlWorkspaceComponentCache _componentCache = new(new Parser());
    private readonly CsxamlExternalControlCache _externalControlCache = new(new ExternalControlMetadataBuilder());

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
