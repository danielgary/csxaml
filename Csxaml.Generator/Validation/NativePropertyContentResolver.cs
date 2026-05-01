namespace Csxaml.Generator;

internal static class NativePropertyContentResolver
{
    private const string ObjectTypeName = "System.Object";
    private const string UiElementCollectionTypeName = "Microsoft.UI.Xaml.Controls.UIElementCollection";

    public static bool TryResolve(
        ControlMetadataModel control,
        string propertyName,
        out NativePropertyContentTarget target)
    {
        if (IsDefaultContentProperty(control.Content, propertyName))
        {
            target = new NativePropertyContentTarget(
                propertyName,
                control.Content.Kind,
                control.Content.PropertyTypeName);
            return true;
        }

        var property = control.Properties.SingleOrDefault(
            candidate => string.Equals(candidate.Name, propertyName, StringComparison.Ordinal));
        if (property is null)
        {
            target = null!;
            return false;
        }

        if (IsCollectionProperty(property))
        {
            target = new NativePropertyContentTarget(
                propertyName,
                ControlContentKind.Collection,
                property.ClrTypeName);
            return true;
        }

        if (IsSingleContentProperty(property))
        {
            target = new NativePropertyContentTarget(
                propertyName,
                ControlContentKind.Single,
                property.ClrTypeName);
            return true;
        }

        target = null!;
        return false;
    }

    private static bool IsDefaultContentProperty(
        ControlContentMetadata content,
        string propertyName)
    {
        return !string.IsNullOrWhiteSpace(content.DefaultPropertyName) &&
            string.Equals(content.DefaultPropertyName, propertyName, StringComparison.Ordinal);
    }

    private static bool IsCollectionProperty(PropertyMetadata property)
    {
        return string.Equals(property.ClrTypeName, UiElementCollectionTypeName, StringComparison.Ordinal);
    }

    private static bool IsSingleContentProperty(PropertyMetadata property)
    {
        return property.ValueKindHint == ValueKindHint.Object ||
            string.Equals(property.ClrTypeName, ObjectTypeName, StringComparison.Ordinal) ||
            IsUiElementType(property.ClrTypeName);
    }

    private static bool IsUiElementType(string typeName)
    {
        return string.Equals(typeName, "Microsoft.UI.Xaml.UIElement", StringComparison.Ordinal) ||
            typeName.EndsWith(".UIElement", StringComparison.Ordinal);
    }
}
