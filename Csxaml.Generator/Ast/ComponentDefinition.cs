namespace Csxaml.Generator;

internal sealed record ComponentDefinition(
    ComponentKind Kind,
    string Name,
    IReadOnlyList<ComponentParameter> Parameters,
    IReadOnlyList<InjectFieldDefinition> InjectFields,
    IReadOnlyList<StateFieldDefinition> StateFields,
    IReadOnlyList<RootPropertyDeclaration> RootProperties,
    ApplicationRootDeclaration? Application,
    ComponentHelperCodeBlock? HelperCode,
    ChildNode Root,
    bool SupportsDefaultSlot,
    IReadOnlyList<string> NamedSlots,
    TextSpan Span);
