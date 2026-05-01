namespace Csxaml.Generator;

internal sealed class MarkupValidator
{
    private readonly ComponentTagValidator _componentTagValidator = new();
    private readonly MarkupTagResolver _tagResolver = new();
    private readonly NativeElementValidator _nativeElementValidator = new();

    public void Validate(
        SourceDocument source,
        ParsedComponent component,
        MarkupNode node,
        CompilationContext compilation)
    {
        ValidateRoot(source, node);
        var bindingResolver = new AttachedPropertyBindingResolver(component);
        Validate(source, component, node, compilation, null, bindingResolver);
    }

    private void Validate(
        SourceDocument source,
        ParsedComponent component,
        MarkupNode node,
        CompilationContext compilation,
        string? parentTagName,
        AttachedPropertyBindingResolver bindingResolver)
    {
        ValidateCurrentNode(source, component, node, compilation, parentTagName, bindingResolver);
        foreach (var child in node.Children)
        {
            ValidateChildNode(source, component, child, compilation, node.TagName, bindingResolver);
        }

        foreach (var propertyContent in node.PropertyContent)
        {
            foreach (var child in propertyContent.Children)
            {
                ValidateChildNode(source, component, child, compilation, node.TagName, bindingResolver);
            }
        }
    }

    private void ValidateChildNode(
        SourceDocument source,
        ParsedComponent component,
        ChildNode childNode,
        CompilationContext compilation,
        string? parentTagName,
        AttachedPropertyBindingResolver bindingResolver)
    {
        switch (childNode)
        {
            case ForEachBlockNode forEachBlock:
                foreach (var child in forEachBlock.Children)
                {
                    ValidateChildNode(source, component, child, compilation, parentTagName, bindingResolver);
                }
                break;

            case IfBlockNode ifBlock:
                foreach (var child in ifBlock.Children)
                {
                    ValidateChildNode(source, component, child, compilation, parentTagName, bindingResolver);
                }
                break;

            case MarkupNode markupNode:
                Validate(source, component, markupNode, compilation, parentTagName, bindingResolver);
                break;

            case SlotOutletNode:
                break;
        }
    }

    private static void ValidateRoot(SourceDocument source, MarkupNode node)
    {
        if (!PropertyContentName.TrySplit(node.TagName, out _, out _))
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            node.Span,
            "property content cannot be the component root");
    }

    private void ValidateCurrentNode(
        SourceDocument source,
        ParsedComponent component,
        MarkupNode node,
        CompilationContext compilation,
        string? parentTagName,
        AttachedPropertyBindingResolver bindingResolver)
    {
        var resolvedTag = _tagResolver.Resolve(source, component, node, compilation);
        if (resolvedTag.Kind == ResolvedTagKind.Native)
        {
            _nativeElementValidator.Validate(source, node, resolvedTag.NativeControl!, parentTagName, bindingResolver);
            return;
        }

        _componentTagValidator.Validate(source, node, resolvedTag.Component!, parentTagName, bindingResolver);
    }
}
