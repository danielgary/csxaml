namespace Csxaml.Tooling.Core.Markup;

public sealed record CsxamlUsingDirectiveInfo(
    string QualifiedName,
    string? Alias,
    bool IsStatic,
    int Start,
    int Length);
