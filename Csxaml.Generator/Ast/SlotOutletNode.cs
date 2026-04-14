namespace Csxaml.Generator;

internal sealed record SlotOutletNode(
    IReadOnlyList<PropertyNode> Properties,
    IReadOnlyList<ChildNode> Children,
    TextSpan Span) : ChildNode(Span);
