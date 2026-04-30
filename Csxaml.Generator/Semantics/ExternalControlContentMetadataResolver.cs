using System.Reflection;

namespace Csxaml.Generator;

internal static class ExternalControlContentMetadataResolver
{
    private const string ContentPropertyAttributeTypeName =
        "Microsoft.UI.Xaml.Markup.ContentPropertyAttribute";

    public static ControlContentMetadata Resolve(Type controlType)
    {
        var contentPropertyName = ReadContentPropertyAttribute(controlType);
        if (contentPropertyName is not null)
        {
            return CreateFromProperty(
                controlType,
                contentPropertyName,
                ControlContentSource.ContentPropertyAttribute);
        }

        return ResolveConvention(controlType);
    }

    private static ControlContentMetadata ResolveConvention(Type controlType)
    {
        if (InheritsFrom(controlType, "Microsoft.UI.Xaml.Controls.Panel"))
        {
            return CreateFromProperty(controlType, "Children", ControlContentSource.Convention);
        }

        var children = CreateFromProperty(controlType, "Children", ControlContentSource.Convention);
        if (children.Kind == ControlContentKind.Collection)
        {
            return children;
        }

        if (InheritsFrom(controlType, "Microsoft.UI.Xaml.Controls.ContentControl"))
        {
            return CreateFromProperty(controlType, "Content", ControlContentSource.Convention);
        }

        if (InheritsFrom(controlType, "Microsoft.UI.Xaml.Controls.Border"))
        {
            return CreateFromProperty(controlType, "Child", ControlContentSource.Convention);
        }

        if (InheritsFrom(controlType, "Microsoft.UI.Xaml.Controls.ScrollViewer"))
        {
            return CreateFromProperty(controlType, "Content", ControlContentSource.Convention);
        }

        var child = CreateFromProperty(controlType, "Child", ControlContentSource.Convention);
        if (child.Kind == ControlContentKind.Single)
        {
            return child;
        }

        var content = CreateFromProperty(controlType, "Content", ControlContentSource.Convention);
        return content.Kind == ControlContentKind.Single ? content : ControlContentMetadata.None;
    }

    private static ControlContentMetadata CreateFromProperty(
        Type controlType,
        string propertyName,
        ControlContentSource source)
    {
        var property = controlType.GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);
        if (property is null)
        {
            return new ControlContentMetadata(
                propertyName,
                ControlContentKind.None,
                null,
                null,
                source);
        }

        if (IsUiElementCollection(property.PropertyType))
        {
            return new ControlContentMetadata(
                property.Name,
                ControlContentKind.Collection,
                FormatTypeName(property.PropertyType),
                "Microsoft.UI.Xaml.UIElement",
                source);
        }

        if (IsSingleChildProperty(property))
        {
            return new ControlContentMetadata(
                property.Name,
                ControlContentKind.Single,
                FormatTypeName(property.PropertyType),
                null,
                source);
        }

        return new ControlContentMetadata(
            property.Name,
            ControlContentKind.None,
            FormatTypeName(property.PropertyType),
            null,
            source);
    }

    private static bool IsSingleChildProperty(PropertyInfo property)
    {
        return property.SetMethod is not null &&
            (IsUiElement(property.PropertyType) ||
                string.Equals(property.PropertyType.FullName, typeof(object).FullName, StringComparison.Ordinal));
    }

    private static bool IsUiElementCollection(Type type)
    {
        return string.Equals(
            type.FullName,
            "Microsoft.UI.Xaml.Controls.UIElementCollection",
            StringComparison.Ordinal);
    }

    private static bool IsUiElement(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (string.Equals(current.FullName, "Microsoft.UI.Xaml.UIElement", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string? ReadContentPropertyAttribute(Type controlType)
    {
        foreach (var attribute in controlType.GetCustomAttributes(inherit: true))
        {
            if (!string.Equals(
                    attribute.GetType().FullName,
                    ContentPropertyAttributeTypeName,
                    StringComparison.Ordinal))
            {
                continue;
            }

            return attribute.GetType().GetProperty("Name")?.GetValue(attribute) as string;
        }

        return null;
    }

    private static bool InheritsFrom(Type type, string baseTypeName)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (string.Equals(current.FullName, baseTypeName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string FormatTypeName(Type type)
    {
        return type.FullName ?? type.Name;
    }
}
