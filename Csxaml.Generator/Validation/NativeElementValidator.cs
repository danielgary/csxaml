namespace Csxaml.Generator;

internal sealed class NativeElementValidator
{
    private readonly AttachedPropertyValidator _attachedPropertyValidator = new();

    public void Validate(
        SourceDocument source,
        MarkupNode node,
        ControlMetadataModel control,
        string? parentTagName,
        AttachedPropertyBindingResolver bindingResolver)
    {
        ValidateAttributes(source, node, control, parentTagName, _attachedPropertyValidator, bindingResolver);
        ValidateChildren(source, node, control);
    }

    private static bool AllowsStringLiteral(PropertyMetadata property)
    {
        return property.ValueKindHint is ValueKindHint.Object or ValueKindHint.String;
    }

    private static void ValidateAttributes(
        SourceDocument source,
        MarkupNode node,
        ControlMetadataModel control,
        string? parentTagName,
        AttachedPropertyValidator attachedPropertyValidator,
        AttachedPropertyBindingResolver bindingResolver)
    {
        var seenNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in node.Properties)
        {
            if (!seenNames.Add(property.Name))
            {
                throw DiagnosticFactory.FromSpan(
                    source,
                    property.Span,
                    $"duplicate attribute name '{property.Name}' on native control '{node.TagName}'");
            }

            if (string.Equals(property.Name, "Key", StringComparison.Ordinal))
            {
                continue;
            }

            if (property.IsAttached)
            {
                attachedPropertyValidator.Validate(source, node, property, parentTagName, bindingResolver);
                continue;
            }

            var nativeEvent = control.Events.SingleOrDefault(
                eventMetadata => eventMetadata.ExposedName == property.Name);
            if (nativeEvent is not null)
            {
                ValidateEventValue(source, property, node);
                continue;
            }

            var nativeProperty = control.Properties.SingleOrDefault(
                propertyMetadata => propertyMetadata.Name == property.Name);
            if (nativeProperty is not null)
            {
                ValidatePropertyValue(source, property, node, nativeProperty);
                continue;
            }

            throw DiagnosticFactory.FromSpan(
                source,
                property.Span,
                DiagnosticMessageFormatter.WithSuggestion(
                    $"unknown attribute '{property.Name}' on native control '{node.TagName}'",
                    property.Name,
                    control.Properties.Select(candidate => candidate.Name)
                        .Concat(control.Events.Select(candidate => candidate.ExposedName))
                        .Concat(["Key"])
                        .Concat(AttachedPropertyMetadataRegistry.Properties.Select(candidate => candidate.QualifiedName))));
        }
    }

    private static void ValidateChildren(
        SourceDocument source,
        MarkupNode node,
        ControlMetadataModel control)
    {
        if (control.ChildKind == ControlChildKind.None && node.Children.Count > 0)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                node.Span,
                $"native control '{node.TagName}' does not support child content");
        }

        if (control.ChildKind == ControlChildKind.Single && node.Children.Count > 1)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                node.Span,
                $"native control '{node.TagName}' supports only one child");
        }
    }

    private static void ValidateEventValue(
        SourceDocument source,
        PropertyNode property,
        MarkupNode node)
    {
        if (property.ValueKind == PropertyValueKind.Expression)
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            property.Span,
            $"native event '{property.Name}' on '{node.TagName}' requires an expression value");
    }

    private static void ValidatePropertyValue(
        SourceDocument source,
        PropertyNode property,
        MarkupNode node,
        PropertyMetadata nativeProperty)
    {
        if (property.ValueKind == PropertyValueKind.Expression)
        {
            return;
        }

        if (AllowsStringLiteral(nativeProperty))
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            property.Span,
            $"native property '{property.Name}' on '{node.TagName}' requires an expression value");
    }
}
