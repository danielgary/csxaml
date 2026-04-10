namespace Csxaml.Generator;

internal sealed record StateFieldDefinition(
    string TypeName,
    string Name,
    string InitialValueExpression,
    TextSpan Span);
