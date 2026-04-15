namespace Csxaml.Generator;

internal sealed record InjectFieldDefinition(
    string TypeName,
    string Name,
    TextSpan Span);
