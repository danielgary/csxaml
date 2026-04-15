namespace Csxaml.Generator;

internal sealed record ComponentDefinition(
    string Name,
    IReadOnlyList<ComponentParameter> Parameters,
    IReadOnlyList<InjectFieldDefinition> InjectFields,
    IReadOnlyList<StateFieldDefinition> StateFields,
    ComponentHelperCodeBlock? HelperCode,
    ChildNode Root,
    bool SupportsDefaultSlot,
    TextSpan Span);
