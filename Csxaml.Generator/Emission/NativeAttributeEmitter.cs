using System.Globalization;

namespace Csxaml.Generator;

internal sealed class NativeAttributeEmitter
{
    private readonly string _componentName;
    private readonly SourceDocument _source;

    public NativeAttributeEmitter(SourceDocument source, string componentName)
    {
        _source = source;
        _componentName = componentName;
    }

    public string BuildEventsExpression(MarkupNode markupNode, ControlMetadataModel control)
    {
        var events = markupNode.Properties
            .Where(property => !property.IsAttached)
            .Where(property => control.Events.Any(eventMetadata => eventMetadata.ExposedName == property.Name))
            .Select(property => BuildEventValue(control, property))
            .ToList();

        return FormatArray("NativeEventValue", events);
    }

    public string BuildKeyExpression(MarkupNode markupNode)
    {
        var key = markupNode.Properties.SingleOrDefault(property => property.Name == "Key");
        return key is null ? "null" : FormatArgumentValue(key);
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

        return FormatArray("NativePropertyValue", properties);
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

        return FormatArray("NativeAttachedPropertyValue", properties);
    }

    private string BuildEventValue(ControlMetadataModel control, PropertyNode property)
    {
        var eventMetadata = control.Events.Single(
            metadata => metadata.ExposedName == property.Name);

        return
            $$"""
            new NativeEventValue(
                "{{eventMetadata.ExposedName}}",
                ({{FormatHandlerType(eventMetadata.HandlerTypeName)}})(
            {{CodeBlockFormatter.Indent(FormatExpressionValue(property), 8)}}
                    ),
                {{FormatValueKindHint(eventMetadata.ValueKindHint)}},
                {{FormatSourceInfo(property, property.Name)}})
            """;
    }

    private string BuildPropertyValue(ControlMetadataModel control, PropertyNode property)
    {
        var propertyMetadata = control.Properties.Single(
            metadata => metadata.Name == property.Name);
        if (property.ValueKind == PropertyValueKind.StringLiteral)
        {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"new NativePropertyValue(\"{propertyMetadata.Name}\", {FormatArgumentValue(property)}, {FormatValueKindHint(propertyMetadata.ValueKindHint)}, {FormatSourceInfo(property, property.Name)})");
        }

        return
            $$"""
            new NativePropertyValue(
                "{{propertyMetadata.Name}}",
            {{CodeBlockFormatter.Indent(FormatExpressionValue(property), 8)}}
                ,
                {{FormatValueKindHint(propertyMetadata.ValueKindHint)}},
                {{FormatSourceInfo(property, property.Name)}})
            """;
    }

    private string BuildAttachedPropertyValue(PropertyNode property)
    {
        var metadata = AttachedPropertyMetadataRegistry.GetProperty(
            property.OwnerName!,
            property.PropertyName);

        if (property.ValueKind == PropertyValueKind.StringLiteral)
        {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"new NativeAttachedPropertyValue(\"{metadata.OwnerName}\", \"{metadata.PropertyName}\", {FormatArgumentValue(property)}, {FormatValueKindHint(metadata.ValueKindHint)}, {FormatSourceInfo(property, property.Name)})");
        }

        return
            $$"""
            new NativeAttachedPropertyValue(
                "{{metadata.OwnerName}}",
                "{{metadata.PropertyName}}",
            {{CodeBlockFormatter.Indent(FormatExpressionValue(property), 8)}}
                ,
                {{FormatValueKindHint(metadata.ValueKindHint)}},
                {{FormatSourceInfo(property, property.Name)}})
            """;
    }

    private static string FormatArray(string elementTypeName, IReadOnlyList<string> entries)
    {
        if (entries.Count == 0)
        {
            return $"Array.Empty<{elementTypeName}>()";
        }

        return
            $$"""
            new {{elementTypeName}}[]
            {
            {{string.Join(Environment.NewLine, entries.Select(entry => CodeBlockFormatter.FormatListItem(entry, 4)))}}
            }
            """;
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

    private static string FormatArgumentValue(PropertyNode property)
    {
        return property.ValueKind == PropertyValueKind.StringLiteral
            ? $"\"{EscapeString(property.ValueText)}\""
            : property.ValueText;
    }

    private string FormatExpressionValue(PropertyNode property)
    {
        return LineDirectiveFormatter.Wrap(_source, property.ValueSpan, property.ValueText);
    }

    private string FormatSourceInfo(PropertyNode property, string memberName)
    {
        return SourceInfoEmitter.Emit(
            _source,
            _componentName,
            property.Span,
            memberName: memberName);
    }
}
