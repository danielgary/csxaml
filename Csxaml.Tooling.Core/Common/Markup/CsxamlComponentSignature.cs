namespace Csxaml.Tooling.Core.Markup;

public sealed record CsxamlComponentSignature(
    string Name,
    int NameStart,
    int NameLength,
    IReadOnlyList<CsxamlComponentParameterSignature> Parameters);
