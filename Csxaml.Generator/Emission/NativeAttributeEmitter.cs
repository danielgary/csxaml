using System.Globalization;

namespace Csxaml.Generator;

internal sealed class NativeAttributeEmitter
{
    public string BuildEventsExpression(MarkupNode markupNode)
    {
        var control = ControlMetadataRegistry.GetControl(markupNode.TagName);
        var events = markupNode.Properties
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

    public string BuildPropertiesExpression(MarkupNode markupNode)
    {
        var control = ControlMetadataRegistry.GetControl(markupNode.TagName);
        var properties = markupNode.Properties
            .Where(property =>
                !string.Equals(property.Name, "Key", StringComparison.Ordinal) &&
                control.Properties.Any(propertyMetadata => propertyMetadata.Name == property.Name))
            .Select(property => BuildPropertyValue(control, property))
            .ToList();

        return properties.Count == 0
            ? "Array.Empty<NativePropertyValue>()"
            : $"new NativePropertyValue[] {{ {string.Join(", ", properties)} }}";
    }

    private static string BuildEventValue(ControlMetadataModel control, PropertyNode property)
    {
        var eventMetadata = control.Events.Single(
            metadata => metadata.ExposedName == property.Name);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"new NativeEventValue(\"{eventMetadata.ExposedName}\", {FormatValue(property)}, {FormatValueKindHint(eventMetadata.ValueKindHint)})");
    }

    private static string BuildPropertyValue(ControlMetadataModel control, PropertyNode property)
    {
        var propertyMetadata = control.Properties.Single(
            metadata => metadata.Name == property.Name);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"new NativePropertyValue(\"{propertyMetadata.Name}\", {FormatValue(property)}, {FormatValueKindHint(propertyMetadata.ValueKindHint)})");
    }

    private static string FormatValueKindHint(ValueKindHint hint)
    {
        return $"global::Csxaml.ControlMetadata.ValueKindHint.{hint}";
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
