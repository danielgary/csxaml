namespace Csxaml.Generator;

internal sealed class AttachedPropertyValidator
{
    public void Validate(
        SourceDocument source,
        MarkupNode node,
        PropertyNode property,
        string? parentTagName,
        AttachedPropertyBindingResolver bindingResolver)
    {
        var metadata = bindingResolver.ResolveOrThrow(source, node.TagName, property);
        ValidateValue(source, node, property, metadata);
        ValidateParent(source, node, property, metadata, parentTagName);
    }

    private static void ValidateParent(
        SourceDocument source,
        MarkupNode node,
        PropertyNode property,
        AttachedPropertyMetadata metadata,
        string? parentTagName)
    {
        if (metadata.RequiredParentTagName is null)
        {
            return;
        }

        if (string.Equals(parentTagName, metadata.RequiredParentTagName, StringComparison.Ordinal))
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            property.Span,
            $"attached property '{property.Name}' on '{node.TagName}' requires parent '{metadata.RequiredParentTagName}'");
    }

    private static void ValidateValue(
        SourceDocument source,
        MarkupNode node,
        PropertyNode property,
        AttachedPropertyMetadata metadata)
    {
        if (property.ValueKind == PropertyValueKind.Expression)
        {
            return;
        }

        if (metadata.ValueKindHint is ValueKindHint.Object or ValueKindHint.String)
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            property.Span,
            $"attached property '{property.Name}' on '{node.TagName}' requires an expression value");
    }
}
