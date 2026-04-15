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
            .FirstOrDefault(directive => !directive.IsStatic && string.Equals(directive.Alias, alias, StringComparison.Ordinal))?
            .QualifiedName;
        if (namespaceName is null)
        {
            return new CsxamlResolvedTag(CsxamlResolvedTagKind.None, $"{alias}:{localName}", null, null);
        }

        var components = workspace.FindComponents(namespaceName, localName);
        var externalControls = workspace.FindExternalControls(namespaceName, localName);
        return CreateResolvedTag($"{alias}:{localName}", null, components, externalControls);
    }

    private static CsxamlResolvedTag ResolveSimple(
        string localName,
        IReadOnlyList<CsxamlUsingDirectiveInfo> usingDirectives,
        string currentNamespace,
        CsxamlWorkspaceSnapshot workspace)
    {
        ControlMetadataRegistry.TryGetControl(localName, out var builtIn);

        var components = new List<CsxamlWorkspaceComponentSymbol>();
        components.AddRange(workspace.FindComponents(currentNamespace, localName));
        foreach (var directive in usingDirectives.Where(directive => directive.Alias is null && !directive.IsStatic))
        {
            components.AddRange(workspace.FindComponents(directive.QualifiedName, localName));
        }

        var externalControls = new List<ControlMetadataModel>();
        foreach (var directive in usingDirectives.Where(directive => directive.Alias is null && !directive.IsStatic))
        {
            externalControls.AddRange(workspace.FindExternalControls(directive.QualifiedName, localName));
        }

        return CreateResolvedTag(localName, builtIn, components, externalControls);
    }

    private static CsxamlResolvedTag CreateResolvedTag(
        string tagName,
        ControlMetadataModel? builtIn,
        IReadOnlyList<CsxamlWorkspaceComponentSymbol> components,
        IReadOnlyList<ControlMetadataModel> externalControls)
    {
        var supportedCount = components.Count + externalControls.Count + (builtIn is null ? 0 : 1);
        if (supportedCount == 0)
        {
            return new CsxamlResolvedTag(CsxamlResolvedTagKind.None, tagName, null, null);
        }

        if (supportedCount > 1)
        {
            return new CsxamlResolvedTag(CsxamlResolvedTagKind.Ambiguous, tagName, null, null);
        }

        if (components.Count == 1 && externalControls.Count == 0)
        {
            return new CsxamlResolvedTag(CsxamlResolvedTagKind.Component, tagName, null, components[0]);
        }

        if (builtIn is not null)
        {
            return new CsxamlResolvedTag(CsxamlResolvedTagKind.Native, tagName, builtIn, null);
        }

        if (components.Count == 0 && externalControls.Count == 1)
        {
            return new CsxamlResolvedTag(CsxamlResolvedTagKind.External, tagName, externalControls[0], null);
        }

        return new CsxamlResolvedTag(CsxamlResolvedTagKind.None, tagName, null, null);
    }
}
