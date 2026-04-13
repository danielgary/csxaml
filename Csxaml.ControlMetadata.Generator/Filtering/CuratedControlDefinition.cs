namespace Csxaml.ControlMetadata.Generator;

internal sealed record CuratedControlDefinition(
    Type ControlType,
    ControlChildKind ChildKind,
    IReadOnlyList<string> PropertyNames,
    IReadOnlyList<CuratedEventDefinition> Events);
