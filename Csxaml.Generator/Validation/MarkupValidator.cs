namespace Csxaml.Generator;

internal sealed class MarkupValidator
{
    private readonly ComponentTagValidator _componentTagValidator = new();
    private readonly NativeElementValidator _nativeElementValidator = new();

    public void Validate(
        SourceDocument source,
        MarkupNode node,
        ComponentCatalog catalog)
    {
        ValidateCurrentNode(source, node, catalog);
        foreach (var child in node.Children)
        {
            ValidateChildNode(source, child, catalog);
        }
    }

    private void ValidateChildNode(
        SourceDocument source,
        ChildNode childNode,
        ComponentCatalog catalog)
    {
        switch (childNode)
        {
            case ForEachBlockNode forEachBlock:
                foreach (var child in forEachBlock.Children)
                {
                    ValidateChildNode(source, child, catalog);
                }
                break;

            case IfBlockNode ifBlock:
                foreach (var child in ifBlock.Children)
                {
                    ValidateChildNode(source, child, catalog);
                }
                break;

            case MarkupNode markupNode:
                Validate(source, markupNode, catalog);
                break;
        }
    }

    private void ValidateCurrentNode(
        SourceDocument source,
        MarkupNode node,
        ComponentCatalog catalog)
    {
        if (ControlMetadataRegistry.TryGetControl(node.TagName, out var nativeControl))
        {
            _nativeElementValidator.Validate(source, node, nativeControl!);
            return;
        }

        if (catalog.Contains(node.TagName))
        {
            _componentTagValidator.Validate(source, node, catalog.GetComponent(node.TagName));
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            node.Span,
            $"unsupported tag name '{node.TagName}'");
    }
}
