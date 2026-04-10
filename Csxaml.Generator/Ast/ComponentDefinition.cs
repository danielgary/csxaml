namespace Csxaml.Generator;

internal sealed record ComponentDefinition(
    string Name,
    IReadOnlyList<ComponentParameter> Parameters,
    IReadOnlyList<StateFieldDefinition> StateFields,
    MarkupNode Root,
    TextSpan Span);
