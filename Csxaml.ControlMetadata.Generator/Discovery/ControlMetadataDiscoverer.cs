using System.Reflection;

namespace Csxaml.ControlMetadata.Generator;

internal sealed class ControlMetadataDiscoverer
{
    public DiscoveredControl Discover(Type controlType)
    {
        return new DiscoveredControl(
            controlType.Name,
            controlType,
            controlType.FullName ?? controlType.Name,
            controlType.BaseType?.FullName,
            controlType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.GetIndexParameters().Length == 0)
                .ToDictionary(property => property.Name, StringComparer.Ordinal),
            controlType
                .GetEvents(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(eventInfo => eventInfo.Name, StringComparer.Ordinal));
    }
}
