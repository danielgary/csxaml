namespace Csxaml.ControlMetadata.Generator;

internal static class CuratedControlContentMetadataFactory
{
    public static ControlContentMetadata Create(
        CuratedControlDefinition definition,
        DiscoveredControl discoveredControl)
    {
        return definition.ChildKind switch
        {
            ControlChildKind.None => new(
                null,
                ControlContentKind.None,
                null,
                null,
                ControlContentSource.BuiltInMetadata),
            ControlChildKind.Single => CreateSingle(discoveredControl),
            ControlChildKind.Multiple => CreateCollection(discoveredControl),
            _ => throw new ArgumentOutOfRangeException(nameof(definition), definition.ChildKind, null)
        };
    }

    private static ControlContentMetadata CreateCollection(DiscoveredControl discoveredControl)
    {
        if (!discoveredControl.Properties.TryGetValue("Children", out var property))
        {
            return ControlContentMetadata.FromChildKind(ControlChildKind.Multiple);
        }

        return new ControlContentMetadata(
            property.Name,
            ControlContentKind.Collection,
            FormatTypeName(property.PropertyType),
            "Microsoft.UI.Xaml.UIElement",
            ControlContentSource.BuiltInMetadata);
    }

    private static ControlContentMetadata CreateSingle(DiscoveredControl discoveredControl)
    {
        if (!TryGetProperty(discoveredControl, ["Child", "Content"], out var property))
        {
            return ControlContentMetadata.FromChildKind(ControlChildKind.Single);
        }

        return new ControlContentMetadata(
            property.Name,
            ControlContentKind.Single,
            FormatTypeName(property.PropertyType),
            null,
            ControlContentSource.BuiltInMetadata);
    }

    private static bool TryGetProperty(
        DiscoveredControl discoveredControl,
        IReadOnlyList<string> propertyNames,
        out System.Reflection.PropertyInfo property)
    {
        foreach (var propertyName in propertyNames)
        {
            if (discoveredControl.Properties.TryGetValue(propertyName, out property!))
            {
                return true;
            }
        }

        property = null!;
        return false;
    }

    private static string FormatTypeName(Type type)
    {
        return type.FullName ?? type.Name;
    }
}
