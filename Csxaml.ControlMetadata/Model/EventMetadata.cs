namespace Csxaml.ControlMetadata;

public sealed record EventMetadata(
    string? ClrEventName,
    string ExposedName,
    string HandlerTypeName,
    bool ExposedInCsxaml,
    ValueKindHint ValueKindHint,
    EventBindingKind BindingKind);
