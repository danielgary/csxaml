namespace Csxaml.Generator;

internal static class ExternalControlCandidateCollector
{
    public static IReadOnlyList<string> Collect(IReadOnlyList<ParsedComponent> components)
    {
        var candidates = new HashSet<string>(StringComparer.Ordinal);
        foreach (var component in components)
        {
            var imports = ImportScope.Create(component.Source, component.File.UsingDirectives);
            Collect(component.Definition.Root, imports, candidates);
        }

        return candidates.OrderBy(name => name, StringComparer.Ordinal).ToList();
    }

    private static void Collect(
        ChildNode node,
        ImportScope imports,
        ISet<string> candidates)
    {
        if (node is not MarkupNode markupNode)
        {
            CollectBlock(GetChildren(node), imports, candidates);
            return;
        }

        if (markupNode.Tag.Prefix is not null)
        {
            if (imports.TryGetAliasNamespace(markupNode.Tag.Prefix, out var namespaceName))
            {
                candidates.Add($"{namespaceName}.{markupNode.Tag.LocalName}");
            }
        }
        else if (!ControlMetadataRegistry.IsNativeTag(markupNode.TagName))
        {
            foreach (var namespaceName in imports.ImportedNamespaces)
            {
                candidates.Add($"{namespaceName}.{markupNode.Tag.LocalName}");
            }
        }

        foreach (var child in markupNode.Children)
        {
            Collect(child, imports, candidates);
        }
    }

    private static void CollectBlock(
        IReadOnlyList<ChildNode> children,
        ImportScope imports,
        ISet<string> candidates)
    {
        foreach (var child in children)
        {
            Collect(child, imports, candidates);
        }
    }

    private static IReadOnlyList<ChildNode> GetChildren(ChildNode node)
    {
        return node switch
        {
            IfBlockNode ifBlock => ifBlock.Children,
            ForEachBlockNode forEachBlock => forEachBlock.Children,
            _ => Array.Empty<ChildNode>()
        };
    }
}
