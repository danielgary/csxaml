namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Describes a component parameter declaration discovered in CSXAML source text.
/// </summary>
/// <param name="Name">The parameter name.</param>
/// <param name="TypeName">The declared parameter type text.</param>
/// <param name="Start">The zero-based start offset of the parameter name.</param>
/// <param name="Length">The length of the parameter name.</param>
public sealed record CsxamlComponentParameterSignature(
    string Name,
    string TypeName,
    int Start,
    int Length);
