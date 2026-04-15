namespace Csxaml.Tooling.Core.Markup;

public sealed record CsxamlUsingDirectiveInfo(
    string NamespaceName,
    string? Alias,
    int Start,
    int Length);
