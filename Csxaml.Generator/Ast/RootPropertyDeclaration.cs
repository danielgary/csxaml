namespace Csxaml.Generator;

internal sealed record RootPropertyDeclaration(
    string Name,
    string ValueExpression,
    TextSpan ValueSpan,
    TextSpan Span);
