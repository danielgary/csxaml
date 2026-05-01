using Csxaml.ControlMetadata;
using ControlMetadataModel = Csxaml.ControlMetadata.ControlMetadata;

namespace Csxaml.Tooling.Core.Completion;

internal static class CsxamlMarkupCompletionItemFactory
{
    public static CsxamlCompletionItem CreateAttachedPropertyItem(AttachedPropertyMetadata property)
    {
        var snippet = property.ValueKindHint == ValueKindHint.String
            ? $"{property.QualifiedName}=\"${{1}}\""
            : $"{property.QualifiedName}={{${{1}}}}";
        var detail = property.RequiredParentTagName is null
            ? property.ClrTypeName
            : $"{property.ClrTypeName} (children of <{property.RequiredParentTagName}>)";
        return new CsxamlCompletionItem(
            property.QualifiedName,
            CsxamlCompletionItemKind.Property,
            detail,
            snippet,
            $"1a-{property.QualifiedName}",
            IsSnippet: true);
    }

    public static CsxamlCompletionItem CreateComponentParameterItem(ComponentParameterMetadata parameter)
    {
        var snippet = IsStringType(parameter.TypeName)
            ? $"{parameter.Name}=\"${{1}}\""
            : $"{parameter.Name}={{${{1}}}}";
        return new CsxamlCompletionItem(
            parameter.Name,
            CsxamlCompletionItemKind.Parameter,
            parameter.TypeName,
            snippet,
            $"4-{parameter.Name}",
            IsSnippet: true);
    }

    public static CsxamlCompletionItem CreateEventItem(EventMetadata @event)
    {
        return new CsxamlCompletionItem(
            @event.ExposedName,
            CsxamlCompletionItemKind.Event,
            @event.HandlerTypeName,
            $"{@event.ExposedName}={{${{1}}}}",
            $"3-{@event.ExposedName}",
            IsSnippet: true);
    }

    public static CsxamlCompletionItem CreatePropertyItem(PropertyMetadata property)
    {
        var snippet = property.ValueKindHint == ValueKindHint.String
            ? $"{property.Name}=\"${{1}}\""
            : $"{property.Name}={{${{1}}}}";
        return new CsxamlCompletionItem(
            property.Name,
            CsxamlCompletionItemKind.Property,
            property.ClrTypeName,
            snippet,
            $"2-{property.Name}",
            IsSnippet: true);
    }

    public static CsxamlCompletionItem CreateRefItem(ControlMetadataModel control)
    {
        return new CsxamlCompletionItem(
            "Ref",
            CsxamlCompletionItemKind.Property,
            $"Csxaml.Runtime.ElementRef<{control.ClrTypeName}>",
            "Ref={${1}}",
            "1-Ref",
            IsSnippet: true);
    }

    public static CsxamlCompletionItem CreatePropertyContentItem(
        string ownerName,
        string propertyName,
        string detail)
    {
        return new CsxamlCompletionItem(
            propertyName,
            CsxamlCompletionItemKind.Property,
            detail,
            $"{propertyName}>$0</{ownerName}.{propertyName}>",
            $"0-property-content-{propertyName}",
            IsSnippet: true);
    }

    public static CsxamlCompletionItem CreateTagItem(
        string label,
        string detail,
        bool canHaveChildren)
    {
        return new CsxamlCompletionItem(
            label,
            CsxamlCompletionItemKind.Class,
            detail,
            canHaveChildren
                ? $"{label}>$0</{label}>"
                : $"{label} />",
            $"1-{label}",
            IsSnippet: true);
    }

    private static bool IsStringType(string typeName)
    {
        return string.Equals(typeName, "string", StringComparison.OrdinalIgnoreCase)
            || string.Equals(typeName, "System.String", StringComparison.Ordinal);
    }
}
