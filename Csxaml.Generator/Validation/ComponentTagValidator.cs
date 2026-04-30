namespace Csxaml.Generator;

internal sealed class ComponentTagValidator
{
    private readonly AttachedPropertyValidator _attachedPropertyValidator = new();

    public void Validate(
        SourceDocument source,
        MarkupNode node,
        ComponentCatalogEntry component,
        string? parentTagName,
        AttachedPropertyBindingResolver bindingResolver)
    {
        ValidateRenderableComponent(source, node, component);

        if (node.Children.Count > 0 && !component.SupportsDefaultSlot)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                node.Span,
                $"component '{node.TagName}' does not support child content");
        }

        if (node.Ref is not null)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                node.Ref.Span,
                $"Ref is not supported on component '{node.TagName}'");
        }

        ValidatePropertyContent(source, node, component);
        var seenAttributes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in node.Properties)
        {
            if (!seenAttributes.Add(property.Name))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    property.Span,
                    $"duplicate attribute name '{property.Name}' on component '{node.TagName}'");
            }

            if (string.Equals(property.Name, "Key", StringComparison.Ordinal))
            {
                continue;
            }

            if (property.IsAttached)
            {
                _attachedPropertyValidator.Validate(source, node, property, parentTagName, bindingResolver);
                continue;
            }

            if (component.Parameters.All(parameter => parameter.Name != property.Name))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    property.Span,
                    DiagnosticMessageFormatter.WithSuggestion(
                        $"prop validation failure: unsupported prop name '{property.Name}' on component '{node.TagName}'",
                        property.Name,
                        component.Parameters.Select(parameter => parameter.Name)));
            }
        }

        foreach (var parameter in component.Parameters)
        {
            if (node.Properties.All(property => property.Name != parameter.Name))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    node.Span,
                $"prop validation failure: missing required prop '{parameter.Name}' on component '{node.TagName}'");
            }
        }
    }

    private static void ValidateRenderableComponent(
        SourceDocument source,
        MarkupNode node,
        ComponentCatalogEntry component)
    {
        if (component.Kind is Csxaml.ControlMetadata.ComponentKind.Element or Csxaml.ControlMetadata.ComponentKind.Page)
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            node.Span,
            $"component kind '{component.Kind}' cannot be rendered as child content");
    }

    private static void ValidatePropertyContent(
        SourceDocument source,
        MarkupNode node,
        ComponentCatalogEntry component)
    {
        var seenSlots = new HashSet<string>(StringComparer.Ordinal);
        foreach (var propertyContent in node.PropertyContent)
        {
            PropertyContentNodeValidator.ValidateCommon(source, node, propertyContent);
            ValidateNamedSlot(source, node, component, propertyContent, seenSlots);
        }
    }

    private static void ValidateNamedSlot(
        SourceDocument source,
        MarkupNode node,
        ComponentCatalogEntry component,
        PropertyContentNode propertyContent,
        HashSet<string> seenSlots)
    {
        if (component.Parameters.Any(parameter => parameter.Name == propertyContent.PropertyName))
        {
            throw DiagnosticFactory.FromSpan(
                source,
                propertyContent.Span,
                $"named slot '{propertyContent.PropertyName}' on component '{node.TagName}' collides with a prop assignment");
        }

        if (component.NamedSlots.All(slot => slot.Name != propertyContent.PropertyName))
        {
            throw DiagnosticFactory.FromSpan(
                source,
                propertyContent.Span,
                DiagnosticMessageFormatter.WithSuggestion(
                    $"unknown named slot '{propertyContent.PropertyName}' on component '{node.TagName}'",
                    propertyContent.PropertyName,
                    component.NamedSlots.Select(slot => slot.Name)));
        }

        if (seenSlots.Add(propertyContent.PropertyName))
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            propertyContent.Span,
            $"named slot '{propertyContent.PropertyName}' on component '{node.TagName}' is assigned more than once");
    }
}
