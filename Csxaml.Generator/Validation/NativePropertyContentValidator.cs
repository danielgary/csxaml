namespace Csxaml.Generator;

internal sealed class NativePropertyContentValidator
{
    public void Validate(
        SourceDocument source,
        MarkupNode node,
        ControlMetadataModel control)
    {
        var seenNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var propertyContent in node.PropertyContent)
        {
            PropertyContentNodeValidator.ValidateCommon(source, node, propertyContent);
            ValidateDuplicate(source, node, propertyContent, seenNames);
            ValidateAttributeCollision(source, node, propertyContent);
            ValidateTarget(source, node, control, propertyContent);
        }
    }

    private static void ValidateAttributeCollision(
        SourceDocument source,
        MarkupNode node,
        PropertyContentNode propertyContent)
    {
        if (node.Properties.All(property => property.Name != propertyContent.PropertyName))
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            propertyContent.Span,
            $"property '{propertyContent.PropertyName}' on native control '{node.TagName}' is assigned by both attribute and property content");
    }

    private static void ValidateDuplicate(
        SourceDocument source,
        MarkupNode node,
        PropertyContentNode propertyContent,
        HashSet<string> seenNames)
    {
        if (seenNames.Add(propertyContent.PropertyName))
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            propertyContent.Span,
            $"property content '{node.TagName}.{propertyContent.PropertyName}' is assigned more than once");
    }

    private static void ValidateTarget(
        SourceDocument source,
        MarkupNode node,
        ControlMetadataModel control,
        PropertyContentNode propertyContent)
    {
        if (!NativePropertyContentResolver.TryResolve(
                control,
                propertyContent.PropertyName,
                out var target))
        {
            throw DiagnosticFactory.FromSpan(
                source,
                propertyContent.Span,
                DiagnosticMessageFormatter.WithSuggestion(
                    $"unknown property content '{propertyContent.PropertyName}' on native control '{node.TagName}'",
                    propertyContent.PropertyName,
                    GetPropertyContentNames(control)));
        }

        ValidateChildCount(source, node, propertyContent, target);
    }

    private static void ValidateChildCount(
        SourceDocument source,
        MarkupNode node,
        PropertyContentNode propertyContent,
        NativePropertyContentTarget target)
    {
        if (target.Kind == ControlContentKind.None)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                propertyContent.Span,
                $"property content '{node.TagName}.{propertyContent.PropertyName}' has unsupported type '{target.PropertyTypeName ?? "unknown"}'");
        }

        if (target.Kind == ControlContentKind.Single && propertyContent.Children.Count > 1)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                propertyContent.Span,
                $"property content '{node.TagName}.{propertyContent.PropertyName}' supports only one child");
        }
    }

    private static IReadOnlyList<string> GetPropertyContentNames(ControlMetadataModel control)
    {
        return control.Properties
            .Where(IsPropertyContentCandidate)
            .Select(property => property.Name)
            .Concat(
                string.IsNullOrWhiteSpace(control.Content.DefaultPropertyName)
                    ? []
                    : [control.Content.DefaultPropertyName!])
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static bool IsPropertyContentCandidate(PropertyMetadata property)
    {
        return property.ValueKindHint == ValueKindHint.Object ||
            string.Equals(
                property.ClrTypeName,
                "Microsoft.UI.Xaml.Controls.UIElementCollection",
                StringComparison.Ordinal);
    }
}
