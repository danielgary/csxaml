namespace Csxaml.Generator;

internal sealed record PropertyNode(
    string Name,
    PropertyValueKind ValueKind,
    string ValueText,
    TextSpan Span);
