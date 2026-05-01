namespace Csxaml.Generator;

internal sealed class ResourceDictionaryRootValidator
{
    public void Validate(SourceDocument source, ComponentDefinition definition)
    {
        if (definition.Root is not MarkupNode root ||
            !string.Equals(root.Tag.LocalName, "ResourceDictionary", StringComparison.Ordinal))
        {
            throw DiagnosticFactory.FromSpan(
                source,
                definition.Root.Span,
                "component ResourceDictionary must render a ResourceDictionary root");
        }

        foreach (var propertyContent in root.PropertyContent)
        {
            ValidatePropertyContent(source, propertyContent);
        }

        if (root.Children.Count > 0)
        {
            throw DiagnosticFactory.FromSpan(
                source,
                root.Span,
                "component ResourceDictionary supports only ResourceDictionary.MergedDictionaries content in this phase");
        }
    }

    private static void ValidatePropertyContent(
        SourceDocument source,
        PropertyContentNode propertyContent)
    {
        if (string.Equals(propertyContent.OwnerName, "ResourceDictionary", StringComparison.Ordinal) &&
            string.Equals(propertyContent.PropertyName, "MergedDictionaries", StringComparison.Ordinal))
        {
            return;
        }

        throw DiagnosticFactory.FromSpan(
            source,
            propertyContent.Span,
            $"unsupported ResourceDictionary property content '{propertyContent.OwnerName}.{propertyContent.PropertyName}'");
    }
}
