namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Describes a component declaration discovered in CSXAML source text.
/// </summary>
/// <param name="Name">The component name.</param>
/// <param name="NameStart">The zero-based start offset of the component name.</param>
/// <param name="NameLength">The length of the component name.</param>
/// <param name="Parameters">The component parameters declared with the component.</param>
public sealed record CsxamlComponentSignature(
    string Name,
    int NameStart,
    int NameLength,
    IReadOnlyList<CsxamlComponentParameterSignature> Parameters);
