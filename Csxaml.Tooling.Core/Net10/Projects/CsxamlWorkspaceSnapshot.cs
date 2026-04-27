using Csxaml.ControlMetadata;
using ControlMetadataModel = Csxaml.ControlMetadata.ControlMetadata;

namespace Csxaml.Tooling.Core.Projects;

/// <summary>
/// Captures the project symbols visible to CSXAML editor services.
/// </summary>
public sealed class CsxamlWorkspaceSnapshot
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<CsxamlWorkspaceComponentSymbol>> _componentsByQualifiedName;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<ControlMetadataModel>> _externalControlsByQualifiedName;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsxamlWorkspaceSnapshot"/> class.
    /// </summary>
    /// <param name="project">The current project metadata.</param>
    /// <param name="components">The CSXAML components visible to the project.</param>
    /// <param name="externalControls">The external controls visible to the project.</param>
    public CsxamlWorkspaceSnapshot(
        CsxamlProjectInfo project,
        IReadOnlyList<CsxamlWorkspaceComponentSymbol> components,
        IReadOnlyList<ControlMetadataModel> externalControls)
    {
        Project = project;
        Components = components;
        ExternalControls = externalControls;
        _componentsByQualifiedName = components
            .GroupBy(symbol => GetQualifiedName(symbol.Metadata.NamespaceName, symbol.Metadata.Name), StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<CsxamlWorkspaceComponentSymbol>)group.ToList(),
                StringComparer.Ordinal);
        _externalControlsByQualifiedName = externalControls
            .GroupBy(control => GetQualifiedName(GetNamespace(control.ClrTypeName), GetName(control.ClrTypeName)), StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<ControlMetadataModel>)group.ToList(),
                StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets the CSXAML components visible to the project.
    /// </summary>
    public IReadOnlyList<CsxamlWorkspaceComponentSymbol> Components { get; }

    /// <summary>
    /// Gets the external controls visible to the project.
    /// </summary>
    public IReadOnlyList<ControlMetadataModel> ExternalControls { get; }

    /// <summary>
    /// Gets the current project metadata.
    /// </summary>
    public CsxamlProjectInfo Project { get; }

    /// <summary>
    /// Finds visible CSXAML components by namespace and component name.
    /// </summary>
    /// <param name="namespaceName">The component namespace to search.</param>
    /// <param name="name">The component name to search for.</param>
    /// <returns>The matching component symbols.</returns>
    public IReadOnlyList<CsxamlWorkspaceComponentSymbol> FindComponents(string namespaceName, string name)
    {
        return _componentsByQualifiedName.TryGetValue(GetQualifiedName(namespaceName, name), out var matches)
            ? matches
            : Array.Empty<CsxamlWorkspaceComponentSymbol>();
    }

    /// <summary>
    /// Finds visible external controls by namespace and control name.
    /// </summary>
    /// <param name="namespaceName">The control namespace to search.</param>
    /// <param name="name">The control type name to search for.</param>
    /// <returns>The matching external control metadata entries.</returns>
    public IReadOnlyList<ControlMetadataModel> FindExternalControls(string namespaceName, string name)
    {
        return _externalControlsByQualifiedName.TryGetValue(GetQualifiedName(namespaceName, name), out var matches)
            ? matches
            : Array.Empty<ControlMetadataModel>();
    }

    private static string GetNamespace(string clrTypeName)
    {
        var separatorIndex = clrTypeName.LastIndexOf('.');
        return separatorIndex < 0 ? string.Empty : clrTypeName[..separatorIndex];
    }

    private static string GetName(string clrTypeName)
    {
        var separatorIndex = clrTypeName.LastIndexOf('.');
        return separatorIndex < 0 ? clrTypeName : clrTypeName[(separatorIndex + 1)..];
    }

    private static string GetQualifiedName(string namespaceName, string name)
    {
        return $"{namespaceName}.{name}";
    }
}
