namespace Csxaml.Tooling.Core.Markup;

/// <summary>
/// Describes a using directive discovered in CSXAML source text.
/// </summary>
/// <param name="QualifiedName">The namespace or type name referenced by the directive.</param>
/// <param name="Alias">The alias assigned by the directive, or <see langword="null"/> when none is present.</param>
/// <param name="IsStatic">A value indicating whether the directive is a static using.</param>
/// <param name="Start">The zero-based start offset of the directive.</param>
/// <param name="Length">The length of the directive.</param>
public sealed record CsxamlUsingDirectiveInfo(
    string QualifiedName,
    string? Alias,
    bool IsStatic,
    int Start,
    int Length);
