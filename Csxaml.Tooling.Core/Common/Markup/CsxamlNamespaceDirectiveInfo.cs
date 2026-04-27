namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Describes a namespace directive discovered in CSXAML source text.
/// </summary>
/// <param name="NamespaceName">The namespace declared by the directive.</param>
/// <param name="Start">The zero-based start offset of the directive.</param>
/// <param name="Length">The length of the directive.</param>
public sealed record CsxamlNamespaceDirectiveInfo(
    string NamespaceName,
    int Start,
    int Length);
