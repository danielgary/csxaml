namespace Csxaml.Generator;

internal sealed class ComponentTagValidator
{
    private readonly AttachedPropertyValidator _attachedPropertyValidator = new();

    public void Validate(
        SourceDocument source,
        MarkupNode node,
        ComponentCatalogEntry component,
        string? parentTagName)
    {
        if (node.Children.Count > 0 && !component.SupportsDefaultSlot)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                node.Span,
                $"component '{node.TagName}' does not support child content");
        }

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
                _attachedPropertyValidator.Validate(source, node, property, parentTagName);
                continue;
            }

            if (component.Parameters.All(parameter => parameter.Name != property.Name))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    property.Span,
                    $"prop validation failure: unsupported prop name '{property.Name}' on component '{node.TagName}'");
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
}
