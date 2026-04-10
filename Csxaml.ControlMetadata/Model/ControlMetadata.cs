namespace Csxaml.ControlMetadata;

public sealed record ControlMetadata(
    string TagName,
    string ClrTypeName,
    string? BaseTypeName,
    ControlChildKind ChildKind,
    IReadOnlyList<PropertyMetadata> Properties,
    IReadOnlyList<EventMetadata> Events);
