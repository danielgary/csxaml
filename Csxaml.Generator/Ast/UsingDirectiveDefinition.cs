namespace Csxaml.Generator;

internal sealed record UsingDirectiveDefinition(
    string? Alias,
    string NamespaceName,
    TextSpan Span)
{
    public bool IsAlias => Alias is not null;
}
