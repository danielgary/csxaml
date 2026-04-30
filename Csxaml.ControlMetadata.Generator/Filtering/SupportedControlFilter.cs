using System.Reflection;

namespace Csxaml.ControlMetadata.Generator;

internal sealed class SupportedControlFilter
{
    public ControlMetadata BuildMetadata(
        CuratedControlDefinition definition,
        DiscoveredControl discoveredControl)
    {
        return new ControlMetadata(
            discoveredControl.TagName,
            discoveredControl.ClrTypeName,
            discoveredControl.BaseTypeName,
            definition.ChildKind,
            CuratedControlContentMetadataFactory.Create(definition, discoveredControl),
            BuildProperties(definition, discoveredControl),
            BuildEvents(definition, discoveredControl));
    }

    private static IReadOnlyList<EventMetadata> BuildEvents(
        CuratedControlDefinition definition,
        DiscoveredControl discoveredControl)
    {
        return definition.Events
            .OrderBy(eventDefinition => eventDefinition.ExposedName, StringComparer.Ordinal)
            .Select(eventDefinition => BuildEventMetadata(discoveredControl, eventDefinition))
            .ToList();
    }

    private static EventMetadata BuildEventMetadata(
        DiscoveredControl discoveredControl,
        CuratedEventDefinition eventDefinition)
    {
        foreach (var clrEventName in eventDefinition.RequiredClrEventNames)
        {
            if (discoveredControl.Events.ContainsKey(clrEventName))
            {
                continue;
            }

            throw new InvalidOperationException(
                $"Curated event '{clrEventName}' was not found on '{discoveredControl.TagName}'.");
        }

        return new EventMetadata(
            eventDefinition.ClrEventName,
            eventDefinition.ExposedName,
            eventDefinition.HandlerTypeName,
            true,
            eventDefinition.ValueKindHint,
            eventDefinition.BindingKind);
    }

    private static bool IsDependencyProperty(DiscoveredControl discoveredControl, string propertyName)
    {
        for (var currentType = discoveredControl.ClrType;
             currentType is not null;
             currentType = currentType.BaseType)
        {
            if (currentType.GetField(
                $"{propertyName}Property",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) is not null)
            {
                return true;
            }

            var dependencyProperty = currentType.GetProperty(
                $"{propertyName}Property",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (string.Equals(
                    dependencyProperty?.PropertyType.FullName,
                    "Microsoft.UI.Xaml.DependencyProperty",
                    StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyList<PropertyMetadata> BuildProperties(
        CuratedControlDefinition definition,
        DiscoveredControl discoveredControl)
    {
        return definition.PropertyNames
            .OrderBy(name => name, StringComparer.Ordinal)
            .Select(name => BuildPropertyMetadata(discoveredControl, name))
            .ToList();
    }

    private static PropertyMetadata BuildPropertyMetadata(
        DiscoveredControl discoveredControl,
        string propertyName)
    {
        if (!discoveredControl.Properties.TryGetValue(propertyName, out var property))
        {
            throw new InvalidOperationException(
                $"Curated property '{propertyName}' was not found on '{discoveredControl.TagName}'.");
        }

        return new PropertyMetadata(
            property.Name,
            property.PropertyType.FullName ?? property.PropertyType.Name,
            property.SetMethod is not null,
            IsDependencyProperty(discoveredControl, propertyName),
            false,
            true,
            ValueKindHintResolver.Resolve(property.PropertyType));
    }
}
