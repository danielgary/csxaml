using Csxaml.ControlMetadata;
using ControlMetadataModel = Csxaml.ControlMetadata.ControlMetadata;

namespace Csxaml.Tooling.Core.Projects;

public sealed class CsxamlWorkspaceSnapshot
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<CsxamlWorkspaceComponentSymbol>> _componentsByQualifiedName;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<ControlMetadataModel>> _externalControlsByQualifiedName;

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

    public IReadOnlyList<CsxamlWorkspaceComponentSymbol> Components { get; }

    public IReadOnlyList<ControlMetadataModel> ExternalControls { get; }

    public CsxamlProjectInfo Project { get; }

    public IReadOnlyList<CsxamlWorkspaceComponentSymbol> FindComponents(string namespaceName, string name)
    {
        return _componentsByQualifiedName.TryGetValue(GetQualifiedName(namespaceName, name), out var matches)
            ? matches
            : Array.Empty<CsxamlWorkspaceComponentSymbol>();
    }

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
