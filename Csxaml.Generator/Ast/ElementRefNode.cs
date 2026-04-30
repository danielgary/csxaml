namespace Csxaml.Generator;

internal sealed record ElementRefNode(
    PropertyValueKind ValueKind,
    string ValueText,
    TextSpan ValueSpan,
    TextSpan Span);
