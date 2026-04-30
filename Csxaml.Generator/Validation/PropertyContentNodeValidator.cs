namespace Csxaml.Generator;

internal static class PropertyContentNodeValidator
{
    public static void ValidateCommon(
        SourceDocument source,
        MarkupNode parent,
        PropertyContentNode propertyContent)
    {
        if (!string.Equals(propertyContent.OwnerName, parent.TagName, StringComparison.Ordinal))
        {
            throw DiagnosticFactory.FromSpan(
                source,
                propertyContent.Span,
                $"property content owner '{propertyContent.OwnerName}' does not match parent '{parent.TagName}'");
        }

        if (propertyContent.Ref is not null)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                propertyContent.Ref.Span,
                $"property content '{propertyContent.Name}' does not support Ref");
        }

        if (propertyContent.Properties.Count > 0)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                propertyContent.Properties[0].Span,
                $"property content '{propertyContent.Name}' does not support attributes");
        }
    }
}
