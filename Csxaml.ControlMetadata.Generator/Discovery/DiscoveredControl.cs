using System.Reflection;

namespace Csxaml.ControlMetadata.Generator;

internal sealed record DiscoveredControl(
    string TagName,
    Type ClrType,
    string ClrTypeName,
    string? BaseTypeName,
    IReadOnlyDictionary<string, PropertyInfo> Properties,
    IReadOnlyDictionary<string, EventInfo> Events);
