using System.Globalization;

namespace Csxaml.Generator;

internal sealed class NativeAttributeEmitter
{
    public string BuildEventsExpression(MarkupNode markupNode, ControlMetadataModel control)
    {
        var events = markupNode.Properties
            .Where(property => !property.IsAttached)
            .Where(property => control.Events.Any(eventMetadata => eventMetadata.ExposedName == property.Name))
            .Select(property => BuildEventValue(control, property))
            .ToList();

        return events.Count == 0
            ? "Array.Empty<NativeEventValue>()"
            : $"new NativeEventValue[] {{ {string.Join(", ", events)} }}";
    }

    public string BuildKeyExpression(MarkupNode markupNode)
    {
        var key = markupNode.Properties.SingleOrDefault(property => property.Name == "Key");
        return key is null ? "null" : FormatValue(key);
    }

    public string BuildPropertiesExpression(MarkupNode markupNode, ControlMetadataModel control)
    {
        var properties = markupNode.Properties
            .Where(property =>
                !property.IsAttached &&
                !string.Equals(property.Name, "Key", StringComparison.Ordinal) &&
                control.Properties.Any(propertyMetadata => propertyMetadata.Name == property.Name))
            .Select(property => BuildPropertyValue(control, property))
            .ToList();

        return properties.Count == 0
            ? "Array.Empty<NativePropertyValue>()"
            : $"new NativePropertyValue[] {{ {string.Join(", ", properties)} }}";
    }

    public bool HasAttachedProperties(MarkupNode markupNode)
    {
        return markupNode.Properties.Any(property => property.IsAttached);
    }

    public string BuildAttachedPropertiesExpression(MarkupNode markupNode)
    {
        var properties = markupNode.Properties
            .Where(property => property.IsAttached)
            .Select(BuildAttachedPropertyValue)
            .ToList();

        return properties.Count == 0
            ? "Array.Empty<NativeAttachedPropertyValue>()"
            : $"new NativeAttachedPropertyValue[] {{ {string.Join(", ", properties)} }}";
    }

    private static string BuildEventValue(ControlMetadataModel control, PropertyNode property)
    {
        var eventMetadata = control.Events.Single(
            metadata => metadata.ExposedName == property.Name);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"new NativeEventValue(\"{eventMetadata.ExposedName}\", ({FormatHandlerType(eventMetadata.HandlerTypeName)})({FormatValue(property)}), {FormatValueKindHint(eventMetadata.ValueKindHint)})");
    }

    private static string BuildPropertyValue(ControlMetadataModel control, PropertyNode property)
    {
        var propertyMetadata = control.Properties.Single(
            metadata => metadata.Name == property.Name);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"new NativePropertyValue(\"{propertyMetadata.Name}\", {FormatValue(property)}, {FormatValueKindHint(propertyMetadata.ValueKindHint)})");
    }

    private static string BuildAttachedPropertyValue(PropertyNode property)
    {
        var metadata = AttachedPropertyMetadataRegistry.GetProperty(
            property.OwnerName!,
            property.PropertyName);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"new NativeAttachedPropertyValue(\"{metadata.OwnerName}\", \"{metadata.PropertyName}\", {FormatValue(property)}, {FormatValueKindHint(metadata.ValueKindHint)})");
    }

    private static string FormatValueKindHint(ValueKindHint hint)
    {
        return $"global::Csxaml.ControlMetadata.ValueKindHint.{hint}";
    }

    private static string FormatHandlerType(string handlerTypeName)
    {
        return handlerTypeName.StartsWith("global::", StringComparison.Ordinal)
            ? handlerTypeName
            : $"global::{handlerTypeName}";
    }

    private static string EscapeString(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private static string FormatValue(PropertyNode property)
    {
        return property.ValueKind == PropertyValueKind.StringLiteral
            ? $"\"{EscapeString(property.ValueText)}\""
            : property.ValueText;
    }
}
