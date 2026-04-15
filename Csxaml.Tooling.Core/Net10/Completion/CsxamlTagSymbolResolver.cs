using Csxaml.ControlMetadata;
using Csxaml.Tooling.Core.Markup;
using Csxaml.Tooling.Core.Projects;
using ControlMetadataModel = Csxaml.ControlMetadata.ControlMetadata;

namespace Csxaml.Tooling.Core.Completion;

public sealed class CsxamlTagSymbolResolver
{
    public CsxamlResolvedTag Resolve(
        string tagName,
        IReadOnlyList<CsxamlUsingDirectiveInfo> usingDirectives,
        string currentNamespace,
        CsxamlWorkspaceSnapshot workspace)
    {
        var separatorIndex = tagName.IndexOf(':');
        return separatorIndex >= 0
            ? ResolveQualified(tagName[..separatorIndex], tagName[(separatorIndex + 1)..], usingDirectives, workspace)
            : ResolveSimple(tagName, usingDirectives, currentNamespace, workspace);
    }

    private static CsxamlResolvedTag ResolveQualified(
        string alias,
        string localName,
        IReadOnlyList<CsxamlUsingDirectiveInfo> usingDirectives,
        CsxamlWorkspaceSnapshot workspace)
    {
        var namespaceName = usingDirectives
            .FirstOrDefault(directive => string.Equals(directive.Alias, alias, StringComparison.Ordinal))?
            .NamespaceName;
        if (namespaceName is null)
        {
            return new CsxamlResolvedTag(CsxamlResolvedTagKind.None, $"{alias}:{localName}", null, null);
        }

        var components = workspace.FindComponents(namespaceName, localName);
        var externalControls = workspace.FindExternalControls(namespaceName, localName);
        return CreateResolvedTag($"{alias}:{localName}", components, externalControls);
    }

    private static CsxamlResolvedTag ResolveSimple(
        string localName,
        IReadOnlyList<CsxamlUsingDirectiveInfo> usingDirectives,
        string currentNamespace,
        CsxamlWorkspaceSnapshot workspace)
    {
        if (ControlMetadataRegistry.TryGetControl(localName, out var builtIn))
        {
            return new CsxamlResolvedTag(CsxamlResolvedTagKind.Native, localName, builtIn, null);
        }

        var components = new List<CsxamlWorkspaceComponentSymbol>();
        components.AddRange(workspace.FindComponents(currentNamespace, localName));
        foreach (var directive in usingDirectives.Where(directive => directive.Alias is null))
        {
            components.AddRange(workspace.FindComponents(directive.NamespaceName, localName));
        }

        var externalControls = new List<ControlMetadataModel>();
        foreach (var directive in usingDirectives.Where(directive => directive.Alias is null))
        {
            externalControls.AddRange(workspace.FindExternalControls(directive.NamespaceName, localName));
        }

        return CreateResolvedTag(localName, components, externalControls);
    }

    private static CsxamlResolvedTag CreateResolvedTag(
        string tagName,
        IReadOnlyList<CsxamlWorkspaceComponentSymbol> components,
        IReadOnlyList<ControlMetadataModel> externalControls)
    {
        if (components.Count == 1 && externalControls.Count == 0)
        {
            return new CsxamlResolvedTag(CsxamlResolvedTagKind.Component, tagName, null, components[0]);
        }

        if (components.Count == 0 && externalControls.Count == 1)
        {
            return new CsxamlResolvedTag(CsxamlResolvedTagKind.External, tagName, externalControls[0], null);
        }

        return components.Count + externalControls.Count == 0
            ? new CsxamlResolvedTag(CsxamlResolvedTagKind.None, tagName, null, null)
            : new CsxamlResolvedTag(CsxamlResolvedTagKind.Ambiguous, tagName, null, null);
    }
}
