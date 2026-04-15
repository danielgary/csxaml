namespace Csxaml.Tooling.Core.Markup;

public sealed record CsxamlComponentParameterSignature(
    string Name,
    string TypeName,
    int Start,
    int Length);
