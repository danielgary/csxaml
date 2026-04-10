namespace Csxaml.Generator;

internal readonly record struct Token(
    TokenKind Kind,
    string Text,
    TextSpan Span);
