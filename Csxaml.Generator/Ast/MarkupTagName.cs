namespace Csxaml.Generator;

internal sealed record MarkupTagName(
    string Text,
    string? Prefix,
    string LocalName,
    TextSpan Span);
