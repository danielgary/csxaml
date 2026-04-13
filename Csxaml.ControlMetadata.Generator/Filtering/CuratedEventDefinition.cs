namespace Csxaml.ControlMetadata.Generator;

internal sealed record CuratedEventDefinition(
    IReadOnlyList<string> RequiredClrEventNames,
    string? ClrEventName,
    string ExposedName,
    string HandlerTypeName,
    ValueKindHint ValueKindHint,
    EventBindingKind BindingKind);
