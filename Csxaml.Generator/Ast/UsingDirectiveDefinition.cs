namespace Csxaml.Generator;

internal sealed record UsingDirectiveDefinition(
    string? Alias,
    string QualifiedName,
    bool IsStatic,
    TextSpan Span)
{
    public bool IsAlias => Alias is not null;
}
